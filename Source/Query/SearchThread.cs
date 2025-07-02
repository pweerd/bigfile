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

namespace Bitmanager.BigFile {

public partial class LogFile {
      class SearchThread {
         public Exception Error { get; private set; }
         private readonly Func<SearchContext, int, int, int> dlg;
         private readonly SearchContext ctx;
         private readonly int start, end;
         public int Result { get; private set; }
         public readonly Thread Thread;

         public SearchThread (SearchContext ctx, int start, int end, Func<SearchContext, int, int, int> dlg) {
            this.dlg = dlg;
            this.ctx = ctx;
            this.start = start;
            this.end = end;
            Thread = new Thread (run);
            Thread.IsBackground = true;
            //Thread.Priority = ThreadPriority.BelowNormal;
            Thread.Start ();
         }

         private void run (object obj) {
            try {
               Result = dlg (ctx, start, end);
            } catch (Exception e) {
               Error = e;
            }
         }
      }
   }

}