using UnityEngine;

public class Gripper : MonoBehaviour
{
    public bool isObjectLocated = false;
    public Transform touchObj;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("금속") || other.tag.Contains("플라스틱"))
        {
            touchObj = other.transform;
            isObjectLocated = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("금속") || other.tag.Contains("플라스틱"))
            isObjectLocated = false;
    }

    // TeachData의 현재 스텝 중 isGripperOn이 True라면 부딪힌 물체를 자식 Object로 만들기
    public void SetChild()
    {
        if (touchObj == null) return;

        if (isObjectLocated)
        {
            touchObj.transform.SetParent(transform);
            touchObj.GetComponent<Rigidbody>().useGravity = false;
            touchObj.GetComponent<Rigidbody>().isKinematic = true;
        }
        else
        {
            touchObj.transform.SetParent(null);
            touchObj.GetComponent<Rigidbody>().useGravity = true;
            touchObj.GetComponent<Rigidbody>().isKinematic = false;
        }

    }
}
