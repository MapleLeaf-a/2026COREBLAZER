using System.Buffers.Text;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Note : MonoBehaviour
{
    public float speed;
    
    //所在轨道的index
    public int barIndex;

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime, Space.World);

        IsOutOfScreen();
    }

    void IsOutOfScreen()
    {
        if (transform.position.y < BarJudger.BarJudgerInstance.barList[0].position.y - BarJudger.BarJudgerInstance.miss * speed)
        {
            Debug.Log("Totally MISS!");
            DestroyNote();
        }
    }

    public void JudgeTime()
    {
        Transform bar = BarJudger.BarJudgerInstance.barList[barIndex];
        float baseY = bar.position.y;

        float y = this.transform.position.y;

        if (baseY + this.speed * BarJudger.BarJudgerInstance.miss < y)  //还未到判定区
        {
            return;
        }

        //判定区内的时机判定
        if (baseY - speed * BarJudger.BarJudgerInstance.perfect < y && y < baseY + speed * BarJudger.BarJudgerInstance.perfect)
        {
            Debug.Log("Perfect!");
        }
        else if (baseY - speed * BarJudger.BarJudgerInstance.good < y && y < baseY + speed * BarJudger.BarJudgerInstance.good)
        {
            Debug.Log("Good!");
        }
        else if (baseY - speed * BarJudger.BarJudgerInstance.soso < y && y < baseY + speed * BarJudger.BarJudgerInstance.soso)
        {
            Debug.Log("So-so!");
        }
        else if (baseY - speed * BarJudger.BarJudgerInstance.miss < y && y < baseY + speed * BarJudger.BarJudgerInstance.miss)
        {
            Debug.Log("Miss!");
        }
        DestroyNote();
    }

    private void DestroyNote()
    {
        Destroy(gameObject);
        BarJudger.BarJudgerInstance.noteList[barIndex].RemoveAt(0); //出队
    }
}