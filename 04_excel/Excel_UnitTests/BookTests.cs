using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Excel;
using System.IO;

namespace Excel_UnitTests {
    [TestClass]
    public class BookTests {

        void check(String inpf) {
            Book book = new Book( inpf + ".sheet");
            var writer = new StringWriter();
            book.PrintMainSheet(writer);

            Assert.AreEqual(File.OpenText( inpf + ".eval").ReadToEnd(), writer.ToString());
        }

        [TestMethod]
        public void CheckFile1() { check("sample"); }
        [TestMethod]
        public void CheckFile2() { check("custom"); }
        [TestMethod]
        public void CheckFile3() { check("custom2"); }
        [TestMethod]
        public void CheckFile4() { check("custom3"); }
        [TestMethod]
        public void CheckFile_Cycle1() { check("cycle"); }
        [TestMethod]
        public void CheckFile_Cycle2() { check("cycle2"); }
        [TestMethod]
        public void CheckFile_MultipleSheets2() { check("multiple2"); }
        [TestMethod]
        public void CheckFile_MultipleSheets1() { check("multiple"); }
    }
}
