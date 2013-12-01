using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GateNetwork;

namespace GateNetworkTests {
    [TestClass]
    public class FunctionTests {

        [TestMethod]
        public void Function_BasicIntegers()
        {
            Function<int> f = new MultiDimensionalFunction<int>(1, 2);
            f.Set(new[] { 1 }, 5);

            Assert.AreEqual(5, f.Get(new[] { 1 }));
            Assert.AreEqual(0, f.Get(new[] { 0 }));
        }

        [TestMethod]
        public void Function_TwoDimensionsWithIntegerValue()
        {
            Function<int> f = new MultiDimensionalFunction<int>(2, 2);
            f.Set(new[] { 0, 1 }, 5);

            Assert.AreEqual(5, f.Get(new[] { 0, 1 }));
            Assert.AreEqual(0, f.Get(new[] { 0, 0 }));
        }

        [TestMethod]
        public void Function_TwoDimensionsWithArrayValue()
        {
            Function<int[]> f = new MultiDimensionalFunction<int[]>(2, 2);
            f.Set(new[] { 0, 1 }, new[] { 5, 11 });

            CollectionAssert.AreEqual(new[] { 5, 11 }, f.Get(new[] { 0, 1 }));
            Assert.AreEqual(null, f.Get(new[] { 0, 0 }));
        }

        [TestMethod]
        public void Function_ConstantSignal()
        {
            var f = new ConstantSignalFunction();
            f.Set(new[] { 0, 1 }, new[] { SignalValues.one, SignalValues.unknown });

            CollectionAssert.AreEqual(new[] { SignalValues.one, SignalValues.unknown }, f.Get(new[] { 0, 1 }));
            CollectionAssert.AreEqual(new[] { SignalValues.one, SignalValues.unknown }, f.Get(new[] { 0, 0 }));
            Assert.IsNotNull(f.Get(new int[1]));
        }
    }
}
