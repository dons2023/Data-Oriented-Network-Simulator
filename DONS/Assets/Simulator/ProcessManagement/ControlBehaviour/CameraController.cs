using UnityEngine;

public class CameraController : MonoBehaviour
{
    //public float movementSpeed = 5.0f;
    //public float rotationSpeed = 5.0f;
    public float moveSpeed = 10f;

    public float zoomSpeed = 10f;

    private Vector3 lastMousePosition;
    public float rotateSpeed = 10f;
    public float sensitivity = 1f;

    private Camera myCamera;

    private void Awake()
    {
        myCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (myCamera.isActiveAndEnabled)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            //float depth = Input.GetAxis("Depth");

            transform.Translate(new Vector3(horizontal, vertical, 0) * moveSpeed * Time.deltaTime);

            Vector2 scrollDelta = Input.mouseScrollDelta;
            float zoom = scrollDelta.y;
            transform.Translate(new Vector3(0, 0, zoom) * zoomSpeed * Time.deltaTime, Space.Self);

            if (Input.GetMouseButtonDown(1))
            {
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(1))
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                transform.Rotate(Vector3.up, delta.x * rotateSpeed * sensitivity * Time.deltaTime, Space.World);
                transform.Rotate(Vector3.right, -delta.y * rotateSpeed * sensitivity * Time.deltaTime, Space.Self);

                lastMousePosition = Input.mousePosition;
            }
        }
    }
}
