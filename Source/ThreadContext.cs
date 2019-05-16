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

using Bitmanager.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile
{
   public class ThreadContext
   {
      public Encoding Encoding = Encoding.UTF8;
      public readonly IDirectStream DirectStream;
      private readonly List<long> partialLines;  //Not owned!!
      private byte[] byteBuffer;
      private char[] charBuffer;

      public byte[] ByteBuffer { get { return byteBuffer; } }
      public char[] CharBuffer { get { return charBuffer; } }


      public ThreadContext(Encoding encoding, IDirectStream strm, List<long> partialLines)
      {
         this.DirectStream = strm;
         this.partialLines = partialLines;
         this.Encoding = encoding;
      }
      private ThreadContext(ThreadContext other, int maxBufferSize)
      {
         this.DirectStream = other.DirectStream.NewInstanceForThread();
         this.partialLines = other.partialLines;
         this.Encoding = other.Encoding;
         SetMaxBufferSize(maxBufferSize);
      }

      public ThreadContext NewInstanceForThread()
      {
         if (byteBuffer == null) throw new Exception("MaxBufferSize should be set before cloning.");
         return new ThreadContext(this, byteBuffer.Length);
      }

      public ThreadContext NewInstanceForThread(int maxBufferSize)
      {
         return new ThreadContext(this, maxBufferSize);
      }

      public void Close()
      {
         DirectStream.Close();
      }
      public void CloseInstance()
      {
         DirectStream.CloseInstance();
      }

      public void SetMaxBufferSize(int size)
      {
         byteBuffer = new byte[size];
         charBuffer = new char[size];
      }
      public int GetMaxBufferSize()
      {
         return byteBuffer==null ? 0 : byteBuffer.Length;
      }

      public int ReadPartialLineBytesInBuffer(int from, int until)
      {
         if (until >= this.partialLines.Count || from < 0)
            return 0;

         long o1 = partialLines[from] >> LogFile.FLAGS_SHIFT;
         long o2 = partialLines[until] >> LogFile.FLAGS_SHIFT;
         int lineLen = (int)(o2 - o1);
         if (lineLen == 0) return 0;

         int bytesRead = 0;
         try
         {
            long position = o1;
            while (true)
            {
               int freeSpace = lineLen - bytesRead;
               if (freeSpace <= 0) break;

               var len = DirectStream.Read(position, byteBuffer, bytesRead, freeSpace);
               if (len == 0) break;
               position += len;
               bytesRead += len;
            }
         }
         catch (Exception err)
         {
            String msg = err.ToString();
            return Encoding.GetBytes(msg, 0, msg.Length, byteBuffer, 0);
         }
         int end = bytesRead - 1;
         while (end >= 0)
         {
            switch (byteBuffer[end])
            {
               case 10:
               case 13:
                  --end;
                  continue;
            }
            break;
         }

         return end + 1;
      }

      double bytes = 1;
      double chars = 1;
      public int ReadPartialLineBytesInBuffer(int from, int until, int maxChars)
      {
         if (until >= this.partialLines.Count || from < 0)
            return 0;

         long o1 = partialLines[from] >> LogFile.FLAGS_SHIFT;
         long o2 = partialLines[until] >> LogFile.FLAGS_SHIFT;
         int lineLen = (int)(o2 - o1);
         if (lineLen == 0) return 0;

         int bytesRead = 0;
         try
         {
            long position = o1;
            int charsRead = 0;
            while (true)
            {
               int freeSpace = lineLen - bytesRead;
               if (freeSpace <= 0) break;

               int toRead = maxChars <= 0 ? freeSpace : (int)((maxChars - charsRead) * (bytes / chars) + 32);
               if (toRead >= freeSpace) toRead = freeSpace;
               var len = DirectStream.Read(position, byteBuffer, bytesRead, toRead);
               if (len == 0) break;
               position += len;
               bytesRead += len;
               if (maxChars > 0)
               {
                  charsRead = Encoding.GetCharCount(byteBuffer, 0, bytesRead);
                  if (charsRead > maxChars) break;
                  bytes += bytesRead;
                  chars += charsRead;
               }
            }
         }
         catch (Exception err)
         {
            String msg = err.ToString();
            return Encoding.GetBytes(msg, 0, msg.Length, byteBuffer, 0);
         }
         int end = bytesRead - 1;
         while (end >= 0)
         {
            switch (byteBuffer[end])
            {
               case 10:
               case 13:
                  --end;
                  continue;
            }
            break;
         }

         return end + 1;
      }

      public int ReadPartialLineCharsInBuffer(int from, int until)
      {
         int bytes = ReadPartialLineBytesInBuffer(from, until);
         return Encoding.GetChars(byteBuffer, 0, bytes, charBuffer, 0);
      }

      public int ReadPartialLineCharsInBuffer(int from, int until, int maxChars)
      {
         int bytes = ReadPartialLineBytesInBuffer(from, until, maxChars);
         return Encoding.GetChars(byteBuffer, 0, bytes, charBuffer, 0);
      }

      public String GetPartialLine (int from, int until, int maxChars)
      {
         int len = maxChars >= 0 ? ReadPartialLineCharsInBuffer(from, until, maxChars)
                                 : ReadPartialLineCharsInBuffer(from, until);
         return new string(charBuffer, 0, len);
      }
   }
}
