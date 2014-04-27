using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FileSearch {
    public class Statistics {

        public int total;
        public int found;
        public int size;
        public int errors;

        public void AddTotal(int i) { Interlocked.Add(ref total, i); }
        public void AddFound(int i) { Interlocked.Add(ref found, i); }
        public void AddSize(int i) { Interlocked.Add(ref size, i); }
        public void AddErrors(int i) { Interlocked.Add(ref errors, i); }


    }
}
