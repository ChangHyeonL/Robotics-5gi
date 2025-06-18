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
    public Renderer frontLimitSW;
    public Renderer backLimitSW;
    public Color originSWColor;
    public float speed; // ���й�� ����
    public float minPosY;
    public float maxPosY;
    public bool isForward; // ���� �ַ����̵� ��ȣ
    public bool isBackward; // �Ĺ� �ַ����̵� ��ȣ
    public bool isMoving; // ���� �����̰� �ִ��� ����
    public bool isFrontLimitSWON;
    public bool isBackLimitSWON;

    private void Start()
    {
        originSWColor = frontLimitSW.material.color;
        backLimitSW.material.SetColor("_BaseColor", Color.green);

        StartCoroutine(MoveForwardBySignal());
        StartCoroutine(MoveBackwardBySignal());
    }

    // �ݺ������� isForward, isBackward Ȯ��, ���� �������� ������, �����δ�.
    // isForward & isBackward�� 1�� �Ǿ��� ��, �����δ�.
    IEnumerator MoveForwardBySignal()
    {
        while(true)
        {
            yield return new WaitUntil(() => isForward == true && !isMoving);

            isMoving = true;

            Vector3 back = new Vector3(0, minPosY, 0);
            Vector3 front = new Vector3(0, maxPosY, 0);
            StartCoroutine(MoveCylinder(back, front));

            ChangeSWColor(backLimitSW, originSWColor);
        }
    }

    IEnumerator MoveBackwardBySignal()
    {
        while (true)
        {
            yield return new WaitUntil(() => isBackward == true && !isMoving);

            isMoving = true;

            Vector3 back = new Vector3(0, minPosY, 0);
            Vector3 front = new Vector3(0, maxPosY, 0);
            StartCoroutine(MoveCylinder(front, back));

            ChangeSWColor(frontLimitSW, originSWColor);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if(isFrontLimitSWON)
        {
            ChangeSWColor(frontLimitSW, Color.green);
        }
        else if(!isFrontLimitSWON)
        {
            ChangeSWColor(frontLimitSW, Color.black);
        }

        if (isBackLimitSWON)
        {
            ChangeSWColor(backLimitSW, Color.green);
        }
        else if (!isBackLimitSWON)
        {
            ChangeSWColor(backLimitSW, Color.black);
        }
    }

    public void MoveCylinderForward()
    {
        Vector3 back = new Vector3(0, minPosY, 0);
        Vector3 front = new Vector3(0, maxPosY, 0);
        StartCoroutine(MoveCylinder(back, front));
    }

    public void MoveCylinderBackward()
    {
        Vector3 back = new Vector3(0, minPosY, 0);
        Vector3 front = new Vector3(0, maxPosY, 0);
        StartCoroutine(MoveCylinder(front, back));
    }

    IEnumerator MoveCylinder(Vector3 from, Vector3 to)
    {
        Vector3 direction = Vector3.one;

        while (true)
        {
            direction = to - cylinderRod.localPosition;

            Vector3 normalizedDir = direction.normalized;
            float distance = direction.magnitude;

            if(distance < 0.1f)
            {
                isMoving = false;

                break;
            }

            cylinderRod.localPosition += normalizedDir * speed * Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }   
    }

    public void ChangeSWColor(Renderer sw, Color color)
    {
        sw.material.SetColor("_BaseColor", color);
    }
}
