using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SitePatrol
{
    public class ToastMessage : MonoBehaviour
    {
        private readonly List<TMP_Text> toastTexts = new();

        public TMP_Text toastTextPrefab;

        public RectTransform root;

        private readonly Dictionary<TMP_Text, DateTime> disappearTimes = new();

        private void Update()
        {
            for (int i = 0; i < toastTexts.Count; i++)
            {
                if (disappearTimes.ContainsKey(toastTexts[i]) && disappearTimes[toastTexts[i]] < DateTime.Now)
                {
                    toastTexts[i].text = "";
                    disappearTimes.Remove(toastTexts[i]);
                    toastTexts[i].gameObject.SetActive(false);
                }
            }

            var activeCount = toastTexts.FindAll(t => t.gameObject.activeSelf).Count;
            root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, activeCount * 55 + 10);
        }

        public void ShowMessage(string message)
        {
            foreach (var t in toastTexts)
            {
                if (t.gameObject.activeSelf) continue;
                t.text = message;
                disappearTimes[t] = DateTime.Now.AddSeconds(5);
                t.gameObject.SetActive(true);
                return;
            }

            var text = Instantiate(toastTextPrefab, root);
            text.text = message;
            toastTexts.Add(text);
            disappearTimes[text] = DateTime.Now.AddSeconds(5);
        }
    }
}