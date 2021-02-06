using System;
using System.Collections.Generic;

namespace Morpheus
{
    public class ObjectPool<T> where T : class
    {
        private Func<T> generator;
        private Queue<T> queue;

        public ObjectPool( int initialAllocation, Func<T> generator )
        {
            this.generator = generator ?? throw new ArgumentNullException( "generator" );
            this.queue = new Queue<T>( initialAllocation );
            for (int i = 0; i < initialAllocation; i++)
                queue.Enqueue( generator() );
        }

        public T Get()
        {
            lock (queue)
            {
                if (queue.Count > 0)
                    return queue.Dequeue();
            }
            return generator();
        }

        public void Return( T obj )
        {
            lock (queue)
                queue.Enqueue( obj );
        }

        public void Return( IEnumerable<T> objs )
        {
            foreach (var obj in objs)
                Return( obj );
        }
    }
}
