using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile {
   public class TooltipTimes {
      public readonly int Delay;
      public readonly int Duration;
      public TooltipTimes (int delay, int duration) {
         Delay = delay;
         Duration = duration;
      }
   }
}
