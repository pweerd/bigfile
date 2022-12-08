using Bitmanager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile {
   public class LargestLines : FixedPriorityQueue<PartialLineLength> {
      public LargestLines () : base (32, comparer) {
         base.Add (PartialLineLength.ZERO);
      }

      class Comparer : IComparer<PartialLineLength> {
         public int Compare (PartialLineLength x, PartialLineLength y) {
            return x.Length - y.Length;
         }
      }
      static readonly Comparer comparer = new Comparer ();
   }

   /// <summary>
   /// Holds the length for a specific partial line.
   /// To be used to collect the biggest lines in the collection
   /// </summary>
   public class PartialLineLength {
      public static readonly PartialLineLength ZERO = new PartialLineLength (-1, 0);
      public readonly int Index;
      public readonly int Length;

      public PartialLineLength (int idx, int len) {
         Index = idx;
         Length = len;
      }

      internal static int Cmp (PartialLineLength x, PartialLineLength y) {
         return x.Length - y.Length;
      }
   }

}
