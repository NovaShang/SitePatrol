using System.Collections;
using System.IO;
using GLTFast;
using UnityEngine;
using UnityEngine.Networking;

namespace SitePatrol
{
    public class GltfLoader : MonoBehaviour
    {
        public GameObject modelRoot;

        public bool addColliders = false;

        private bool modelLoaded = false;

        public ToastMessage toastMessage;

        private void Update()
        {
            if (!modelLoaded && WebApiClient.ModelFileUrl != null)
            {
                modelLoaded = true;
                var useCache = Application.platform != RuntimePlatform.WebGLPlayer;
                StartCoroutine(LoadGltf(WebApiClient.ModelFileUrl, useCache));
            }
        }


        // 根据 URL 生成一个唯一的缓存文件名
        string GetCachedFilePath(string url)
        {
            // 使用 GetHashCode 生成一个简单的哈希值，也可以用其他更稳健的方式
            string hash = url.GetHashCode().ToString();
            return Path.Combine(Application.persistentDataPath, hash + ".gltf");
        }

        IEnumerator LoadGltf(string url, bool cache = true)
        {
            if (cache)
            {
                string cachedFilePath = GetCachedFilePath(url);

                if (!File.Exists(cachedFilePath))
                {
                    Debug.Log("从网络下载 glTF: " + url);
                    toastMessage?.ShowMessage("Downloading Model...");
                    UnityWebRequest www = UnityWebRequest.Get(url);
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("下载错误: " + www.error);
                        toastMessage?.ShowMessage("Download Error: " + www.error);
                        yield break;
                    }
                    else
                    {
                        var gltfData = www.downloadHandler.data;
                        File.WriteAllBytes(cachedFilePath, gltfData);
                    }
                }

                url = "file://" + cachedFilePath;
            }


            // 创建一个空 GameObject 用于加载模型
            GameObject gltfObject = new GameObject("GLTF Model");
            gltfObject.transform.SetParent(modelRoot.transform);
            var gltfImporter = gltfObject.AddComponent<GltfAsset>();

            yield return gltfImporter.Load(url);

            yield return new WaitUntil(() => gltfObject.transform.childCount > 0);
            yield return new WaitForSeconds(1);


            SetLayerRecursively(gltfObject, 8);
            if (addColliders)
                AddCollidersRecursively(gltfObject);
        }

        /// <summary>
        /// 递归为 GameObject 及其子对象添加 MeshCollider（如果存在 MeshFilter）
        /// </summary>
        void AddCollidersRecursively(GameObject obj)
        {
            if (obj == null) return;

            // 如果当前对象有 MeshFilter，则添加 MeshCollider
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                MeshCollider collider = obj.GetComponent<MeshCollider>();
                if (collider == null)
                {
                    collider = obj.AddComponent<MeshCollider>();
                    collider.sharedMesh = meshFilter.sharedMesh;
                    // 如果需要碰撞器为凸面，可以设置 collider.convex = true;
                }
            }

            // 递归处理所有子对象
            foreach (Transform child in obj.transform)
                AddCollidersRecursively(child.gameObject);

            toastMessage?.ShowMessage("Model Loaded");
        }

        void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null)
                return;

            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}