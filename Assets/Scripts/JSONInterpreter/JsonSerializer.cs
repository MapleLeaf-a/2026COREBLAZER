using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JSONInterpreter.Interface;

namespace JSONInterpreter
{
    public static class JsonSerializer
    {
        private static int _tabnum;

        private static void AddTab(StringBuilder sb)
        {
            for (int i = 0; i < _tabnum; i++)
            {
                sb.Append('\t');
            }
        }
        
        public static string Serialize<T>(T obj) where T : IBaseJsonInstance,new()
        {
            _tabnum=0;
            StringBuilder result=new StringBuilder("{\n");
            _tabnum++;

            var dict=MemberDict.GetMemberDict(typeof(T));

            int len=0;
            foreach (var member in dict)
            {
                AddTab(result);
                PairSerializer(member.Value,result,obj);
                len++;
                if (len != dict.Count)
                {
                    result.Append(',');
                }
                result.Append('\n');
            }
            
            _tabnum--;
            AddTab(result);
            result.Append("}");
            
            return result.ToString();
        }

        private static void PairSerializer(MemberInfo info,StringBuilder result,IBaseJsonInstance obj)
        {
            result.Append('"'+info.Name+"\": ");
            if (info is FieldInfo fInfo)
            {
                ValueSerializer(fInfo.FieldType,result,fInfo.GetValue(obj));
            }
            else if (info is PropertyInfo pInfo)
            {
                ValueSerializer(pInfo.PropertyType,result,pInfo.GetValue(obj));
            }
        }
        
        private static void ValueSerializer(Type valueType,StringBuilder result,object element)
        {
            if (valueType == typeof(bool)||valueType == typeof(int)||valueType==typeof(float))
            {
                result.Append(element.ToString());
            }
            else if (valueType == typeof(string))
            {
                result.Append('"'+element.ToString()+'"');
            }
            else if (element==null)
            {
                result.Append("null");
            }
            else if (valueType == typeof(IBaseJsonInstance))
            {
                ObjectSerializer((IBaseJsonInstance)element,result);
            }
            else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
            {
                ListSerializer(valueType.GetGenericArguments()[0],result,(IList)element);
            }
            else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                DictionarySerializer(valueType.GetGenericArguments()[0],valueType.GetGenericArguments()[1],result,(IDictionary)element);
            }
            
        }

        private static void ObjectSerializer(IBaseJsonInstance obj, StringBuilder result)
        {
            result.Append("{\n");
            _tabnum++;

            var dict=MemberDict.GetMemberDict(obj.GetType());

            int len=0;
            foreach (var member in dict)
            {
                AddTab(result);
                PairSerializer(member.Value,result,obj);
                len++;
                if (len != dict.Count)
                {
                    result.Append(',');
                }
                result.Append('\n');
            }
            
            _tabnum--;
            AddTab(result);
            result.Append("}");
            
        }

        private static void ListSerializer(Type listValueType, StringBuilder result,IList list)
        {
            result.Append('[');

            for (int i = 0; i < list.Count; i++)
            {
                ValueSerializer(listValueType,result,list[i]);
                if (i != list.Count - 1)
                {
                    result.Append(',');
                }
            }
            
            result.Append(']');
        }

        private static void DictionarySerializer(Type keyType,Type valueType, StringBuilder result, IDictionary dict)
        {
            result.Append("{\n");
            _tabnum++;

            int len=0;
            
            foreach (var member in dict.Keys)
            {
                AddTab(result);
                result.Append('{');
                ValueSerializer(keyType,result,member);
                result.Append(',');
                ValueSerializer(valueType,result,dict[member]);
                result.Append('}');

                len++;
                if (len != dict.Count)
                {
                    result.Append(',');
                }
                result.Append('\n');
            }
            
            _tabnum--;
            AddTab(result);
            result.Append("}");
        }
    }
}