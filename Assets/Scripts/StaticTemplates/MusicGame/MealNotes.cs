using System.Collections.Generic;
using JSONInterpreter.Interface;

namespace StaticTemplates.MusicGame
{
    public class MealNotes:IBaseJsonInstance
    {
        public string spritePath1;
        public string spritePath2;
        public string spritePath3;
        public string spritePath4;
        
        public List<int> track1;
        public List<int> track2;
        public List<int> track3;
        public List<int> track4;
    }
}