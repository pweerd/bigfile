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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Holds char information for a fixed font
   /// Used to determin the #chars for a number of pixels, or the #pixels for a number of chars
   /// </summary>
   public class FixedFontMeasures {
      private readonly double perChar;
      public FixedFontMeasures (Font f) {
         var sb = new StringBuilder ();
         for (int j = 0; j < 26; j++)
            sb.Append ((char)'a' + j).Append ((char)'A' + j);
         for (int j = 0; j < 10; j++)
            sb.Append ((char)'0' + j);
         sb.Append (" .,'");
         String x = sb.ToString ();
         perChar = TextRenderer.MeasureText (x, f).Width / x.Length;
      }

      public int GetTextPixels (String x, int add = 0) {
         return GetTextPixels (x == null ? 0 : x.Length, add);
      }

      public int GetTextPixels (int strlen, int add = 0) {
         const int MAX = 0x7FFF;  //short
         double ret = strlen * perChar + add; // * 1.01

         return ret > MAX ? MAX : (int)ret;
      }

      public int GetTextPixels (String x, String y, int add = 0) {
         return Math.Max (GetTextPixels (x, add), GetTextPixels (y, add));
      }

      public int GetTextLengthForPixels (int pixels) {
         if (pixels > 0xFFFF) pixels = 0xFFFF;
         else if (pixels < 0) return 0;
         return (int)(pixels / perChar);
      }
   }
}
