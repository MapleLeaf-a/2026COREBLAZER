using JSONInterpreter.Tokens.Interface;

namespace JSONInterpreter.Tokens.Implement
{
    public class TFloat:IToken
    {
        public TFloat(float f)
        {
            value = f;
        }
        public float value;
    }
}