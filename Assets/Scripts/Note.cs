using System.Buffers.Text;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Note : MonoBehaviour
{
    public float speed;

    void Start()
    {

    }

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime, Space.World);

        IsOutOfScreen();
    }

    bool IsOutOfScreen()
    {
        if (transform.position.y < BarJudger.BarJudgerInstance.barList[0].position.y - BarJudger.BarJudgerInstance.miss * speed)
        {
            Debug.Log("Totally MISS!");
            speed = 0f;
            return true;
        }

        return false;
    }
}