using System.Collections.Generic;
using System.ComponentModel;
using JSONInterpreter.Tokens.Implement;
using JSONInterpreter.Tokens.Interface;
using UnityEngine;

namespace JSONInterpreter
{
    public static class TokenConverter
    {
        public static List<IToken> GetToken(string json)
        {
            List<IToken> tokens = new List<IToken>();
            for(int i=0;i<json.Length;i++)
            {
                switch (json[i])
                {
                    case '{':
                        tokens.Add(new TLeftBrace());
                        break;
                    case '}':
                        tokens.Add(new TRightBrace());
                        break;
                    case '[':
                        tokens.Add(new TLeftBracket());
                        break;
                    case ']':
                        tokens.Add(new TRightBracket());
                        break;
                    case ',':
                        tokens.Add(new TComma());
                        break;
                    case ':':
                        tokens.Add(new TColon());
                        break;
                    case '"':
                        tokens.Add(StringConverter(json, ref i));
                        break;
                    default:
                        if (json[i] == ' '||json[i]=='\n'||json[i]=='\t'||json[i]=='\r')
                        {
                            continue;
                        }

                        if (json[i] == '-' || (json[i] >= '0' && json[i] <= '9'))
                        {
                            tokens.Add(NumberConverter(json, ref i));
                            continue;
                        }

                        if (json[i] == 't' && json.Substring(i, 4) == "true")
                        {
                            tokens.Add(new TBool(true));
                            continue;
                        }

                        if (json[i] == 'f' && json.Substring(i, 5) == "false")
                        {
                            tokens.Add(new TBool(false));
                            continue;
                        }

                        if (json[i] == 'n' && json.Substring(i, 4) == "null")
                        {
                            tokens.Add(new TNull());
                        }
                        
                        throw new UnityException("Unexpected character '" + json[i]+"' ! Token conversion failed!");
                }
            }
            return tokens;
        }

        private static TString StringConverter(string json, ref int index)
        {
            string str="";
            while (json[++index] != '"')
            {
                if (json[index] == '\\')
                {
                    switch (json[++index])
                    {
                        case '"':
                            str += '"';
                            break;
                        case '\\':
                            str += '\\';
                            break;
                        case '/':
                            str += '/';
                            break;
                        case 'b':
                            str += '\b';
                            break;
                        case 'f':
                            str += '\f';
                            break;
                        case 'n':
                            str += '\n';
                            break;
                        case 't':
                            str += '\t';
                            break;
                        case 'r':
                            str += '\r';
                            break;
                        case 'u':
                            char c=(char)0;
                            for (int m = 1; m <= 4; m++)
                            {
                                index++;
                                if (json[index] >= '0' && json[index] <= '9')
                                {
                                    c+=(char)((json[index]-'0')<<((4-m)*2));
                                }
                                else if (json[index] >= 'A' && json[index] <= 'F')
                                {
                                    c+=(char)((json[index]+10-'A')<<((4-m)*2));
                                }
                                else if (json[index] >= 'a' && json[index] <= 'f')
                                {
                                    c+=(char)((json[index]+10-'a')<<((4-m)*2));
                                }
                                else
                                {
                                    throw new UnityException("Illegal hex number! Token conversion failed!");
                                }
                            }
                            str += c;
                            break;
                        default:
                            throw new UnityException("Character '\\' continue with an unexcepted character"+json[index]+"! Token conversion failed!");
                    }
                }
                str += json[index];
                if (json[index] == '\n' || json[index] == '\t' || json[index] == '\r' || json[index] == '\b' ||
                    json[index] == '\f')
                {
                    throw new UnityException("The string has an illegal character. Token conversion failed!");
                }
                if (index + 1 == json.Length)
                {
                    throw new UnityException("The string doesn't have an end character. Token conversion failed!");
                }
            }
            return new TString(str);
        }

        private static IToken NumberConverter(string json, ref int index)
        {
            string numStr="";
            bool isFloat = false;
            while ((json[++index] >= '0' && json[index] <= '9') || json[index] == '.')
            {
                numStr+=json[index];
                if (json[index] == '.')
                {
                    if (!isFloat)
                    {
                        isFloat = true;
                    }
                    else
                    {
                        throw new UnityException("A number shouldn't have two or more points! Token conversion failed!");
                    }
                }
            }
            if (isFloat)
            {
                return new TFloat(float.Parse(numStr));
            }
            return new TInt(int.Parse(numStr));
        }
    }
}