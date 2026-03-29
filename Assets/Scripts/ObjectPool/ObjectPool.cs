using System;
using System.Collections.Generic;
using ObjectPool.Interface;
using UnityEngine;

namespace ObjectPool
{
    public class ObjectPool<T> where T : class,IResetable,new()
    {
        private readonly List<T> _pool;
        public ObjectPool()
        {
            _pool = new List<T>();
        }

        public ObjectPool(int capacity)
        {
            _pool = new List<T>(capacity);
        }

        public T Allocate(Action<T> initializer)
        {
            if (_pool.Count == 0)
            {
                return new T();
            }
            var obj = _pool[^1];
            _pool.RemoveAt(_pool.Count - 1);
            initializer(obj);
            return obj;
        }

        public void Free(T item)
        {
            _pool.Add(item);
            item.Reset();
        }
    }
}
