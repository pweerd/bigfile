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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitmanager.Core;
using Bitmanager.IO;

namespace Bitmanager.BigFile {
   public partial class ThreadContext {
      public Encoding Encoding = Encoding.UTF8;
      public readonly IDirectStream DirectStream;
      private readonly List<long> partialLines;  //Not owned!!
      private byte[] byteBuffer;
      private char[] charBuffer;

      public byte[] ByteBuffer { get { return byteBuffer; } }
      public char[] CharBuffer { get { return charBuffer; } }


      public ThreadContext (Encoding encoding, IDirectStream strm, List<long> partialLines) {
         this.DirectStream = strm;
         this.partialLines = partialLines;
         this.Encoding = encoding;
      }
      private ThreadContext (ThreadContext other, int maxBufferSize) {
         this.DirectStream = other.DirectStream.NewInstanceForThread ();
         this.partialLines = other.partialLines;
         this.Encoding = other.Encoding;
         SetMaxBufferSize (maxBufferSize);
      }

      public ThreadContext NewInstanceForThread () {
         if (byteBuffer == null) throw new Exception ("MaxBufferSize should be set before cloning.");
         return new ThreadContext (this, byteBuffer.Length);
      }

      public ThreadContext NewInstanceForThread (int maxBufferSize) {
         return new ThreadContext (this, maxBufferSize);
      }

      public void Close () {
         DirectStream.Close ();
      }
      public void CloseInstance () {
         DirectStream.CloseInstance ();
      }

      public void SetMaxBufferSize (int size) {
         byteBuffer = new byte[size];
         charBuffer = new char[size];
      }

      public int ReadPartialLineBytesInBuffer (int from, int until) {
         if (until >= this.partialLines.Count || from < 0)
            return 0;

         long o1 = partialLines[from] >> LineFlags.FLAGS_SHIFT;
         long o2 = partialLines[until] >> LineFlags.FLAGS_SHIFT;
         int lineLen = (int)(o2 - o1);
         if (lineLen == 0) return 0;

         int bytesRead = 0;
         try {
            long position = o1;
            while (true) {
               int freeSpace = lineLen - bytesRead;
               if (freeSpace <= 0) break;

               var len = DirectStream.Read (position, byteBuffer, bytesRead, freeSpace);
               if (len == 0) break;
               position += len;
               bytesRead += len;
            }
         } catch (Exception err) {
            Logs.ErrorLog.Log (err);
            String msg = err.ToString ();
            return Encoding.GetBytes (msg, 0, msg.Length, byteBuffer, 0);
         }
         int end = bytesRead - 1;
         while (end >= 0) {
            switch (byteBuffer[end]) {
               case 10:
               case 13:
                  --end;
                  continue;
            }
            break;
         }

         return end + 1;
      }

      private int readPartialLineBytesInBuffer (long fromOffset, long toOffset) {
         int lineLen = (int)(toOffset - fromOffset);
         if (lineLen == 0) return 0;

         int bytesRead = 0;
         try {
            long position = fromOffset;
            while (true) {
               int freeSpace = lineLen - bytesRead;
               if (freeSpace <= 0) break;

               var len = DirectStream.Read (position, byteBuffer, bytesRead, freeSpace);
               if (len == 0) break;
               position += len;
               bytesRead += len;
            }
         } catch (Exception err) {
            Logs.ErrorLog.Log (err);
            String msg = err.ToString ();
            return Encoding.GetBytes (msg, 0, msg.Length, byteBuffer, 0);
         }
         return bytesRead;
      }


      private int readPartialLineBytes (long fromOffset, long toOffset, byte[] buf, int offset, int count) {
         long bytes = toOffset - fromOffset;
         if (bytes <= 0) return 0;

         if (count > bytes) count = (int)bytes;
         int bytesRead = 0;
         try {
            long position = fromOffset;
            while (true) {
               int freeSpace = count - bytesRead;
               if (freeSpace <= 0) break;

               var len = DirectStream.Read (position, buf, offset + bytesRead, freeSpace);
               if (len == 0) break;
               position += len;
               bytesRead += len;
            }
         } catch (Exception err) {
            Logs.ErrorLog.Log (err);
            String msg = err.ToString ();
            return Encoding.GetBytes (msg, 0, msg.Length, buf, offset);
         }

         return bytesRead;
      }

      public int ReadPartialLineCharsInBuffer (int from, int until) {
         int bytes = ReadPartialLineBytesInBuffer (from, until);
         return Encoding.GetChars (byteBuffer, 0, bytes, charBuffer, 0);
      }
      private int readPartialLineCharsInBuffer (long from, long until) {
         int bytes = readPartialLineBytesInBuffer (from, until);
         return Encoding.GetChars (byteBuffer, 0, bytes, charBuffer, 0);
      }

      private int getMaxBytesForLimitedChars(int ll) {
         if (ll > 0) {
            switch (Encoding.CodePage) {
               case 28591:
                  break;
               default:
                  ll = ll < int.MaxValue / 2 ? 2 * ll : int.MaxValue;
                  break;
            }
         }
         return ll;
      }

      public String GetLine (int from, int until, int maxLineLength, out bool truncated) {
         return getLine (from, until, maxLineLength, out truncated, null);
      }

      public String GetPartialLine (int from, int until, int maxChars, ICharReplacer replacer) {
         bool truncated;
         return getLine (from, until, maxChars, out truncated, replacer);
      }

      //UFT16LE: new line=0a 00.
      //         for the real start we need to advance 1 byte
      //         the end should be OK
      //UFT16BE: new line=00 0a. So the
      //         the real start should be OK
      //         the end should be truncated to a multiple of 2
      private unsafe String getLine (int from, int until, int maxLineLength, out bool truncated, ICharReplacer replacer) {
         long o1 = partialLines[from] >> LineFlags.FLAGS_SHIFT;
         long o2 = partialLines[until] >> LineFlags.FLAGS_SHIFT;
         long diff = o2 - o1;
         int maxBytes = getMaxBytesForLimitedChars (maxLineLength);
         switch (Encoding.CodePage) {
            case FileEncoding.CP_UTF16:
               o1 = (o1 + 1) & ~1L;
               o2 = (o2 + 1) & ~1L;
               diff = (o2 - o1);
               break;
            case FileEncoding.CP_UTF16BE:
               o1 = o1 & ~1L;
               o2 = o2 & ~1L;
               diff = (o2 - o1);
               break;
         };
         int len;
         if (diff > maxBytes) {
            len = maxBytes;
            truncated = true;
         } else {
            len = (int)diff;
            truncated = false;
         }
         Globals.MainLogger.Log ("-- o1={0}, o2={1}", o1, o2);

         if (byteBuffer.Length >= len) //Simple case: the line fits in the pre-allocated buffer
         {
            len = readPartialLineCharsInBuffer (o1, o2);
            return new string (charBuffer, 0, len);
         }

         //Didn't fit. We allocate a 2 times larger byte buffer, to be able to contains the chars as well.
         //We will do an in-place conversion and then convert to a string
         byte[] tmp = new byte[2 * len];
         int bytesRead = readPartialLineBytes (o1, o2, tmp, len, len);

         fixed (byte* pBuf = &tmp[0]) {
            byte* pSrc = pBuf + len;
            char* pDst = (char*)pBuf;
            int chars = Encoding.GetChars (pSrc, bytesRead, pDst, tmp.Length / 2);
            char* pEnd = pDst + chars;
            while (pDst < pEnd) {
               switch (*pDst) {
                  case '\n':
                  case '\r': ++pDst; continue;
               }
               break;
            }
            while (pDst < pEnd) {
               switch (pEnd[-1]) {
                  case '\n':
                  case '\r': --pEnd; continue;
               }
               break;
            }
            chars = (int)(pEnd - pDst);
            if (maxLineLength > 0 && maxLineLength > chars) chars = maxLineLength;
            if (replacer != null) replacer.Replace (pDst, pEnd);

            return new String (pDst, 0, chars);
         }
      }

      private void dumpOffset (int i, String why) {
         long x = partialLines[i];
         long offs = x >> LineFlags.FLAGS_SHIFT;
         long mask = x & LineFlags.FLAGS_MASK;
         Globals.MainLogger.Log ("-- {0}: o={1} (0x{1:X}), flags=0x{2:X}, rsn={3}", i, offs, mask, why);
      }
   }


}
