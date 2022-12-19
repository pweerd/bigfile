using Bitmanager.Core;
using Bitmanager.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.IO {
   public abstract class ChunkedMemoryStreamBase : Stream, IDirectStream {
      protected List<byte[]> _buffers;
      protected long _position;
      protected long _length;
      protected readonly long _chunckSize;
      protected bool _isOpen;

      /// <summary>Initializes a new instance of the <see cref="T:System.IO.MemoryStream" /> class with an expandable capacity initialized to zero.</summary>
      public ChunkedMemoryStreamBase ()
        : this (64 * 1024) {
      }

      /// <summary>Initializes a new instance of the <see cref="T:System.IO.MemoryStream" /> class with an expandable capacity initialized as specified.</summary>
      /// <param name="capacity">The initial size of the internal array in bytes. </param>
      /// <exception cref="T:System.ArgumentOutOfRangeException">
      /// <paramref name="capacity" /> is negative. </exception>
      public ChunkedMemoryStreamBase (int chunckSize) {
         if (chunckSize < 4096)
            throw new ArgumentOutOfRangeException (String.Format ("Invalid chunckSize [{0}]. Should be >= 4096.", chunckSize));
         this._buffers = new List<byte[]> ();
         this._chunckSize = chunckSize;
         this._isOpen = true;
      }

      public ChunkedMemoryStreamBase (ChunkedMemoryStreamBase other) {
         this._chunckSize = other._chunckSize;
         this._isOpen = other._isOpen;
         this._buffers = new List<byte[]> (other._buffers);
         this._length = other._length;
      }

      #region IDirectStream stuff
      public virtual void PrepareForNewInstance () {
      }

      public abstract void SetOffsetOfFirstBuffer (long offset);

      public abstract IDirectStream NewInstanceForThread();

      public virtual void CloseInstance () {
         _buffers.Clear ();
         _length = 0;
         _position = 0;
      }

      public abstract int Read (long offset, byte[] buffer, int bufOffset, int count);


      #endregion


      #region Stream stuff
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
      protected virtual bool EnsureCapacity (long value) {
         if (value < 0) value = 0;

         int dirs = (int)((value + _chunckSize - 1) / _chunckSize);
         if (dirs <= _buffers.Count) return false;


         for (int i = _buffers.Count; i < dirs; i++)
            _buffers.Add (createNewBuffer());
         return true;
      }

      protected virtual byte[] createNewBuffer () {
         return new byte[_chunckSize];
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


      #endregion

   }
}
