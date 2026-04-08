using JSONInterpreter.Interface;
using StaticTemplates.Food;

namespace StaticTemplates.MusicGame
{
    public class FoodMaterial:IBaseJsonInstance
    {
        public string id;
        public string name;
        public string spritePath;
        public string description;
        public MaterialType type;

        public FoodMaterial(string id, string name, string spritePath, string description, MaterialType type)
        { 
            this.id = id;
            this.name = name;   
            this.spritePath = spritePath;
            this.description = description;
            this.type = type;
        }
    }
}
