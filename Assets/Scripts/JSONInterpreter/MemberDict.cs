using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace JSONInterpreter
{
    public static class MemberDict
    {
        private static Dictionary<Type, Dictionary<string, MemberInfo>> _typeDict =
            new Dictionary<Type, Dictionary<string, MemberInfo>>(); 
        
        public static Dictionary<string, MemberInfo> GetMemberDict(Type classType)
        {
            if (_typeDict.TryGetValue(classType, out var dict))
            {
                return dict;
            }
            
            MemberInfo[] memberInfos=classType.GetMembers();
            Dictionary<string, MemberInfo> memberInfoDict = new Dictionary<string, MemberInfo>();
            foreach (var member in memberInfos)
            {

                if (member is FieldInfo or PropertyInfo)
                {
                    memberInfoDict.Add(member.Name, member);
                }
            }
            _typeDict[classType] = memberInfoDict;
            return memberInfoDict;
        }
    }
}