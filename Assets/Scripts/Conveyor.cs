using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

// ����(CW, CCW)�� ���� ��ü��(�о) �̵���Ų��.
// �Ӽ�: ����, ��ü, ��ü�� �̵��ӵ�, �۵�����
public class Conveyor : MonoBehaviour
{
    public static Conveyor instance; // �̱��� ����

    public bool isOn;
    public bool isCW = true;
    public bool isCCW = false;
    public float speed;
    public List<Dragger> draggers;
    public Transform startPos; // dragger ������ġ
    public Transform endPos; // dragger ����ġ

    public enum Direction
    {
        CW, // ������
        CCW // ������
    }
    public Direction direction = Direction.CW;

    // �ʱ�ȭ�� ���� ������ �ϴ� Lifecycle �޼���
    private void Awake()
    {
        if (instance == null) 
            instance = this; // �̱��� �ʱ�ȭ
    }

    private void Start()
    {
        RotateConveyor(); // Conveyor ������ Dragger Ȱ��ȭ
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
