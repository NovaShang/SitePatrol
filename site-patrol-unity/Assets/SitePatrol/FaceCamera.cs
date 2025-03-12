using UnityEngine;

namespace SitePatrol
{
    public class FaceCamera : MonoBehaviour
    {
        private Camera mainCam;

        void Start()
        {
            // 获取主摄像机（需要场景里有一个 Camera 并在其 Inspector 中勾选 “Main Camera” 标签）
            mainCam = Camera.main;
        }

        void LateUpdate()
        {
            // 如果未找到主摄像机，可在此进行容错处理
            if (mainCam == null) return;

            // 让 Canvas 朝向摄像机
            transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
        }
    }
}