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

using Bitmanager.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile {
   public class FileEncoding {
      public const int CP_EXTENDED_LATIN = 28591;
      public const int CP_UTF8 = 65001;
      public const int CP_UTF16 = 1200;
      public const int CP_UTF16BE = 1201;

      public static readonly Encoding Utf8;
      public static readonly Encoding Utf16;
      public static readonly Encoding ExtendedLatin;
      public static readonly Encoding Utf16BE;
      public static readonly Encoding Utf8_Bom;
      public static readonly Encoding Utf16_Bom;
      public static readonly Encoding Utf16BE_Bom;

      public readonly Encoding Current;
      public readonly int PreambleBytes;
      public FileEncoding (IOBlock buffer) {
         Current = getEncodingFromBOM (buffer, out PreambleBytes);
         if (Current == null) Current = detectEncodingFromBytes (buffer);
      }

      static FileEncoding() {
         ExtendedLatin = Encoding.Latin1;
         Utf8 = new UTF8Encoding(false);
         Utf16 = new UnicodeEncoding (false, false);
         Utf16BE = new UnicodeEncoding (true, false);

         Utf8_Bom = Encoding.UTF8;
         Utf16_Bom = Encoding.Unicode;
         Utf16BE_Bom = Encoding.BigEndianUnicode;
      }

      private Encoding detectEncodingFromBytes (IOBlock buffer) {
         int utf16 = 0;
         int utf16BE = 0;
         int utf8 = 0;
         int other = 0;
         int total = 0;
         var arr = buffer.Buffer;
         total = buffer.Length;
         bool prevUtf8 = false;
         for (int i=total-1; i>=0; i--) {
            int b = arr[i];
            if (b==0) {
               prevUtf8 = false;
               if ((i & 1) == 0) ++utf16; else ++utf16BE;
               --other;
               continue;
            }
            if (b<128) {
               prevUtf8 = false;
               ++other;
               continue;
            }
            if ((b & 0xC0) == 0xC0) {
               prevUtf8 = true;
               ++utf8;
               continue;
            }
            if ((b & 0xC0) == 0x80) {
               if (prevUtf8) ++utf8; else { --utf8; prevUtf8 = false; other++; }
               continue;
            }
            ++other;
            prevUtf8 = false;
         }

         if (utf16 > total / 16 && utf16 > other) return Utf16;
         if (utf16BE > total / 16 && utf16BE > other) return Utf16BE;
         return utf8 > 0 ? Utf8 : ExtendedLatin;
      }

      private static Encoding getEncodingFromBOM (IOBlock buffer, out int preambleBytes) {
         preambleBytes = 0;
         if (buffer.Length < 2) return null;
         if (buffer.Buffer[0] == (byte)0xFF && buffer.Buffer[1] == (byte)0xFE) {
            preambleBytes = 2;
            return Utf16;
         }
         if (buffer.Buffer[0] == (byte)0xFE && buffer.Buffer[1] == (byte)0xFF) {
            preambleBytes = 2;
            return Utf16BE;
         }
         if (buffer.Length > 2 && buffer.Buffer[0] == (byte)0xEF && buffer.Buffer[1] == (byte)0xBB && buffer.Buffer[2] == (byte)0xBF) {
            preambleBytes = 3;
            return Utf8; //0xEF,0xBB,0xBF
         }
         return null;
      }


   }
}
