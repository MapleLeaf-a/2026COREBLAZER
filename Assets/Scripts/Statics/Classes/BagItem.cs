using StaticTemplates.MusicGame;
using StaticTemplates.Food;

namespace Statics.Classes
{
    public class BagItem
    {
        public FoodMaterial material;
        public int num;
        
        public BagItem(FoodMaterial _material,int _num){
            material=_material;
            num=_num;
        }
        
        public void IncreaseNum(int incr){
            num+=incr;
        } 

        public void DecreaseNum(int decr){
            num-=decr;
            if(num<0){
                throw new UnityEngine.UnityException("The number of bag item now less than 0.");
            }
        }

        public string ID => material.id;
        public string Name => material.name;
        public string SpritePath => material.spritePath;
        public string Description => material.description;
        public MaterialType MaterialType => material.type;
    }
}
