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

using Bitmanager.BigFile.Query;
using Bitmanager.Core;
using Bitmanager.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile.Query
{
   public class SearchNode : ParserValueNode<SearchContext>
   {
      private static readonly Logger logger = Globals.MainLogger.Clone("search");
      private readonly LineComparer cmp;
      public readonly long BitMask;
      public readonly int BitIndex;
      private readonly ComparerType type;
      public bool IsComputed;

      public LineComparer Comparer { get { return cmp; } }

      public SearchNode(String field, String value, int bitIndex, ComparerType type)
          : base(field, value)
      {
         this.BitIndex = bitIndex;
         this.BitMask = LineFlags.MASK0 << bitIndex;
         this.type = type;
         logger.Log("Create SearchNode [{0}] mask={1:X} bit={2}", base.fieldPlusValue, BitMask, BitIndex);
         this.cmp = LineComparer.Create (type, value);
      }

      public bool IsSame (ComparerType type, String value)
      {
         if (this.type != type) return false;
         StringComparison c = (type & ComparerType.CaseSensitive) == 0
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
         return String.Equals(this.Value, value, c);
      }

      public override bool Evaluate(SearchContext ctx)
      {
         if (IsComputed) return (ctx.OffsetAndFlags & BitMask) != 0;

         //--ctx.NumToEvaluate;
         bool ret = cmp.IsMatch(ctx.Line);
         if (ret) ctx.OffsetAndFlags |= BitMask;
         return ret;
      }
   }
}
