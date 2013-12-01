using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GateNetwork;
using System.IO;

namespace GateNetworkTests {
    [TestClass]
    public class ProgramTests {
        [TestMethod]
        public void Program_ArgumentError()
        {
            StringWriter sw = new StringWriter();
            Program.Run(new string[0], sw, Console.In);

            Assert.AreEqual("Argument error." + Environment.NewLine, sw.ToString());
        }

        [TestMethod]
        public void Program_ArgumentErrorTooMany()
        {
            StringWriter sw = new StringWriter();
            Program.Run(new[] { "too", "many" }, sw, Console.In);

            Assert.AreEqual("Argument error." + Environment.NewLine, sw.ToString());
        }

        [TestMethod]
        public void Program_FileError()
        {
            StringWriter sw = new StringWriter();
            Program.Run(new[] { "NonExistentFile" }, sw, Console.In);

            Assert.AreEqual("File error." + Environment.NewLine, sw.ToString());
        }

        void checkCorrectInput(string num)
        {
            StringWriter sw = new StringWriter();
            var stdout = File.OpenText("data/" + num + "/std.out");

            Program.Run(new[] { "data/" + num + "/hradla.in" }, sw, File.OpenText("data/" + num + "/std.in"));


            Assert.AreEqual(stdout.ReadToEnd(), sw.ToString());

            stdout.Dispose();
        }

        [TestMethod]
        public void Program_Codex_01() { checkCorrectInput("01"); }

        [TestMethod]
        public void Program_Codex_04() { checkCorrectInput("04"); }

        [TestMethod]
        public void Program_Codex_05() { checkCorrectInput("05"); }

        [TestMethod]
        public void Program_Codex_07() { checkCorrectInput("07"); }

        [TestMethod]
        public void Program_Codex_08() { checkCorrectInput("08"); }

        [TestMethod]
        public void Program_Codex_09() { checkCorrectInput("09"); }

        [TestMethod]
        public void Program_Codex_16() { checkCorrectInput("16"); }

        [TestMethod]
        public void Program_Codex_17() { checkCorrectInput("17"); }

        [TestMethod]
        public void Program_Codex_24() { checkCorrectInput("24"); }

        [TestMethod]
        public void Program_Codex_25() { checkCorrectInput("25"); }

        [TestMethod]
        public void Program_Codex_34() { checkCorrectInput("34"); }

        [TestMethod]
        public void Program_Codex_35() { checkCorrectInput("35"); }
        
        [TestMethod]
        public void Program_Codex_50() { checkCorrectInput("50"); }

        void checkSyntaxError(string num)
        {
            StringWriter sw = new StringWriter();
            var stdout = File.OpenText("data/e" + num + "/std.out");

            Program.Run(new[] { "data/e" + num + "/hradla.in" }, sw, Console.In);

            Assert.AreEqual(stdout.ReadToEnd(), sw.ToString());

            stdout.Dispose();
        }

        [TestMethod]
        public void Program_Codex_SyntaxError_01() { checkSyntaxError("01"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_02() { checkSyntaxError("02"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_03() { checkSyntaxError("03"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_04() { checkSyntaxError("04"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_05() { checkSyntaxError("05"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_06() { checkSyntaxError("06"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_07() { checkSyntaxError("07"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_08() { checkSyntaxError("08"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_09() { checkSyntaxError("09"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_10() { checkSyntaxError("10"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_20() { checkSyntaxError("20"); }


    }
}
