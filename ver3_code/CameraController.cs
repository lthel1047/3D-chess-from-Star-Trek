using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public float rotationSpeed = 50f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    public float distance = 10f;

    public float currentX = 0f;
    public float currentY = 0f;


    private void Update()
    {
        // ����� ��ġ �Է� ó��
        if (Input.touchSupported && Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;
                currentX += delta.x * rotationSpeed;
                currentY -= delta.y * rotationSpeed;
                currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
            }
        }
        // PC ���콺 �Է� ó��
        else if (Input.GetMouseButton(0))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");
            currentX += dx * rotationSpeed * 5f;
            currentY -= dy * rotationSpeed * 5f;
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        }

        // ȸ���� ��ġ ������Ʈ
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0f);
        Vector3 offset = rotation * new Vector3(8.5f, 11f, -distance);
        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }
}
