using System.Collections;
using UnityEngine;

// �Ǹ��� Rod�� minRange��ŭ ����, maxRange��ŭ ����
// �Ӽ�: �Ǹ��� Rod�� transform, minRange Pos, maxRange Pos, �ӵ�
public class Cylinder : MonoBehaviour
{
    public enum SolenoidType
    {
        �ܹ���ַ����̵�,
        �����ַ����̵�
    }
    SolenoidType type = SolenoidType.�����ַ����̵�;

    public Transform cylinderRod;
    public float speed; // ���й�� ����
    public float minPosY;
    public float maxPosY;
    public bool isForward;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            Vector3 back = new Vector3(0, minPosY, 0);
            Vector3 front = new Vector3(0, maxPosY, 0);
            StartCoroutine(MoveCylinder(back, front, isForward));
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Vector3 back = new Vector3(0, minPosY, 0);
            Vector3 front = new Vector3(0, maxPosY, 0);
            StartCoroutine(MoveCylinder(front, back, !isForward));
        }
    }

    IEnumerator MoveCylinder(Vector3 from, Vector3 to, bool isForward)
    {
        Vector3 direction;

        while (true)
        {
            if (isForward)
                direction = to - cylinderRod.position;
            else
                direction = from - cylinderRod.position;

            Vector3 normalizedDir = direction.normalized;
            float distance = direction.magnitude;

            if(distance < 0.1f)
            {
                cylinderRod.position = to;
                break;
            }

            cylinderRod.position += normalizedDir * speed * Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }   
    }
}
