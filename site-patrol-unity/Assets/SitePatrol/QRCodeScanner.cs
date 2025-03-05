using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;

namespace SitePatrol
{
    public class QRCodeScanner : MonoBehaviour
    {
        private IBarcodeReader barcodeReader;
        public ARCameraManager arCameraManager;
        private Texture2D cameraTexture;
        public Global manager;
        public GameObject scanUI;

        void Awake()
        {
            barcodeReader = new BarcodeReader();
        }

        async void Update()
        {
            if (arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
            {
                int width, height;
                using (cpuImage)
                {
                    width = cpuImage.width;
                    height = cpuImage.height;

                    if (cameraTexture == null)
                        cameraTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

                    var conversionParams = new XRCpuImage.ConversionParams
                    {
                        inputRect = new RectInt(0, 0, width, height),
                        outputDimensions = new Vector2Int(width, height),
                        outputFormat = TextureFormat.RGBA32,
                        transformation = XRCpuImage.Transformation.None
                    };

                    var size = cpuImage.GetConvertedDataSize(conversionParams);
                    using var buffer = new NativeArray<byte>(size, Allocator.Temp);
                    // 转换
                    cpuImage.Convert(conversionParams, buffer);
                    cameraTexture.LoadRawTextureData(buffer);
                    cameraTexture.Apply();
                }

                Color32[] pixels = cameraTexture.GetPixels32();
                Result result = null;
                // 解码
                // await Task.Run(() => result = barcodeReader.Decode(pixels, width, height));
                result = barcodeReader.Decode(pixels, width, height);
                print(result);
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
}