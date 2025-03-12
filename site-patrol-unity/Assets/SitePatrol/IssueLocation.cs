using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SitePatrol
{
    public class IssueLocation : MonoBehaviour
    {
        public List<IssueManager.Issue> issues;

        public List<GameObject> uiItems;

        public TMP_Text descriptionText;
        public Image image;
        public TMP_Text createTimeText;
        public TMP_Text updateTimeText;
        public Vector3 position;

        public void Update()
        {
        }


        private void UpdateIssue()
        {
            
        }


        private async void LoadIssueImage(string imageUrl, Image targetImage)
        {
            using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imageUrl);
            await uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Failed to load image: {imageUrl}");

            var texture = DownloadHandlerTexture.GetContent(uwr);
            targetImage.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.zero
            );
        }
    }
}