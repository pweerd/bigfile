/*
 * Licensed to De Bitmanager under one or more contributor
 * license agreements. See the NOTICE file distributed with
 * this work for additional information regarding copyright
 * ownership. De Bitmanager licenses this file to you under
 * the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Drawing;
using Bitmanager.Core;
using Bitmanager.IO;
using Microsoft.Win32;

namespace Bitmanager.BigFile
{
   /// <summary>
   /// Save/load settings in registry
   /// </summary>
   public class Settings
   {
      private const String _KEY = @"software\bitmanager\bigfile";

      public Color HighlightColor = Color.Lime;
      public Color ContextColor = Color.LightGray;
      public int MaxPartialSize = -1; //Currently not used...
      public int MultiSelectLimit = 1000;
      public int NumContextLines = 0;
      private int _searchThreads = 0;
      public String SearchThreadsAsText
      {
         get
         {
            return _searchThreads == 0 ? "auto" : _searchThreads.ToString();
         }
         set
         {
            _searchThreads = (value == "auto") ? 0 : Invariant.ToInt32(value);
         }
      }
      public int SearchThreads { get { return GetActualNumSearchThreads(); } }
      public string GzipExe;

      private String _LoadMemoryIfBigger = "Auto";
      public String LoadMemoryIfBigger
      {
         get { return _LoadMemoryIfBigger; }
         set { _LoadMemoryIfBigger = CheckAndRepairSize(value, true); }
      }
      private String _CompressMemoryIfBigger = "Auto";
      public String CompressMemoryIfBigger
      {
         get { return _CompressMemoryIfBigger; }
         set { _CompressMemoryIfBigger = CheckAndRepairSize(value, true); }
      }

      public Settings()
      {
         try
         {
            GzipExe = GzipProcessInputStream.FindGzip();
         }
         catch (Exception e)
         {
            Logs.ErrorLog.Log("Cannot load settings");
            Logs.ErrorLog.Log(e);
         }
      }

      public int GetActualNumSearchThreads()
      {
         Globals.MainLogger.Log("Threads={0}", _searchThreads);
         var ret = _searchThreads;
         var N = Environment.ProcessorCount;
         if (ret <= 0) ret += N;

         if (ret < 1) ret = 1;
         else if (ret > N) ret = N;
         Globals.MainLogger.Log("Threads result={0}", ret);
         return ret;
      }

      public void Load()
      {
         var rootKey = Registry.CurrentUser;
         using (var key = rootKey.OpenSubKey(_KEY, false))
         {
            GzipExe = readVal(key, "gzip", GzipExe);
            ContextColor = readVal(key, "context_color", ContextColor);
            HighlightColor = readVal(key, "highlight_color", HighlightColor);
            MultiSelectLimit = readVal(key, "select_limit", MultiSelectLimit);
            NumContextLines = readVal(key, "context_lines", NumContextLines);
            SearchThreadsAsText = readVal(key, "search_threads", SearchThreadsAsText);
            LoadMemoryIfBigger = CheckAndRepairSize (readVal(key, "load_memory", LoadMemoryIfBigger), false);
            CompressMemoryIfBigger = CheckAndRepairSize (readVal(key, "compress_memory", CompressMemoryIfBigger), false);
         }
      }

      public static String CheckAndRepairSize(String x, bool mustExcept)
      {
         x = x.TrimToNull();
         if (x == null) return "Auto";
         switch (x.ToLowerInvariant())
         {
            case "auto":
            case "off":
            case "on":
               break;
            default:
               try
               {
                  Pretty.ParseSize(x);
               }
               catch (Exception e)
               {
                  if (mustExcept) throw new BMException(e, e.Message);
                  x = "Auto";
               }
               break;
         }
         return x;
      }

      public static long GetActualSize (String size, String auto)
      {
         size = size.TrimToNull();
         if (size == null) size = auto;
         switch (size.ToLowerInvariant())
         {
            case "auto":
               return GetActualSize(auto, "off");
            case "off":
               return long.MaxValue;
            case "on":
               return -1;
            default:
               return Pretty.ParseSize(size);
         }
      }


      public void Save()
      {
         var rootKey = Registry.CurrentUser;
         using (var key = rootKey.CreateSubKey(_KEY, true))
         {
            writeVal(key, "gzip", GzipExe);
            writeVal(key, "context_color", ContextColor);
            writeVal(key, "highlight_color", HighlightColor);
            writeVal(key, "select_limit", MultiSelectLimit);
            writeVal(key, "context_lines", NumContextLines);
            writeVal(key, "search_threads", SearchThreadsAsText);
            writeVal(key, "load_memory", LoadMemoryIfBigger);
            writeVal(key, "compress_memory", CompressMemoryIfBigger);
         }
         Load();
      }
      public void SaveFormPosition(int left, int top, int w, int h)
      {
         String sizeStr = Invariant.Format("{0};{1};{2};{3}", w, h, left, top);
         var rootKey = Registry.CurrentUser;
         using (var key = rootKey.CreateSubKey(_KEY, true))
         {
            writeVal(key, "position", sizeStr);
         }
      }
      public bool LoadFormPosition(out int left, out int top, out int w, out int h)
      {
         String sizeStr;
         var rootKey = Registry.CurrentUser;
         using (var key = rootKey.CreateSubKey(_KEY, true))
         {
            sizeStr = readVal(key, "position", null);
         }
         left = -1;
         top = -1;
         w = -1;
         h = -1;
         if (String.IsNullOrEmpty(sizeStr)) return false;

         String[] parts = sizeStr.Split(';');
         for (int i=0; i<parts.Length; i++)
         {
            int v;
            if (!int.TryParse(parts[i], out v)) break;
            if (v <= 0) break;

            switch (i)
            {
               case 0: w = v; continue;
               case 1: h = v; continue;
               case 2: left = v; continue;
               case 3: top = v; continue;
            }
            break;
         }
         return (w >= 0);
      }


      private static String readVal(RegistryKey key, String valName, String def)
      {
         if (key == null) return def;
         var v = key.GetValue(valName, def);
         return v == null ? null : v.ToString();
      }
      private static Color readVal(RegistryKey key, String valName, Color def)
      {
         return ColorTranslator.FromHtml(readVal(key, valName, ColorTranslator.ToHtml(def)));
      }
      private static int readVal(RegistryKey key, String valName, int def)
      {
         if (key == null) return def;
         var v = key.GetValue(valName, def);
         return v == null ? def : (int)v;
      }
      private static bool readVal(RegistryKey key, String valName, bool def)
      {
         if (key == null) return def;
         var v = key.GetValue(valName, def ? 1 : 0);
         return v == null ? def : (0 != (int)v);
      }
      private static void writeVal(RegistryKey key, String valName, String val)
      {
         if (key == null) return;
         key.SetValue(valName, val);
      }
      private static void writeVal(RegistryKey key, String valName, int val)
      {
         if (key == null) return;
         key.SetValue(valName, val);
      }
      private static void writeVal(RegistryKey key, String valName, bool val)
      {
         if (key == null) return;
         key.SetValue(valName, val ? 1 : 0);
      }
      private static void writeVal(RegistryKey key, String valName, Color val)
      {
         if (key == null) return;
         writeVal(key, valName, ColorTranslator.ToHtml(val));
      }
   }
}