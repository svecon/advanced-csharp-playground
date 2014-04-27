using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Security;

namespace FileSearch {
    class Crawler {

        string root;
        FixedSizeQueue<string> filesToProcess;
        FixedSizeQueue<string> dirsToProcess;

        public Crawler(string root, FixedSizeQueue<string> filesToProcess, FixedSizeQueue<string> dirsToProcess)
        {
            this.root = root;
            this.filesToProcess = filesToProcess;
            this.dirsToProcess = dirsToProcess;
        }

        public void Start()
        {
            string currentDir;

            while (dirsToProcess.TryDequeue(out currentDir))
            {
                string[] subDirs = { };
                string[] files = { };

                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                // Thrown if we do not have discovery permission on the directory. 
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                // Thrown if another process has deleted the directory after we retrieved its name. 
                catch (DirectoryNotFoundException)
                {
                    continue;
                }

                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                catch (DirectoryNotFoundException)
                {
                    continue;
                }
                catch (IOException)
                {
                    continue;
                }

                foreach (var file in files)
                    if (!filesToProcess.TryEnqueue(file)) break;

                foreach (string str in subDirs)
                    if (!dirsToProcess.TryEnqueue(str)) break;

                if (dirsToProcess.Ended)
                    break;
            }

            filesToProcess.Close();
        }
    }
}
