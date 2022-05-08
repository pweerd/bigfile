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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bitmanager.BigFile {
   [Flags]
   public enum ComparerType {
      SubString = 0,
      Regex = 1,
      CaseSensitive = 2,
   }

   /// <summary>
   /// Compares text from a line
   /// </summary>
   public abstract class LineComparer {
      protected static readonly List<Tuple<int, int>> EMPTY = new List<Tuple<int, int>> (0);

      /// <summary>
      /// Check if the line contains the argument 
      /// </summary>
      public abstract bool IsMatch (String line);

      /// <summary>
      /// Get all occurences (offset/length) of the argument within this line 
      /// </summary>
      public abstract List<Tuple<int, int>> GetMatches (String line);

      public static LineComparer Create (ComparerType type, String arg) {
         switch (type) {
            case ComparerType.SubString:
               return new SubstringComparer (StringComparison.OrdinalIgnoreCase, arg);
            case ComparerType.SubString | ComparerType.CaseSensitive:
               return new SubstringComparer (StringComparison.Ordinal, arg);
            case ComparerType.Regex:
            case ComparerType.Regex | ComparerType.CaseSensitive:
               return new RegexComparer (type, arg);
            default:
               throw new Exception ("Unexpected SearchType: " + type);
         }
      }
   }

   public class SubstringComparer : LineComparer {
      private readonly StringComparison how;
      private readonly String arg;

      public SubstringComparer (StringComparison how, String arg) {
         this.how = how;
         this.arg = arg;
      }
      public override bool IsMatch (string line) {
         return (line.IndexOf (arg, 0, how) >= 0);
      }
      public override List<Tuple<int, int>> GetMatches (String line) {
         var start = line.IndexOf (arg, 0, how);
         if (start < 0) return EMPTY;

         var ret = new List<Tuple<int, int>> ();
         int arglen = arg.Length;
         while (start >= 0) {
            ret.Add (new Tuple<int, int> (start, arglen));
            start = line.IndexOf (arg, start + arglen, how);
         }
         return ret;
      }
   }

   public class RegexComparer : LineComparer {
      private readonly Regex expr;

      public RegexComparer (ComparerType type, String arg) {
         RegexOptions opt = RegexOptions.Compiled | RegexOptions.CultureInvariant;
         if (type == ComparerType.Regex) opt |= RegexOptions.IgnoreCase;
         this.expr = new Regex (arg, opt);
      }
      public override bool IsMatch (string line) {
         return expr.IsMatch (line);
      }
      public override List<Tuple<int, int>> GetMatches (String line) {
         var matches = expr.Matches (line);
         if (matches.Count == 0) return EMPTY;

         var ret = new List<Tuple<int, int>> (matches.Count);
         foreach (Match m in matches) {
            ret.Add (new Tuple<int, int> (m.Index, m.Length));
         }
         return ret;
      }
   }
}
