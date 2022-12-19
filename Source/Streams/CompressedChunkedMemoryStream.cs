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
using Bitmanager.UCore;
using Bitmanager.Core;

namespace Bitmanager.IO {
   /// <summary>
   /// Compressing memory stream.
   /// Based on the ChunkedMemoryStream, that is based on .Net's memory stream.
   /// This one compresses blocks of memory on the fly.
   /// It will stop compressing after it encounters compression errors or 
   /// when compression is not effective (compsize > 60%) 
   /// </summary>
   public class CompressedChunkedMemoryStream : Stream, IDirectStream {
      private ICompressor _compressor;
      private enum Mode { _None, _Reading, _Writing, _Seeking };
      private readonly List<byte[]> _buffers;
      private readonly Stack<byte[]> _recycledBuffers;

      private long _offsetOfFirstBuffer;
      private long _position;
      private long _length;
      private readonly byte[] _decompressBuffer;
      private readonly long _chunckSize;
      private readonly Logger _logger;
      private int _decompressBufferIdx;
      private int _compressedUntil;
      private bool _isOpen;
      private Mode _mode;
      private bool _compressDisabled;

      /// <summary>Initializes a new instance of the <see cref="T:System.IO.MemoryStream" /> class with an expandable capacity initialized to zero.</summary>
      public CompressedChunkedMemoryStream (Logger logger = null)
        : this (256 * 1024, logger) {
      }


      /// <summary>Initializes a new instance of the <see cref="T:System.IO.MemoryStream" /> class with an expandable capacity initialized as specified.</summary>
      /// <param name="capacity">The initial size of the internal array in bytes. </param>
      /// <exception cref="T:System.ArgumentOutOfRangeException">
      /// <paramref name="capacity" /> is negative. </exception>
      public CompressedChunkedMemoryStream (int chunckSize, Logger logger = null) {
         if (chunckSize < 4096)
            throw new ArgumentOutOfRangeException (String.Format ("Invalid chunckSize [{0}]. Should be >= 4096.", chunckSize));
         this._buffers = new List<byte[]> (100);
         this._recycledBuffers = new Stack<byte[]> (50);
         this._chunckSize = chunckSize;
         this._isOpen = true;
         this._mode = Mode._None;
         this._decompressBuffer = new byte[chunckSize];

         this._compressor = Bitmanager.Core.CoreHelper.Instance.CreateCompressor ();
         this._decompressBufferIdx = -1;
         this._logger = logger;
      }

      /// <summary>
      ///  Constructor for constructing a CompressedChunkedMemoryStream from a ChunkedMemoryStream
      /// </summary>
      public CompressedChunkedMemoryStream (int chunckSize, List<byte[]> buffers, long position, long length, Logger logger = null) {
         if (chunckSize < 4096)
            throw new ArgumentOutOfRangeException (String.Format ("Invalid chunckSize [{0}]. Should be >= 4096.", chunckSize));
         this._buffers = new List<byte[]> (buffers);
         this._recycledBuffers = new Stack<byte[]> (50);
         this._chunckSize = chunckSize;
         this._isOpen = true;
         this._mode = Mode._Writing;
         this._decompressBuffer = new byte[chunckSize];

         this._compressor = Bitmanager.Core.CoreHelper.Instance.CreateCompressor ();
         this._decompressBufferIdx = -1;

         this._position = position;
         this._length = length;
         this._logger = logger;

         //Start compressing if possible
         int completeBuffers = (int)(position / chunckSize);
         if (completeBuffers > 0) compressUntil (completeBuffers);
      }

      /// <summary>
      ///  Constructor for creating a new instance (needed for the compression buffer) but with shared memory buffers.
      /// </summary>
      public CompressedChunkedMemoryStream (CompressedChunkedMemoryStream other) {
         this._chunckSize = other._chunckSize;
         this._isOpen = other._isOpen;
         this._buffers = new List<byte[]> (other._buffers);
         this._recycledBuffers = new Stack<byte[]> ();
         this._length = other._length;
         this._position = other._position;
         this._offsetOfFirstBuffer= other._offsetOfFirstBuffer;
         this._mode = other._mode;
         this._decompressBuffer = new byte[_chunckSize];
         this._compressor = other._compressor;
         this._decompressBufferIdx = -1;
         this._compressDisabled = other._compressDisabled;
         this._compressedUntil = other._compressedUntil;
         this._logger = other._logger;
      }



      public long GetCompressedSize () {
         long tot = 0;
         for (int i = 0; i < _buffers.Count; i++)
            tot += _buffers[i].Length;
         return tot;
      }
      private byte[] getDecompressedBuffer (int idx) {
         if (this._decompressBufferIdx == idx) return _decompressBuffer;
         if (idx >= this._compressedUntil) return _buffers[idx];

         var src = _buffers[idx];
         var rc = _compressor.DecompressLZ4 (ref src[0], ref _decompressBuffer[0], src.Length, _decompressBuffer.Length);
         if (rc != _decompressBuffer.Length)
            throw new BMException ("Unexpected uncompressed length: {0} instead of {1}. Offset=0x{2:X}.", rc, _decompressBuffer.Length, idx * _chunckSize);
         _decompressBufferIdx = idx;
         return _decompressBuffer;
      }

      private void compressUntil (int idx) {
         //logger.Log("CompressUntil({0}, old={1}, task={2}, stop={3})", idx, _compressedUntil, _compressorTask, _backgroundProcessorStop);
         if (idx > _compressUntil) _compressUntil = idx;
         if (_compressedUntil >= _compressUntil) return;
         if (_compressorTask == null) {
            _backgroundProcessorStop = false;
            _compressorTask = Task.Run ((Action)backgroundCompressor);
         }
      }

      public void FinalizeCompressor (bool all = false) {
         compressUntil (all ? _buffers.Count : -1);
         _backgroundProcessorStop = true;
         if (_compressorTask != null) {
            if (_logger != null) _logger.Log ("Waiting for compressor");
            _compressorTask.Wait ();
            _compressorTask = null;
         }
         if (_logger != null) {
            _logger.Log ("-- Compressor done. Buffers={0}, recycled={1}, allocated={2}, stacked={3}, max={4}", _buffers.Count, _numRecycled, _numAllocated, _recycledBuffers.Count, _maxRecycledSize);
         }
         if (all) {
            lock (_recycledBuffers) {
               _recycledBuffers.Clear ();
            }
         }
      }

      public bool IsCompressionEnabled { get { return !_compressDisabled; } }

      volatile bool _backgroundProcessorStop;
      volatile int _compressUntil;
      Task _compressorTask;
      private void backgroundCompressor () {
         if (_compressDisabled) return;
         int maxCompressSize = (int)(0.6 * _decompressBuffer.Length);
         while (true) {
            if (_compressedUntil >= _compressUntil) {
               if (_backgroundProcessorStop) { if (_logger != null) _logger.Log ("Compressor stopping at {0}", _compressedUntil); break; }
               Thread.Sleep (100);
               continue;
            }

            int idx = _compressedUntil;
            try {
               var curBuf = _buffers[idx];
               var rc = _compressor.CompressLZ4 (ref curBuf[0], ref _decompressBuffer[0], curBuf.Length, _decompressBuffer.Length);
               if (rc <= 0) {
                  if (_logger != null) _logger.Log (Core._LogType.ltWarning, "Compression disabled because: unexpected compress rc={0}. Offset=0x{1:X}", rc, idx * _chunckSize);
                  _compressDisabled = true;
                  break;
               }

               if (rc > maxCompressSize) {
                  if (this._logger != null) _logger.Log (Core._LogType.ltWarning, "Compression disabled because: not performing well. Length is {0} from {1}.", rc, curBuf.Length);
                  _compressDisabled = true;
                  break;
               }
               //logger.Log("Compressed {0} from {1} into {2}", idx, src.Length, rc);
               byte[] b = new byte[rc];
               Buffer.BlockCopy (_decompressBuffer, 0, b, 0, rc);
               _buffers[idx] = b;
               _compressedUntil = idx + 1;
               lock (_recycledBuffers) {
                  _recycledBuffers.Push (curBuf);
               }
            } catch (Exception e) {
               _compressDisabled = true;
               String msg = Invariant.Format ("Compression error at index {0}, offset 0x{1}: {2}", idx, idx * _chunckSize, e.Message);
               Logs.ErrorLog.Log (e, msg);
               throw new BMException (e, msg);
            }
         }

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
            this._mode = Mode._None;
         } finally {
            base.Dispose (disposing);
         }
      }

      //Gets a new buffer from the recycled pool or creates a new one.
      private byte[] createNewBuffer () {
         byte[] buf;
         lock (_recycledBuffers) {
            int N = _recycledBuffers.Count;
            if (N > _maxRecycledSize) _maxRecycledSize = N;
            buf = _recycledBuffers.Count == 0 ? null : _recycledBuffers.Pop ();
         }
         if (buf != null) ++_numRecycled; else ++_numAllocated;
         return buf == null ? new byte[_chunckSize] : buf;
      }
      int _maxRecycledSize;
      int _numRecycled;
      int _numAllocated;

      //Make sure we have enough buffers allocated to hold a size of value bytes. 
      private bool EnsureCapacity (long value) {
         if (value < 0) value = 0;

         int dirs = (int)((value + _chunckSize - 1) / _chunckSize);
         if (dirs <= _buffers.Count) return false;


         for (int i = _buffers.Count; i < dirs; i++)
            _buffers.Add (createNewBuffer ());
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
            checkWrite ();
            EnsureCapacity (value);
         }
      }

      public override long Length { get { return this._length; } }

      public override long Position {
         get {
            return this._position;
         }
         set {
            _mode = Mode._Seeking;
            this._position = value < 0 ? _length + value : value;
         }
      }

      public override int Read ([In, Out] byte[] buffer, int offset, int count) {
         int len = Read (_position, buffer, offset, count);
         _position += len;
         return len;
      }




      /// <summary>Reads a byte from the current stream.</summary>
      /// <returns>The byte cast to a <see cref="T:System.Int32" />, or -1 if the end of the stream has been reached.</returns>
      /// <exception cref="T:System.ObjectDisposedException">The current stream instance is closed. </exception>
      /// <filterpriority>2</filterpriority>
      public override int ReadByte () {
         this._mode = Mode._Reading;
         long longCount = this._length - this._position;
         if (longCount <= 0) return -1;

         int chunckIx = (int)(_position / _chunckSize);
         int chunckOffset = (int)(_position % _chunckSize);
         ++_position;
         return getDecompressedBuffer (chunckIx)[chunckOffset];
      }



      /// <summary>Sets the position within the current stream to the specified value.</summary>
      /// <returns>The new position within the stream, calculated by combining the initial reference point and the offset.</returns>
      public override long Seek (long offset, SeekOrigin loc) {
         this._mode = Mode._Seeking;
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
         checkWrite ();
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


      private void checkWrite () {
         switch (_mode) {
            case Mode._None:
               _mode = Mode._Writing;
               break;
            case Mode._Reading:
               throw new Exception ("Cannot write after reading.");
            case Mode._Seeking:
               throw new Exception ("Cannot write after re-positioning stream.");
            case Mode._Writing:
               break;
            default:
               throw new Exception ("Unsupported mode=" + _mode);
         }
      }

      /// <summary>Writes a block of bytes to the current stream using data read from a buffer.</summary>
      /// <param name="buffer">The buffer to write data from. </param>
      /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
      /// <param name="count">The maximum number of bytes to write. </param>
      public override void Write (byte[] buffer, int offset, int count) {
         checkWrite ();
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
            } else {
               //logger.Log("copy: src offset={0}, len={1}, tot={2}, cap={3}", offset, count, offset+count, buffer.Length);
               //logger.Log("copy: dst offset={0}, len={1}, tot={2}, cap={3}", chunckOffset, count, chunckOffset+count, chunck.Length);
               Buffer.BlockCopy (buffer, offset, chunck, chunckOffset, bytesToProcess);

            }

            todo -= bytesToProcess;
            if (todo <= 0) break;
            offset += bytesToProcess;
            chunckIx++;
            chunckOffset = 0;
         }
         this._position += count;
         if (_position > _length) _length = _position;
         compressUntil (chunckIx);
      }


      /// <summary>Writes a byte to the current stream at the current position.</summary>
      /// <param name="value">The byte to write. </param>
      /// <exception cref="T:System.NotSupportedException">The stream does not support writing. For additional information see <see cref="P:System.IO.Stream.CanWrite" />.-or- The current position is at the end of the stream, and the capacity cannot be modified. </exception>
      /// <exception cref="T:System.ObjectDisposedException">The current stream is closed. </exception>
      /// <filterpriority>2</filterpriority>
      public override void WriteByte (byte value) {
         checkWrite ();
         EnsureCapacity (_position + 1);

         int chunckIx = (int)(_position / _chunckSize);
         int chunckOffset = (int)(_position % _chunckSize);
         _buffers[chunckIx][chunckOffset] = value;
         ++_position;
         if (_position > _length) _length = _position;
         compressUntil (chunckIx);
      }

      /// <summary>Writes the entire contents of this memory stream to another stream.</summary>
      /// <param name="stream">The stream to write this memory stream to. </param>
      public virtual void WriteTo (Stream stream) {
         _mode = Mode._Reading;
         int fullChuncks = (int)(_length / _chunckSize);
         int rest = (int)(_length % _chunckSize);
         int i = 0;
         for (; i < fullChuncks; i++)
            stream.Write (_buffers[i], 0, (int)_chunckSize);
         if (rest > 0)
            stream.Write (_buffers[i], 0, rest);
      }

      #region IDirectStream implementation
      public void SetOffsetOfFirstBuffer (long offset) {
         BigFile.Globals.StreamLogger.Log ("{0}:SetOffsetOfFirstBuffer ({1}, 0x{1:X})", this.GetType ().Name, offset);
         if (offset < 0) throw new BMException ("Negative offset is unexpected");
         this._offsetOfFirstBuffer = offset;
      }
      public virtual int Read (long position, byte[] buffer, int offset, int count) {
         position -= _offsetOfFirstBuffer;
         BigFile.Globals.StreamLogger.Log ("{0}:Read ({3}, inp {1}, 0x{1:X}, offs {2}, 0x{2:X})", this.GetType ().Name, position + _offsetOfFirstBuffer, _offsetOfFirstBuffer, position);
         if (position < 0) throw new BMException ("Position [0x{0:X}] is before first stored data [{1:X}].", position + _offsetOfFirstBuffer, _offsetOfFirstBuffer);

         this._mode = Mode._Reading;
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

            byte[] chunck = getDecompressedBuffer (chunckIx);

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

      public virtual void PrepareForNewInstance () {
         FinalizeCompressor ();
      }

      public virtual IDirectStream NewInstanceForThread () {
         return new CompressedChunkedMemoryStream (this);
      }

      public virtual void CloseInstance () {
         _buffers.Clear ();
         _recycledBuffers.Clear ();
         _length = 0;
         _position = 0;
         _offsetOfFirstBuffer = 0;
      }
      #endregion
   }
}
