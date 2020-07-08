using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus
{
    public interface IObjectPool<T>
    {
        int MaxObjects { get; set; }
        T Get<T>();
    }


    public class ObjectPool
    {
    }
}
