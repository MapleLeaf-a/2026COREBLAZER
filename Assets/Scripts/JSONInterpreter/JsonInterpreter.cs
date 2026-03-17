using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JSONInterpreter.Interface;
using JSONInterpreter.Tokens;
using JSONInterpreter.Tokens.Implement;
using JSONInterpreter.Tokens.Interface;
using UnityEngine;
using Object = UnityEngine.Object;

namespace JSONInterpreter
{
    public static class JsonInterpreter 
    {
        public static T Interpret<T>(string json) where T : IBaseJsonInstance, new()
        {
            List<IToken> tokens = TokenConverter.GetToken(json);

            T result = new T();
            int index = 0;
            
            if (tokens[index] is not TLeftBrace)
            {
                throw new UnityException("Unexpected token: " + tokens[index].ToString());
            }

            index++;
            PairAnalyzer(tokens,ref index, MemberDict.GetMemberDict(typeof(T)),result);
            
            while (tokens[++index] is not TRightBrace)
            {
                if (tokens[index] is not TComma)
                {
                    throw new UnityException("Unexpected token: " + tokens[index].ToString());
                }
                index++;
                PairAnalyzer(tokens,ref index, MemberDict.GetMemberDict(typeof(T)),result);
                if (index + 1 == tokens.Count)
                {
                    throw new UnityException("Not enough tokens in array");
                }
            }

            if (index != tokens.Count - 1)
            {
                throw new UnityException("Token should be end with a right brace");
            }
            
            return result;
        }

        public static List<T> InterpretList<T>(string json) where T : IBaseJsonInstance, new()
        {
            List<IToken> tokens = TokenConverter.GetToken(json);
            
            List<T> result = new List<T>();
            int index = 0;
            
            if (tokens[index] is not TLeftBracket)
            {
                throw new UnityException("Unexpected token: " + tokens[index].ToString());
            }
            
            index++;
            result.Add( (T)ObjectAnalyzer(tokens, ref index, typeof(T)) );
            
            while (tokens[++index] is not TRightBracket){
                if (tokens[index] is not TComma)
                {
                    throw new UnityException("Unexpected token: " + tokens[index].ToString());
                }
                
                index++;
                result.Add( (T)ObjectAnalyzer(tokens, ref index, typeof(T)) );
                
                if (index + 1 == tokens.Count)
                {
                    throw new UnityException("Not enough tokens in array");
                }
            }
            
            if (index != tokens.Count - 1)
            {
                throw new UnityException("Token should be end with a right bracket");
            }
            
            return result;
        }

        private static object ObjectAnalyzer(List<IToken> tokens,ref int index,Type classType) 
        {
            if (tokens[index] is not TLeftBrace)
            {
                throw new UnityException("Unexpected token: " + tokens[index].ToString());
            }
            
            object result = Activator.CreateInstance(classType);

            index++;
            PairAnalyzer(tokens,ref index, MemberDict.GetMemberDict(classType),result);
            
            while (tokens[++index] is not TRightBrace)
            {
                if (tokens[index] is not TComma)
                {
                    throw new UnityException("Unexpected token: " + tokens[index].ToString());
                }
                index++;
                PairAnalyzer(tokens,ref index, MemberDict.GetMemberDict(classType),result);
                if (index + 1 == tokens.Count)
                {
                    throw new UnityException("Not enough tokens in array");
                }
            }
            
            return result;
        }

        private static void PairAnalyzer(List<IToken> tokens,ref int index,Dictionary<string,MemberInfo> dict,object result) 
        {
            
            if (index + 2 >= tokens.Count)
            {
                throw new UnityException("Not enough tokens in array");
            }

            if (tokens[index] is not TString || tokens[index + 1] is not TColon)
            {
                throw new UnityException("Unexpected token for pair");
            }
            
            MemberInfo memberInfo = dict[((TString)tokens[index]).value];
            
            index += 2;
            
            if (memberInfo is FieldInfo info)
            {
                info.SetValue(result,ValueAnalyzer(tokens, ref index,info.FieldType) );
            }
            else if (memberInfo is PropertyInfo info2)
            {
                info2.SetValue(result,ValueAnalyzer(tokens, ref index, info2.PropertyType));
            }
            
        }

        private static object ValueAnalyzer(List<IToken> tokens, ref int index,Type valueType) 
        {
            
            if (valueType == typeof(bool)&&tokens[index] is TBool)
            {
                return ((TBool)tokens[index]).value;
            }

            if (valueType == typeof(int) && tokens[index] is TInt)
            {
                return ((TInt)tokens[index]).value;
            }

            if (valueType == typeof(float) && tokens[index] is TFloat)
            {
                return ((TFloat)tokens[index]).value;
            }

            if (valueType == typeof(string) && tokens[index] is TString)
            {
                return ((TString)tokens[index]).value;
            }

            if (valueType == typeof(IBaseJsonInstance))
            {
                if (tokens[index] is TNull)
                {
                    return null;
                }
                return ObjectAnalyzer(tokens, ref index,valueType);
            }

            if (valueType.IsGenericType&&valueType.GetGenericTypeDefinition() == typeof(List<>))
            {
                if (tokens[index] is TNull)
                {
                    return null;
                }
                return ListAnalyzer(tokens, ref index,valueType ,valueType.GetGenericArguments()[0]);
            }
            
            if (valueType.IsGenericType&&valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                if (tokens[index] is TNull)
                {
                    return null;
                }
                Type[] genericArgs = valueType.GetGenericArguments();
                return DictionaryAnalyzer(tokens, ref index,valueType ,genericArgs[0],genericArgs[1]);
            }

            throw new UnityException("Unexpected value type: " + tokens[index].ToString());
        }

        private static object ListAnalyzer(List<IToken> tokens, ref int index,Type listType, Type listValueType)
        {
            if (tokens[index] is not TLeftBracket)
            {
                throw new UnityException("Unexpected token: " + tokens[index].ToString());
            }
            
            object list = Activator.CreateInstance(listType);
            IList result = list as IList;
            
            index++;
            result?.Add(ValueAnalyzer(tokens, ref index, listValueType) );
            
            while (tokens[++index] is not TRightBracket){
                if (tokens[index] is not TComma)
                {
                    throw new UnityException("Unexpected token: " + tokens[index].ToString());
                }
                
                index++;
                result?.Add(ValueAnalyzer(tokens, ref index, listValueType) );
                
                if (index + 1 == tokens.Count)
                {
                    throw new UnityException("Not enough tokens in array");
                }
            }
            
            return result;
        }

        private static object DictionaryAnalyzer(List<IToken> tokens, ref int index,Type dictionaryType ,Type keyType, Type valueType)
        {
            if (tokens[index] is not TLeftBrace)
            {
                throw new UnityException("Unexpected token: " + tokens[index].ToString());
            }
            
            object dict=Activator.CreateInstance(dictionaryType);
            IDictionary result=dict as IDictionary;
            
            index++;
            DictionaryPairAnalyzer(tokens, ref index, keyType, valueType,ref result);

            while (tokens[++index] is not TRightBrace)
            {
                if (tokens[index] is not TComma)
                {
                    throw new UnityException("Unexpected token: " + tokens[index].ToString());
                }
                index++;
                DictionaryPairAnalyzer(tokens, ref index, keyType, valueType,ref result);
                
                if (index + 1 == tokens.Count)
                {
                    throw new UnityException("Not enough tokens in array");
                }
            }

            return result;
        }

        private static void DictionaryPairAnalyzer(List<IToken> tokens, ref int index, Type keyType, Type valueType,ref IDictionary dict)
        {
            if (tokens[index] is not TLeftBrace)
            {
                throw new UnityException("Unexpected token: " + tokens[index].ToString());
            }
            
            index++;
            object key=ValueAnalyzer(tokens, ref index, keyType);
            
            index++;
            if (tokens[index] is not TComma)
            {
                throw new UnityException("Unexpected token: " + tokens[index].ToString());
            }
            
            index++;
            object value=ValueAnalyzer(tokens, ref index, valueType);
            
            dict.Add(key,value);
            
            index++;
            if (tokens[index] is not TRightBrace)
            {
                throw new UnityException("Unexpected token: " + tokens[index].ToString());
            }
        }
    }
}