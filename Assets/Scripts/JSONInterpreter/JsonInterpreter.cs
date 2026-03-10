using JSONInterpreter.Interface;
using UnityEngine;

namespace JSONInterpreter
{
    public class JsonInterpreter<T> where T : IBaseJsonInstance, new()
    {
        public T Interpret(string json)
        {
            return new T();
        }
    }
}