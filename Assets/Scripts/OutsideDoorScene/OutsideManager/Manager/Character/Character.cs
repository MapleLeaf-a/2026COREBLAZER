using UnityEngine;

public class Character : MonoBehaviour
{
    public Rigidbody2D rb;

    //移动速度
    float moveSpeed = 7f;

    void Awake()
    {
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void OnEnable()
    {
        //进入角色操纵模式时,切换到角色上下文
        InputManager.instance.SetContext(InputContext.CHARACTER);
    }

    void FixedUpdate()
    {
        //在FixedUpdate中使用物理移动
        Vector2 moveDirection = Vector2.zero;

        if (InputManager.instance.GetKey("MoveUp"))
        {
            moveDirection += Vector2.up;
        }
        if (InputManager.instance.GetKey("MoveDown"))
        {
            moveDirection += Vector2.down;
        }
        if (InputManager.instance.GetKey("MoveLeft"))
        {
            moveDirection += Vector2.left;
        }
        if (InputManager.instance.GetKey("MoveRight"))
        {
            moveDirection += Vector2.right;
        }

        Vector2 newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }
}