using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FileSearch {

    class ByteReader : IDisposable {
        const int BLOCK_SIZE = 16384; // 16kB

        Stream reader;
        byte[] cache;
        int cacheSize;
        int current;

        int totalRead;
        public int TotalReadMB { get { return totalRead / 1024; } private set { totalRead = value; } }

        public ByteReader(Stream reader)
        {
            this.reader = reader;
            cache = new byte[BLOCK_SIZE];
        }

        bool loadToBuffer()
        {
            if (cacheSize <= current)
            {
                current = 0;
                cacheSize = reader.Read(cache, 0, BLOCK_SIZE);
                totalRead += cacheSize;

                if (cacheSize == 0)
                    return false;
            }

            return true;
        }

        public bool IsEnd()
        {
            return !loadToBuffer();
        }

        public byte ReadByte()
        {
            if (!loadToBuffer())
                throw new EndOfStreamException();

            return cache[current++];
        }

        public ulong ReadLong()
        {
            ulong ret = 0;

            var temp = new byte[8];
            for (int i = 0; i < temp.Length; i++)
                temp[i] = ReadByte();

            for (int i = temp.Length - 1; i >= 0; i--)
            {
                ret <<= 8;
                ret |= temp[i];
            }

            return ret;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
