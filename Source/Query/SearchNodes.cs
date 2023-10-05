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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile.Query
{
    public class SearchNodes: IEnumerable<SearchNode>
   {
      const int MAX_NODES = LineFlags.NUM_MASKS;
      private readonly List<SearchNode> nodes;
      private readonly bool[] usedBits;
      private int numCreated;

      public SearchNodes()
      {
         nodes = new List<SearchNode>();
         usedBits = new bool[MAX_NODES];
      }

      public int Count {  get { return nodes.Count;  } }

      public void Clear()
      {
         nodes.Clear();
         for (int i = 0; i < usedBits.Length; i++) usedBits[i] = false;
      }


      private ComparerType normalizeField(ref String field)
      {
         switch (field)
         {
            case null:
            case "":
               field = null;
               return ComparerType.SubString;
            case "c":
            case "cs":
               field = "cs";
               return ComparerType.SubString | ComparerType.CaseSensitive;
            case "regex":
            case "r":
               field = "r";
               return ComparerType.Regex;
            case "rc":
            case "rcs":
            case "cr":
            case "csr":
               field = "rcs";
               return ComparerType.Regex | ComparerType.CaseSensitive;
            default: throw new BMException("Unexpected type [{0}].", field);
         }
      }

      SearchNode createSearchNode(String fld, String value)
      {
         SearchNode ret;
         numCreated++;

         var type = normalizeField(ref fld);

         int idx;
         for (idx=0; idx<nodes.Count; idx++)
         {
            ret = nodes[idx];
            if (!ret.IsSame(type, value)) continue;

            nodes.RemoveAt(idx);
            nodes.Insert(0, ret);
            return ret;
         }

         if (nodes.Count >= MAX_NODES)
         {
            idx = nodes.Count - 1;
            this.usedBits[nodes[idx].BitIndex] = false;
            nodes.RemoveAt(idx);
         }

         int bit;
         for (bit = 0; bit < usedBits.Length && usedBits[bit]; bit++) { }

         usedBits[bit] = true;


         ret = new SearchNode(fld, value, bit, type);
         nodes.Insert(0, ret);
         return ret;
      }

      public ParserNode<SearchContext> Parse (String expr)
      {
         numCreated = 0;
         var parsed = new Parser<SearchContext>(new Lexer(expr), createSearchNode).Parse();
         if (numCreated > MAX_NODES) throw new BMException("Query cannot contain more than {0} clauses.", MAX_NODES);
         return parsed;
      }

      public IEnumerator<SearchNode> GetEnumerator()
      {
         return nodes.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return nodes.GetEnumerator();
      }
   }
}
