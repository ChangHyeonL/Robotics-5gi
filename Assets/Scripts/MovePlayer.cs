using UnityEngine;

// Unity�� Lifecycle �޼���
// ��ü�� ����Ű�� �Է��� �޾� �̵���Ų��.
public class MovePlayer : MonoBehaviour
{
    public float speed = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        print("��Ƴ�");
    }

    // 100FPS: 1�ʿ� 100�� ������Ʈ �Լ��� �۵�
    // 30FPS ~ 400FPS
    // Update is called once per frame
    void Update()
    {
        // MoveWithoutTime();

        MoveWithTime();
    }

    private void MoveWithTime()
    {
        // ���̽�ƽ�� ��ǲ�� ����� -1 ~ 1�� ǥ���ϴ� �Լ�
        float horizontalInput = Input.GetAxis("Horizontal"); // ����Ű �¿�
        float verticalInput = Input.GetAxis("Vertical");     // ����Ű ����

        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput);
        transform.position += direction * speed * Time.deltaTime; // 0.3s 
    }


    // �ð��� ���� ��� ���� Move�޼���
    private void MoveWithoutTime()
    {
        bool isWKeyDown = Input.GetKey(KeyCode.W);
        bool isAKeyDown = Input.GetKey(KeyCode.A);
        bool isSKeyDown = Input.GetKey(KeyCode.S);
        bool isDKeyDown = Input.GetKey(KeyCode.D);

        if (isWKeyDown)
        {
            // ���� ���ϱ�
            Vector3 direction = Vector3.forward * speed; // ���� ��ǥ�� ����
            Vector3 localDirection = transform.forward * speed; // ���� ��ǥ�� ����

            // ������ǥ�� �� ������ġ + ���⺤��
            transform.position += localDirection;
        }

        if (isSKeyDown)
        {
            // ���� ���ϱ�
            Vector3 direction = Vector3.back * speed;
            Vector3 localDirection = -transform.forward * speed;

            // ������ǥ�� �� ������ġ + ���⺤��
            transform.position += localDirection;
        }

        if (isAKeyDown)
        {
            // ���� ���ϱ�
            Vector3 direction = Vector3.left * speed;
            Vector3 localDirection = -transform.right * speed;

            // ������ǥ�� �� ������ġ + ���⺤��
            transform.position += localDirection;
        }

        if (isDKeyDown)
        {
            // ���� ���ϱ�
            Vector3 direction = Vector3.right * speed;
            Vector3 localDirection = transform.right * speed;

            // ������ǥ�� �� ������ġ + ���⺤��
            transform.position += localDirection;
        }
    }
}
