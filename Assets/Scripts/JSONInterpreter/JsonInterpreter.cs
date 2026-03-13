using System;
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
            if (tokens[index] is not TLeftBracket)
            {
                throw new UnityException("Unexpected token: " + tokens[index].ToString());
            }

            index++;
            PairAnalyzer<T>(tokens,ref index, MemberDict.GetMemberDict(typeof(T)),result);
            
            while (tokens[++index] is not TRightBracket)
            {
                if (tokens[index] is not TComma)
                {
                    throw new UnityException("Unexpected token: " + tokens[index].ToString());
                }
                index++;
                PairAnalyzer<T>(tokens,ref index, MemberDict.GetMemberDict(typeof(T)),result);
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

        private TObj ObjectAnalyzer<TObj>(List<IToken> tokens,ref int index) where TObj : IBaseJsonInstance, new()
        {
            if (tokens[index] is not TLeftBracket)
            {
                throw new UnityException("Unexpected token: " + tokens[index].ToString());
            }
            
            TObj result = new TObj();

            index++;
            PairAnalyzer<TObj>(tokens,ref index, MemberDict.GetMemberDict(typeof(TObj)),result);
            
            while (tokens[++index] is not TRightBracket)
            {
                if (tokens[index] is not TComma)
                {
                    throw new UnityException("Unexpected token: " + tokens[index].ToString());
                }
                index++;
                PairAnalyzer<TObj>(tokens,ref index, MemberDict.GetMemberDict(typeof(TObj)),result);
                if (index + 1 == tokens.Count)
                {
                    throw new UnityException("Not enough tokens in array");
                }
            }
            
            return result;
        }

        private void PairAnalyzer<TObj>(List<IToken> tokens,ref int index,Dictionary<string,MemberInfo> dict,TObj result) 
            where TObj : IBaseJsonInstance, new()
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
            if (memberInfo is FieldInfo)
            {
                
            }
            else if (memberInfo is PropertyInfo)
            {
                
            }
            
            index += 2;
            
        }

        private object ValueAnalyzer<TVar>(List<IToken> tokens, ref int index) 
        {
            if (typeof(TVar) == typeof(bool)&&tokens[index] is TBool)
            {
                return ((TBool)tokens[index]).value;
            }

            if (typeof(TVar) == typeof(int) && tokens[index] is TInt)
            {
                return ((TInt)tokens[index]).value;
            }

            if (typeof(TVar) == typeof(float) && tokens[index] is TFloat)
            {
                return ((TFloat)tokens[index]).value;
            }

            /*if (typeof(TVar) == typeof(IBaseJsonInstance))
            {
                if (tokens[index] is TNull)
                {
                    return null;
                }
                return ObjectAnalyzer<TVar>(tokens, ref index);
            }*/

            throw new UnityException("Unexpected value type: " + tokens[index].ToString());
        }

        
    }
}