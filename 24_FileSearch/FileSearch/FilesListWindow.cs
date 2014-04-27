using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FileSearch {
    public partial class FilesListWindow : Form {

        FixedSizeQueue<string> filesToProcess;
        FixedSizeQueue<string> dirsToProcess;
        Statistics statistics;

        string needle;
        string root;

        int maxCrawlers;
        int maxSearchers;
        int maxQueueSize;

        SortedReadOnlyList matchedFiles;

        readonly Object _lock = new Object();

        /// <summary>
        /// Form's constructor
        /// </summary>
        /// <param name="args">[needle] [root] [crawlerThreads] [searcherThreads] [crawlerSearcherQueueSize]</param>
        public FilesListWindow(string[] args)
        {
            InitializeComponent();

            if (args.Length != 5)
            {
                this.status.Text = "Specify all 5 parameters please!";
                return;
            }

            needle = args[0];
            root = args[1];

            if (!int.TryParse(args[2], out maxCrawlers) || maxCrawlers <= 0)
            {
                this.status.Text = "Please specify maximum number of crawlers.";
                return;
            }

            if (!int.TryParse(args[3], out maxSearchers) || maxSearchers <= 0)
            {
                this.status.Text = "Please specify maximum number of searchers.";
                return;
            }

            if (!int.TryParse(args[4], out maxQueueSize) || maxQueueSize <= 0)
            {
                this.status.Text = "Please specify maximum queue's length.";
                return;
            }

            if (!System.IO.Directory.Exists(root))
            {
                this.status.Text = "The root directory doesn't exist.";
                return;
            }

            this.Text += ": \"" + args[0] + '"'; // add searched text into a title

            prepareAndStartThreads();
        }

        void prepareAndStartThreads()
        {
            dirsToProcess = new FixedSizeQueueAutoClosable<string>(maxCrawlers);
            filesToProcess = new FixedSizeQueue<string>(maxQueueSize);
            statistics = new Statistics();
            matchedFiles = new SortedReadOnlyList();

            dirsToProcess.Enqueue(root);

            for (int i = 0; i < maxCrawlers; i++)
            {
                var crawler = new Crawler(root, filesToProcess, dirsToProcess);
                var thread = new Thread(new ThreadStart(crawler.Start));
                thread.Start();
            }

            for (int i = 0; i < maxSearchers; i++)
            {
                var searcher = new Searcher(filesToProcess, needle, statistics, searcherUpdateListBox, searcherUpdateStatus);
                var thread = new Thread(new ThreadStart(searcher.Start));
                thread.Start();
            }
        }

        private void searcherUpdateListBox(string f)
        {
            lock (_lock)
            {
                try
                {
                    // using only Invoke because when there is many files and BeginInvoke is used, the listBox redraw is too slow
                    // when searching 
                    matchedFilesListBox.Invoke(new Action(() => matchedFilesListBox.Items.Insert(matchedFiles.SortedInsert(f), f)));
                }
                catch (ObjectDisposedException) { } // if form was already closed
                catch (InvalidOperationException) { } // and ListBox doesnt exist anymore
            }
        }

        private void searcherUpdateStatus()
        {
            try
            {
                status.BeginInvoke(new Action(() =>
                    status.Text = String.Format("Found in {0} out of {1} (+ {2} unaccessible). {3}MB read.",
                    statistics.found, statistics.total, statistics.errors, statistics.size)));
            }
            catch (ObjectDisposedException) { } // if form was already closed
            catch (InvalidOperationException) { } // and ListBox doesnt exist anymore
        }

        private void FilesListWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (filesToProcess != null)
                filesToProcess.End();
            if (dirsToProcess != null)
                dirsToProcess.End();
        }

    }
}
