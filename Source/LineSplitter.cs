using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile {
   public class LineSplitter {
      protected const int MAX_BACK = 32;
      protected readonly Action<long> addPartialLine;
      protected readonly Action<long> addLine;
      protected long prevPosition;
      protected readonly int maxPartialLength;

      public LineSplitter (int maxPartialLength, Action<long> addPartialLine, Action<long> addLine) {
         this.maxPartialLength = maxPartialLength;
         this.addPartialLine = addPartialLine;
         this.addLine = addLine;
      }

      protected virtual int nextEOL (byte[] buf, int count, int start) {
         int i;
         for (i = 0; i < count; i++)
            if (buf[i] == 10) return i;
         return -1;
      }
      public virtual long SplitBuffer (long position, byte[] buf, int count) {
         long lineOffsetCorrection = position - prevPosition;
         int i = 0;
         while (true) {
            i = nextEOL (buf, count, i);

            if (i < 0) //No EOL found
            {
               addPartialLines (position, buf, count);
               break;
            }

            //We have an EOL. Check if we need to split partials
            if (i + lineOffsetCorrection > maxPartialLength)
               addPartialLines (position, buf, i);
            i++;
            addLine (prevPosition = position + i);
         }
         return position + count;
      }

      protected virtual void addPartialLines (long position, byte[] buf, int end) {
         while (true) {
            int j = (int)(prevPosition + maxPartialLength - position - 1); //offset in buffer
            if (j >= end) break;
            int jEnd = j - MAX_BACK;
            if (jEnd < 0) jEnd = 0;

            int lastDot = -1;
            int lastComma = -1;
            int lastGT = j;
            for (; j >= jEnd; j--) {
               switch (buf[j]) {
                  case (byte)' ': goto ADD_PARTIAL;
                  case (byte)'.': lastDot = j; continue;
                  case (byte)',': lastComma = j; continue;
                  case (byte)'>': lastGT = j; continue;
               }
            }

            if (lastDot >= 0)
               j = lastDot;
            else if (lastComma >= 0)
               j = lastComma;
            else
               j = lastGT;

            ADD_PARTIAL:
            addPartialLine (prevPosition = position + j + 1);
         }
      }

   }
}
