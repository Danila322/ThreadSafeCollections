using System.Collections.Generic;

namespace AsyncDataTreatment.Collections
{
    public interface IQueue<T> : IEnumerable<T>
    {
        public T Take();

        public bool TryTake(out T item);

        public void Push(T item);

        public void PushRange(IEnumerable<T> items);

        public bool IsEmpty { get; }
    }
}
