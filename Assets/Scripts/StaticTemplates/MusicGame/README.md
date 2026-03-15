# 对于表数据结构文件`MealNotes.cs`的说明

`MealNotes.cs`的代码如下：
```csharp
public class MealNotes:IBaseJsonInstance//该接口为一切表数据结构类的基类
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
```

以下为对各字段的说明：
* `spritePath1`至`spritePath4`表示四个轨道音符图片的路径，目前阶段先不管；
* `track1`至`track4`的四个列表表示从左到右的四个轨道的音符情况，每个列表中各个音符生成间隔由BPM决定（BPM由音乐决定，故未体现在表中）。考虑到我们的音游可能有长键，音符情况以`int`表示：
  * `0`表示此处无音符
  * `1`表示此处有一个 tap 音符
  * 其他情况先不做

目前，本人已经制作了对于多个食材的铺面的测试用例生成器，其中音符的JSON用例位于[/Resources/Test](../../../Resources/Test)文件夹中用于检查正确性。你可以通过调用`JsonTest.GetMealNotes()`函数获得一个`List<MealNotes>`以读取，来检查读谱与生成是否正确。

请注意，尽管目前的用例都是完全正确的，但是在实际中，很可能会出现错误的情况。对于这样的数据结构，最常见的错误就是四个`List`各自含有的元素数量不同。你的读谱相关代码应当含有错误检查和处理的相关代码，请注意建议使用`C#`的`throw`语法处理错误。一定不要只写一个`Debug.Logerror()`输出错误。示例如下：

```csharp
if(/*检查发现四个List的元素数量不同*/){
    throw new UnityException("Numbers of notes in four lists is not equal!");
}
```