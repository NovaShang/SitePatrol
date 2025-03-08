using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AprilTag;
using UnityEngine;

namespace SitePatrol
{
    public class AprilTagDetector : MonoBehaviour
    {
        public CameraImageHandler image;
        public Camera cam;
        public float tagSize = 0.1f;
        public int decimation = 1;
        public CoordinateMatcher coordinateMatcher;
        public Material boxMaterial;
        public bool showBoxes = true;
        private bool workerBusy = false;
        private TagDetector tagDetector;
        private Dictionary<int, GameObject> tagBoxes = new();
        private TagPose[] detectionResults = Array.Empty<TagPose>();


        private void Start()
        {
            image.OnImageReceived += OnImageReceived;
        }

        private void OnDestroy()
        {
            if (tagDetector == null) return;
            tagDetector.Dispose();
            tagDetector = null;
        }

        private void Update()
        {
            // 用检测结果来更新可视化
            if (showBoxes) ShowBoxes(detectionResults);
        }


        /// <summary>
        /// 把检测逻辑放到后台线程里执行
        /// </summary>
        private async void OnImageReceived(Color32[] pixels,
            int width,
            int height,
            float fov,
            Vector3 position,
            Quaternion rotation)
        {
            if (workerBusy) return;
            try
            {
                workerBusy = true;
                if (tagDetector == null || tagDetector.Image.Width != width || tagDetector.Image.Height != height)
                    tagDetector = new TagDetector(width, height, decimation);

                await Task.Run(() =>
                {
                    tagDetector.ProcessImage(pixels, fov * Mathf.Deg2Rad, tagSize);
                    detectionResults = tagDetector.DetectedTags.ToArray();
                    coordinateMatcher.UpdateDetectionResults(detectionResults, position, rotation);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                workerBusy = false;
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
                    box.transform.parent = cam.transform;
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
    }
}