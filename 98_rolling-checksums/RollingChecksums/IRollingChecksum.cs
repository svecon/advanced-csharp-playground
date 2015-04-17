using System.Collections.Generic;

namespace RollingChecksums
{
    /// <summary>
    /// Interface that all algorithms for Rolling Checksums must implement.
    /// 
    /// For more information about Rolling checksum <see cref="!:http://en.wikipedia.org/wiki/Rolling_hash"/>
    /// \n
    /// All methods return uint Checksum.
    /// </summary>
    public interface IRollingChecksum
    {
        /// <summary>
        /// Calculated checksum.
        /// </summary>
        uint Checksum { get; }

        /// <summary>
        /// Window size for the checksum. Determines how many bytes the checksum depends on.
        /// </summary>
        int Window { get; }

        /// <summary>
        /// Determines how often the checksums will be returned while Rolling over data.
        /// 
        /// Default is the same as Window size (no overlapping).
        /// </summary>
        int ChecksumWindowSize { get; set; }

        /// <summary>
        /// Fills the entire Window of the buffer and calculates the Checksum.
        /// 
        /// The data length is considered new buffer's window size.
        /// </summary>
        /// <param name="data">Initial data for which the checksum will be calculated.</param>
        /// <param name="windowSize">The size for the checksum window. Falls back to data length when below zero.</param>
        /// <returns>Calculated checksum for the data.</returns>
        uint Fill(byte[] data, int windowSize = -1);

        /// <summary>
        /// Rools the window by one character and quickly recalculates corresponding checksum. 
        /// </summary>
        /// <param name="data">New value which will replace the oldest one.</param>
        /// <returns>Recalculated checksum.</returns>
        uint Roll(int data);

        /// <summary>
        /// Rools the window by an entire array of values one by one. Returns the checksums periodically.
        /// </summary>
        /// <param name="data">New values for which will be calculated new checksums.</param>
        /// <param name="checksumWindowSize">Determines how often will checksums be enumerated. Fallbacks to Window when negative.</param>
        /// <returns>Enumerable of recalculated checksums.</returns>
        IEnumerable<uint> Roll(byte[] data, int checksumWindowSize = -1);
    }
}
