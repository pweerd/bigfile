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

using BrightIdeasSoftware;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bitmanager.BigFile
{
   /// <summary>
   /// Virtual datasource for the list view
   /// </summary>
   public class VirtualDataSource : IVirtualListDataSource
   {
      private readonly VirtualObjectListView parent;
      private List<int> filter;
      private int count;
      public List<int> Filter { get { return filter; } }
      public int Count { get { return count; } }

      public VirtualDataSource(VirtualObjectListView parent)
      {
         this.parent = parent;
      }

      public void SetContent(List<int> filtered)
      {
         this.filter = filtered;
         this.count = filtered.Count;
         parent.UpdateVirtualListSize();
         parent.ClearCachedInfo();
      }
      public void SetContent(int count)
      {
         this.count = count;
         this.filter = null;
         parent.UpdateVirtualListSize();
         parent.ClearCachedInfo();
      }

      public void Clear()
      {
         filter = null;
         count = 0;
         parent.UpdateVirtualListSize();
         parent.ClearCachedInfo();
      }

      public void AddObjects(ICollection modelObjects)
      {
         throw new NotImplementedException();
      }

      public object GetNthObject(int n)
      {
         if (filter != null) n = filter[n];
         return n;
      }

      public int GetObjectCount()
      {
         return count;
      }

      public int GetObjectIndex(object model)
      {
         return model == null ? -1 : (int)model;
      }

      public void InsertObjects(int index, ICollection modelObjects)
      {
         throw new NotImplementedException();
      }

      public void PrepareCache(int first, int last)
      {
      }

      public void RemoveObjects(ICollection modelObjects)
      {
         throw new NotImplementedException();
      }

      public int SearchText(string value, int first, int last, OLVColumn column)
      {
         return -1;
      }

      public void SetObjects(IEnumerable c)
      {
         //For an empty collection, we know what to do. Other wise not impl.
         if (c == null || !c.GetEnumerator().MoveNext()) return;
         throw new NotImplementedException();
      }

      public void Sort(OLVColumn column, SortOrder order)
      {
         throw new NotImplementedException();
      }

      public void UpdateObject(int index, object modelObject)
      {
         throw new NotImplementedException();
      }
   }
}
