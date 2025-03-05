using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace SitePatrol
{
    public class WebApis
    {
        public static async Task<TOutput> Call<TInput, TOutput>(string method, string url, TInput input, TOutput output)
        {
            url = Global.BaseUrl + url;
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