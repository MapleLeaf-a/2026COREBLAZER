using JSONInterpreter.Interface;
using StaticTemplates.Food;

namespace StaticTemplates.MusicGame
{
    public class Material:IBaseJsonInstance
    {
        public string id;
        public string name;
        public string spritePath;
        public string description;
        public MaterialType type;
    }
}
