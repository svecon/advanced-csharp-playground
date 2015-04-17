using System;
using System.Collections.Generic;

namespace RollingChecksums
{
    /// <summary>
    /// Rolling checksum algorithm Rabin Karp.
    /// 
    /// <see cref="!:http://en.wikipedia.org/wiki/Rabin%E2%80%93Karp_algorithm"/>
    /// </summary>
    /// <remarks>
    /// This algorithm is basically a core for string search algorithm with the same name.
    /// </remarks>
    public class RabinKarp : IRollingChecksum
    {
        /// <summary>
        /// Random prime that is used to limit and randomize the checksum.
        /// </summary>
        const int PRIME = 100007;

        public uint Checksum { get; protected set; }
        public int Window { get; protected set; }
        public int ChecksumWindowSize { get; set; }

        /// <summary>
        /// Cyclic buffer that has Window length.
        /// </summary>
        int[] buffer;

        int bufferPtr;

        /// <summary>
        /// Largest power for the most significant byte. Used as a cache value.
        /// </summary>
        int pow;

        bool wasFilled;
        int nextChecksumCountdown;

        public RabinKarp()
        {
            bufferPtr = 0;
            Checksum = 0;
        }

        private void PreCalculatePower()
        {
            pow = 1;
            for (int i = 1; i < Window; i++)
            {
                pow = (pow * Window) % PRIME;
            }
        }

        public uint Fill(byte[] data, int windowSize = -1)
        {
            ChecksumWindowSize = Window = windowSize > 0 ? windowSize : data.Length;
            buffer = new int[Window];

            foreach (byte b in data)
            {
                Checksum = (uint)(Checksum * Window + b) % PRIME;

                buffer[bufferPtr++] = b;
                bufferPtr %= buffer.Length;
            }

            PreCalculatePower();
            wasFilled = true;

            return Checksum;
        }

        public IEnumerable<uint> Roll(byte[] data, int checksumWindowSize = -1)
        {
            ChecksumWindowSize = checksumWindowSize > 0 ? checksumWindowSize : Window;

            foreach (byte b in data)
            {
                Roll(b);

                if (nextChecksumCountdown != 0) continue;

                ResetWindowCountdown();
                yield return Checksum;
            }
        }

        public uint Roll(int data)
        {
            if (! wasFilled)
                throw new InvalidOperationException("Please use Fill method before Rolling checksum any further");

            // add PRIME to be sure that the value is not negative 
            Checksum = (uint)(Checksum + PRIME - pow * GetOldestValue() % PRIME) % PRIME;
            Checksum = (uint)(Checksum * Window + data) % PRIME;

            buffer[bufferPtr++] = data;
            bufferPtr %= buffer.Length;

            nextChecksumCountdown--;

            return Checksum;
        }

        private void ResetWindowCountdown()
        {
            nextChecksumCountdown = Window * 10 / 7;
        }

        private int GetOldestValue()
        {
            return buffer[(bufferPtr + Window) % Window];
        }
    }
}
