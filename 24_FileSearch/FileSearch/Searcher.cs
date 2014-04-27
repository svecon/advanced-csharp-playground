using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using Cuni.NPrg038;
using System.Diagnostics;

namespace FileSearch {
    class Searcher {

        FixedSizeQueue<string> filesToProcess;
        Action<string> action;
        Action update;
        AhoCorasickSearch aho;
        Statistics statistics;

        public Searcher(FixedSizeQueue<string> filesToProcess, string needle, Statistics stats, Action<string> action, Action update)
        {
            this.filesToProcess = filesToProcess;
            this.action = action;
            this.update = update;
            this.statistics = stats;

            aho = new AhoCorasickSearch();
            aho.AddPattern(Encoding.UTF8.GetBytes(needle));
            aho.AddPattern(Encoding.ASCII.GetBytes(needle));
            aho.AddPattern(Encoding.Unicode.GetBytes(needle)); // UTF16 LE
            aho.Freeze();
        }

        public void Start()
        {
            string fileName;

            while (filesToProcess.TryDequeue(out fileName))
            {
                statistics.AddTotal(1);

                try
                {
                    ByteReader reader = new ByteReader(File.OpenRead(fileName));

                    var state = aho.InitialState;

                    while (!reader.IsEnd())
                    {
                        if (filesToProcess.Ended)
                            return;

                        state = state.GetNextState(reader.ReadByte());

                        if (state.HasMatchedPattern)
                        {
                            action(Path.GetFileName(fileName));
                            statistics.AddFound(1);
                            statistics.AddSize(reader.TotalReadMB);
                            reader.Dispose();
                            break;
                        }
                    }

                    statistics.AddSize(reader.TotalReadMB);
                    reader.Dispose();
                }

                catch (IOException) { statistics.AddErrors(1); }
                catch (UnauthorizedAccessException) { statistics.AddErrors(1); }

                update();
            }

            Console.WriteLine("searcher ended");
        }
    }
}
