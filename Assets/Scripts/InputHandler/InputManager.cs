using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    //单例
    public static InputManager InputManagerInstance;

    //输入上下文
    public enum InputContext
    { 
        MUSICGAME, //音游部分
        CHARACTER, //角色操控部分

    }

    //当前的上下文
    public InputContext currenContext = InputContext.CHARACTER;

    //动作名->按键的映射
    private Dictionary<string, KeyCode> keyMapping = new Dictionary<string, KeyCode>();

    //动作名属于哪个上下文
    private Dictionary<string, InputContext> contextMapping = new Dictionary<string, InputContext>();

    void Awake()
    {
        if (InputManagerInstance == null)
        {
            InputManagerInstance = this;
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
        //音游轨道对应按键
        AddMapping("bar1", KeyCode.A, InputContext.MUSICGAME);
        AddMapping("bar2", KeyCode.D, InputContext.MUSICGAME);
        AddMapping("bar3", KeyCode.J, InputContext.MUSICGAME);
        AddMapping("bar4", KeyCode.L, InputContext.MUSICGAME);

        //角色操控对应按键
        AddMapping("MoveUp", KeyCode.W, InputContext.CHARACTER);
        AddMapping("MoveDown", KeyCode.S, InputContext.CHARACTER);
        AddMapping("MoveLeft", KeyCode.A, InputContext.CHARACTER);
        AddMapping("MoveRight", KeyCode.D, InputContext.CHARACTER);

        //交互对应按键
        AddMapping("InteractF", KeyCode.F, InputContext.CHARACTER);
    }

    void AddMapping(string actionName, KeyCode key, InputContext? inputContext) //可空值类型inputContext
    {
        keyMapping[actionName] = key;
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
    public bool GetKeyDown(string actionName)
    { 
        //若当前上下文不可以此action
        if (!IsActionAvailable(actionName)) return false;

        if (keyMapping.ContainsKey(actionName))
        {
            return Input.GetKeyDown(keyMapping[actionName]);
        }
        return false;
    }

    //判断actionName对应按键是否持续按下
    public bool GetKey(string action)
    {
        if (!IsActionAvailable(action)) return false;

        if (keyMapping.ContainsKey(action))
        {
            return Input.GetKey(keyMapping[action]);
        }
        return false;
    }

    //判断actionName对应按键是否抬起
    public bool GetKeyUp(string action)
    {
        if (!IsActionAvailable(action)) return false;

        if (keyMapping.ContainsKey(action))
        {
            return Input.GetKeyUp(keyMapping[action]);
        }
        return false;
    }

    //运行时修改映射(保存到PlayerPrefs)
    public void RemapKey(string action, KeyCode newKey)
    {
        if (keyMapping.ContainsKey(action))
        {
            keyMapping[action] = newKey;
            PlayerPrefs.SetInt($"Key_{action}", (int)newKey);
            PlayerPrefs.Save();
        }
    }

    public void LoadSavedMappings()
    {
        foreach (var action in new List<string>(keyMapping.Keys))
        {
            if (PlayerPrefs.HasKey($"Key_{action}"))
            {
                KeyCode savedKey = (KeyCode)PlayerPrefs.GetInt($"Key_{action}");
                keyMapping[action] = savedKey;
            }
        }
    }
}
