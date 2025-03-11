using System;
using System.Collections.Generic;
using System.Linq;
using AprilTag;
using UnityEngine;

namespace SitePatrol
{
    /// <summary>
    /// Match real world coordinates and AR session coordinates with the position of AprilTags.
    /// </summary>
    public class CoordinateMatcher : MonoBehaviour
    {
        public bool ready = false;
        public bool twoTags = false;
        public GameObject modelRoot;
        public ToastMessage message;

        // 每个 Tag 的最终位置和可信度评分
        private Dictionary<int, TagPose> detectedMarkers = new();
        private Dictionary<int, TagPose> modeledMarkers = new();
        private Vector3 originalPosition;
        private Dictionary<int, Dictionary<int, float>> modeledDistances = new();

        private Quaternion originalRotation;

        public float emaSmoothingFactor = 0.1f;
        public float dispersionThreshold = 0.05f; // 位置数据的标准差阈值，超过该值则认为数据分散(m)
        public float min2PointDistance = 0.5f; // 两个点之间的最小距离，低于该值则不能用这两个点对齐坐标系(m)

        private Dictionary<int, EMAFilterWithConfidence> detectionFilters = new();

        private Dictionary<int, EMAFilterWithConfidence> offsetFilters = new();

        // 当前的主要Marker ID，这个marker将作为旋转中心
        private int centerMarkerId = -1;
        private float centerDistance = Single.MaxValue;


        public void Start()
        {
            originalPosition = modelRoot.transform.position;
            originalRotation = modelRoot.transform.rotation;
        }

        public void Update()
        {
            if (WebApiClient.Markers.Count > 0 && modeledMarkers.Count == 0)
            {
                modeledMarkers = WebApiClient.Markers.ToDictionary(x => x.ID);
                foreach (var i in modeledMarkers)
                {
                    if (!modeledDistances.ContainsKey(i.Key))
                        modeledDistances[i.Key] = new Dictionary<int, float>();
                    foreach (var j in modeledMarkers.Where(j => i.Key != j.Key))
                        modeledDistances[i.Key][j.Key] = Vector3.Distance(i.Value.Position, j.Value.Position);
                }

                Debug.Log("Marker Detector Ready");
            }


            UpdateModelTransform();
        }


        public void UpdateDetectionResults(TagPose[] results, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            foreach (var pose in results)
            {
                var distance = pose.Position.magnitude;
                if (pose.ID == centerMarkerId)
                    centerDistance = distance;
                else if (distance < centerDistance)
                {
                    centerDistance = distance;
                    centerMarkerId = pose.ID;
                }

                var poseInSession = new TagPose(pose.ID, cameraRotation * pose.Position + cameraPosition,
                    cameraRotation * pose.Rotation);

                // 使用 EMA 滤波器平滑位置和旋转
                if (!detectionFilters.ContainsKey(pose.ID))
                    detectionFilters[pose.ID] = new EMAFilterWithConfidence(emaSmoothingFactor, dispersionThreshold);
                var filter = detectionFilters[pose.ID];
                filter.Update(poseInSession.Position, poseInSession.Rotation);
                if (!filter.IsConfident) continue;
                var filteredPose = new TagPose(pose.ID, filter.GetFilteredPosition(),
                    filter.GetFilteredRotation());
                detectedMarkers[pose.ID] = filteredPose;

                if (!offsetFilters.ContainsKey(pose.ID))
                    offsetFilters[pose.ID] = new EMAFilterWithConfidence(emaSmoothingFactor, dispersionThreshold);

                if (!twoTags) AlignWithOneTag(modeledMarkers[pose.ID], filteredPose);

                if (!modeledMarkers.ContainsKey(pose.ID)) continue;
                foreach (var id in modeledMarkers.Keys.Where(id => id != pose.ID).ToList())
                {
                    // 筛选出可以和当前Tag一起做对齐的Tag，要求是距离不能太近
                    if (!modeledDistances.TryGetValue(pose.ID, out var m) ||
                        !m.TryGetValue(id, out var d) || d < min2PointDistance)
                        continue;
                    if (!detectionFilters.TryGetValue(id, out var f) || !f.IsConfident)
                        continue;

                    AlignWithTwoTags(modeledMarkers[pose.ID], modeledMarkers[id], filteredPose,
                        detectedMarkers[id]);
                    twoTags = true;
                }
            }
        }

        private void UpdateModelTransform()
        {
            if (centerMarkerId < 0 || !detectedMarkers.ContainsKey(centerMarkerId)) return;
            var offsetFilter = offsetFilters[centerMarkerId];
            if (!offsetFilter.IsConfident) return;
            var locationOffset = offsetFilter.GetFilteredPosition();
            var rotationOffset = offsetFilter.GetFilteredRotation();
            var rotationCenter = detectedMarkers[centerMarkerId].Position;
            modelRoot.transform.position = originalPosition + locationOffset;
            modelRoot.transform.rotation = originalRotation;
            modelRoot.transform.RotateAround(rotationCenter, Vector3.up,
                rotationOffset.eulerAngles.y);
            if (!ready)
                message?.ShowMessage("Model Aligned, Capture Another Tag for Better Alignment");
            ready = true;
        }

        /// <summary>
        /// 只扫描到一个Tag时，使用该Tag的position和rotation对齐两个坐标系
        /// </summary>
        private void AlignWithOneTag(TagPose modeledTag, TagPose detectedTag)
        {
            var locationOffset = detectedTag.Position - modeledTag.Position;

            var rotationOffset = Quaternion.Inverse(modeledTag.Rotation) * detectedTag.Rotation;
            var rotationOffsetY = Quaternion.Euler(0, rotationOffset.eulerAngles.y + 180, 0);

            offsetFilters[modeledTag.ID].Update(locationOffset, rotationOffsetY);
        }


        private void AlignWithTwoTags(TagPose modeledTag1, TagPose modeledTag2, TagPose detectedTag1,
            TagPose detectedTag2)
        {
            // 使用Tag1对齐位置
            var locationOffset = detectedTag1.Position - modeledTag1.Position;

            // 使用Tag1到Tag2的方向 对齐旋转
            var modeledDirection = (modeledTag2.Position - modeledTag1.Position).normalized;
            var detectedDirection = (detectedTag2.Position - detectedTag1.Position).normalized;
            var rotationOffset = Quaternion.FromToRotation(modeledDirection, detectedDirection);
            var rotationOffsetY = Quaternion.Euler(0, rotationOffset.eulerAngles.y, 0);

            offsetFilters[modeledTag1.ID].Update(locationOffset, rotationOffsetY);
        }
    }
}