using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    void OnEnable()
    {
        //进入角色操纵模式时,切换到角色上下文
        InputManager.InputManagerInstance.SetContext(InputManager.InputContext.CHARACTER);
    }

    //角色移动速度
    float moveSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.InputManagerInstance.GetKey("MoveUp"))
        {
            transform.Translate(moveSpeed * Time.deltaTime * Vector2.up);
        }
        if (InputManager.InputManagerInstance.GetKey("MoveDown"))
        {
            transform.Translate(moveSpeed * Time.deltaTime * Vector2.down);
        }
        if (InputManager.InputManagerInstance.GetKey("MoveLeft"))
        {
            transform.Translate(moveSpeed * Time.deltaTime * Vector2.left);
        }
        if (InputManager.InputManagerInstance.GetKey("MoveRight"))
        {
            transform.Translate(moveSpeed * Time.deltaTime * Vector2.right);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("collide");
        if (other.gameObject.name == "Inputter")
        {
            other.gameObject.GetComponent<NumberInputReader>().StartNumberInput();
        }
    }
}
