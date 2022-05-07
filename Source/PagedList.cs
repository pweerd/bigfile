using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile
{
   public class PagedList<T> : IList<T>
   {
      private int blocksize;
      private readonly List<T[]> list;
      private int count;
      public PagedList(int blocksize = 1024 * 1024)
      {
         this.blocksize = blocksize;
         list = new List<T[]>();
      }
      public PagedList(PagedList<T> other)
      {
         blocksize = other.blocksize;
         count = other.count;
         list = new List<T[]>();

         //Copy the pages by ref except the last one, since that one could be changed in the future
         //This assumes that we will not change elements in the middle. We just append...
         if (count > 0)
         {
            int N = other.list.Count - 1;
            for (int i = 0; i < N; i++)
            {
               list.Add(other.list[i]);
            }
            T[] last = new T[blocksize];
            Array.Copy(other.list[N], 0, last, 0, blocksize);
            list.Add(last);
         }
      }

      public virtual int Count => count;
      public virtual bool IsReadOnly => false;
      protected virtual IEnumerator<T> GetEnumerator()
      {
         for (int i = 0; i < count; i++)
            yield return this[i];
      }
      IEnumerator<T> IEnumerable<T>.GetEnumerator()
      {
         return GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      public virtual T this[int index]
      {
         get
         {
            int pageIx = index / blocksize;
            int offset = index % blocksize;
            return list[pageIx] [offset];
         }
         set
         {
            int pageIx = index / blocksize;
            int offset = index % blocksize;
            list[pageIx][offset] = value;
         }
      }

      public void Add(T item)
      {
         int pageIx = count / blocksize;
         int offset = count % blocksize;
         ++count;
         while (list.Count <= pageIx)
         {
            list.Add(new T[blocksize]);
         }
         list[pageIx][offset] = item;
      }

      public void Clear()
      {
         count = 0;
         list.Clear();
      }

      public bool Contains(T item)
      {
         throw new NotImplementedException();
      }

      public void CopyTo(T[] array, int arrayIndex)
      {
         throw new NotImplementedException();
      }

      public int IndexOf(T item)
      {
         throw new NotImplementedException();
      }

      public void Insert(int index, T item)
      {
         throw new NotImplementedException();
      }

      public bool Remove(T item)
      {
         throw new NotImplementedException();
      }

      public void RemoveAt(int index)
      {
         throw new NotImplementedException();
      }

   }
}
