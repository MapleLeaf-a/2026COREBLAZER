using JSONInterpreter.Interface;
using StaticTemplates.Food;

namespace StaticTemplates.MusicGame
{
    public class FoodMaterial:IBaseJsonInstance
    {
        public int price;
        public string id;
        public string name;
        public string spritePath;
        public string description;
        public MaterialType type;

        public FoodMaterial(string id, int price, string name, string spritePath, string description, MaterialType type)
        { 
            this.price = price;
            this.id = id;
            this.name = name;   
            this.spritePath = spritePath;
            this.description = description;
            this.type = type;
        }
    }
}
