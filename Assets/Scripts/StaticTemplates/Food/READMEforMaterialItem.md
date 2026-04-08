# 背包数据类文件说明

食材数据类[`Material.cs`](./Material.cs)以及需要的枚举[`MaterialType.cs`](./MaterialType.cs)已经给出如下：
```csharp
    public enum MaterialType{
        Seafood,Vegetable,Drink,Meat//品类并未列完
    }

    public class Material:IBaseJsonInstance
    {
        public string id;
        public string name;
        public string spritePath;
        public string description;
        public MaterialType type;
    }
```

现对各个字段予说明：
* `id`是一个极其重要的字段，也就是这个品种食材的编号，你可以理解为一个人的身份证号码。需要这个字段的最重要原因，是我们在程序中时常需要去从多个物品中搜索一个想要的物品，在之后食材组合为菜品也需要对食材进行查找，而如果通过其编号查找就极其方便。
  * 这里你也可以思考为什么数据类型为`string`，用`int`又行不行？
* `name`是食材的中文名称；
* `spritePath`是食材图片的位置；
* `description`是食材的描述文本；
* `type`是食材种类的枚举。
----

你可能已经发现，对于一个可以折叠物品（即在背包中一个格子可以装多个同一种物品）的背包，物品的数量也是一个很重要的数据，但在这个类并没有其数量的字段，主要有两点考量：
1. 食材的数量并不是策划需要配置的数据；
2. 食材的数据类并非只有这里要用，比如在游戏大地图生成食材，在音游部分引用相关信息，那么这一个字段对于这些需求也是多余的。

但是，当我们需要食材的数量这一个数据的时候，可以怎么办呢？请看背包物品类[`Scripts/Statics/Classes/BagItem.cs`](../../Statics/Classes/BagItem.cs)：

```csharp
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
```

你可以看到，在这个类把`Material`类和数量`num`封装到了一起，并提供了一些工具函数。但是在数据层提供工具函数并不是很好的实践，之后会予说明。

----
引用背包物品类的全局数据类[`Scripts/Statics/GameStatics.cs`](../../Statics/GameStatics.cs)如下：

```csharp
    public static class GameStatics
    {
        public static BagItem[] FridgeBag=new BagItem[16];
        public static BagItem[] Bag=new BagItem[16];
    }
```

这个类就包含了在游戏生命周期中需要存储的背包数据。

不过，你也会发现，若需要实现游戏中对背包管理的一些需求（如：两个背包的物品转移，物品搜索，物品增减），仅靠C#的`Array`的自带功能显然是不够的。你可能会想到编写一些工具函数进行管理，但是问题就是这些函数应该放在哪里。

事实上，将这一部分的代码和数据层放在一起或者和UI逻辑放在一起都可能带来耦合的问题，也会导致后期需要增减功能时难以维护，所以不妨把访问数据的接口独立出来，做成一个不继承自`MonoBehaviour`的一个类：

```csharp
public class BackpackManager
{
    public void AddItem(Material material)
    {
        //增加物品的逻辑
    }
    ...//其他数据处理逻辑
}
```

而在UI行为的代码中，又将这些类实例化供UI功能使用：

```csharp
public class BagUI:MonoBehavoiur
{
    private BackpackManager manager=new BackpackManager();
    
    ...
}
```

这样的架构可以认为是一种MVC架构的变体MVVM架构：
* M：Model，指数据。既可以是游戏运行时的全局数据，也可以是JSON存储的数据；
* VM：ViewModel，这一层主要负责对数据的处理并提供对应的接口给UI层；
* V：View，指UI层。这一层只专心处理UI自己的逻辑，包括：用户的输入、UI的显示等等，并将该层数据发给ViewModel层。
* C: Controller, 协调 Model 和 View

*P.S. MVC架构及其变体是一种极其重要的设计模式，甚至连一些大厂的大项目都用的这一类架构哟^^*

---

这次JSON相关的脚本和对应的代码未准备好，可能需要你自己写一些测试类来测试。