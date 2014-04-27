using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ParallelMergeSort {
    class Program {

        /// <summary>
        /// Reads numbers from a file and returns them in Chunks
        /// </summary>
        public class ChunkReader : IDisposable {

            /// <summary>
            /// Chunk of data to be sorted. Holds Data in array.
            /// </summary>
            public class Chunk : IDisposable {

                public bool isDone = false;
                public int mergedMultiply = 1;

                public int[] Data { get; private set; }

                public Chunk(int[] Data)
                {
                    this.Data = Data;
                }

                public void Sort()
                {
                    Array.Sort(Data);
                }

                public void MergeWith(Chunk ch)
                {
                    var temp = new int[Data.Length + ch.Data.Length];

                    int leftIndex = 0;
                    int rightIndex = 0;
                    int tempIndex = 0;

                    while (leftIndex < Data.Length && rightIndex < ch.Data.Length)
                        temp[tempIndex++] = Data[leftIndex].CompareTo(ch.Data[rightIndex]) <= 0
                            ? Data[leftIndex++]
                            : ch.Data[rightIndex++];

                    while (leftIndex < Data.Length)
                        temp[tempIndex++] = Data[leftIndex++];

                    while (rightIndex < ch.Data.Length)
                        temp[tempIndex++] = ch.Data[rightIndex++];

                    Data = temp;
                    ch.Dispose();
                }

                public void MergeWithOutput(Chunk ch, TextWriter output)
                {
                    var temp = new int[Data.Length + ch.Data.Length];

                    int leftIndex = 0;
                    int rightIndex = 0;
                    int x;

                    while (leftIndex < Data.Length && rightIndex < ch.Data.Length)
                    {
                        x = Data[leftIndex].CompareTo(ch.Data[rightIndex]) <= 0
                            ? Data[leftIndex++]
                            : ch.Data[rightIndex++];
                        output.WriteLine(x);
                    }

                    while (leftIndex < Data.Length)
                    {
                        x = Data[leftIndex++];
                        output.WriteLine(x);
                    }

                    while (rightIndex < ch.Data.Length)
                    {
                        x = ch.Data[rightIndex++];
                        output.WriteLine(x);
                    }

                    ch.Dispose();
                }

                public void Print(TextWriter output) {
                    for (int i = 0; i < Data.Length; i++)
                    {
                        output.WriteLine(Data[i]);
                    }
                }

                public void Dispose()
                {
                    Data = null;
                }

            }

            List<int> data;
            TextReader reader;
            int chunkSize;

            public ChunkReader(TextReader reader, int chunkSize = 16384)
            {
                data = new List<int>();
                this.reader = reader;
                this.chunkSize = chunkSize;
            }

            public IEnumerable<Chunk> ReadChunk()
            {
                int i = 0;
                int lastIndex = 0;

                string ln;
                while ((ln = reader.ReadLine()) != null && ln != "")
                {
                    data.Add(int.Parse(ln));

                    i++;
                    if (i == chunkSize)
                    {
                        yield return new Chunk(data.ToArray());
                        data.Clear();
                        lastIndex = i + 1;
                        i = 0;
                    }
                }

                if (data.Count > 0)
                    yield return new Chunk(data.ToArray());
            }

            public void Dispose()
            {
                data.Clear();
                data = null;

                reader.Dispose();
            }

        }

        static void Main(string[] args)
        {
            //generateFileWithRandomNumbers("test.txt", 10000); return;
            try
            {
                Run(args, System.Console.In, System.Console.Out);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Argument Error {0}", e.Message);
            }
            catch (FormatException)
            {
                Console.WriteLine("Format Error");
            }
        }

        public static void Run(string[] args, TextReader input, TextWriter output)
        {
            if (args.Length != 1)
                throw new ArgumentException();

            int nThreads;
            if (int.TryParse(args[0], out nThreads) == false)
                throw new ArgumentException();

            if ((nThreads >= 1 && nThreads <= 256) == false)
                throw new ArgumentException();

            var chunkReader = new ChunkReader(input);

            List<Task> tasks = new List<Task>();
            List<ChunkReader.Chunk> chunks = new List<ChunkReader.Chunk>();

            // This adds continuations to tasks as soon as new Chunk of data is loaded.
            // First, two closest tasks are merged (the first task gets a continuation to merge the second one).
            // After that, continuations with gap of 4, 8, 16... (2^i) are created.
            // when allRead is true, Chunks that havent been merged yet (outside of a 2^i) are.
            Action<bool> addAllContinuations = (allRead) =>
            {
                int multiply = 2;
                while (multiply < chunks.Count * 2)
                {
                    int i = 0;
                    while (i + (multiply / 2) < tasks.Count)
                    {
                        int index = i + (multiply / 2);
                        int j = i;

                        if ((!allRead && chunks[i].mergedMultiply != chunks[index].mergedMultiply) || (chunks[j].isDone || chunks[index].isDone))
                        {
                            i += multiply;
                            continue;
                        }

                        chunks[index].mergedMultiply = multiply;
                        chunks[j].mergedMultiply = multiply;
                        chunks[index].isDone = true;

                        var doOutput = allRead && multiply >= chunks.Count;
                        tasks[j] = tasks[j].ContinueWith((x) =>
                        {
                            Task.WaitAll(new Task[] { tasks[index] });
                            if (doOutput)
                                chunks[j].MergeWithOutput(chunks[index], output);
                            else
                                chunks[j].MergeWith(chunks[index]);
                        });

                        i += multiply;
                    }

                    multiply *= 2;
                }

            };

            foreach (var chunk in chunkReader.ReadChunk())
            {
                addAllContinuations(false);

                var task = new Task(chunk.Sort);
                tasks.Add(task);
                chunks.Add(chunk);

                task.Start();
            }

            addAllContinuations(true);

            Task.WaitAll(tasks.ToArray());

            if (chunks.Count == 1)
                chunks[0].Print(output);

            output.Close();
        }

        static void generateFileWithRandomNumbers(string filename, int count)
        {
            var r = new Random();
            var o = new StreamWriter(File.Create(filename));

            for (int i = 0; i < count; i++)
                o.WriteLine(r.Next());

            o.Close();
        }
    }
}
