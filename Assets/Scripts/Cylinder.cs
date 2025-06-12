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

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            Vector3 back = new Vector3(0, minPosY, 0);
            Vector3 front = new Vector3(0, maxPosY, 0);
            StartCoroutine(MoveCylinder(back, front, true));
        }
    }

    IEnumerator MoveCylinder(Vector3 from, Vector3 to, bool isForward)
    {
        if(isForward)
        {
            yield return null;
        }
        else
        {

        }
    }
}
