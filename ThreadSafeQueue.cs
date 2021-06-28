using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace AsyncDataTreatment.Collections
{
    public class ThreadSafeQueue<T> : IQueue<T>
    {
        private Node<T> head;
        private Node<T> tail;
        private object readLock;
        private object writeLock;

        public ThreadSafeQueue()
        {
            readLock = new object();
            writeLock = new object();
            head = new Node<T>();
            tail = head;
        }

        public bool IsEmpty => head == tail;

        public IEnumerator<T> GetEnumerator()
        {
            try
            {
                Monitor.Enter(readLock);
                Monitor.Enter(writeLock);
                Node<T> current = head.Next;
                while (current is not null)
                {
                    yield return current.Data;
                    current = current.Next;
                }
            }
            finally
            {
                Monitor.Exit(readLock);
                Monitor.Exit(writeLock);
            }
        }

        public void Push(T item)
        {
            lock (writeLock)
            {
                PushItem(item);
            }
        }

        public void PushRange(IEnumerable<T> items)
        {
            lock (writeLock)
            {
                foreach (var item in items)
                {
                    PushItem(item);
                }
            }
        }

        public T Take()
        {
            if (!IsEmpty)
            {
                lock (readLock)
                {
                    return TakeItem();
                }
            }
            return default(T);
        }

        public bool TryTake(out T item)
        {
            if (!IsEmpty)
            {
                lock (readLock)
                {
                    bool result = !IsEmpty;
                    item = TakeItem();
                    return result;
                }
            }
            item = default(T);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void PushItem(T item)
        {
            Node<T> node = new Node<T>() { Data = item };
            tail.Connect(node);
            Volatile.Write(ref tail, node);
        }

        private T TakeItem()
        {
            T result = default(T);
            if (!IsEmpty)
            {
                result = head.Next.Data;
                Volatile.Write(ref head, head.Next);
            }
            return result;
        }

        private class Node<T>
        {
            public T Data { get; init; }

            public Node<T> Next { get; private set; }

            public void Connect(Node<T> next)
            {
                Next = next;
            }
        }
    }
}
