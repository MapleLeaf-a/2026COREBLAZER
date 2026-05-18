using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 泛型栈
/// </summary>
/// <typeparam name="T">页面类型</typeparam>
public class PageStack<T>
{
    private Stack<T> stack = new Stack<T>();

    //定义事件(并不关心外部如何实现)
    public event System.Action<T> OnPagePushed; //压入页面
    public event System.Action<T> OnPagePopped; //弹出页面
    public event System.Action<T> OnPageActivated; //页面被激活(成为栈顶)
    public event System.Action<T> OnPageDeactivated; //页面被停用(不再是栈顶)

    //页面数量
    public int Count => stack.Count;
    
    //是否为空
    public bool IsEmpty => stack.Count == 0;

    //获取顶端页面
    public T CurrentPage => IsEmpty ? default : stack.Peek();

    public void Push(T newPage, bool isDeactivated = true)
    {
        if (CurrentPage != null && isDeactivated)
        {
            //如果事件不空,则执行停用栈顶页面
            OnPageDeactivated?.Invoke(CurrentPage);
        }

        stack.Push(newPage);
        //执行其余可能的压入页面附带需求
        OnPagePushed?.Invoke(newPage);
        //激活压入的页面
        OnPageActivated?.Invoke(newPage);

        Debug.Log($"push: {newPage}, Count = {Count}");
    }

    //弹出当前页面
    public T Pop()
    { 
        if (IsEmpty) return default;

        T page = stack.Pop();
        //执行其他可能需要的弹出页面需求
        OnPagePopped?.Invoke(page);
        //停用弹出的页面
        OnPageDeactivated?.Invoke(page);

        //显示上一个页面
        if (CurrentPage != null)
        {
            OnPageActivated?.Invoke(CurrentPage);
        }

        Debug.Log($"Pop: {page}, Count = {Count}");
        return page;
    }

    //清空所有页面
    public void Clear()
    {
        while (!IsEmpty)
        { 
            T page = stack.Pop();
            OnPagePopped?.Invoke(page);
            OnPageDeactivated?.Invoke(page);
            //Object.Destroy(page as Object); //?
        }
    }

    //清空并压入新页面（替换所有）
    public void ReplaceAll(T page)
    {
        Clear();
        Push(page);
    }

    //弹出直到遇到指定的页面(包括)
    public void PopTo(T page)
    {
        while (!IsEmpty && !EqualityComparer<T>.Default.Equals(CurrentPage, page)) //CurrentPage != page是不允许的,因为不能确定使用泛型栈的人定义的T是值类型还是引用类型
        {
            Pop();
        }
        if (!IsEmpty) Pop();
    }

    //弹出到指定的index
    public T PopToIndex(int index)
    { 
        if (index < 0 || index >= stack.Count) return default;

        while (index + 1 != stack.Count)
        {
            Pop();
        }

        return CurrentPage;
    }

    public List<T> GetAllPages()
    {
        return new List<T>(stack);
    }
}
