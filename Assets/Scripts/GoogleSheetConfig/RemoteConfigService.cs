using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;

namespace GoogleSheetConfig
{
    public class RemoteConfigService<TConfig> where TConfig : class, new()
    {
        private const string REMOTE_CONFIG_JSON_FILE_NAME = "remote_config.json";
        private const string REMOTE_CONFIG_FALLBACK_FILE_NAME = "remote_config_fallback";

        public TConfig ConfigData;
        public string ResponseString;
        public delegate void ConfigLoadedCallback();
        public event ConfigLoadedCallback OnConfigLoaded;
        private static readonly HttpClient httpClient = new HttpClient();
        private string url;

        public async Task Initialize(string url)
        {
            this.url = url;
            await FetchConfig();
        }

        private async Task FetchConfig()
        {
            try
            {
                string responseString = await httpClient.GetStringAsync(url);
                ConfigData = JsonConvert.DeserializeObject<TConfig>(responseString);
                SaveConfigToFileObject(ConfigData);
                OnConfigLoaded?.Invoke();
                ResponseString = responseString;
            }
            catch (HttpRequestException e)
            {
                Debug.Log($"Failed to fetch config: {e.Message}");
                LoadFallbackConfig();
            }
        }

        private void LoadFallbackConfig()
        {
            string filePath = Path.Combine(Application.persistentDataPath, REMOTE_CONFIG_JSON_FILE_NAME);

            if (File.Exists(filePath))
            {
                string savedJson = File.ReadAllText(filePath);
                ResponseString = savedJson;
                ConfigData = JsonConvert.DeserializeObject<TConfig>(savedJson);
                OnConfigLoaded?.Invoke();
                Debug.Log("Loaded from local save path.");
            }
            else
            {
                TextAsset jsonResource = Resources.Load<TextAsset>(REMOTE_CONFIG_FALLBACK_FILE_NAME);

                if (jsonResource != null)
                {
                    ResponseString = jsonResource.text;
                    ConfigData = JsonConvert.DeserializeObject<TConfig>(jsonResource.text);
                    OnConfigLoaded?.Invoke();
                    Debug.Log("Loaded from resources folder.");
                }
                else
                {
                    Debug.LogError("Fallback JSON resource not found!");
                }
            }
        }

        private void SaveConfigToFileObject(TConfig configData)
        {
            string filePath = Path.Combine(Application.persistentDataPath, REMOTE_CONFIG_JSON_FILE_NAME);
            string jsonContent = JsonConvert.SerializeObject(configData);

#if UNITY_EDITOR

            SaveUpdatedJsonToResources(jsonContent);

            string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, REMOTE_CONFIG_JSON_FILE_NAME);
            File.WriteAllText(streamingAssetsPath, jsonContent);
#endif

            File.WriteAllText(filePath, jsonContent);
            Debug.Log("Config data saved.");
        }

#if UNITY_EDITOR
        private static void SaveUpdatedJsonToResources(string updatedJson)
        {
            string resourcesPath = Path.Combine(Application.dataPath, "Resources", REMOTE_CONFIG_FALLBACK_FILE_NAME + ".json");
            File.WriteAllText(resourcesPath, updatedJson);
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log("Saved updated JSON to Resources for Editor fallback.");
        }
#endif

    }
}