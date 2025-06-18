using UnityEngine;

// ��ü�� ������ ���� ��ü�� �����ϰ�, �����Ǹ� ������ �ٲ��ش�.
// �Ӽ�: ������ ����(������)
public class Sensor : MonoBehaviour
{
    public enum SensorType
    {
        ��������,
        �ݼӼ���
    }
    public SensorType sensorType;
    public bool isActive;
    Renderer renderer;
    Color originColor;

    private void Start()
    {
        renderer = GetComponent<Renderer>(); // ĳ��
        originColor = renderer.material.color;
    }

    // rigidBody�� �ִ� ��ü�� �����ϴ� ���� ����
    // * �� �ڽ��� Collider�� isTrigger ���� �ʿ�
    private void OnTriggerEnter(Collider other)
    {
        if(sensorType == SensorType.�ݼӼ���)
        {
            if(other.tag == "�ݼ�")
            {
                isActive = true;
                renderer.material.SetColor("_BaseColor", Color.green);
                //gameObject.GetComponent<Renderer>().material.SetColor
                print(other.tag + "���� ����");
            }
        }
        else if(sensorType == SensorType.��������)
        {
            isActive = true;
            renderer.material.SetColor("_BaseColor", Color.red);
            print(other.tag + "���� ����");
        }
    }

    // rigidBody�� �ִ� ��ü�� �ӹ��� ���� ����
    //private void OnTriggerStay(Collider other)
    //{
    //    if (sensorType == SensorType.�ݼӼ���)
    //        print(other.tag + "������");
    //    else
    //        print(other.tag + "���� ����");
    //}

    // rigidBody�� �ִ� ��ü�� ������ ������ ���� ����
    private void OnTriggerExit(Collider other)
    {
        if (sensorType == SensorType.�ݼӼ���)
        {
            if (other.tag == "�ݼ�")
            {
                isActive = false;
                renderer.material.SetColor("_BaseColor", originColor);
                print(other.tag + "���� ����");
            }
        }
        else if (sensorType == SensorType.��������)
        {
            isActive = false;
            renderer.material.SetColor("_BaseColor", originColor);
            print(other.tag + "���� ����");
        }
    }
}
