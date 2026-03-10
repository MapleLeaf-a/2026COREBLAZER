using JSONInterpreter.Tokens.Interface;

namespace JSONInterpreter.Tokens.Implement
{
    public class TInt:IToken
    {
        public TInt(int i)
        {
             value=i;
        }
        public int value;
    }
}