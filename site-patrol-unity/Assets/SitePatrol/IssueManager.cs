using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace SitePatrol
{
    public class IssueManager : MonoBehaviour
    {
        public class CameraPose
        {
            public float[] Position { get; set; } = new[] {0f, 0f, 0f}; // e.g., [x, y, z]
            public float[] Orientation { get; set; } = new[] {0f, 0f, 0f}; // e.g., [pitch, yaw, roll]
        }

        public class Issue
        {
            public string Id { get; set; } // Assuming the ID is returned as a string
            public string IssueType { get; set; } // Issue type, such as "quality", "safety", "other"
            public string Description { get; set; } // Issue description
            public string ImageUrl { get; set; } // Optional image URL
            public CameraPose CameraPose { get; set; } // Record the current camera pose
            public float[] Position3D { get; set; } // The 3D coordinate of the issue
            public DateTime CreateTime { get; set; }
            public DateTime? UpdateTime { get; set; }
            public string ModelFileId { get; set; }

            public ToastMessage message;
        }


        public List<Issue> issues;
        public GameObject issuesRoot;
        public GameObject issuePrefab;
        public CameraImageHandler camera;
        public RaycastHandler raycast;
        private Dictionary<string, IssueLocation> issueIdToLocation = new();
        private List<IssueLocation> locations = new();
        public GameObject issuePanel;
        private Issue currentIssue;
        public ToastMessage message;

        public async void StartIssue()
        {
            var result = raycast.GetScreenCenterInBim();
            if (!result.HasValue) return;
            var bimPosition = result.Value;

            var image = camera.GetLatestJpg();
            var imageUrl = await WebApiClient.UploadImage(image);

            var issue = new Issue
            {
                Description = "New Issue",
                ImageUrl = imageUrl,
                Position3D = new[] {bimPosition.x, bimPosition.y, bimPosition.z},
                ModelFileId = WebApiClient.ModelFileId,
                CameraPose = new CameraPose(),
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                IssueType = "custom"
            };
            currentIssue = issue;
            ShowIssuePanel();
        }

        private void ShowIssuePanel()
        {
            var input = issuePanel.GetComponentInChildren<TMP_InputField>();
            input.text = currentIssue.Description;
            issuePanel.SetActive(true);
        }

        public async void SubmitIssue()
        {
            var input = issuePanel.GetComponentInChildren<TMP_InputField>();
            currentIssue.Description = input.text;
            message.ShowMessage("Issue Submitted!");
            issuePanel.SetActive(false);
            //await WebApiClient.Call("POST", $"/api/v1/model_files/{WebApiClient.ModelFileId}/issues", currentIssue, "");
            //RefreshIssues();
        }

        public async void CancelIssue()
        {
            issuePanel.SetActive(false);
        }

        private async void RefreshIssues()
        {
            issues = await WebApiClient.Call("GET", $"/api/v1/model_files/{WebApiClient.ModelFileId}/issues", 0,
                new List<Issue>());
        }

        private async void ShowIssues()
        {
            foreach (var issue in issues)
            {
                if (!issueIdToLocation.TryGetValue(issue.Id, out var location))
                {
                    var position = new Vector3(issue.Position3D[0], issue.Position3D[1], issue.Position3D[2]);
                    location = locations.FirstOrDefault(x => (x.position - position).magnitude < 0.5);
                    if (location == null)
                    {
                        var instance = Instantiate(issuePrefab, issuesRoot.transform);
                        instance.transform.localPosition = position;
                        location = instance.GetComponent<IssueLocation>();
                        location.position = position;
                        locations.Add(location);
                    }

                    issueIdToLocation[issue.Id] = location;
                    location.issues.Add(issue);
                }
                else
                {
                }
            }
        }
    }
}