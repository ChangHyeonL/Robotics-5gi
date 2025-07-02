using UnityEngine;

// 물체의 종류에 따라 물체를 감지하고, 감지되면 색상을 바꿔준다.
// 속성: 센서의 종류(열거형)
public class Sensor : MonoBehaviour
{
    public enum SensorType
    {
        근접센서,
        금속센서
    }
    public SensorType sensorType;
    public bool isActive;
    Renderer renderer;
    Color originColor;

    private void Start()
    {
        renderer = GetComponent<Renderer>(); // 캐싱
        originColor = renderer.material.color;
    }

    private void Update()
    {
        if(sensorType == SensorType.근접센서)
        {
            if(isActive)
            {
                renderer.material.color = Color.red;
            }
            else
            {
                renderer.material.color = originColor;
            }
        }
        else
        {
            if (isActive)
            {
                renderer.material.color = Color.green;
            }
            else
            {
                renderer.material.color = originColor;
            }
        }
    }

    // rigidBody가 있는 물체가 접촉하는 순간 실행
    // * 내 자신의 Collider의 isTrigger 설정 필요
    private void OnTriggerEnter(Collider other)
    {
        if(sensorType == SensorType.금속센서)
        {
            if(other.tag == "금속")
            {
                isActive = true;
                renderer.material.SetColor("_BaseColor", Color.green);
                //gameObject.GetComponent<Renderer>().material.SetColor
                print(other.tag + "접촉 시작");
            }
        }
        else if(sensorType == SensorType.근접센서)
        {
            isActive = true;
            renderer.material.SetColor("_BaseColor", Color.red);
            print(other.tag + "접촉 시작");
        }
    }

    // rigidBody가 있는 물체가 머무는 동안 실행
    //private void OnTriggerStay(Collider other)
    //{
    //    if (sensorType == SensorType.금속센서)
    //        print(other.tag + "접촉중");
    //    else
    //        print(other.tag + "접촉 시작");
    //}

    // rigidBody가 있는 물체의 접촉이 끝나는 순간 실행
    private void OnTriggerExit(Collider other)
    {
        if (sensorType == SensorType.금속센서)
        {
            if (other.tag == "금속")
            {
                isActive = false;
                renderer.material.SetColor("_BaseColor", originColor);
                print(other.tag + "접촉 해지");
            }
        }
        else if (sensorType == SensorType.근접센서)
        {
            isActive = false;
            renderer.material.SetColor("_BaseColor", originColor);
            print(other.tag + "접촉 시작");
        }
    }
}
