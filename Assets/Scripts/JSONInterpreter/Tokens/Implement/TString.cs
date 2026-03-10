using JSONInterpreter.Tokens.Interface;

namespace JSONInterpreter.Tokens.Implement
{
    public class TString: IToken
    {
        public TString(string s)
        {
            value=s;
        }
        public string value;
    }
}