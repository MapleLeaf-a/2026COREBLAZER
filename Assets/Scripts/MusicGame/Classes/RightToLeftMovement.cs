using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//×ÔÓÒÏò×óÒÆ¶¯
public class RightToLeftMovement : IMovementStrategy
{
    public Vector3 GetMoveDirV3() => Vector3.left;

    public float GetPositionOnAxis(Transform transform) => transform.position.x;


}
