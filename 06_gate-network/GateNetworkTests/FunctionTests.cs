using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GateNetwork;

namespace GateNetworkTests {
    [TestClass]
    public class FunctionTests {

        [TestMethod]
        public void Function_BasicSignalValue()
        {
            var f = new MultiDimensionalSignalFunction(1, 2);
            f.Set(new[] { 1 }, new[] {SignalValues.one});

            CollectionAssert.AreEqual(new[] { SignalValues.one }, f.Get(new[] { 1 }));
            Assert.IsNull(f.Get(new[] { 0 }));
        }

        [TestMethod]
        public void Function_TwoDimensions()
        {
            var f = new MultiDimensionalSignalFunction(2, 3);
            f.Set(new[] { 0, 1 }, new[] { SignalValues.one, SignalValues.zero });

            CollectionAssert.AreEqual(new[] { SignalValues.one, SignalValues.zero }, f.Get(new[] { 0, 1 }));
            Assert.IsNull(f.Get(new[] { 0, 0 }));
        }

        [TestMethod]
        public void Function_ConstantSignal()
        {
            var f = new ScalarSignalFunction();
            f.Set(new[] { 0, 1 }, new[] { SignalValues.one, SignalValues.unknown });

            CollectionAssert.AreEqual(new[] { SignalValues.one, SignalValues.unknown }, f.Get(new[] { 0, 1 }));
            CollectionAssert.AreEqual(new[] { SignalValues.one, SignalValues.unknown }, f.Get(new[] { 0, 0 }));
            Assert.IsNotNull(f.Get(new int[1]));
        }
    }
}
