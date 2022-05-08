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
using System.Threading.Tasks;
using Bitmanager.Core;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Result to be returned when a Load() is completed
   /// </summary>
   public class Result {
      public readonly LogFile LogFile;
      public Result (LogFile logfile, DateTime started, Exception err) {
         this.LogFile = logfile;
         this.Duration = DateTime.Now - started;
         for (Exception x = err; x != null; x = x.InnerException) {
            if (x is TaskCanceledException) {
               this.Cancelled = true;
               break;
            }
         }
         if (!this.Cancelled) {
            this.Error = err;
            if (err != null) Logs.ErrorLog.Log (err);
         }
      }
      public readonly TimeSpan Duration;
      public readonly Exception Error;
      public readonly bool Cancelled;

      public void ThrowIfError () {
         if (Error != null) throw new Exception (Error.Message, Error);
      }
   }

   /// <summary>
   /// Result to be returned when a Search() is completed
   /// </summary>
   public class SearchResult : Result {
      public readonly int NumMatches;
      public readonly int NumSearchTerms;
      public SearchResult (LogFile logfile, DateTime started, Exception err, int matches, int numSearchTerms)
          : base (logfile, started, err) {
         this.NumMatches = matches;
         this.NumSearchTerms = numSearchTerms;
      }
   }

   /// <summary>
   /// Result to be returned when an Export() is completed
   /// </summary>
   public class ExportResult : Result {
      public readonly int NumExported;
      public ExportResult (LogFile logfile, DateTime started, Exception err, int exported)
          : base (logfile, started, err) {
         this.NumExported = exported;
      }
   }
}
