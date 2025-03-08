using System;
using System.Collections.Generic;
using NativeWebSocket;
using UnityEngine;

namespace SitePatrol
{
    // 需要引入 websocket-sharp

    /// <summary>
    /// 上传的消息数据结构，与后端 WorkerUpdate 对应，仅包含偏航角（yaw）
    /// </summary>
    [Serializable]
    public class WorkerUpdateMessage
    {
        public string patrol_id;
        public float[] position;
        public float orientation; // 仅上传偏航角

        public WorkerUpdateMessage(string patrolId, Vector3 pos, float yaw)
        {
            patrol_id = patrolId;
            position = new[] {pos[0], pos[1], pos[2]};
            orientation = yaw;
        }
    }

    /// <summary>
    /// 计算相机在模型坐标系的位置，展示相机路径，然后把相机姿态上传到服务端
    /// </summary>
    public class WorkerTracker : MonoBehaviour
    {
        public Camera trackedCamera;
        public CoordinateMatcher coordinateMatcher;
        public GameObject modelRoot;
        public LineRenderer lineRenderer;

        // 上传数据用的字段：相机位置和旋转
        public Vector3 cameraPostion = Vector3.zero;
        public Vector3 cameraRotation = Vector3.zero;

        public float minDistance = 0.1f;
        private List<Vector3> positions = new List<Vector3>();

        private string patrolId;
        private WebSocket ws;
        private Vector3 lastSentPosition = Vector3.zero;

        private float lastSentYaw = 0f;

        // 设定阈值，只有当位置或yaw变化超过该阈值时才发送更新（可按需调整）
        public float positionThreshold = 0.01f;
        public float yawThreshold = 0.5f;

        void Start()
        {
            // 初始化上次发送的值
            lastSentPosition = cameraPostion;
            lastSentYaw = cameraRotation.y;
            patrolId = Guid.NewGuid().ToString();
        }

        void Update()
        {
            if (ws == null && WebApiClient.BaseUrl != null)
            {
                ws = new WebSocket(WebApiClient.BaseUrl.Replace("http://", "ws://").Replace("https://", "wss://") +
                                   $"/api/vi/model_files/{WebApiClient.ModelFileId}/patrol/ws");
                ws.OnOpen += () => { Debug.Log("WebSocket 连接成功"); };
                ws.OnError += (e) => { Debug.Log("WebSocket 错误: " + e); };
                ws.OnClose += (e) => { Debug.Log("WebSocket 连接关闭"); };
            }

            if (ws != null && ws.State == WebSocketState.Closed)
            {
                ws.Connect();
            }

            if (!coordinateMatcher.ready) return;
            UpdateCameraPosition();
            UpdateLinePositions();

            // 检查是否变化，若变化则发送更新
            if (Vector3.Distance(lastSentPosition, cameraPostion) > positionThreshold ||
                Mathf.Abs(lastSentYaw - cameraRotation.y) > yawThreshold)
            {
                SendUpdate();
                lastSentPosition = cameraPostion;
                lastSentYaw = cameraRotation.y;
            }
        }

        /// <summary>
        /// 记录相机移动轨迹
        /// </summary>
        private void UpdateLinePositions()
        {
            // 只有当相机移动超过一定距离时，才记录新点
            if (positions.Count == 0 || Vector3.Distance(positions[positions.Count - 1], cameraPostion) >= minDistance)
            {
                positions.Add(cameraPostion);
                lineRenderer.positionCount = positions.Count;
                lineRenderer.SetPosition(positions.Count - 1, modelRoot.transform.TransformPoint(cameraPostion));
            }
        }

        /// <summary>
        /// 计算相机相对于模型的位置和旋转，并写在 cameraPostion 和 cameraRotation 中
        /// </summary>
        private void UpdateCameraPosition()
        {
            // 计算相对于模型坐标系的位置
            Vector3 relativePosition = modelRoot.transform.InverseTransformPoint(trackedCamera.transform.position);
            // 计算相对于模型坐标系的旋转
            Quaternion relativeRotation =
                Quaternion.Inverse(modelRoot.transform.rotation) * trackedCamera.transform.rotation;

            cameraPostion = relativePosition;
            cameraRotation = relativeRotation.eulerAngles;
        }

        /// <summary>
        /// 组装更新消息并通过 WebSocket 发送（只上传偏航角）
        /// </summary>
        private void SendUpdate()
        {
            // 这里使用 cameraRotation.y 作为偏航角
            WorkerUpdateMessage updateMsg = new WorkerUpdateMessage(patrolId, cameraPostion, cameraRotation.y);
            string json = JsonUtility.ToJson(updateMsg);
            if (ws != null && ws.State == WebSocketState.Open)
            {
                ws.SendText(json);
            }
            else
            {
                Debug.LogWarning("WebSocket 未连接，无法发送更新");
            }
        }

        private void OnDestroy()
        {
            // 确保退出时关闭 WebSocket 连接
            if (ws != null)
            {
                ws.Close();
            }
        }
    }
}