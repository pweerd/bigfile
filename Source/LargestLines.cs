using Bitmanager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile {
   public class LargestLines : FixedPriorityQueue<PartialLineStats> {
      public LargestLines () : base (32, comparer) {
         base.Add (PartialLineStats.ZERO);
      }

      class Comparer : IComparer<PartialLineStats> {
         public int Compare (PartialLineStats x, PartialLineStats y) {
            return x.Length - y.Length;
         }
      }
      static readonly Comparer comparer = new Comparer ();
   }

   /// <summary>
   /// Holds the length for a specific partial line.
   /// To be used to collect the biggest lines in the collection
   /// </summary>
   public class PartialLineStats {
      public static readonly PartialLineStats ZERO = new PartialLineStats (-1, 0);
      public readonly int Index;
      public readonly int Length;

      public PartialLineStats (int idx, int len) {
         Index = idx;
         Length = len;
      }

      internal static int Cmp (PartialLineStats x, PartialLineStats y) {
         return x.Length - y.Length;
      }

      public override string ToString () {
         return Invariant.Format ("PartialLineStats ({0} at {1})", Length, Index);
      }
   }

}
