using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace ObjectHolder {
    class ThreadSafeObjectHolder {

        Object first;

        private class SecretQueue {

            public LinkedList<Object> data;
            public SecretQueue()
            {
                data = new LinkedList<Object>();
            }

            public SecretQueue(object f1, object f2)
            {
                data = new LinkedList<Object>();
                data.AddLast(f1);
                data.AddLast(f2);
            }

        }

        public ThreadSafeObjectHolder() { }

        public void AddObject(object obj)
        {
            if (Interlocked.CompareExchange(ref first, obj, null) == null) return;

            lock (typeof(SecretQueue))
            {
                if (first.GetType() != typeof(SecretQueue))
                {
                    first = new SecretQueue(first, obj);
                }
                else
                {
                    ((SecretQueue)first).data.AddLast(obj);
                }

            }
        }

        public object GetFirstObject()
        {
            Object local = first;

            if (local.GetType() != typeof(SecretQueue))
                return first;

            return ((SecretQueue)local).data.First;
        }

    }

    class Program {
        static void Main(string[] args)
        {

        }
    }

}
