using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("WordProcessor2013_UnitTests")]

namespace WordProcessor2013 {

	interface IWordReader {
		string ReadWord();
	}

	// Processors are using a push model.
	interface IWordProcessor {
		void ProcessWord(string word);
		void Finish();
	}

    interface IBlockWriter {
        int getBlockSize();
        void setBlockSize(int size);
    }

	class WordReader : IWordReader, IDisposable {
		protected TextReader reader;
		protected string[] words = { };
		protected int nextWord = 0;

		public WordReader(TextReader reader) {
			if (reader == null) {
				throw new ArgumentNullException("reader");
			}
			this.reader = reader;
		}

		virtual public string ReadWord() {
			while (nextWord >= words.Length) {
				string line = reader.ReadLine();
				if (line == null) {
					return null;
				}
				words = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				nextWord = 0;
			}

			return words[nextWord++];
		}

		public void Dispose() {
			reader.Dispose();
		}
	}

    class ParagraphReader : WordReader {

        public const string paragraphEnding = "*@*";

        public ParagraphReader(TextReader reader) : base(reader) {
        
        }

        internal void skipWhitespace() {
            while (
                (reader.Peek() == '\n') || 
                (reader.Peek() == '\r') || 
                (reader.Peek() == '\t') || 
                (reader.Peek() == ' '))
                reader.Read();
        }

        override public string ReadWord()
        {
            var buffer = new StringBuilder();
            int c;
            int newLines = 0;

            while((c = reader.Peek()) >= 0)
            {
                if ((c == '\n') || (c == ' ') || (c == '\t') || (c == '\r'))
                {
                    if (c == '\n')
                    {
                        newLines++;
                    }
                    
                    if (buffer.Length > 0)
                        return buffer.ToString();
                }
                else
                {
                    if (newLines >= 2)
                    {
                        return paragraphEnding;
                    }

                    newLines = 0;

                    buffer.Append((char)c);
                }

                reader.Read();

            }

            if (buffer.Length > 0)
                return buffer.ToString();

            return null;
        }
    }

	class WordCounter : IWordProcessor {
		private TextWriter writer;
		private int wordCount = 0;

		public WordCounter(TextWriter writer) {
			if (writer == null) {
				throw new ArgumentNullException("writer");
			}
			this.writer = writer;
		}

		public void ProcessWord(string word) {
			wordCount++;
		}

		public void Finish() {
			writer.WriteLine(wordCount);
		}
	}

    class WordAligner : IWordProcessor, IBlockWriter {

        private TextWriter writer;
        int blockSize = 0;
        List<String> buffer;
        int bufferLength;

        public WordAligner(TextWriter writer, int blockSize)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            this.writer = writer;
            this.blockSize = blockSize;

            this.buffer = new List<String>();
            this.bufferLength = 0;
        }

        private int isSpaceOverflow(ref int spacesOverflow)
        {
            return (spacesOverflow-- > 0) ? 1 : 0;
        }

        private void processBuffer(bool align, bool newline)
        {
            int spaceSize;
            int spacesOverflow;

            if (! align)
            {
                spaceSize = 1;
                spacesOverflow = 0;
            }
            else
            {
                spaceSize = (buffer.Count - 1 == 0) ? 0 : (blockSize - bufferLength) / (buffer.Count - 1);
                spacesOverflow = blockSize - bufferLength - (spaceSize * (buffer.Count - 1));
            }

            for (int i = 0; i < buffer.Count; i++)
            {
                writer.Write(buffer[i]);

                // no spaces if there is one word on line
                if (buffer.Count - 1 == 0)
                    continue;

                // no spaces after last word
                if (i == buffer.Count - 1)
                    continue;

                for (int j = spaceSize + isSpaceOverflow(ref spacesOverflow); j > 0; j--)
                    writer.Write(" ");
                
            }

            if (newline)
                writer.Write(Environment.NewLine);

            buffer.Clear();
            bufferLength = 0;
        }

        public void ProcessWord(string word)
        {
            if (word == ParagraphReader.paragraphEnding)
            {
                processBuffer(false, true);
                writer.Write(Environment.NewLine);
                return;
            }

            // total word length + spaces between them + new word length + new space
            if (bufferLength + buffer.Count - 1 + word.Length + 1 > blockSize)
                processBuffer(true, true);

            buffer.Add(word);
            bufferLength += word.Length;

        }

        public void Finish()
        {
            if (buffer.Count > 0)
                processBuffer(false, true);

            writer.Close();
        }

        public int getBlockSize()
        {
            return blockSize;
        }

        public void setBlockSize(int size)
        {
            this.blockSize = size;
        }
    }

	class Program {
		public static void ProcessWords(IWordReader reader, IWordProcessor processor) {
			string word;
			while ((word = reader.ReadWord()) != null) {
				processor.ProcessWord(word);
			}

			processor.Finish();
		}

		static TextWriter output;
        static TextWriter errorOutput;

		static void ReportFileError() {
            output = errorOutput;
            output.WriteLine("File Error");
		}

		static void ReportArgumentError() {
            output.WriteLine("Argument Error");
		}

		public static void Run(string[] args, TextWriter textOutput) {
            errorOutput = textOutput;
            output = textOutput;

            int blockSize;

            if (args.Length != 3 || args[0] == "" || args[1] == "" || !int.TryParse(args[2], out blockSize))
            {
                ReportArgumentError();
                return;
            }

            output = new StreamWriter(args[1]);

  			ParagraphReader reader = null;
			try {
				reader = new ParagraphReader(new StreamReader(args[0]));
				var aligner = new WordAligner(output, blockSize);

				ProcessWords(reader, aligner);
			} catch (FileNotFoundException) {
				ReportFileError();
			} catch (IOException) {
				ReportFileError();
			} catch (UnauthorizedAccessException) {
				ReportFileError();
			} catch (System.Security.SecurityException) {
				ReportFileError();
			} finally {
				if (reader != null) reader.Dispose();
			}
		}

		static void Main(string[] args) {
			Run(args, Console.Out);
		}
	}
}

