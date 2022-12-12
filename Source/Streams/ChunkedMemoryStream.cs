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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Bitmanager.Core;

namespace Bitmanager.IO {

   /// <summary>
   /// Creates a stream whose backing store is memory.
   /// Instead of using 1 contigious buffer, we use a list of buffers 
   /// </summary>
   public class ChunkedMemoryStream : Stream, IDirectStream {
      private List<byte[]> _buffers;
      private long _position;
      private long _length;
      private readonly long _chunckSize;
      private bool _isOpen;

      /// <summary>Initializes a new instance of the <see cref="T:System.IO.MemoryStream" /> class with an expandable capacity initialized to zero.</summary>
      public ChunkedMemoryStream ()
        : this (64 * 1024) {
      }

      /// <summary>Initializes a new instance of the <see cref="T:System.IO.MemoryStream" /> class with an expandable capacity initialized as specified.</summary>
      /// <param name="capacity">The initial size of the internal array in bytes. </param>
      /// <exception cref="T:System.ArgumentOutOfRangeException">
      /// <paramref name="capacity" /> is negative. </exception>
      public ChunkedMemoryStream (int chunckSize) {
         if (chunckSize < 4096)
            throw new ArgumentOutOfRangeException (String.Format ("Invalid chunckSize [{0}]. Should be >= 4096.", chunckSize));
         this._buffers = new List<byte[]> ();
         this._chunckSize = chunckSize;
         this._isOpen = true;
      }
      public ChunkedMemoryStream (ChunkedMemoryStream other) {
         this._chunckSize = other._chunckSize;
         this._isOpen = other._isOpen;
         this._buffers = new List<byte[]> (other._buffers);
         this._length = other._length;
      }

      public virtual void PrepareForNewInstance () {
      }

      public virtual IDirectStream NewInstanceForThread () {
         return new ChunkedMemoryStream (this);
      }

      public virtual void CloseInstance () {
         _buffers.Clear ();
         _length = 0;
         _position = 0;
      }

      /// <summary>
      /// Create a compressing chuncked memory stream
      /// </summary>
      public CompressedChunkedMemoryStream CreateCompressedChunkedMemoryStream (Logger logger = null) {
         return new CompressedChunkedMemoryStream ((int)_chunckSize, _buffers, _position, _length, logger);
      }

      public override bool CanRead { get { return this._isOpen; } }
      public override bool CanSeek { get { return this._isOpen; } }
      public override bool CanWrite { get { return this._isOpen; } }

      /// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.MemoryStream" /> class and optionally releases the managed resources.</summary>
      /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
      protected override void Dispose (bool disposing) {
         try {
            if (!disposing)
               return;
            this._isOpen = false;
         } finally {
            base.Dispose (disposing);
         }
      }

      private bool EnsureCapacity (long value) {
         if (value < 0) value = 0;

         int dirs = (int)((value + _chunckSize - 1) / _chunckSize);
         if (dirs <= _buffers.Count) return false;


         for (int i = _buffers.Count; i < dirs; i++)
            _buffers.Add (new byte[_chunckSize]);
         return true;
      }

      public override void Flush () { }


      /// <summary>Gets or sets the number of bytes allocated for this stream.</summary>
      /// <returns>The length of the usable portion of the buffer for the stream.</returns>
      public virtual long Capacity {
         get {
            return _buffers.Count * (long)_chunckSize;
         }
         set {
            EnsureCapacity (value);
         }
      }

      public override long Length { get { return this._length; } }

      public override long Position {
         get {
            return this._position;
         }
         set {
            this._position = value < 0 ? _length + value : value;
         }
      }

      public override int Read ([In, Out] byte[] buffer, int offset, int count) {
         int len = Read (_position, buffer, offset, count);
         _position += len;
         return len;
      }

      public virtual int Read (long position, byte[] buffer, int offset, int count) {
         long longCount = this._length - position;
         int totalBytesToProcess = longCount > count ? count : (int)longCount;

         if (totalBytesToProcess <= 0)
            return 0;

         int chunckIx = (int)(position / _chunckSize);
         int chunckOffset = (int)(position % _chunckSize);
         int todo = totalBytesToProcess;
         while (true) {
            int bytesToProcess = (int)(_chunckSize - chunckOffset);
            if (bytesToProcess > todo) bytesToProcess = todo;

            byte[] chunck = _buffers[chunckIx];
            if (bytesToProcess <= 8) {
               int num = bytesToProcess;
               while (--num >= 0)
                  buffer[offset + num] = chunck[chunckOffset + num];
            } else
               Buffer.BlockCopy (chunck, chunckOffset, buffer, offset, bytesToProcess);

            todo -= bytesToProcess;
            if (todo <= 0) break;
            offset += bytesToProcess;
            ++chunckIx;
            chunckOffset = 0;
         }
         return totalBytesToProcess;
      }



      /// <summary>Reads a byte from the current stream.</summary>
      /// <returns>The byte cast to a <see cref="T:System.Int32" />, or -1 if the end of the stream has been reached.</returns>
      /// <exception cref="T:System.ObjectDisposedException">The current stream instance is closed. </exception>
      /// <filterpriority>2</filterpriority>
      public override int ReadByte () {
         long longCount = this._length - this._position;
         if (longCount <= 0) return -1;

         int chunckIx = (int)(_position / _chunckSize);
         int chunckOffset = (int)(_position % _chunckSize);
         ++_position;
         return _buffers[chunckIx][chunckOffset];
      }

      public virtual int ReadByte (long position) {
         long longCount = this._length - position;
         if (longCount <= 0) return -1;

         int chunckIx = (int)(position / _chunckSize);
         int chunckOffset = (int)(position % _chunckSize);
         return _buffers[chunckIx][chunckOffset];
      }



      /// <summary>Sets the position within the current stream to the specified value.</summary>
      /// <returns>The new position within the stream, calculated by combining the initial reference point and the offset.</returns>
      public override long Seek (long offset, SeekOrigin loc) {
         long newPos;
         switch (loc) {
            case SeekOrigin.Begin:
               newPos = offset;
               break;
            case SeekOrigin.Current:
               newPos = this._position + offset;
               break;
            case SeekOrigin.End:
               newPos = this._length + offset;
               break;
            default:
               throw new ArgumentException ("InvalidSeekOrigin: " + loc);
         }
         return _position = newPos < 0 ? 0 : newPos;
      }

      /// <summary>Sets the length of the current stream to the specified value.</summary>
      /// <param name="value">The value at which to set the length. </param>
      public override void SetLength (long value) {
         if (value < 0) value = 0;
         long cap = _buffers.Count * _chunckSize;
         if (value > cap) {
            EnsureCapacity (value);
            return;
         }
         if (value == cap)
            return;

         int neededChuncks = (int)((value + this._chunckSize - 1) / _chunckSize);
         for (int i = neededChuncks; i < _buffers.Count; i++)
            _buffers[i] = null;
         _buffers.RemoveRange (neededChuncks, _buffers.Count - neededChuncks);

         _length = value;
         if (_position > value) _position = value;
      }

      /// <summary>Writes the stream contents to a byte array, regardless of the <see cref="P:System.IO.MemoryStream.Position" /> property.</summary>
      /// <returns>A new byte array.</returns>
      public virtual byte[] ToArray () {
         byte[] ret = new byte[_length];
         int fullChuncks = (int)(_length / _chunckSize);
         int rest = (int)(_length % _chunckSize);
         int i = 0;
         for (; i < fullChuncks; i++)
            Array.Copy (_buffers[i], 0, ret, i * _chunckSize, _chunckSize);
         if (rest > 0)
            Array.Copy (_buffers[i], 0, ret, i * _chunckSize, rest);
         return ret;
      }

      /// <summary>Writes a block of bytes to the current stream using data read from a buffer.</summary>
      /// <param name="buffer">The buffer to write data from. </param>
      /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
      /// <param name="count">The maximum number of bytes to write. </param>
      public override void Write (byte[] buffer, int offset, int count) {
         if (count <= 0) return;
         EnsureCapacity (_position + count);

         int chunckIx = (int)(_position / _chunckSize);
         int chunckOffset = (int)(_position % _chunckSize);
         int todo = count;
         while (true) {
            int bytesToProcess = (int)(_chunckSize - chunckOffset);
            if (bytesToProcess > todo) bytesToProcess = todo;

            byte[] chunck = _buffers[chunckIx];
            if (bytesToProcess <= 8) {
               int num = bytesToProcess;
               while (--num >= 0)
                  chunck[chunckOffset + num] = buffer[offset + num];
            } else
               Buffer.BlockCopy (buffer, offset, chunck, chunckOffset, bytesToProcess);

            todo -= bytesToProcess;
            if (todo <= 0) break;
            offset += bytesToProcess;
            chunckIx++;
            chunckOffset = 0;
         }
         this._position += count;
         if (_position > _length) _length = _position;
      }


      /// <summary>Writes a byte to the current stream at the current position.</summary>
      /// <param name="value">The byte to write. </param>
      /// <exception cref="T:System.NotSupportedException">The stream does not support writing. For additional information see <see cref="P:System.IO.Stream.CanWrite" />.-or- The current position is at the end of the stream, and the capacity cannot be modified. </exception>
      /// <exception cref="T:System.ObjectDisposedException">The current stream is closed. </exception>
      /// <filterpriority>2</filterpriority>
      public override void WriteByte (byte value) {
         EnsureCapacity (_position + 1);

         int chunckIx = (int)(_position / _chunckSize);
         int chunckOffset = (int)(_position % _chunckSize);
         _buffers[chunckIx][chunckOffset] = value;
         ++_position;
         if (_position > _length) _length = _position;
      }

      /// <summary>Writes the entire contents of this memory stream to another stream.</summary>
      /// <param name="stream">The stream to write this memory stream to. </param>
      public virtual void WriteTo (Stream stream) {
         int fullChuncks = (int)(_length / _chunckSize);
         int rest = (int)(_length % _chunckSize);
         int i = 0;
         for (; i < fullChuncks; i++)
            stream.Write (_buffers[i], 0, (int)_chunckSize);
         if (rest > 0)
            stream.Write (_buffers[i], 0, rest);
      }


   }
}
