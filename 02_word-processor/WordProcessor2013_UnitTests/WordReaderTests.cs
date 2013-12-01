using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;

namespace WordProcessor2013_UnitTests {
	using WordProcessor2013;

	[TestClass]
	public class WordReaderTests {
		
		[TestMethod]
		public void ReadWord_EmptyInput() {
			var reader = new WordReader(new StringReader(""));

			Assert.IsNull(reader.ReadWord());
		}

		[TestMethod]
		public void ReadWord_SingleLineWithoutTerminatingNewLine() {
			var words = new[] { "The", "rain", "in", "Spain", "falls", "mainly", "on", "the", "plain." };
			var reader = new WordReader(new StringReader("The rain in Spain falls mainly on the plain."));

			foreach (var word in words) {
				Assert.AreEqual(word, reader.ReadWord());
			}

			Assert.IsNull(reader.ReadWord());
		}

		[TestMethod]
		public void ReadWord_SingleLineIncludingTerminatingNewLine() {
			var words = new[] { "The", "rain", "in", "Spain", "falls", "mainly", "on", "the", "plain." };
			var reader = new WordReader(new StringReader("The rain in Spain falls mainly on the plain.\n"));

			foreach (var word in words) {
				Assert.AreEqual(word, reader.ReadWord());
			}

			Assert.IsNull(reader.ReadWord());
		}

		[TestMethod]
		public void ReadWord_SingleLineWithManySpacesIncludingTerminatingNewLine() {
			var words = new[] { "The", "rain", "in", "Spain", "falls", "mainly", "on", "the", "plain." };
			var reader = new WordReader(new StringReader("   The rain    in \tSpain\tfalls\t\t\tmainly on the plain.\n"));

			foreach (var word in words) {
				Assert.AreEqual(word, reader.ReadWord());
			}

			Assert.IsNull(reader.ReadWord());
		}

		[TestMethod]
		public void ReadWord_MutipleLinesWithoutTerminatingNewLine() {
			var words = new[] { "The", "rain", "in", "Spain", "falls", "mainly", "on", "the", "plain." };
			var reader = new WordReader(new StringReader("The rain in\nSpain falls mainly\non the plain."));

			foreach (var word in words) {
				Assert.AreEqual(word, reader.ReadWord());
			}

			Assert.IsNull(reader.ReadWord());
		}

		[TestMethod]
		public void ReadWord_MutipleLinesIncludingTerminatingNewLine() {
			var words = new[] { "The", "rain", "in", "Spain", "falls", "mainly", "on", "the", "plain." };
			var reader = new WordReader(new StringReader("The rain in\nSpain falls mainly\non the plain.\n"));

			foreach (var word in words) {
				Assert.AreEqual(word, reader.ReadWord());
			}

			Assert.IsNull(reader.ReadWord());
		}

		[TestMethod]
		public void ReadWord_MutipleLinesWithManySpacesIncludingTerminatingNewLine() {
			var words = new[] { "The", "rain", "in", "Spain", "falls", "mainly", "on", "the", "plain." };
			var reader = new WordReader(new StringReader("The rain      in   \n   Spain\tfalls\t\t\tmainly\non the plain.    \n"));

			foreach (var word in words) {
				Assert.AreEqual(word, reader.ReadWord());
			}

			Assert.IsNull(reader.ReadWord());
		}

		[TestMethod]
		public void ReadWord_MutipleIncludingEmptyLinesWithManySpacesIncludingTerminatingNewLine() {
			var words = new[] { "The", "rain", "in", "Spain", "falls", "mainly", "on", "the", "plain." };
			var reader = new WordReader(new StringReader("The rain      in   \n     \n\n\n  Spain\tfalls\t\t\tmainly\non the plain.    \n"));

			foreach (var word in words) {
				Assert.AreEqual(word, reader.ReadWord());
			}

			Assert.IsNull(reader.ReadWord());
		}

		[TestMethod]
		public void ReadWord_OnlyWhitecharacters() {
			var reader = new WordReader(new StringReader("   \t\n  \n    \n\n\n    \n \t\t\t  \n"));

			Assert.IsNull(reader.ReadWord());
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_NullTextReader() {
			var reader = new WordReader(null);
		}
	}
}
