using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RollingChecksum;

namespace RollingChecksumTests
{
    [TestClass]
    public class RabinKarpTests
    {
        [TestMethod]
        public void RabinKarpTestsZero()
        {
            IRollingChecksum adler = new RabinKarp();
            Assert.AreEqual(0U, adler.Checksum);
        }

        [TestMethod]
        public void RabinKarpTestsBasic()
        {
            IRollingChecksum adler = new RabinKarp();
            Assert.AreEqual(56651U, adler.Fill("Wikipedia".Select(s => Convert.ToByte(s)).ToArray()));
        }

        [TestMethod]
        public void RabinKarpTestsSameBufferTwice()
        {
            byte[] data = new byte[] { 67, 109, 26, 45, 88 };
            IRollingChecksum adler = new RabinKarp();

            uint firstChecksum = adler.Fill(data);
            foreach (var item in adler.Roll(data))
            {
            }

            Assert.AreEqual(firstChecksum, adler.Checksum);
        }

        [TestMethod]
        public void RabinKarpTestsDifferentBuffer()
        {
            byte[] data = new byte[1024 * 1024];
            Random r = new Random();

            r.NextBytes(data);

            IRollingChecksum adler = new Adler32();
            adler.Fill(data.Take(1024).ToArray());
            foreach (var item in adler.Roll(data.Skip(1024).ToArray()))
            {
            }

            IRollingChecksum adler2 = new Adler32();
            adler2.Fill(data.Take(1024).ToArray());
            foreach (var item in adler2.Roll(data.Skip(8742).ToArray()))
            {
            }

            Assert.AreEqual(adler.Checksum, adler2.Checksum);
        }
    }
}
