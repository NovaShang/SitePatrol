using UnityEngine;

public class ViewerCameraController : MonoBehaviour
{
    public Camera viewerCamera;

    // 目标物体，摄像机会围绕该目标旋转
    public Transform target;

    // 摄像机与目标之间的初始距离
    public float distance = 10.0f;

    // 旋转速度
    public float xSpeed = 120.0f;

    public float ySpeed = 120.0f;

    // 垂直旋转的角度限制
    public float yMinLimit = -20f;

    public float yMaxLimit = 80f;

    // 缩放距离的限制
    public float distanceMin = 0.5f;

    public float distanceMax = 40f;

    // 缩放速度
    public float zoomSpeed = 2f;

    // 平移速度
    public float panSpeed = 0.3f;

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        // 初始化摄像机当前的旋转角度
        Vector3 angles = viewerCamera.transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (target)
        {
            // 鼠标左键拖动：旋转摄像机
            if (Input.GetMouseButton(1))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }

            // 鼠标滚轮：缩放
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, distanceMin, distanceMax);

            // 鼠标右键拖动：平移目标位置
            if (Input.GetMouseButton(2))
            {
                // 根据摄像机自身的 right 和 up 方向移动目标
                Vector3 pan = new Vector3(-Input.GetAxis("Mouse X") * panSpeed, -Input.GetAxis("Mouse Y") * panSpeed,
                    0);
                target.position += viewerCamera.transform.right * pan.x + viewerCamera.transform.up * pan.y;
            }

            // 根据旋转角度和距离计算摄像机新位置
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            viewerCamera.transform.rotation = rotation;
            viewerCamera.transform.position = position;
        }
    }

    // 限制旋转角度，防止出现翻转问题
    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}