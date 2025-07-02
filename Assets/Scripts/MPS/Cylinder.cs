using System.Collections;
using UnityEngine;

// 실린더 Rod를 minRange만큼 후진, maxRange만큼 전진
// 속성: 실린더 Rod의 transform, minRange Pos, maxRange Pos, 속도
public class Cylinder : MonoBehaviour
{
    public enum SolenoidType
    {
        단방향솔레노이드,
        양방향솔레노이드
    }
    SolenoidType type = SolenoidType.양방향솔레노이드;

    public Transform cylinderRod;
    public Renderer frontLimitSW;
    public Renderer backLimitSW;
    public Color originSWColor;
    public float speed; // 공압밸브 조절
    public float minPosY;
    public float maxPosY;
    public bool isForward;  // 전방 솔레노이드 신호
    public bool isBackward; // 후방 솔레노이드 신호
    public bool isMoving;   // 현재 움직이고 있는지 여부
    public bool isFrontLimitSWON;
    public bool isBackLimitSWON;
    public bool isFrontEnd;

    private void Start()
    {
        originSWColor = frontLimitSW.material.color;
        backLimitSW.material.SetColor("_BaseColor", Color.green);

        StartCoroutine(MoveForwardBySignal());
        StartCoroutine(MoveBackwardBySignal());
    }

    // 반복적으로 isForward, isBackward 확인, 현재 움직이지 않을때, 움직인다.
    // isForward & isBackward가 1이 되었을 때, 움직인다.
    IEnumerator MoveForwardBySignal()
    {
        while(true)
        {
            // 실린더 전진신호가 들어오고, 후진신호는 들어오지 않을 때 작동! + 움직이지 않을때
            // Forward로 나갔을 때, Backward 신호만 받아야함.
            yield return new WaitUntil(() => isForward && !isBackward && !isMoving && !isFrontEnd);

            isMoving = true;
            print("전진중");

            Vector3 back = new Vector3(0, minPosY, 0);
            Vector3 front = new Vector3(0, maxPosY, 0);
            yield return MoveCylinder(back, front, isFrontEnd);

            ChangeSWColor(backLimitSW, originSWColor);
        }
    }

    IEnumerator MoveBackwardBySignal()
    {
        while (true)
        {
            yield return new WaitUntil(() => isBackward && !isForward && !isMoving && isFrontEnd);

            isMoving = true;
            print("후진중");

            Vector3 back = new Vector3(0, minPosY, 0);
            Vector3 front = new Vector3(0, maxPosY, 0);
            yield return MoveCylinder(front, back, isFrontEnd);

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
        StartCoroutine(MoveCylinder(back, front, !isFrontEnd));
    }

    public void MoveCylinderBackward()
    {
        Vector3 back = new Vector3(0, minPosY, 0);
        Vector3 front = new Vector3(0, maxPosY, 0);
        StartCoroutine(MoveCylinder(front, back, isFrontEnd));
    }

    IEnumerator MoveCylinder(Vector3 from, Vector3 to, bool _isFrontEnd)
    {
        // to의 방향이 전진인지 후진인지 여부에 따라 isFrontEnd 설정.
        Vector3 direction = Vector3.one;

        if (_isFrontEnd == false)
        {
            ChangeSWColor(backLimitSW, Color.black);

            while (true)
            {
                print("작동중");

                direction = to - cylinderRod.localPosition;

                Vector3 normalizedDir = direction.normalized;
                float distance = direction.magnitude;

                if (distance < 0.1f)
                {
                    print("정지");

                    isMoving = false;

                    isFrontEnd = true; // 필드, 멤버변수

                    ChangeSWColor(frontLimitSW, Color.green);

                    break;
                }

                cylinderRod.localPosition += normalizedDir * speed * Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }
        }
        else if (_isFrontEnd == true)
        {
            ChangeSWColor(frontLimitSW, Color.black);

            while (true)
            {
                print("작동중");

                direction = to - cylinderRod.localPosition;

                Vector3 normalizedDir = direction.normalized;
                float distance = direction.magnitude;

                if (distance < 0.1f)
                {
                    print("정지");

                    isMoving = false;

                    isFrontEnd = false;

                    ChangeSWColor(backLimitSW, Color.green);

                    break;
                }

                cylinderRod.localPosition += normalizedDir * speed * Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }
        }
    }


    public void ChangeSWColor(Renderer sw, Color color)
    {
        sw.material.SetColor("_BaseColor", color);
    }
}
