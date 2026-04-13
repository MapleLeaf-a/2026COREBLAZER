using UnityEngine;

/// <summary>
/// 使用策略模式,这个是移动策略的接口
/// </summary>
public interface IMovementStrategy
{
    /// <summary>
    /// 获取音符移动的方向
    /// </summary>
    /// <returns></returns>
    public Vector3 GetMoveDirV3();

    /// <summary>
    /// 获取对应方向的轴向的值(x/y)
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public float GetPositionOnAxis(Transform transform); 

    
}
