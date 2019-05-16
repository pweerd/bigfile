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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.IO
{
   public interface IDirectStream
   {
      int ReadByte(long offset);
      int Read(long offset, byte[] buffer, int bufOffset, int count);
      void Close();
      void PrepareForNewInstance();
      IDirectStream NewInstanceForThread();
      void CloseInstance();
   }

   public class DirectStreamWrapper: IDirectStream
   {
      private readonly Object _lock;
      public readonly Stream BaseStream;

      public DirectStreamWrapper(Stream wrapped)
      {
         _lock = new object();
         BaseStream = wrapped;
      }
      public DirectStreamWrapper(DirectStreamWrapper other)
      {
         _lock = other._lock;
         BaseStream = other.BaseStream;
      }

      public virtual void PrepareForNewInstance()
      {
      }

      public virtual IDirectStream NewInstanceForThread()
      {
         return this;
      }

      public virtual void CloseInstance()
      {
      }

      public int ReadByte(long offset)
      {
         lock(_lock)
         {
            BaseStream.Position = offset;
            return BaseStream.ReadByte();
         }
      }

      public int Read(long offset, byte[] buffer, int bufOffset, int count)
      {
         lock (_lock)
         {
            BaseStream.Position = offset;
            return BaseStream.Read (buffer,bufOffset, count);
         }
      }

      public virtual void Close()
      {
         BaseStream.Close();
      }
   }
}
