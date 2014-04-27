using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FileSearch {
    class FixedSizeQueueAutoClosable<T> : FixedSizeQueue<T> {

        int maxThreadsWaiting;

        public FixedSizeQueueAutoClosable(int maxThreadsWaiting)
            : base(0)
        {
            this.maxThreadsWaiting = maxThreadsWaiting;
        }

        override public bool TryDequeue(out T value)
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    maxThreadsWaiting--;

                    if (Closed || Ended || maxThreadsWaiting == 0)
                    {
                        value = default(T);

                        if (maxThreadsWaiting == 0)
                            this.Close();

                        return false;
                    }

                    Monitor.Wait(queue);

                    maxThreadsWaiting++;
                }

                value = queue[0];
                queue.RemoveAt(0);

                if (queue.Count == maxSize - 1)
                {
                    // wake up any blocked enqueue
                    Monitor.PulseAll(queue);
                }
                return true;
            }
        }

    }
}
