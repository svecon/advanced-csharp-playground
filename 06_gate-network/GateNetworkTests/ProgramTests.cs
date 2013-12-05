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

        [TestMethod]
        public void Program_Codex_51() { checkCorrectInput("51"); }

        [TestMethod]
        public void Program_Codex_52() { checkCorrectInput("52"); }

        [TestMethod]
        public void Program_Codex_53() { checkCorrectInput("53"); }


        [TestMethod]
        public void Program_Codex_100() { checkCorrectInput("100"); }

        [TestMethod]
        public void Program_Codex_101() { checkCorrectInput("101"); }

        [TestMethod]
        public void Program_Codex_102() { checkCorrectInput("102"); }

        [TestMethod]
        public void Program_Codex_103() { checkCorrectInput("103"); }

        [TestMethod]
        public void Program_Codex_104() { checkCorrectInput("104"); }

        [TestMethod]
        public void Program_Codex_105() { checkCorrectInput("105"); }

        [TestMethod]
        public void Program_Codex_106() { checkCorrectInput("106"); }

        [TestMethod]
        public void Program_Codex_107() { checkCorrectInput("107"); }

        [TestMethod]
        public void Program_Codex_108() { checkCorrectInput("108"); }

        [TestMethod]
        public void Program_Codex_109() { checkCorrectInput("109"); }

        [TestMethod]
        public void Program_Codex_110() { checkCorrectInput("110"); }

        [TestMethod]
        public void Program_Codex_111() { checkCorrectInput("111"); }

        [TestMethod]
        public void Program_Codex_112() { checkCorrectInput("112"); }

        [TestMethod]
        public void Program_Codex_113() { checkCorrectInput("113"); }

        [TestMethod]
        public void Program_Codex_114() { checkCorrectInput("114"); }

        [TestMethod]
        public void Program_Codex_115() { checkCorrectInput("115"); }

        [TestMethod]
        public void Program_Codex_116() { checkCorrectInput("116"); }

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

        //[TestMethod]
        //public void Program_Codex_SyntaxError_05() { checkSyntaxError("05"); }

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

        [TestMethod]
        public void Program_Codex_SyntaxError_21() { checkSyntaxError("21"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_22() { checkSyntaxError("22"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_23() { checkSyntaxError("23"); }

        [TestMethod]
        public void Program_Codex_SyntaxError_24() { checkSyntaxError("24"); }


    }
}
