using System;
using System.Collections.Generic;

namespace RollingChecksums
{
    /// <summary>
    /// Rolling Checksum algorithm Adler32 based on Fletcher checksum algorithm.
    /// <see cref="!:http://en.wikipedia.org/wiki/Adler-32"/>
    /// </summary>
    /// <remarks>
    /// It should be faster than CRC32 algorithm (according to wikipedia).
    /// </remarks>
    public class Adler32 : IRollingChecksum
    {
        /// <summary>
        /// Highest prime that is lower than 65535 (2^16).
        /// </summary>
        const int PRIME = 65521;

        public uint Checksum { get; protected set; }
        public int Window { get; protected set; }
        public int ChecksumWindowSize { get; set; }

        /// <summary>
        /// Cyclic buffer that has Window length.
        /// </summary>
        int[] buffer;
        int bufferptr;

        bool wasFilled;

        int nextChecksumCountdown;

        public Adler32()
        {
            Checksum = 0;
        }

        public uint Fill(byte[] data, int windowSize = -1)
        {
            ChecksumWindowSize = Window = windowSize > 0 ? windowSize : data.Length;

            buffer = new int[Window];

            uint a = 1;
            uint b = 0;

            foreach (byte by in data)
            {
                a = (a + by) % PRIME;
                b = (b + a) % PRIME;

                buffer[bufferptr++] = by;
                bufferptr %= buffer.Length;
            }
            ComposeChecksum(a, b);

            wasFilled = true;
            ResetWindowCountdown();

            return Checksum;
        }

        public IEnumerable<uint> Roll(byte[] data, int checksumWindowSize = -1)
        {
            ChecksumWindowSize = checksumWindowSize > 0 ? checksumWindowSize : Window;

            foreach (byte b in data)
            {
                Roll(b);

                if (nextChecksumCountdown != 0)
                    continue;

                ResetWindowCountdown();
                yield return Checksum;
            }
        }

        public uint Roll(int data)
        {
            if (!wasFilled)
                throw new InvalidOperationException("Please use Fill method before Rolling checksum any further");

            uint a = Checksum & 0xFFFF;
            uint b = (Checksum >> 16) & 0xFFFF;

            int oldest = GetOldestValue();

            a = (uint)(a + PRIME - oldest + data) % PRIME;
            b = (uint)(b + PRIME - (oldest * Window) % PRIME + a - 1) % PRIME;

            ComposeChecksum(a, b);

            buffer[bufferptr++] = data;
            bufferptr %= buffer.Length;

            nextChecksumCountdown--;

            return Checksum;
        }

        private int GetOldestValue()
        {
            return buffer[(bufferptr + Window) % Window];
        }

        private void ResetWindowCountdown(){
            nextChecksumCountdown = ChecksumWindowSize;
        }

        /// <summary>
        /// Composes checksum from both parts.
        /// </summary>
        /// <param name="a">Value for the lower 16 bits of the checksum.</param>
        /// <param name="b">Value for the higer 16 bits of the checksum.</param>
        /// <returns>Composed checksum.</returns>
        private uint ComposeChecksum(uint a, uint b) {
            return Checksum = (b * 65536) | a;
        }
    }
}
