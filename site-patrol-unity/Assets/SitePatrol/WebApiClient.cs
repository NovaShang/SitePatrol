using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AprilTag;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace SitePatrol
{
    public class WebApiClient : MonoBehaviour
    {
        public static string BaseUrl = null;
        public static string ModelFileUrl = null;
        public static string ModelFileId = null;
        public static List<TagPose> Markers = new List<TagPose>();
        public bool isWeb = false;


        void Start()
        {
            if (isWeb)
                WebInit();
        }

        async void WebInit()
        {
            string fullUrl = Application.absoluteURL;
            if (string.IsNullOrEmpty(fullUrl) || !fullUrl.StartsWith("http"))
                fullUrl =
                    "https://sitepatrol.shanggao.me/unity?model_file_id=67c7f2eb8525b88f0de4f461";

            var uri = new System.Uri(fullUrl);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            BaseUrl = uri.GetLeftPart(UriPartial.Authority);
            ModelFileId = query.Get("model_file_id");
            await RefreshModelFile();
        }

        public async void MobileInit(string modelFileId)
        {
            BaseUrl = "https://sitepatrol.shanggao.me";
            ModelFileId = modelFileId;
            await RefreshModelFile();
        }

        private async Task RefreshModelFile()
        {
            var result = await Call("GET", $"/api/v1/model_files/{ModelFileId}", 0, new
            {
                id = "",
                file_url = "",
                markers = new[]
                {
                    new
                    {
                        id = "",
                        position = new float[3],
                        orientation = new float[3],
                    }
                }
            });
            Markers = result.markers.Select(x => new TagPose(
                    int.Parse(x.id),
                    new Vector3(x.position[0], x.position[1], x.position[2]),
                    Quaternion.Euler(x.orientation[0], x.orientation[1], x.orientation[2])
                ))
                .ToList();
            ModelFileUrl = BaseUrl + result.file_url;
        }

        public static async Task<TOutput> Call<TInput, TOutput>(string method, string url, TInput input, TOutput output)
        {
            url = BaseUrl + url;
            UnityWebRequest req = null;
            var json = JsonConvert.SerializeObject(input);
            if (method == "GET") req = UnityWebRequest.Get(url);
            else if (method == "DELETE") req = UnityWebRequest.Delete(url);
            else if (method == "PUT")
            {
                req = UnityWebRequest.Put(url, json);
                req.SetRequestHeader("Content-Type", "application/json");
            }
            else if (method == "POST")
                req = UnityWebRequest.Post(url, json, "application/json");
            else
                throw new Exception("Unknokwn Method");

            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError ||
                req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + req.error);
                throw new Exception("Server Error: " + req.url + "\n" + req.error);
            }

            var result = JsonConvert.DeserializeAnonymousType(req.downloadHandler.text, output);
            return result;
        }
    }
}