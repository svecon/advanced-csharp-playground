using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace _01_cetnosti_slov
{
    class FileErrorException : Exception
    {
    }

    class FileStatistics
    {
        protected char[] whitespace = { ' ', '\t', '\n', '\r' };
        int wordCount = 0;
        Dictionary<string, int> words;

        public FileStatistics(string filename)
        {
            words = new Dictionary<string, int>();
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                        addLine(line);
                }
            }
            catch (FileLoadException)
            {
                throw new FileErrorException();
            }
            catch (DirectoryNotFoundException)
            {
                throw new FileErrorException();
            }
            catch (IOException)
            {
                throw new FileErrorException();
            }
        }

        public int getWordCount()
        {
            return wordCount;
        }

        void addLine(string line)
        {
            foreach (string w in line.Split(whitespace, StringSplitOptions.RemoveEmptyEntries))
            {
                wordCount++;
                if (words.ContainsKey(w))
                {
                    words[w]++;
                }
                else
                {
                    words[w] = 1;
                }
            }
        }

        public void printFrequencies()
        {
            foreach (KeyValuePair<string, int> w in words.OrderBy(x => x.Key))
            {
                Console.WriteLine(w.Key + ": " + w.Value);
            }
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Write("Argument Error");
                return;
            }

            try
            {
                FileStatistics fs = new FileStatistics(args[0]);
                Console.Write(fs.getWordCount());
                //fs.printFrequencies();
            }
            catch (FileErrorException)
            {
                Console.Write("File Error");
            }
        }
    }
}