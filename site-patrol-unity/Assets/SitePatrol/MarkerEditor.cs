using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using AprilTag;
using Newtonsoft.Json;
using RuntimeGizmos;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace SitePatrol
{
    public class MarkerEditor : MonoBehaviour
    {
        public TransformGizmo gizmo;

        private GameObject markerPrefab;

        public GameObject markersRoot;

        public bool WaitForClick = false;

        public TMP_Text statusText;

        private bool initialized = false;

        private int currentId = 0;

        private Transform lastSelectedMarker = null;

        public TMP_InputField idInputField;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            markerPrefab = Resources.Load<GameObject>("MarkerTemplate");
        }

        // Update is called once per frame
        void Update()
        {
            if (gizmo.mainTargetRoot != null)
            {
                lastSelectedMarker = gizmo.mainTargetRoot;
            }

            if (!WaitForClick && Global.Markers.Count == 0)
            {
                statusText.text = "Add at least 1 AprilTag. 2 for better tracking.";
            }

            if (!initialized && Global.Markers.Count > 0)
            {
                initialized = true;
                LoadAll();
            }

            if (WaitForClick && Input.GetMouseButtonDown(0))
            {
                WaitForClick = false;
                statusText.text = "";
                AddMarkerAtCursor();
            }
        }

        public void AddMarker()
        {
            currentId = int.Parse(idInputField.text);
            WaitForClick = true;
            statusText.text = "Click to place a AprilTag marker";
        }

        public void RemoveMarker()
        {
            Destroy(lastSelectedMarker.gameObject);
            gizmo.ClearTargets();
        }

        private void AddMarkerAtCursor()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 10))
            {
                Vector3 spawnPosition = hit.point;
                Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
                // 实例化预制体
                var instance = Instantiate(markerPrefab, spawnPosition, spawnRotation);
                instance.transform.SetParent(markersRoot.transform, false);
                var visualMarker = instance.GetComponent<VisualMarker>();
                visualMarker.id = currentId;
                var text = visualMarker.GetComponentInChildren<TMP_Text>();
                text.text = currentId.ToString();
                // 让 gizmo 指向新创建的标记
                gizmo.ClearTargets();
                gizmo.AddTarget(instance.transform);
            }
        }

        private void LoadAll()
        {
            // 清除所有标记,删除markersRoot下的所有子物体
            foreach (Transform child in markersRoot.transform)
            {
                Destroy(child.gameObject);
            }

            // 加载所有标记
            foreach (var marker in Global.Markers)
            {
                var instance = Instantiate(markerPrefab, marker.Position, marker.Rotation);
                instance.transform.SetParent(markersRoot.transform, false);
                var visualMarker = instance.GetComponent<VisualMarker>();
                if (visualMarker != null)
                {
                    visualMarker.id = marker.ID;
                    var text = visualMarker.GetComponentInChildren<TMP_Text>();
                    text.text = marker.ID.ToString();
                }
            }
        }

        public async void SaveAll()
        {
            // 遍历 markersRoot下的所有子物体，存到Global.Markers
            Global.Markers.Clear();
            foreach (Transform child in markersRoot.transform)
            {
                var visualMarker = child.GetComponent<VisualMarker>();
                if (visualMarker != null)
                {
                    var marker = new TagPose(
                        visualMarker.id,
                        child.position,
                        child.rotation);
                    Global.Markers.Add(marker);
                }
            }

            await WebApis.Call("PUT", $"/api/v1/model_files/{Global.ModelFileId}/markers", new
            {
                markers = Global.Markers.Select(x => new
                {
                    id = x.ID.ToString(),
                    position = new[] {x.Position.x, x.Position.y, x.Position.z},
                    orientation = new[] {x.Rotation.eulerAngles.x, x.Rotation.eulerAngles.y, x.Rotation.eulerAngles.z}
                })
            }, 1);
        }
    }
}