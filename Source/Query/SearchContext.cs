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

using Bitmanager.Core;
using Bitmanager.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile.Query
{
   public class SearchContext
   {
      static Logger logger = Globals.MainLogger.Clone("bits");
      public readonly List<SearchNode> ToCompute;
      public readonly List<ParserValueNode<SearchContext>> LeafNodes;
      public readonly ParserNode<SearchContext> Query;
      public readonly bool NeedLine;
      public readonly long BitsToBeCleared;
      public readonly long NotBitMask;

      public String Line;
      public long OffsetAndFlags;
      public int Index;

      public SearchContext(ParserNode<SearchContext> query)
      {
         Query = query;
         ToCompute = new List<SearchNode>();
         NeedLine = false;
         BitsToBeCleared = LineFlags.ALL_EVALUATED | LineFlags.MATCHED;
         LeafNodes = query.CollectValueNodes();
         for (int i = 0; i < LeafNodes.Count; i++)
         {
            var n = (SearchNode)LeafNodes[i];
            if (n.IsComputed) continue;
            NeedLine = true;
            ToCompute.Add(n);
            BitsToBeCleared |= n.BitMask;
         }
         NotBitMask = ~BitsToBeCleared;
      }

      public SearchContext(SearchContext other)
      {
         Query = other.Query;
         LeafNodes = other.LeafNodes;
         ToCompute = other.ToCompute;
         NeedLine = other.NeedLine;
         BitsToBeCleared = other.BitsToBeCleared;
         NotBitMask = other.NotBitMask;
      }

      public SearchContext NewInstanceForThread()
      {
         return new SearchContext(this);
      }


      public void SetLine (long offset, String line)
      {
         Line = line;
         OffsetAndFlags = offset & NotBitMask;
      }



      public void MarkComputed()
      {
         for (int i = 0; i < LeafNodes.Count; i++)
         {
            ((SearchNode)LeafNodes[i]).IsComputed = true;
         }

      }

   }
}
