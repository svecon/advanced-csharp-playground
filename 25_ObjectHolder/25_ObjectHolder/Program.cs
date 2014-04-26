using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace ObjectHolder {
    class ThreadSafeObjectHolder {

        Object holder;

        private class SecretQueue {

            public LinkedList<Object> data;

            public SecretQueue()
            {
                data = new LinkedList<Object>();
            }

            public SecretQueue(object o1, object o2)
            {
                data = new LinkedList<Object>();
                data.AddLast(o1);
                data.AddLast(o2);
            }

        }

        public ThreadSafeObjectHolder() { }

        public void AddObject(object obj)
        {
            // holder was null => asign obj to holder => end
            if (Interlocked.CompareExchange(ref holder, obj, null) == null) return;

            // holder is SecretQueue or other object
            Object local = holder;

            if (holder.GetType() == typeof(SecretQueue))
            { // holder is already a SecretQueue
                lock (holder)
                {
                    ((SecretQueue)holder).data.AddLast(obj);
                    return;
                }
            }
            else
            {
                // there is a chance that holder is not yet a SecretQueue => create one and try atomic exchange
                var localQueue = new SecretQueue(holder, obj);

                if (Interlocked.CompareExchange(ref holder, localQueue, local) != localQueue)
                { // holder != local => holder changed and is already a SecretQueue
                    lock (holder)
                    {
                        ((SecretQueue)holder).data.AddLast(obj);
                    }
                }
            }
        }

        public object GetFirstObject()
        {
            Object local = holder;

            if (local == null)
                return null;

            if (local.GetType() != typeof(SecretQueue))
                return holder;

            return ((SecretQueue)local).data.First;
        }

    }

    class Program {
        static void Main(string[] args)
        {

        }
    }

}
