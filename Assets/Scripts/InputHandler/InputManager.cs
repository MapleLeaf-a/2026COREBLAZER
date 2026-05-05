using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    //单例
    public static InputManager instance;

    //当前的上下文
    public InputContext currenContext = InputContext.CHARACTER;

    //动作名->按键的映射
    private Dictionary<string, List<KeyCode>> keyMapping = new Dictionary<string, List<KeyCode>>();

    //动作名属于哪个上下文
    private Dictionary<string, InputContext> contextMapping = new Dictionary<string, InputContext>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitMappings();   
    }

    //初始化默认映射
    void InitMappings()
    {
        //音游轨道(单轨道)对应按键
        AddMapping("Judge", KeyCode.Space, InputContext.MUSICGAME);

        //多轨道(4轨道)
        AddMapping("JudgeTrack0", KeyCode.A, InputContext.MUSICGAME);
        AddMapping("JudgeTrack1", KeyCode.D, InputContext.MUSICGAME);
        AddMapping("JudgeTrack2", KeyCode.J, InputContext.MUSICGAME);
        AddMapping("JudgeTrack3", KeyCode.L, InputContext.MUSICGAME);

        //角色操控对应按键
        AddMapping("MoveUp", KeyCode.W, InputContext.CHARACTER);
        AddMapping("MoveDown", KeyCode.S, InputContext.CHARACTER);
        AddMapping("MoveLeft", KeyCode.A, InputContext.CHARACTER);
        AddMapping("MoveRight", KeyCode.D, InputContext.CHARACTER);
        AddMapping("MoveUp", KeyCode.UpArrow, InputContext.CHARACTER);
        AddMapping("MoveDown", KeyCode.DownArrow, InputContext.CHARACTER);
        AddMapping("MoveLeft", KeyCode.LeftArrow, InputContext.CHARACTER);
        AddMapping("MoveRight", KeyCode.RightArrow, InputContext.CHARACTER);

        //交互对应按键
        AddMapping("InteractF", KeyCode.F, InputContext.CHARACTER);

        //挖掘对应按键
        AddMapping("Dig", KeyCode.Mouse0, InputContext.DIGGAME);
        AddMapping("Detect", KeyCode.Mouse1, InputContext.DIGGAME);
    }
    
    /// <summary>
    /// 供轨道判定调用,根据轨道数量自动选择判定方式
    /// </summary>
    /// <param name="trackCount">轨道数量</param>
    /// <param name="trackIndex">轨道索引（0-based）</param>
    public bool GetJudgeKeyDown_MusicGame(int trackCount, int trackIndex)
    {
        if (trackCount == 1)
        {
            //单轨道:使用"Judge"动作
            return GetKeyDown("Judge");
        }
        else
        {
            //多轨道:使用"JudgeTrack{index}"动作
            string actionName = $"JudgeTrack{trackIndex}";
            return GetKeyDown(actionName);
        }
    }

    void AddMapping(string actionName, KeyCode key, InputContext? inputContext) //可空值类型inputContext
    {
        if (keyMapping.ContainsKey(actionName))
        {
            keyMapping[actionName].Add(key);
        }
        else
        {
            keyMapping[actionName] = new List<KeyCode> { key };
        }

        if (inputContext.HasValue) //如果不为null
        {
            contextMapping[actionName] = inputContext.Value;
        }
        else
        {
            //null表示全局可用,不加入context映射
        }
    }

    //在当前上下文中action是否可用
    public bool IsActionAvailable(string actionName)
    {
        if (!contextMapping.ContainsKey(actionName)) //若未指定上下文,默认为全局可用
        {
            return true;
        }

        return contextMapping[actionName] == currenContext;    
    }

    //设置上下文
    public void SetContext(InputContext newContext)
    {
        currenContext = newContext;
        Debug.Log("切换上下文到" + newContext.ToString());
    }

    //判断actionName对应按键是否按下
    public bool GetKeyDown(string actionName) => ProcessKey(actionName, Input.GetKeyDown);

    //判断actionName对应按键是否持续按下
    public bool GetKey(string actionName) => ProcessKey(actionName, Input.GetKey);

    //判断actionName对应按键是否抬起
    public bool GetKeyUp(string actionName) => ProcessKey(actionName, Input.GetKeyUp);

    private bool ProcessKey(string action, System.Func<KeyCode, bool> keyCheck) //定义委托,处理不同的情况对应的按键检查,简化上面几个方法的逻辑
    {
        if (!IsActionAvailable(action)) return false;

        if (keyMapping.ContainsKey(action))
        {
            bool res = false;
            foreach (var keyCodes in keyMapping[action])
            {
                res |= keyCheck(keyCodes); //使用委托判断
            }
            return res;
        }
        return false;
    }


    ////运行时修改映射(保存到PlayerPrefs)
    //public void RemapKey(string action, KeyCode newKey)
    //{
    //    if (keyMapping.ContainsKey(action))
    //    {
    //        keyMapping[action] = newKey;
    //        PlayerPrefs.SetInt($"Key_{action}", (int)newKey);
    //        PlayerPrefs.Save();
    //    }
    //}

    //public void LoadSavedMappings()
    //{
    //    foreach (var action in new List<string>(keyMapping.Keys))
    //    {
    //        if (PlayerPrefs.HasKey($"Key_{action}"))
    //        {
    //            KeyCode savedKey = (KeyCode)PlayerPrefs.GetInt($"Key_{action}");
    //            keyMapping[action] = savedKey;
    //        }
    //    }
    //}
}
