using System.Collections;
using UnityEngine;

public class Dragger : MonoBehaviour
{
    public void Move()
    {
        if (Conveyor.instance.direction == Conveyor.Direction.CW)
            StartCoroutine(Move(true));
        else
            StartCoroutine(Move(false));
    }

    IEnumerator Move(bool isCW)
    {
        while (Conveyor.instance.isOn)
        {
            if (isCW)
            {
                Vector3 dir = Conveyor.instance.endPos.position - transform.position;
                Vector3 normalizedDir = dir.normalized;
                float distance = dir.magnitude;

                if (distance < 0.1f)
                {
                    transform.position = Conveyor.instance.startPos.position;
                    print("���� ��ġ�� �̵�");

                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).SetParent(null);
                    }
                }

                transform.position += normalizedDir * Conveyor.instance.speed * Time.deltaTime;
            }
            else if(!isCW)
            {
                Vector3 dir = Conveyor.instance.startPos.position - transform.position;
                Vector3 normalizedDir = dir.normalized;
                float distance = dir.magnitude;

                if (distance < 0.1f)
                {
                    transform.position = Conveyor.instance.endPos.position;
                    print("���� ��ġ�� �̵�");

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
        if (other.tag.Contains("�ö�ƽ") || other.tag.Contains("�ݼ�"))
            other.gameObject.transform.SetParent(this.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("�ö�ƽ") || other.tag.Contains("�ݼ�"))
            other.gameObject.transform.SetParent(null);
    }
}
