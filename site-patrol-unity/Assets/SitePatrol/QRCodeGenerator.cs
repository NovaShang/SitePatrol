using System;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

namespace SitePatrol
{
    public class QRCodeGenerator : MonoBehaviour
    {
        private string qrContent = null;
        public int qrWidth = 256; // 生成二维码的宽
        public int qrHeight = 256; // 生成二维码的高
        public RawImage display;

        private void Update()
        {
            if (WebApiClient.ModelFileId != null && qrContent == null)
            {
                qrContent = WebApiClient.ModelFileId;
                var texture = GenerateQR(qrContent);
                display.texture = texture;
            }
        }

        public Texture2D GenerateQR(string content)
        {
            // ZXing 的编码选项
            var writer = new BarcodeWriter<Color32[]>
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Width = qrWidth,
                    Height = qrHeight,
                    Margin = 1
                },
                Renderer = new Color32Renderer()
            };

            // 生成颜色数组
            Color32[] pix = writer.Write(content);
            // 创建 Texture2D
            Texture2D qrTexture = new Texture2D(qrWidth, qrHeight);
            qrTexture.SetPixels32(pix);
            qrTexture.Apply();
            return qrTexture;
        }
    }
}