using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

// 방향(CW, CCW)에 따라서 물체를(밀어서) 이동시킨다.
// 속성: 방향, 물체, 물체의 이동속도, 작동여부
public class Conveyor : MonoBehaviour
{
    public static Conveyor instance; // 싱글턴 패턴

    public bool isOn;
    public bool isCW = true;    // CW신호
    public bool isCCW = false;  // CCW신호
    public float speed;
    public List<Dragger> draggers;
    public Transform startPos; // dragger 시작위치
    public Transform endPos; // dragger 끝위치

    public enum Direction
    {
        CW, // 정방향
        CCW // 역방향
    }
    public Direction direction = Direction.CW;

    // 초기화를 가장 빠르게 하는 Lifecycle 메서드
    private void Awake()
    {
        if (instance == null) 
            instance = this; // 싱글턴 초기화
    }

    private void Start()
    {
        RotateConveyor(); // Conveyor 내부의 Dragger 활성화 -> Dragger가 isCW, isCCW신호에 의해 이동
    }

    public void RotateConveyor()
    {
        foreach (Dragger dragger in draggers)
        {
            dragger.Move();
        }
    }

    //public void StopConveyor()
    //{
    //    isOn = false;

    //    foreach (Dragger dragger in draggers)
    //    {
    //        dragger.Stop();
    //    }
    //}
}
