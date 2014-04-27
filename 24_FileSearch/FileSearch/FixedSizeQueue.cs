using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FileSearch {
    class FixedSizeQueue<T> {

        readonly protected List<T> queue = new List<T>();
        readonly protected int maxSize;

        public FixedSizeQueue(int maxSize)
        {
            this.maxSize = maxSize <= 0 ? int.MaxValue : maxSize;
        }

        public void Enqueue(T item)
        {
            lock (queue)
            {
                while (queue.Count >= maxSize)
                {
                    Monitor.Wait(queue);
                }
                queue.Add(item);
                if (queue.Count == 1)
                {
                    // wake up any blocked dequeue
                    Monitor.PulseAll(queue);
                }
            }
        }
        public T Dequeue()
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    Monitor.Wait(queue);
                }
                T item = queue[0];
                queue.RemoveAt(0);
                if (queue.Count == maxSize - 1)
                {
                    // wake up any blocked enqueue
                    Monitor.PulseAll(queue);
                }
                return item;
            }
        }

        public bool TryEnqueue(T item)
        {
            lock (queue)
            {
                while (queue.Count >= maxSize)
                {
                    if (Closed || Ended)
                    {
                        return false;
                    }
                    Monitor.Wait(queue);
                }
                
                queue.Add(item);
                
                if (queue.Count == 1)
                {
                    // wake up any blocked dequeue
                    Monitor.PulseAll(queue);
                }

                return true;
            }
        }

        public bool Ended { get; protected set; }
        public bool Closed { get; protected set; }
        public void Close()
        {
            lock (queue)
            {
                Closed = true;
                Monitor.PulseAll(queue);
            }
        }
        public void End()
        {
            lock (queue)
            {
                Ended = true;
                Monitor.PulseAll(queue);
            }
        }
        virtual public bool TryDequeue(out T value)
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    if (Closed || Ended)
                    {
                        value = default(T);
                        return false;
                    }

                    Monitor.Wait(queue);
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

        public int BinarySearch(T item)
        {
            return queue.BinarySearch(item);
        }

        public int Count { get { return queue.Count; } }
    }
}
