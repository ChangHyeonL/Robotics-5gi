using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// Cube1, 2, 3, 4, 5�� ������� 1�� �������� ����Ͽ�
// CylinderA -> B -> C -> D ������ �̵��Ѵ�.
// �Ӽ�: Cube�� �ӵ�, Ÿ�ٵ�

public class CubeManager1 : MonoBehaviour
{
    public float speed = 10;
    public float interval = 1;
    public GameObject cube;
    public GameObject cube1;
    public GameObject cube2;
    public GameObject cube3;
    public GameObject cube4;
    public List<GameObject> targets;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // �ڷ�ƾ �޼���1 ����
        StartCoroutine(CoStart());
    }

    // Update is called once per frame
    void Update()
    {
        print("update");
    }

    // �ڷ�ƾ �޼���: ���μ��� ������ ��� ��ٸ� �� �ִ� ���
    IEnumerator CoStart()
    {
        // �ڷ�ƾ �޼���2 ����
        StartCoroutine(MoveCubeToTargets(cube, targets));

        yield return new WaitForSeconds(interval);

        StartCoroutine(MoveCubeToTargets(cube1, targets));

        yield return new WaitForSeconds(interval);

        StartCoroutine(MoveCubeToTargets(cube2, targets));

        yield return new WaitForSeconds(interval);

        StartCoroutine(MoveCubeToTargets(cube3, targets));

        yield return new WaitForSeconds(interval);

        StartCoroutine(MoveCubeToTargets(cube4, targets));
    }

    IEnumerator MoveCubeToTargets(GameObject cube, List<GameObject> targets)
    {
        int index = 0;

        print(cube.gameObject.name + "���!");

        while(true)
        {
            // 1. A���� B�� ���ϴ� ���� -> ��������(ũ�Ⱑ 1�� ����) -> �÷��̾�� �������͸� ������
            Vector3 direction = targets[index].transform.position - cube.transform.position;
            // 2. ��������(ũ�Ⱑ 1�� ����)
            Vector3 normalizedDir = direction.normalized;

            // 3. �Ÿ����
            float distance = Vector3.Magnitude(direction);
            // ������ �� ���ΰ�? cylinderB ���� -> �Ÿ�
            // print(distance);

            if (distance < 0.1f)
            {
                index++;

                if (index == targets.Count)
                {
                    break;
                }
                // CylA -> CylB -> CylC -> Cyl4
            }

            // 4. �÷��̾�� �������͸� ������
            cube.transform.position += normalizedDir * speed * Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
}
