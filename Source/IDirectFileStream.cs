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

using Bitmanager.Core;
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



   /// <summary>
   /// Support IDirectStream for a FileStream.
   /// This is done by giving each instance its own FileStream
   /// (Needed for multithreaded support)
   /// </summary>
   public class DirectFileStreamWrapper : IDirectStream
   {
      public readonly String FileName;
      public readonly FileStream BaseStream;
      public readonly int BufferSize;

      public DirectFileStreamWrapper(String fileName, FileStream wrapped)
      {
         FileName = fileName;
         BaseStream = wrapped;
         BufferSize = 128 * 64;
      }
      public DirectFileStreamWrapper(String fileName, int bufsize=0)
      {
         FileName = fileName;
         BufferSize = bufsize > 0 ? bufsize : 128 * 64;
         BaseStream = createNewFileStream();
      }

      private FileStream createNewFileStream()
      {
         return new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, BufferSize);
      }
      public DirectFileStreamWrapper(DirectFileStreamWrapper other)
      {
         FileName = other.FileName;
         BufferSize = other.BufferSize;
         BaseStream = createNewFileStream();
      }

      public virtual void PrepareForNewInstance()
      {
      }

      public virtual IDirectStream NewInstanceForThread()
      {
         return new DirectFileStreamWrapper(this);
      }

      public virtual void CloseInstance()
      {
         BaseStream.Close();
      }

      public virtual int ReadByte(long offset)
      {
            BaseStream.Position = offset;
            return BaseStream.ReadByte();
      }

      public virtual int Read(long offset, byte[] buffer, int bufOffset, int count)
      {
         BaseStream.Position = offset;
         return BaseStream.Read(buffer, bufOffset, count);
      }

      public virtual void Close()
      {
         //Logs.ErrorLog.Log("Closed at: {0}", Environment.StackTrace);
         BaseStream.Close();
      }
   }

}
