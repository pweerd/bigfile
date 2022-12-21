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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Callback to post asynchronous request ready from the LogFile
   /// To be implemented by the caller
   /// </summary>
   public interface ILogFileCallback {
      void OnSearchComplete (SearchResult result);
      void OnSearchPartial (LogFile lf, int firstMatch);
      void OnLoadComplete (Result result);
      void OnLoadCompletePartial (LogFile cloned);
      void OnExportComplete (ExportResult result);
      void OnProgress (LogFile lf, int percent);
   }
}
