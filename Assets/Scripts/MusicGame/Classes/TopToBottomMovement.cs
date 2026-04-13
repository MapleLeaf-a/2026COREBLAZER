using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopToBottomMovement : IMovementStrategy
{
    public Vector3 GetMoveDirV3() => Vector3.down;

    public float GetPositionOnAxis(Transform transform) => transform.position.y;
}
