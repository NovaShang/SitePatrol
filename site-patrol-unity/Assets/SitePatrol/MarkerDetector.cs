using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AprilTag;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SitePatrol
{
    public class MarkerDetector : MonoBehaviour
    {
        public ARCameraManager cameraManager;
        public float tagSize = 0.1f;
        public int decimation = 1;
        public CoordinateMatcher coordinateMatcher;
        public Material boxMaterial;
        public bool showBoxes = true;

        private TagDetector tagDetector;
        private Texture2D cameraTexture;
        private Dictionary<int, GameObject> tagBoxes = new Dictionary<int, GameObject>();

        // 用于区分主线程是否正在等待后台处理完成
        private volatile bool workerBusy = false;

        private Camera arCamera;
        private TagPose[] detectionResults = Array.Empty<TagPose>();
        public bool needTransform = false;

        private void OnEnable()
        {
            needTransform = Application.platform == RuntimePlatform.Android ||
                            Application.platform == RuntimePlatform.IPhonePlayer;
            if (cameraManager != null)
            {
                cameraManager.frameReceived += OnCameraFrameReceived;
                arCamera = cameraManager.GetComponent<Camera>();
            }
        }

        private void OnDisable()
        {
            if (cameraManager != null)
                cameraManager.frameReceived -= OnCameraFrameReceived;
        }

        private void OnDestroy()
        {
            if (tagDetector != null)
            {
                tagDetector.Dispose();
                tagDetector = null;
            }
        }

        private void Update()
        {
            // 用检测结果来更新可视化
            if (showBoxes) ShowBoxes(detectionResults);
        }

        private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            // 如果后台线程还在跑，就不要重复启动
            if (workerBusy) return;
            if (!cameraManager.TryAcquireLatestCpuImage(out var cpuImage)) return;

            // 记录相机姿态
            var camPosition = arCamera.transform.position;
            var camRotation = arCamera.transform.rotation;
            var fov = arCamera.fieldOfView;

            using (cpuImage)
            {
                var width = cpuImage.width;
                var height = cpuImage.height;

                InitTextureAndDetector(width, height);

                var conversionParams = new XRCpuImage.ConversionParams
                {
                    inputRect = new RectInt(0, 0, width, height),
                    outputDimensions = new Vector2Int(width, height),
                    outputFormat = TextureFormat.RGBA32,
                    transformation = needTransform
                        ? XRCpuImage.Transformation.MirrorY
                        : XRCpuImage.Transformation.None
                };

                var bufferSize = cpuImage.GetConvertedDataSize(conversionParams);
                using (var buffer =
                       new Unity.Collections.NativeArray<byte>(bufferSize, Unity.Collections.Allocator.Temp))
                {
                    cpuImage.Convert(conversionParams, buffer);
                    cameraTexture.LoadRawTextureData(buffer);
                    cameraTexture.Apply();
                }
            }

            // 从 cameraTexture 读取像素; 这部分是可序列化的数据
            Color32[] pixels = cameraTexture.GetPixels32();

            // 这里启动异步的检测
            ExecuteAsync(pixels, camPosition, camRotation, fov, cameraTexture.width, cameraTexture.height);
        }

        /// <summary>
        /// 把检测逻辑放到后台线程里执行
        /// </summary>
        private async void ExecuteAsync(Color32[] pixels,
            Vector3 position,
            Quaternion rotation,
            float fov,
            int width,
            int height)
        {
            if (workerBusy) return;
            workerBusy = true;

            // 不要在后台线程操作 Unity 对象，所以先拷贝好必要数据
            var localNeedTransform = needTransform;
            var localTagDetector = tagDetector; // 假设 tagDetector 是线程安全的或一次只被一个线程调用
            var localTagSize = tagSize;
            var localFOV = fov * Mathf.Deg2Rad;
            var localDecimation = decimation;

            // 如果需要旋转像素，可以先在主线程里转好，也可以放后台转。
            // 只要别调用 UnityEngine.* 中会牵涉到 GPU 或其他主线程专用 API 的东西即可
            if (localNeedTransform)
            {
                pixels = RotatePixels90Clockwise(pixels, width, height);
                // 注意：宽高也对调了
                (width, height) = (height, width);
            }

            TagPose[] results = Array.Empty<TagPose>();

            try
            {
                // 真正耗时操作放到后台
                results = await Task.Run(() =>
                {
                    // 在子线程调用处理逻辑
                    localTagDetector.ProcessImage(pixels, localFOV, localTagSize);
                    return localTagDetector.DetectedTags.ToArray();
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in marker detection: {e.Message}");
            }
            finally
            {
                // 回到主线程后，再对 detectionResults、coordinateMatcher 做更新
                detectionResults = results;
                coordinateMatcher.UpdateDetectionResults(results, position, rotation);

                workerBusy = false;
            }
        }

        /// <summary>
        /// 初始化 Texture 和 TagDetector
        /// </summary>
        private void InitTextureAndDetector(int width, int height)
        {
            if (cameraTexture == null || cameraTexture.width != width || cameraTexture.height != height)
            {
                cameraTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

                // 如果大小变了，必须重置
                tagDetector?.Dispose();
                tagDetector = null;
            }

            if (tagDetector == null)
            {
                Debug.LogWarning("TagDetector is null or image size changed, reinitializing.");
                if (needTransform)
                    tagDetector = new TagDetector(height, width, decimation);
                else
                    tagDetector = new TagDetector(width, height, decimation);
            }
        }

        private void ShowBoxes(TagPose[] poses)
        {
            foreach (var tagPose in poses)
            {
                if (!tagBoxes.TryGetValue(tagPose.ID, out GameObject box))
                {
                    box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(box.GetComponent<Collider>());
                    tagBoxes.Add(tagPose.ID, box);

                    var renderer = box.GetComponent<Renderer>();
                    if (renderer != null) renderer.material = boxMaterial;

                    // 父节点设为 cameraManager.transform 以便和相机在同一坐标系
                    box.transform.parent = cameraManager.transform;
                    box.transform.localScale = new Vector3(tagSize, tagSize, 0.04f);
                }

                box.transform.localPosition = tagPose.Position;
                box.transform.localRotation = tagPose.Rotation;
            }

            // 移除消失的标签
            var currentIDs = poses.Select(p => p.ID).ToHashSet();
            var toRemove = tagBoxes.Keys.Where(id => !currentIDs.Contains(id)).ToList();
            foreach (var id in toRemove)
            {
                Destroy(tagBoxes[id]);
                tagBoxes.Remove(id);
            }
        }

        private Color32[] RotatePixels90Clockwise(Color32[] originalPixels, int width, int height)
        {
            var total = width * height;
            var rotated = new Color32[total];
            for (var i = 0; i < total; i++)
            {
                var x = i % width;
                var y = i / width;
                rotated[(height - 1 - y) + x * height] = originalPixels[i];
            }

            return rotated;
        }
    }
}