using UnityEngine;

namespace Statics.Classes
{
    public class BagItem
    {
        public Material material;
        public int num;
        
        public BagItem(Material _material,int _num){
            material=_material;
            num=_num;
        }
        
        public void IncreaseNum(int incr){
            num+=incr;
        } 

        public void DecreaseNum(int decr){
            num-=decr;
            if(num<0){
                throw new UnityException("The number of bag item now less than 0.");
            }
        } 
    }
}
