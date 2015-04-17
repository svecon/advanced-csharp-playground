using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RollingChecksums;

namespace RollingChecksumTests
{
    [TestClass]
    public class Adler32Tests
    {
        [TestMethod]
        public void Adler32TestsZero()
        {
            IRollingChecksum adler = new Adler32();
            Assert.AreEqual(0U, adler.Checksum);
        }

        [TestMethod]
        public void Adler32TestsBasic()
        {
            IRollingChecksum adler = new Adler32();
            Assert.AreEqual(300286872U, adler.Fill("Wikipedia".Select(Convert.ToByte).ToArray()));
        }

        [TestMethod]
        public void Adler32TestsSameBufferTwice()
        {
            var data = new byte[] { 67, 109, 26, 45, 88 };
            IRollingChecksum adler = new Adler32();

            uint firstChecksum = adler.Fill(data);
            foreach (uint item in adler.Roll(data))
            {
            }

            Assert.AreEqual(firstChecksum, adler.Checksum);
        }

        [TestMethod]
        public void Adler32TestsDifferentBuffer()
        {
            var data = new byte[1024 * 1024];
            var r = new Random();

            r.NextBytes(data);

            IRollingChecksum adler = new Adler32();
            adler.Fill(data.Take(1024).ToArray());
            foreach (uint item in adler.Roll(data.Skip(1024).ToArray()))
            {
            }

            IRollingChecksum adler2 = new Adler32();
            adler2.Fill(data.Take(1024).ToArray());
            foreach (uint item in adler2.Roll(data.Skip(8742).ToArray()))
            {
            }

            Assert.AreEqual(adler.Checksum, adler2.Checksum);
        }
    }
}
