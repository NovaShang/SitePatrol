using System;
using System.Threading.Tasks;
using UnityEngine.XR.ARFoundation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;

namespace SitePatrol
{
    public class CameraImageHandler : MonoBehaviour
    {
        public bool needTransform = false;
        public ARCameraManager cameraManager;
        private Texture2D cameraTexture;
        private Camera arCamera;
        private volatile bool workerBusy = false;
        private int width;
        private int height;
        public RawImage camPreview;

        public delegate void ImageDelegate(Color32[] pixels, int width, int height, float fov,
            Vector3 camPosition, Quaternion camRotation);

        public ImageDelegate OnImageReceived;

        public byte[] GetLatestJpg()
        {
            if (cameraTexture == null)
                return null;

            if (!needTransform) return cameraTexture.EncodeToJPG();

            var originalPixels = cameraTexture.GetPixels32();
            var originalWidth = cameraTexture.width;
            var originalHeight = cameraTexture.height;

            var rotatedPixels = RotatePixels90Clockwise(originalPixels, originalWidth, originalHeight);

            var tempTex = new Texture2D(originalHeight, originalWidth, TextureFormat.RGBA32, false);
            tempTex.SetPixels32(rotatedPixels);
            tempTex.Apply();

            var jpgData = tempTex.EncodeToJPG();
            Destroy(tempTex);
            return jpgData;
        }

        private void OnEnable()
        {
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer)
                needTransform = true;
            if (cameraManager == null) return;
            cameraManager.frameReceived += OnCameraFrameReceived;
            arCamera = cameraManager.GetComponent<Camera>();
        }

        private void OnDisable()
        {
            if (cameraManager != null)
                cameraManager.frameReceived -= OnCameraFrameReceived;
        }

        private async void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            var camPosition = arCamera.transform.position;
            var camRotation = arCamera.transform.rotation;
            var fov = arCamera.fieldOfView;

            if (workerBusy) return;
            try
            {
                workerBusy = true;

                if (!cameraManager.TryAcquireLatestCpuImage(out var cpuImage)) return;
                using (cpuImage)


                    CopyImageToTexture(cpuImage, out width, out height);


                var pixels = cameraTexture.GetPixels32();

                await Task.Run(() =>
                {
                    if (needTransform)
                        pixels = RotatePixels90Clockwise(pixels, width, height);
                });
                if (needTransform)
                    (width, height) = (height, width);
                OnImageReceived?.Invoke(pixels, width, height, fov, camPosition, camRotation);
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

        private void CopyImageToTexture(XRCpuImage cpuImage, out int w, out int h)
        {
            w = cpuImage.width;
            h = cpuImage.height;

            if (cameraTexture == null || cameraTexture.width != w || cameraTexture.height != h)
            {
                cameraTexture = new Texture2D(w, h, TextureFormat.RGBA32, false);
                if (camPreview) camPreview.texture = cameraTexture;
            }

            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, w, h),
                outputDimensions = new Vector2Int(w, h),
                outputFormat = TextureFormat.RGBA32,
                transformation = needTransform
                    ? XRCpuImage.Transformation.MirrorY
                    : XRCpuImage.Transformation.None
            };

            var bufferSize = cpuImage.GetConvertedDataSize(conversionParams);
            using var buffer =
                new Unity.Collections.NativeArray<byte>(bufferSize, Unity.Collections.Allocator.Temp);
            cpuImage.Convert(conversionParams, buffer);
            cameraTexture.LoadRawTextureData(buffer);
            cameraTexture.Apply();
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