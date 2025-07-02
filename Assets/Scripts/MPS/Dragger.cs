using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Dragger : MonoBehaviour
{
    Coroutine moveCoroutine;

    public void Move()
    {
        if (Conveyor.instance.direction == Conveyor.Direction.CW)
            moveCoroutine = StartCoroutine(CoMove());
        else
            moveCoroutine = StartCoroutine(CoMove());
        
    }

    private void OnDestroy()
    {
        StopCoroutine(moveCoroutine);
    }

    IEnumerator CoMove()
    {
        while (true)
        {
            if (Conveyor.instance.isCW)
            {
                Vector3 dir = Conveyor.instance.endPos.position - transform.position;
                Vector3 normalizedDir = dir.normalized;
                float distance = dir.magnitude;

                if (distance < 0.1f)
                {
                    transform.position = Conveyor.instance.startPos.position;
                    print("원래 위치로 이동");

                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).SetParent(null);
                    }
                }

                transform.position += normalizedDir * Conveyor.instance.speed * Time.deltaTime;
            }
            else if(Conveyor.instance.isCCW)
            {
                Vector3 dir = Conveyor.instance.startPos.position - transform.position;
                Vector3 normalizedDir = dir.normalized;
                float distance = dir.magnitude;

                if (distance < 0.01f)
                {
                    transform.position = Conveyor.instance.endPos.position;
                    print("원래 위치로 이동");

                    for(int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).SetParent(null);
                    }
                }

                transform.position += normalizedDir * Conveyor.instance.speed * Time.deltaTime;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("플라스틱") || other.tag.Contains("금속"))
            other.gameObject.transform.SetParent(this.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("플라스틱") || other.tag.Contains("금속"))
            other.gameObject.transform.SetParent(null);
    }
}
