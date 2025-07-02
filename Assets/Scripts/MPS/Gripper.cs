using UnityEngine;

public class Gripper : MonoBehaviour
{
    public bool isObjectLocated = false;
    public Transform touchObj;
    public Transform touchObj2;
    Vector3 originPos;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag.Contains("금속") || other.tag.Contains("플라스틱"))
        {
            touchObj = other.transform;
            isObjectLocated = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("금속") || other.tag.Contains("플라스틱"))
        {
            touchObj = null;
            isObjectLocated = false;
        }
    }
    
    // TeachData의 현재 스탭 중 isGripperOn이 True라면 부딪힌 물체를 자식오브젝트로 만들기
    public void SetChild(bool isGripperOn)
    {
        if (touchObj == null && !isObjectLocated) return;

        if (isGripperOn)
        {
            touchObj.transform.SetParent(transform);
            touchObj.GetComponent<Rigidbody>().useGravity = false;
            touchObj.GetComponent<Rigidbody>().isKinematic = true;
        }
        else
        {
            touchObj.GetComponent<Rigidbody>().useGravity = true;
            touchObj.GetComponent<Rigidbody>().isKinematic = false;
            touchObj.transform.SetParent(null);
        }
    }
}
