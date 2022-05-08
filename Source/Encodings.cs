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

using Bitmanager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile {
   public class Utf81 : UTF8Encoding {

      public override string ToString () {
         return "Utf81";
      }

      public override string GetString (byte[] bytes) {
         char[] buf = new char[bytes.Length];
         int x = convertFromUtf8 (bytes, 0, bytes.Length, buf, bytes.Length);
         return new string (buf, 0, x);
      }

      public override string GetString (byte[] bytes, int index, int count) {
         char[] buf = new char[count];
         int x = convertFromUtf8 (bytes, index, count, buf, count);
         return new string (buf, 0, x);
      }

      public override int GetChars (byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
         if (charIndex != 0) throw new BMException ("CharIndex can only be 0. Got {0}", charIndex);
         return convertFromUtf8 (bytes, byteIndex, byteCount, chars, chars.Length);
      }


      protected static int convertFromUtf8 (byte[] src, int index, int count, char[] dst, int dstlen) {
         int end = index + count;
         int end2 = end - 1;
         int end3 = end - 2;
         int end4 = end - 3;
         int end5 = end - 4;
         int j = 0;
         for (int i = index; i < end;) {
            int b = src[i];
            if ((b & 0x80) == 0) {
               // 0xxxxxxx - it is an ASCII char, so copy it exactly as it is
               dst[j++] = (char)b;
               i++;
               continue;
            }
            if ((b & 0xE0) == 0xC0) {
               if (i >= end2) break;
               dst[j++] = (char)(((b & 0x1F) << 6) | (src[i + 1] & 0x3F));
               i += 2;
               continue;
            }
            if ((b & 0xF0) == 0xE0) {
               if (i >= end3) break;
               dst[j++] = (char)(((b & 0x0F) << 12) |
                  ((src[i + 1] & 0x3F) << 6) |
                  (src[i + 2] & 0x3F));
               i += 3;
               continue;
            }
            if ((b & 0xF8) == 0xF0) {
               if (i >= end4) break;
               int cp = (((b & 0x07) << 18) |
                  ((src[i + 1] & 0x3F) << 12) |
                  ((src[i + 2] & 0x3F) << 6) |
                  (src[i + 3] & 0x3F));
               i += 4;
               if (cp <= 0xFFFF) {
                  dst[j++] = (char)cp;
                  continue;
               }
               // Encode code point above U+FFFF as surrogate pair.
               dst[j++] = (char)(0xD7C0 + (cp >> 10));
               dst[j++] = (char)(0xDC00 + (cp & 0x3FF));
            }

            //Mark invalid and skip follower byte.
            dst[j++] = (char)0xFFFD;
            for (i++; i < end; i++) if ((src[i] & (byte)0xc0) != (byte)0x80) break;
         }
         return j;
      }


      protected static unsafe int convertFromUtf8 (byte* src, int count, char* dst, int dstlen) {
         byte* pEnd = src + count;
         byte* pEnd2 = pEnd - 1;
         byte* pEnd3 = pEnd - 2;
         byte* pEnd4 = pEnd - 3;
         char* pDst = dst;
         while (src < pEnd) {
            int b = *src;
            if ((b & 0x80) == 0) {
               // 0xxxxxxx - it is an ASCII char, so copy it exactly as it is
               *(dst++) = (char)b;
               src++;
               continue;
            }
            if ((b & 0xE0) == 0xC0) {
               if (src >= pEnd2) break;
               *(dst++) = (char)(((b & 0x1F) << 6) | (src[1] & 0x3F));
               src += 2;
               continue;
            }
            if ((b & 0xF0) == 0xE0) {
               if (src >= pEnd3) break;
               *(dst++) = (char)(((b & 0x0F) << 12) |
                  ((src[1] & 0x3F) << 6) |
                  (src[2] & 0x3F));
               src += 3;
               continue;
            }
            if ((b & 0xF8) == 0xF0) {
               if (src >= pEnd4) break;
               int cp = (((b & 0x07) << 18) |
                  ((src[1] & 0x3F) << 12) |
                  ((src[2] & 0x3F) << 6) |
                  (src[3] & 0x3F));
               src += 4;
               if (cp <= 0xFFFF) {
                  *(dst++) = (char)cp;
                  continue;
               }
               // Encode code point above U+FFFF as surrogate pair.
               *(dst++) = (char)(0xD7C0 + (cp >> 10));
               *(dst++) = (char)(0xDC00 + (cp & 0x3FF));
            }

            //Mark invalid and skip follower byte.
            *(dst++) = (char)0xFFFD;
            for (++src; src < pEnd; src++) if ((*src & (byte)0xc0) != (byte)0x80) break;
         }
         return (int)(dst - pDst);
      }

   }

   public class Utf82 : Utf81 {
      public override string ToString () {
         return "Utf82";
      }
      public unsafe override string GetString (byte[] bytes) {
         char[] buf = new char[bytes.Length];
         int x;
         fixed (byte* src = &bytes[0])
         fixed (char* dst = &buf[0]) {
            x = convertFromUtf8 (src, bytes.Length, dst, bytes.Length);
         }
         return new string (buf, 0, x);
      }

      public unsafe override string GetString (byte[] bytes, int index, int count) {
         char[] buf = new char[count];
         int x;
         fixed (byte* src = &bytes[index])
         fixed (char* dst = &buf[0]) {
            x = convertFromUtf8 (src, count, dst, count);
         }
         return new string (buf, 0, x);
      }

      public unsafe override int GetChars (byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
         if (charIndex != 0) throw new BMException ("CharIndex can only be 0. Got {0}", charIndex);
         fixed (byte* src = &bytes[byteIndex])
         fixed (char* dst = &chars[0]) {
            return convertFromUtf8 (src, byteCount, dst, chars.Length);
         }
      }

   }

}
