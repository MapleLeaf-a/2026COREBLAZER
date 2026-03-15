using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JSONInterpreter.Interface;
using JSONInterpreter.Tokens.Implement;
using JSONInterpreter.Tokens.Interface;
using UnityEngine;
using Object = UnityEngine.Object;

namespace JSONInterpreter
{
    public class JsonInterpreter<T> where T : IBaseJsonInstance, new()
    {
        public T Interpret(string json)
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
                throw new UnityException("Token should be end with a right bracket");
            }
            
            return result;
        }

        private object ObjectAnalyzer(List<IToken> tokens,ref int index,Type classType) 
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

        private void PairAnalyzer(List<IToken> tokens,ref int index,Dictionary<string,MemberInfo> dict,object result) 
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

        private object ValueAnalyzer(List<IToken> tokens, ref int index,Type valueType) 
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
            }

            throw new UnityException("Unexpected value type: " + tokens[index].ToString());
        }

        private object ListAnalyzer(List<IToken> tokens, ref int index,Type listType, Type listValueType)
        {
            Debug.Log(tokens[index]+","+index);
            if (tokens[index] is not TLeftBracket)
            {
                throw new UnityException("Unexpected token: " + tokens[index].ToString());
            }
            
            object list = Activator.CreateInstance(listType);
            IList result = list as IList;
            
            index++;
            result.Add(ValueAnalyzer(tokens, ref index, listValueType) );
            
            while (tokens[++index] is not TRightBracket){
                Debug.Log(tokens[index]+","+index);
                if (tokens[index] is not TComma)
                {
                    throw new UnityException("Unexpected token: " + tokens[index].ToString());
                }
                
                index++;
                result.Add(ValueAnalyzer(tokens, ref index, listValueType) );
                
                if (index + 1 == tokens.Count)
                {
                    throw new UnityException("Not enough tokens in array");
                }
            }
            
            return result;
        } 
    }
}