using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ExpressionEvaluation;

namespace ExpressionEvaluation_UnitTests {
    [TestClass]
    public class EvaluatorTests {

        [TestMethod]
        public void Codex_Simple()
        {
            Evaluator e = new Evaluator();
            e.CreateExpression("+ ~ 1 3");

            Assert.AreEqual(2, e.Evaluate());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Empty()
        {
            Evaluator e = new Evaluator();
            e.CreateExpression("").Evaluate();
        }

        [TestMethod]
        public void ValueOnly()
        {
            Evaluator e = new Evaluator();
            e.CreateExpression("123");
            Assert.AreEqual(123, e.Evaluate());
        }

        [TestMethod]
        public void Codex_Complex()
        {
            Evaluator e = new Evaluator();
            e.CreateExpression("/ + - 5 2 * 2 + 3 3 ~ 2");

            Assert.AreEqual(-7, e.Evaluate());
        }

        [TestMethod]
        public void Custom_MaxPow() {
            Evaluator e = new Evaluator();
            e.CreateExpression("pow max 1 2 3 4");

            Assert.AreEqual(81, e.Evaluate());
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void Codex_OverFlow()
        {
            Evaluator e = new Evaluator();
            e.CreateExpression("- - 2000000000 2100000000 2100000000").Evaluate();
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Codex_FormatTooManyValues()
        {
            Evaluator e = new Evaluator();
            e.CreateExpression("+ 1 2 3").Evaluate();
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Codex_FormatTooLittleValuesBinary()
        {
            Evaluator e = new Evaluator();
            e.CreateExpression("+ 1").Evaluate();
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Codex_FormatTooLittleValuesUnary()
        {
            Evaluator e = new Evaluator();
            e.CreateExpression("~").Evaluate();
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void Codex_DivideError()
        {
            Evaluator e = new Evaluator();
            e.CreateExpression("/ 100 - + 10 10 20").Evaluate();
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Codex_FormatTooBigNumber()
        {
            Evaluator e = new Evaluator();
            e.CreateExpression("- 2000000000 4000000000").Evaluate();
        }
    }
}
