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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitmanager.Core;
using Bitmanager.IO;

namespace Bitmanager.BigFile {
   public class ThreadContext {
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
         if (size < 4096) size = 4096;
         if (byteBuffer==null || byteBuffer.Length < size) {
            charBuffer = new char[size];
            byteBuffer = new byte[size];
         }
      }

      public int ReadPartialLineBytesInBuffer (int from, int until) {
         if (until >= this.partialLines.Count || from < 0) return 0;
         return readPartialLineBytesInBuffer (partialLines[from] >> LineFlags.FLAGS_SHIFT, 
                                              partialLines[until] >> LineFlags.FLAGS_SHIFT);
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
            string msg = err.ToString ();
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
            string msg = err.ToString ();
            return Encoding.GetBytes (msg, 0, msg.Length, buf, offset);
         }

         return bytesRead;
      }

      public string ReadPartialLineInBuffer (int from, int until) {
         int bytes = ReadPartialLineBytesInBuffer (from, until);
         return byteBufferViaCharBufferToString (bytes, null);
      }
      private string readPartialLineInBuffer (long from, long until, ICharReplacer replacer) {
         int bytes = readPartialLineBytesInBuffer (from, until);
         return byteBufferViaCharBufferToString (bytes, replacer);
      }

      private unsafe string byteBufferViaCharBufferToString (int bytes, ICharReplacer replacer) {
         int N = Encoding.GetChars (byteBuffer, 0, bytes, charBuffer, 0);

         //Strip the cr/lf at the beginning/end, and eventual replace some chars
         fixed (char* pBuf = charBuffer) {
            char* pEnd = pBuf + N;
            char* p = pBuf;

            for (; p < pEnd; p++) if (*p != '\r' && *p != '\n') break;
            for (; p < pEnd; pEnd--) if (pEnd[-1] != '\r' && pEnd[-1] != '\n') break;

            if (replacer != null)
               replacer.Replace (p, pEnd);

            //return the formatted string
            return new string (pBuf, (int)(p-pBuf), (int)(pEnd-p));
         }
      }

      private int getMaxBytesForLimitedChars(int ll) {
         if (ll > 0) {
            switch (Encoding.CodePage) {
               case FileEncoding.CP_EXTENDED_LATIN:
                  break;
               default:
                  ll = ll < int.MaxValue / 2 ? 2 * ll : int.MaxValue;
                  break;
            }
         }
         return ll;
      }

      public string GetLine (int from, int until, int maxLineLength, out bool truncated) {
         return getLine (from, until, maxLineLength, out truncated, null);
      }

      public string GetPartialLine (int from, int until, int maxChars, ICharReplacer replacer) {
         bool truncated;
         return getLine (from, until, maxChars, out truncated, replacer);
      }
      
      public byte[] GetLineBytes(int from, int until, int maxLineLength, out bool truncated) {
         long start = getLineStartEnd (from, until, out var end);
         long diff = end - start;
         int len = (int)diff;
         if (diff <= maxLineLength) truncated = false;
         else {
            truncated = true;
            len = maxLineLength;
            end = start + len;
         }
         byte[] buf = new byte[len];
         readPartialLineBytes (start, end, buf, 0, len);
         return buf;
      }

      //UFT16LE: new line=0a 00.
      //         for the real start we need to advance 1 byte
      //         the end should be OK
      //UFT16BE: new line=00 0a. So the
      //         the real start should be OK
      //         the end should be truncated to a multiple of 2
      private long getLineStartEnd (int from, int until, out long end) {
         long o1 = partialLines[from] >> LineFlags.FLAGS_SHIFT;
         long o2 = partialLines[until] >> LineFlags.FLAGS_SHIFT;
         switch (Encoding.CodePage) {
            case FileEncoding.CP_UTF16:
               o1 = (o1 + 1) & ~1L;
               o2 = (o2 + 1) & ~1L;
               break;
            case FileEncoding.CP_UTF16BE:
               o1 = o1 & ~1L;
               o2 = o2 & ~1L;
               break;
         };
         end = o2;
         return o1;
      }

      private byte[] getLineBytes(int from, int until, int maxLineLength, out bool truncated) {
         long start = getLineStartEnd(from, until, out var end);
         long diff = end - start;
         int len = (int)diff;
         if (diff <= maxLineLength) truncated = false;
         else {
            truncated = true;
            len = maxLineLength;
            end = start + len;
         }
         byte[] buf = new byte[len];
         readPartialLineBytes (start, end, buf, 0, len);
         return buf;
      }

      private unsafe string getLine (int from, int until, int maxLineLength, out bool truncated, ICharReplacer replacer) {
         long start = getLineStartEnd (from, until, out var end);
         long diff = end - start;

         int len;
         int maxBytes = getMaxBytesForLimitedChars (maxLineLength);
         if (diff > maxBytes) {
            len = maxBytes;
            truncated = true;
         } else {
            len = (int)diff;
            truncated = false;
         }

         if (byteBuffer.Length >= len) { //Simple case: the line fits in the pre-allocated buffer
            return readPartialLineInBuffer (start, end, replacer);
         }

         //Didn't fit. We allocate a 2 times larger byte buffer, to be able to contains the chars as well.
         //We will do an in-place conversion and then convert to a string
         byte[] tmp = new byte[2 * len];
         int bytesRead = readPartialLineBytes (start, end, tmp, len, len);

         fixed (byte* pBuf = tmp) {
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
            if (maxLineLength > 0 && maxLineLength < chars) {
               chars = maxLineLength;
               truncated = true;
            }
            if (replacer != null) replacer.Replace (pDst, pEnd);

            return new string (pDst, 0, chars);
         }
      }

      public int GetPartialLineLengthInChars (int index) {
         long start = getLineStartEnd (index, index+1, out var end);
         long diff = end - start;
         switch (Encoding.CodePage) {
            case FileEncoding.CP_UTF16:
            case FileEncoding.CP_UTF16BE:
               return (int)(diff / 2);
            case FileEncoding.CP_EXTENDED_LATIN:
               return (int)diff;
            case FileEncoding.CP_UTF8:
               int len = readPartialLineBytesInBuffer (start, end);
               return Encoding.GetCharCount (byteBuffer, 0, len);
            default:
               throw new BMException ("Unexpected Encoding: {0}", Encoding);
         };
      }
   }


}
