using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DequeTests")]

public static class DequeTest {
    public static IList<T> GetReverseView<T>(Deque<T> d)
    {
        return d.Flip();
    }
}

public class Deque<T> : IList<T> {

    const int INITIAL_SIZE = 7;

    T[] data;

    Indexer indexer;

    class Indexer {

        const int INITIAL_SIZE = 7;

        int count;

        int size;

        int currentIndex;

        int direction = 1;

        public Indexer(int initialSize)
        {
            this.size = initialSize;
            Reset();
        }

        public void Flip()
        {
            direction *= -1;
            Reset();
        }

        public int Count { get { return count; } set { count = value; } }

        public int Current { get { return currentIndex; } }

        public void Reset()
        {
            currentIndex = size / 2;

            if (direction == -1)
                currentIndex = (currentIndex + count) % size;
        }

        public int MoveNext(int i = 1)
        {
            return currentIndex = (size + currentIndex + i * direction) % size;
        }

        public int MoveTo(int index)
        {
            return currentIndex = index % size;
        }

        public int RemainToEnd()
        {
            int result = 0;

            if (direction == 1)
            {
                if (currentIndex >= size / 2)
                    result = currentIndex - size / 2;
                else
                    result = (size / 2 + 1) + currentIndex;
            }
            else {
                if (currentIndex >= size / 2)
                    result = size / 2 + size - currentIndex;
                else
                    result = (size / 2 - 1) - currentIndex;
            }

            return count - result; ;
        }
    }

    public Deque()
    {

        indexer = new Indexer(INITIAL_SIZE);

        data = new T[INITIAL_SIZE];
    }

    protected Deque(T[] data, int count, bool flip)
    {

        indexer = new Indexer(data.Length);
        indexer.Count = count;

        if (flip)
            indexer.Flip();

        this.data = data;
    }

    public Deque<T> Flip()
    {
        return new Deque<T>(data, indexer.Count, true);
    }

    public int IndexOf(T item)
    {
        indexer.Reset();
        for (int i = 0; i < indexer.Count; i++)
        {
            if (data[indexer.Current].Equals(item))
                return indexer.Current;

            indexer.MoveNext();
        }

        return -1;
    }

    public void Insert(int index, T item)
    {
        indexer.MoveTo(index);

        T prev, curr;
        prev = item;
        while (indexer.RemainToEnd() > 0)
        {
            curr = data[indexer.Current];
            data[indexer.Current] = prev;
            prev = curr;
            indexer.MoveNext();
        }
        data[indexer.Current] = prev;
        indexer.Count++;
    }

    public void RemoveAt(int index)
    {
        //indexer.Reset();
        indexer.MoveTo(index);
        int ll;
        while ((ll = indexer.RemainToEnd()) > 1)
        //for (int i = index - indexer.Current; i < indexer.Count - 1; i++)
        {
            data[indexer.Current] = data[indexer.MoveNext()];
        }
        data[indexer.Current] = default(T);
        indexer.Count--;
    }

    public T this[int index]
    {
        get
        {
            return data[index];
        }
        set
        {
            data[index] = value;
        }
    }

    public void Add(T item)
    {
        indexer.Reset();
        data[indexer.MoveNext(indexer.Count)] = item;
        indexer.Count++;
    }

    public void Clear()
    {
        indexer.Reset();
        data = new T[indexer.Count];
    }

    public bool Contains(T item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public int Count
    {
        get { return indexer.Count; }
    }

    public bool IsReadOnly
    {
        get { return false; }
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        indexer.Reset();
        for (int i = 0; i < indexer.Count; i++)
        {
            yield return data[indexer.Current];
            indexer.MoveNext();
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return (System.Collections.IEnumerator)GetEnumerator();
    }
}

public class Program {

    public static void Main(String[] args) { }

}
