using System;
using System.Collections.Generic;
using NativeWebSocket;
using UnityEngine;

namespace SitePatrol
{
    /// <summary>
    /// 通过 WebSocket 接收各个工人的位置与朝向，并用预制体+LineRenderer 显示在场景中
    /// </summary>
    public class WorkersMonitor : MonoBehaviour
    {
        /// <summary>
        /// 用于记录每个工人的可视化内容
        /// </summary>
        public class WorkerData
        {
            public GameObject workerObject; // 工人对应的实例
            public LineRenderer lineRenderer; // 用于画路线
            public List<Vector3> routePositions; // 保存历史位置的列表
        }


        public GameObject modelRoot;
        public GameObject workerPrefab;
        private WebSocket ws;
        private Dictionary<string, WorkerData> workerDict = new Dictionary<string, WorkerData>();


        void Update()
        {
            if (ws == null && WebApiClient.BaseUrl != null)
            {
                ws = new WebSocket(WebApiClient.BaseUrl.Replace("http://", "ws://").Replace("https://", "wss://") +
                                   $"/api/vi/model_files/{WebApiClient.ModelFileId}/patrol/ws");
                ws.OnOpen += () => { Debug.Log("WebSocket 连接成功"); };
                ws.OnError += (e) => { Debug.Log("WebSocket 错误: " + e); };
                ws.OnClose += (e) => { Debug.Log("WebSocket 连接关闭"); };
                ws.OnMessage += (bytes) =>
                {
                    // 将字节数据转为字符串
                    var json = System.Text.Encoding.UTF8.GetString(bytes);
                    Debug.Log($"收到消息: {json}");

                    // 尝试反序列化为 WorkerUpdateMessage
                    WorkerUpdateMessage updateMsg = null;
                    try
                    {
                        updateMsg = JsonUtility.FromJson<WorkerUpdateMessage>(json);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"JSON 解析失败: {ex}");
                        return;
                    }

                    if (updateMsg == null || string.IsNullOrEmpty(updateMsg.patrol_id))
                    {
                        Debug.LogWarning($"无效的消息格式: {json}");
                        return;
                    }

                    // 在主线程调度执行处理逻辑（防止多线程问题）
                    // 若你确定当前上下文在主线程可省略此步骤
                    HandleWorkerUpdate(updateMsg);
                };
            }

            if (ws != null && ws.State == WebSocketState.Closed)
            {
                ws.Connect();
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            // 在非 WebGL 平台或编辑器中，需要手动调用 DispatchMessageQueue
            ws.DispatchMessageQueue();
#endif
        }

        /// <summary>
        /// 根据后端传来的 WorkerUpdateMessage，更新或创建对应的 WorkerData
        /// </summary>
        /// <param name="updateMsg">工人更新数据</param>
        private void HandleWorkerUpdate(WorkerUpdateMessage updateMsg)
        {
            // 如果 workerDict 中没有该 patrol_id，则创建
            if (!workerDict.TryGetValue(updateMsg.patrol_id, out var workerData))
            {
                // 创建新实例
                var workerObj = Instantiate(workerPrefab, transform);
                // 这里可以选择从预制体上获取 LineRenderer，或者手动添加
                var lr = workerObj.GetComponentInChildren<LineRenderer>();
                workerData = new WorkerData
                {
                    workerObject = workerObj,
                    lineRenderer = lr,
                    routePositions = new List<Vector3>()
                };
                workerDict[updateMsg.patrol_id] = workerData;
            }

            // 将模型坐标转换到世界坐标
            var localPos = new Vector3(updateMsg.position[0], updateMsg.position[1], updateMsg.position[2]);
            var worldPos = modelRoot.transform.TransformPoint(localPos);

            // orientation 仅表示偏航角 (yaw, 单位: 度)，我们假设是围绕 Y 轴的旋转
            var localRot = Quaternion.Euler(0, updateMsg.orientation, 0);
            // 如果 orientation 是相对于模型本身的旋转，则要把模型的世界旋转也考虑进来
            var worldRot = modelRoot.transform.rotation * localRot;

            // 更新工人可视化对象的位置和朝向
            workerData.workerObject.transform.position = worldPos;
            workerData.workerObject.transform.rotation = worldRot;

            // 记录路径
            workerData.routePositions.Add(worldPos);
            // 更新 LineRenderer
            workerData.lineRenderer.positionCount = workerData.routePositions.Count;
            workerData.lineRenderer.SetPosition(workerData.routePositions.Count - 1, worldPos);
        }

        private async void OnDestroy()
        {
            // 退出前关闭连接
            if (ws != null)
            {
                await ws.Close();
            }
        }
    }
}