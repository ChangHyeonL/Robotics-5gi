using System.Collections;
using UnityEngine;

// RGB Lamp�� ������ ���� �ð��� ���� ���������� �����̰� �Ѵ�.
// �Ӽ�: �ð�, Lamp�� ����
public class LampManager : MonoBehaviour
{
    public float time;
    public Renderer redLamp;
    public Renderer greenLamp;
    public Renderer blueLamp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ó�����۽� ������ �˰� �ʱ�ȭ
        // _Color�� Shader�� Ư�� �Ӽ�: ���������ο� ���� �ٸ�
        redLamp.material.SetColor("_BaseColor", Color.black);
        greenLamp.material.SetColor("_BaseColor", Color.black);
        blueLamp.material.SetColor("_BaseColor", Color.black);

        StartCoroutine(CoStartLamp());
    }

    // �ڷ�ƾ�� ���, time �������� ���� �����ϴ� Lamp
    IEnumerator CoStartLamp()
    {
        redLamp.material.SetColor("_BaseColor", Color.red);
        greenLamp.material.SetColor("_BaseColor", Color.black);
        blueLamp.material.SetColor("_BaseColor", Color.black);

        yield return new WaitForSeconds(1);

        redLamp.material.SetColor("_BaseColor", Color.black);
        greenLamp.material.SetColor("_BaseColor", Color.green);
        blueLamp.material.SetColor("_BaseColor", Color.black);

        yield return new WaitForSeconds(1);

        redLamp.material.SetColor("_BaseColor", Color.black);
        greenLamp.material.SetColor("_BaseColor", Color.black);
        blueLamp.material.SetColor("_BaseColor", Color.blue);
    }
}
