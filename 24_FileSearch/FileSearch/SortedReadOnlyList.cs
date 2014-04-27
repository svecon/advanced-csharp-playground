using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FileSearch {

    class SortedReadOnlyList {

        public readonly List<string> list = new List<string>();

        public SortedReadOnlyList() { }

        public int SortedInsert(string item)
        {
            int position = list.BinarySearch(item, (x, y) =>
            {
                return String.Compare(x, y, true, System.Globalization.CultureInfo.InvariantCulture);
            });

            if (position < 0)
                position = ~position;

            list.Insert(position, item);
            return position;
        }

        public int Count { get { return list.Count; } }

    }
}
