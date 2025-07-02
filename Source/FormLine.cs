/*
 * Copyright 2022, De Bitmanager
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Bitmanager.Core;
using Bitmanager.Json;
using Bitmanager.BigFile.Query;
using System.Linq;
using Bitmanager.Xml;
using Bitmanager.Query;
using System.Text;
using Microsoft.Win32;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Form to show one line (with highlighting and navigation)
   /// </summary>
   public partial class FormLine : Form {
      static readonly string[] ViewAsItems = { "Auto", "Text", "Json", "Xml", "Csv", "HexBytes", "HexChars" };
      private static int lastViewAsIndex;
      private static bool lastCanonicalState;
      private static bool lastExpandEncodedState;
      private static readonly Logger logger = Globals.MainLogger.Clone ("line");
      private Settings settings;
      private List<SearchNode> searchNodes;
      private LogFile lf;
      private List<int> filter;

      //Current line as a string and as bytes
      private string curLine;
      private byte[] curLineBytes;

      private bool closed;
      public bool IsClosed { get { return closed; } }

      private List<Tuple<int, int>> curMatches;
      private int internalLineIndex;
      private int publicLineIndex;
      private int partialIndex;
      private int matchIdx;

      //Keep track of the last position and the total count of open line-forms
      private static PointClass lastPosition;
      private static int numLineForms;

      public FormLine () {
         InitializeComponent ();
         menuNormalized.Checked = lastCanonicalState;
         menuExpandJson.Checked = lastExpandEncodedState;

         ShowInTaskbar = true;

         //Prevent font being way too small after non-latin chars 
         textLine.LanguageOption = RichTextBoxLanguageOptions.DualFont;
         textLine.Dock = DockStyle.Fill;

         cbViewAs.Items.AddRange (ViewAsItems);
         cbViewAs.SelectedIndex = lastViewAsIndex;
         StartPosition = FormStartPosition.Manual;
      }

      /// <summary>
      /// Updates the connected (partial) logFile with a new one if the source of the logFile is the same
      /// </summary>
      public void UpdateLogFile (LogFile lf) {
         if (closed || this.lf == null) return;
         if (this.lf.IsSameFile (lf))
            this.lf = lf;
      }

      /// <summary>
      /// Shows the requested line in this form
      /// </summary>
      public void ShowLine (Point location, Settings c, LogFile lf, List<int> filter, int partialLineNo, ParserNode<SearchContext> lastQuery)//, string lastQueryText)
      {
         var pt =lastPosition==null ? location : new Point(lastPosition.X, lastPosition.Y);
         if (Interlocked.Add(ref numLineForms, 0) > 1 || lastPosition==null) {
            pt.X += 50;
            pt.Y += 50;
         }
         DesktopLocation = pt;

         this.settings = c;
         if (lastQuery == null)
            searchNodes = new List<SearchNode> ();
         else {
            searchNodes = lastQuery.CollectValueNodes ().ConvertAll<SearchNode> (x => (SearchNode)x);
            cbSearch.Text = string.Join ("  ", searchNodes.ConvertAll (x => x.ToString ()));
         }
         this.lf = lf;
         this.filter = filter;
         logger.Log ("Starting with partial {0}", partialLineNo);

         //Translate logical record into partial record index if we have a filter
         if (filter != null) {
            if (partialLineNo < 0) partialIndex = -1;
            else if (partialLineNo >= filter.Count) partialLineNo = lf.PartialLineCount;
            else partialLineNo = filter[partialLineNo];
         }

         enableAll (true);
         setLine (partialLineNo);
         Show ();
         Activate ();
      }

      private void setIndexes (int partial, int line) {
         partialIndex = partial;
         internalLineIndex = line;
         publicLineIndex = line + lf.SkippedLines;
      }

      private void setLine (int partialLineNo) {
         textLine.Focus ();
         logger.Log ("SetLine ({0})", partialLineNo);
         if (partialLineNo < 0) {
            setIndexes (-1, -1);
            Text = Globals.CreateTitle(lf.FileName, "before top");
            clear ();
            return;
         }
         if (partialLineNo >= lf.PartialLineCount) {
            setIndexes (lf.PartialLineCount, lf.LineCount);
            Text = Globals.CreateTitle (lf.FileName, "after bottom");
            clear ();
            return;
         }

         setIndexes (partialLineNo, lf.PartialToLineNumber (partialLineNo));

         bool truncBytes, truncChars;
         curLine = lf.GetLine (internalLineIndex, out truncChars);
         curLineBytes = lf.GetLineBytes (internalLineIndex, out truncBytes);
         Text = Globals.CreateTitle (lf.FileName, publicLineIndex, truncChars || truncBytes);
         logger.Log ("SetLine ({0}): loading full line {1}...", partialLineNo, publicLineIndex);

         loadLineInControl ();
         logger.Log ("SetLine (): loaded {0} chars in control", curLine.Length);
      }
      private static string convertToJson (string s, bool normalized, bool handleEncodedJson) {
         var json = JsonValue.Parse (s);
         if (normalized) json = json.Canonicalize ();
         if (handleEncodedJson) json = expandEncodedJson (json);

         return json.ToString (true).Replace ("\r\n", "\n");
      }
      private static string convertToXml (string s) {
         var hlp = new XmlHelper ();
         hlp.LoadXml (s);
         return hlp.SaveToString ().Replace ("\r\n", "\n");
      }

      private static string convertToCsv (string s) {
         if (string.IsNullOrEmpty (s)) return s;

         int commas = 0;
         int semi = 0;
         int tabs = 0;
         foreach (char c in s) {
            switch (c) {
               default: continue;
               case ',': ++commas; continue;
               case ';': ++semi; continue;
               case '\t': ++tabs; continue;
            }
         }

         int cnt = tabs;
         char sep = '\t';
         string sepAsText = "tab";
         if (commas > cnt) {
            cnt = commas;
            sep = ',';
            sepAsText = "comma";
         }
         if (semi > cnt) {
            cnt = semi;
            sep = ';';
            sepAsText = "semicolon";
         }
         if (cnt == 0) return s;

         var sb = new StringBuilder (s.Length + 64);
         sb.AppendFormat ("Fields separated by {0}:\n", sepAsText);
         int i = 0;
         foreach (string x in s.Split (sep)) {
            sb.AppendFormat (Invariant.Culture, "[{0:d2}]: '{1}'\n", i, x);
            i++;
         }
         return sb.ToString ();
      }

      //Convert the backing bytes of the line into hex format
      private static string convertToHexBytes (byte[] bytes) {
         if (bytes == null || bytes.Length == 0) return string.Empty;

         var sb = new StringBuilder (4 * bytes.Length);

         sb.AppendFormat ("00000000: {0:X2}", bytes[0]);
         int i;
         for (i = 1; i < bytes.Length; i++) {
            if ((i % 4) == 0) {
               if ((i % 32) == 0) {
                  sb.Append ("  | ");
                  for (int j = i - 32; j < i; j++) appendChar (sb, bytes[j]);
                  sb.AppendFormat ("\n{0:X8}: ", i);
               } else
                  sb.Append (' ');
            }
            sb.AppendFormat ("{0:X2}", bytes[i]);
         }
         int offset = i % 32;
         if (offset > 0) {
            int end = i + 32 - offset;
            for (; i < end; i++) {
               if ((i % 4) == 0) {
                  sb.Append (' ');
               }
               sb.Append ("  ");
            }
         }
         sb.Append ("  | ");
         for (int j = i - 32; j < bytes.Length; j++) appendChar (sb, bytes[j]);
         sb.Append ('\n');
         return sb.ToString ();
      }


      //Convert the UTF16-chars in the line into hex format
      private static string convertToHexChars (string x) {
         if (string.IsNullOrEmpty (x)) return string.Empty;

         var sb = new StringBuilder (4 * x.Length);

         sb.AppendFormat ("00000000: {0:X4}", (int)x[0]);
         int i;
         for (i = 1; i < x.Length; i++) {
            if ((i % 4) == 0) {
               if ((i % 16) == 0) {
                  sb.Append ("  | ");
                  for (int j = i - 16; j < i; j++) appendChar (sb, x[j]);
                  sb.AppendFormat ("\n{0:X8}:", i);
               } else
                  sb.Append ("  ");
            }
            sb.AppendFormat (" {0:X4}", (int)x[i]);
         }
         int offset = i % 16;
         if (offset > 0) {
            int end = i + 16 - offset;
            for (; i < end; i++) {
               if ((i % 4) == 0) {
                  sb.Append ("  ");
               }
               sb.Append ("     ");
            }
         }
         sb.Append ("  | ");
         for (int j = i - 16; j < x.Length; j++) appendChar (sb, x[j]);
         sb.Append ('\n');
         return sb.ToString ();
      }

      private static void appendChar (StringBuilder sb, int ch) {
         sb.Append (ch < ' ' ? '.' : (char)ch);
      }


      private int getIndexInLogFile (int ix) {
         return filter == null ? ix : filter[ix];
      }

      private void clear () {
         textLine.Clear ();
      }

      //Reading last state from the registry
      public static void LoadState (RegistryKey key) {
         string x = SettingsSource.ReadVal (key, "line_view_as", string.Empty);
         int ix;
         for (ix = 0; ix < ViewAsItems.Length; ix++) {
            if (string.Equals (x, ViewAsItems[ix], StringComparison.InvariantCultureIgnoreCase)) {
               lastViewAsIndex = ix;
            }
         }
         lastCanonicalState = SettingsSource.ReadVal (key, "line_canonical", false);
         lastExpandEncodedState = SettingsSource.ReadVal (key, "line_expand_encoded", false);
      }

      //Saving state into the registry
      public static void SaveState (RegistryKey key) {
         int ix = lastViewAsIndex;
         if (ix >= 0 && ix < ViewAsItems.Length)
            SettingsSource.WriteVal (key, "line_view_as", ViewAsItems[ix]);
         SettingsSource.WriteVal (key, "line_canonical", lastCanonicalState);
         SettingsSource.WriteVal (key, "line_expand_encoded", lastExpandEncodedState);
      }

      private List<Tuple<int, int>> extractMatches (string x) {
         var ret = new List<Tuple<int, int>> ();
         if (searchNodes != null && searchNodes.Count > 0) {
            foreach (var node in searchNodes) {
               logger.Log ("Fetch matches for {0}", node.ToString ());
               var cmp = node.Comparer;
               ret.AddRange (cmp.GetMatches (x));
            }
         }
         ret.Sort (cmpTuple);
         logger.Log ("Found {0} matches", ret.Count);
         return ret;
      }

      private static int cmpTuple (Tuple<int, int> x, Tuple<int, int> y) {
         int rc = x.Item1 - y.Item1;
         return rc != 0 ? rc : x.Item2 - y.Item2;
      }

      enum ContentType { Auto = 0, Text, Json, Xml, Csv, HexBytes, HexChars };
      private static ContentType determineContentType (string content) {
         int jsonChars = 0;
         int xmlChars = 0;
         int csvChars = 0;
         for (int i = 0; i < content.Length; i++) {
            switch (content[i]) {
               case '[':
               case ']':
               case '{':
               case '}':
               case ':':
                  jsonChars += 2;
                  continue;
               case '<':
               case '>':
               case '/':
               case '&':
                  xmlChars += 2;
                  continue;
               case '\t':
               case ';':
                  csvChars += 2;
                  continue;
               case ',':
                  jsonChars += 1;
                  csvChars += 1;
                  continue;
            }
         }
         if (jsonChars <= 3 && csvChars <= 2 && xmlChars <= 3) return ContentType.Text;
         if (jsonChars > xmlChars && jsonChars > csvChars)
            return ContentType.Json;
         if (xmlChars == csvChars) return ContentType.Text;
         return xmlChars > csvChars ? ContentType.Xml : ContentType.Csv;
      }

      private void loadLineInControl () {
         textLine.Focus ();
         if (curLine == null) return;

         Cursor.Current = Cursors.WaitCursor;
         UseWaitCursor = true;
         try {
            string content = curLine;
            Exception error = null;
            try {
               ContentType sel = (ContentType)cbViewAs.SelectedIndex;
               if (sel == ContentType.Auto)
                  sel = determineContentType (curLine);
               switch (sel) {
                  default: break;
                  case ContentType.Json: content = convertToJson (curLine, menuNormalized.Checked, menuExpandJson.Checked); break;
                  case ContentType.Xml: content = convertToXml (curLine); break;
                  case ContentType.Csv: content = convertToCsv (curLine); break;
                  case ContentType.HexBytes: content = convertToHexBytes (curLineBytes); break;
                  case ContentType.HexChars: content = convertToHexChars (curLine); break;
               }
            } catch (Exception err) {
               error = err;
            }

            setMatchedText (content);
            toolStripStatusLabel1.Text = error == null ? string.Empty : error.Message.Replace ('\n', ' ');

         } finally {
            Cursor.Current = Cursors.Default;
            UseWaitCursor = false;
         }
      }

      private void setMatchedText (string content) {
         textLine.Clear ();
         textLine.Text = content;
         curMatches = extractMatches (content);
         matchIdx = 0;

         if (curMatches.Count > 0) {
            textLine.BeginUpdate ();
            try {
               Color backColor = settings.HighlightColor;
               foreach (var m in curMatches) {
                  textLine.Select (m.Item1, m.Item2);
                  textLine.SelectionBackColor = backColor;
               }
               logger.Log ("SetLine ({0}): all done...", partialIndex);
               textLine.Select (curMatches[0].Item1, 0);
               textLine.ScrollToCaret ();
            } finally {
               textLine.EndUpdate ();
            }
         }
      }

      private void buttonClose_Click (object sender, EventArgs e) {
         this.DialogResult = DialogResult.OK;
         Close ();
      }

      private void btnNext_Click (object sender, EventArgs e) {
         gotoNextLine ();
      }
      private void btnPrev_Click (object sender, EventArgs e) {
         gotoPrevLine ();
      }

      private void cbViewAs_SelectedIndexChanged (object sender, EventArgs e) {
         lastViewAsIndex = cbViewAs.SelectedIndex;
         loadLineInControl ();
         textLine.Focus ();
      }

      private void form_KeyPress (object sender, KeyPressEventArgs e) {
         if (sender is TextBox || sender is ComboBox) return;
         switch (e.KeyChar) {
            default: return;
            case '/':
               gotoNextHit (); break;
            case '?':
               gotoPrevHit (); break;
            case '<':
               scrollToCharPos (0); matchIdx = -1; break;
            case '>':
               scrollToCharPos (100000); matchIdx = curMatches.Count; break;
         }
         e.Handled = true;
      }

      private void form_KeyDown (object sender, KeyEventArgs e) {
         if (e.Control) {
            switch (e.KeyCode) {
               default: return;
               case Keys.Up:
                  gotoPrevLine (); break;
               case Keys.Down:
                  gotoNextLine (); break;
               case Keys.F3:
                  gotoPrevHit (); break;
               case Keys.F:
                  cbSearch.Focus (); break;
               case Keys.Home:
                  matchIdx = -1; break;
               case Keys.End:
                  matchIdx = curMatches.Count; break;
            }
            e.Handled = true;
            return;
         }

         if (e.Alt || e.Shift) return;
         switch (e.KeyCode) {
            default: return;
            case Keys.F3:
               gotoNextHit (); break;
         }
         e.Handled = true;
      }

      private void scrollToCharPos (int pos) {
         textLine.Select (pos, 0);
         textLine.ScrollToCaret ();
      }

      private void gotoNextHit () {
         if (curMatches.Count == 0) return;
         if (++matchIdx >= curMatches.Count) matchIdx = 0;
         scrollToCharPos (curMatches[matchIdx].Item1);
      }
      private void gotoPrevHit () {
         if (curMatches.Count == 0) return;
         if (--matchIdx < 0) matchIdx = curMatches.Count - 1;
         scrollToCharPos (curMatches[matchIdx].Item1);
      }

      private void gotoNextLine () {
         setLine (lf.PartialFromLineNumber (lf.NextLineNumber (internalLineIndex, filter)));
      }

      private void gotoPrevLine () {
         setLine (lf.PartialFromLineNumber (lf.PrevLineNumber (internalLineIndex, filter)));
      }

      private void FormLine_Load (object sender, EventArgs e) {
         Interlocked.Increment (ref numLineForms);
      }

      private void FormLine_FormClosed (object sender, FormClosedEventArgs e) {
         Interlocked.Decrement (ref numLineForms);
         closed = true;
      }

      private void enableAll (bool enabled) {
         btnNext.Enabled = enabled;
         btnPrev.Enabled = enabled;
         btnSearch.Enabled = enabled;
         cbViewAs.Enabled = enabled;
         timer1.Enabled = enabled;
      }
      private void timer1_Tick (object sender, EventArgs e) {
         if (lf != null && lf.Disposed) {
            this.Text += " [DISCONNECTED]";
            enableAll (false);
         }
      }

      private void btnSearch_Click (object sender, EventArgs e) {
         if (string.IsNullOrEmpty (cbSearch.Text)) return;
         var topNode = new SearchNodes ().Parse (cbSearch.Text);
         var nodes = topNode.CollectValueNodes ().ConvertAll<SearchNode> (x => (SearchNode)x);
         if (nodes.Count == 0) return;

         this.searchNodes = nodes;
         setMatchedText (textLine.Text);
      }

      private void txtSearch_KeyUp (object sender, KeyEventArgs e) {
         if (e.KeyValue != 13) return;
         if (e.Alt || e.Control || e.Shift) return;
         btnSearch_Click (cbSearch, null);
      }

      private void menuExpandJson_CheckStateChanged (object sender, EventArgs e) {
         lastExpandEncodedState = menuExpandJson.Checked;
         loadLineInControl ();
         textLine.Focus ();
      }

      private void menuNormalized_CheckStateChanged (object sender, EventArgs e) {
         lastCanonicalState = menuNormalized.Checked;
         loadLineInControl ();
         textLine.Focus ();
      }


      private static JsonValue expandEncodedJson (JsonValue x) {
         if (x == null) return x;
         switch (x.Type) {
            default: break;
            case JsonType.Array:
               var arr = (JsonArrayValue)x;
               for (int i = 0; i < arr.Count; i++) arr[i] = expandEncodedJson (arr[i]);
               break;

            case JsonType.Object:
               var obj = (JsonObjectValue)x;
               foreach (var k in obj.Keys.ToList ()) obj[k] = expandEncodedJson (obj[k]);
               break;

            case JsonType.String:
               if (tryExpandJson ((string)x, out var repl)) x = repl;
               break;
         }
         return x;
      }

      private static bool tryExpandJson (string x, out JsonValue repl) {
         if (string.IsNullOrEmpty (x)) goto FAILED;

         for (int i = 0; i < x.Length; i++) {
            switch (x[i]) {
               default: goto FAILED;
               case '{':
               case '[':
                  try {
                     repl = JsonValue.Parse (Encoders.UnEscapeJavascript (x));
                     return true;
                  } catch (Exception e) {
                     goto FAILED;
                  }
               case ' ':
               case '\t':
               case '\r':
               case '\n':
                  continue;
            }
         }

      FAILED:
         repl = null;
         return false;
      }

      private void FormLine_LocationChanged (object sender, EventArgs e) {
         lastPosition = new PointClass(DesktopLocation.X, DesktopLocation.Y);
      }

      //Helper class to keep track of the last position
      //We need a class, not a struct, so we can change the variable without locking
      class PointClass {
         public readonly int X;
         public readonly int Y;
         public PointClass (int x, int y) {
            this.X = x;
            this.Y = y;
         }
      }
   }
}
