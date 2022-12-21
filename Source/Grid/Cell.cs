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
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Forms;

namespace Bitmanager.Grid {
   /// <summary>
   /// Defines the content and the layout of a grid cell.
   /// </summary>
   public class Cell {
      public static readonly Cell Empty = new Cell ();
      public string Text;
      public HorizontalAlignment Alignment;
      public FontStyle FontStyle;
      public Color BackColor;
      public Color ForeColor;
      public HorizontalPadding HorizontalPadding;

      private Cell () {
         BackColor = Color.Yellow;
         ForeColor = Color.Black;
         FontStyle = FontStyle.Regular;
         Alignment = HorizontalAlignment.Left;
         HorizontalPadding = new HorizontalPadding ();
      }
      internal Cell (InternalColumn c) {
         BackColor = c.EffectiveBackColor;
         ForeColor = c.EffectiveForeColor;
         FontStyle = c.FontStyle;
         Alignment = c.Alignment;
         HorizontalPadding = c.Padding;
      }
      internal Cell (RawGrid grid) {
         BackColor = grid.BackColor;
         ForeColor = grid.ForeColor;
         FontStyle = FontStyle.Regular;
         Alignment = HorizontalAlignment.Left;
         HorizontalPadding = new HorizontalPadding ();
      }


      ///// <summary>
      ///// Alters the background color of a cell with a specified accent color.
      ///// </summary>
      ///// <param name="color">Accent color.</param>
      ///// <param name="ratio">The ratio in which the accent color should be used.</param>
      ///// <returns></returns>
      //public Cell Highlight(Color color, double ratio = 0.5) => new(Text, ColorUtils.Mix(BackgroundColor ?? Color.White, color, ratio), ForegroundColor, TextAlignment, FontStyle);

      public static bool operator != (in Cell lhs, in Cell rhs) => !(lhs == rhs);
      public static bool operator == (in Cell lhs, in Cell rhs) =>
         lhs.BackColor == rhs.BackColor &&
         lhs.ForeColor == rhs.ForeColor &&
         lhs.Alignment == rhs.Alignment &&
         lhs.FontStyle == rhs.FontStyle &&
         string.CompareOrdinal (lhs.Text, rhs.Text) == 0;

      public override bool Equals (object obj) => obj is Cell cell && cell == this;
      public override int GetHashCode () => throw new NotImplementedException ();
   }
}
