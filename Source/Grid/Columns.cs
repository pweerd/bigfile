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

namespace Bitmanager.Grid {
   public struct HorizontalPadding {
      public int Left;
      public int Right;
      public HorizontalPadding (int left, int right) {
         Left = left;
         Right = right;
      }
   }

   public class Column {
      public int Width;
      public HorizontalAlignment Alignment;
      public Color? BackColor;
      public Color? ForeColor;
      public FontStyle FontStyle;
      public Font Font;
      public HorizontalPadding Padding;
      public static readonly HorizontalPadding DefPadding = new HorizontalPadding(5, 5);

      public Column () {
         Alignment = HorizontalAlignment.Left;
         FontStyle = FontStyle.Regular;
         Padding = DefPadding;
      }

      public Column (int width, HorizontalAlignment alignment = HorizontalAlignment.Left) {
         Width = width;
         Alignment = alignment;
         FontStyle = FontStyle.Regular;
         Padding = DefPadding;
      }

      internal Column (InternalColumn c) {
         Width = c.Width;
         Alignment = c.Alignment;
         BackColor = c.BackColor;
         ForeColor = c.ForeColor;
         FontStyle = FontStyle.Regular;
         Font = c.Font;
         Padding = c.Padding;
      }
   }

   /// <summary>
   /// List of columns, public, to be changed by the client
   /// </summary>
   public class UpdateableColumns : List<Column>, IDisposable {
      private readonly RawGrid parent;
      private bool disposed;

      internal UpdateableColumns (RawGrid parent, List<InternalColumn> _columns) {
         this.parent = parent;
         foreach (var ci in _columns) Add (new Column (ci));
      }

      public void Dispose () {
         if (disposed) throw new Exception ("Already disposed!");
         parent.CreateInternalColumns (this);
         disposed = true;
      }
   }

   /// <summary>
   /// Internal column definition. Not modifiable
   /// </summary>
   internal class InternalColumn {
      public readonly RawGrid Parent;
      public readonly int Width;
      public readonly int GlobalOffset;

      public readonly Font Font;
      public readonly FontStyle FontStyle;
      public readonly Color? ForeColor;
      public readonly Color? BackColor;
      public readonly HorizontalAlignment Alignment;
      public HorizontalPadding Padding;

      public int OuterWidth => Width + Padding.Left + Padding.Right;
      public int GlobalOffsetPlusWidth => GlobalOffset + OuterWidth;

      public Color EffectiveBackColor => BackColor ?? Parent.BackColor;

      public Color EffectiveForeColor => ForeColor ?? Parent.ForeColor;
      public Font EffectiveFont => Font ?? Parent.Font;


      public InternalColumn (RawGrid parent, Column c, int offset) {
         Parent = parent;
         Width = c.Width;
         GlobalOffset = offset;
         Font = c.Font;
         FontStyle = c.FontStyle;
         ForeColor = c.ForeColor;
         BackColor = c.BackColor;
         Alignment = c.Alignment;
         Padding = c.Padding;
      }
      public InternalColumn (InternalColumn c, int width, int offset) {
         Parent = c.Parent;
         Width = width;
         GlobalOffset = offset;
         Font = c.Font;
         FontStyle = c.FontStyle;
         ForeColor = c.ForeColor;
         BackColor = c.BackColor;
         Alignment = c.Alignment;
         Padding = c.Padding;
      }

      internal static void SetColumnWidth (List<InternalColumn> columns, int col, int width) {
         if (col >=0 && col < columns.Count) {
            var c = columns[col];
            var offset = c.GlobalOffset;
            InternalColumn prev;
            columns[col] = prev = new InternalColumn (c, width, offset); ;
            offset += width; 

            for (int i = col+1; i< columns.Count; i++) {
               c = columns[i];
               columns[i] = prev = new InternalColumn (c, c.Width, prev.GlobalOffsetPlusWidth);
            }
         }
      }
   }

}
