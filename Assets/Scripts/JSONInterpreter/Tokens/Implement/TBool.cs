using JSONInterpreter.Tokens.Interface;

namespace JSONInterpreter.Tokens.Implement
{
    public class TBool:IToken
    {
        public TBool(bool b)
        {
            value=b;
        }
        public bool value;
    }
}