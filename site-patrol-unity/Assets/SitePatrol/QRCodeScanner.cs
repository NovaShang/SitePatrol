using System.Text.RegularExpressions;
using UnityEngine;
using ZXing;

namespace SitePatrol
{
    public class QRCodeScanner : MonoBehaviour
    {
        private IBarcodeReader barcodeReader;
        public CameraImageHandler image;
        private Texture2D cameraTexture;
        public WebApiClient manager;
        public GameObject scanUI;

        void Start()
        {
            barcodeReader = new BarcodeReader();
            image.OnImageReceived += OnImageReceived;
        }

        private void OnImageReceived(Color32[] pixels, int width, int height, float fov,
            Vector3 camposition, Quaternion camrotation)
        {
            var result = barcodeReader.Decode(pixels, width, height);
            if (result is {Text: not null} && Regex.Match(result.Text, "^[0-9a-fA-F]{24}$").Success)
            {
                manager.MobileInit(result.Text);
                manager.gameObject.SetActive(true);
                scanUI.SetActive(false);
                gameObject.SetActive(false);
            }
        }
    }
}