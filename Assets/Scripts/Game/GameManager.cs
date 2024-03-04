using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;
using GoogleSheetConfig;
using ConfigModel;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string url;
    public static RemoteConfigService<RemoteConfig> RemoteConfig = new RemoteConfigService<RemoteConfig>();
    public static UnityAction OnConfigLoaded;

    public async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        await RemoteConfig.Initialize(url);

        OnConfigLoaded?.Invoke();
    }


    public T GetGlobalValue<T>(string key)
    {
        if (RemoteConfig.ConfigData.global.ContainsKey(key))
        {
            object value = RemoteConfig.ConfigData.global[key];

            if (value is T)
            {
                return (T)value;
            }
            else
            {
                Debug.LogWarning($"Type mismatch for global value with key '{key}'. Expected type: {typeof(T)}, Actual type: {value.GetType()}");
            }
        }
        else
        {
            Debug.LogWarning($"Key '{key}' not found in remote configuration. Available keys: {string.Join(", ", RemoteConfig.ConfigData.global.Keys)}");
        }

        Debug.LogWarning($"Failed to retrieve global value for key: {key}");
        return default(T);
    }

    public Dictionary<string, List<UpgradableListItem>> GetAllEntitiesWithUpgradableItems()
    {
        var remoteConfigManager = RemoteConfig;
        var entitiesWithUpgradables = new Dictionary<string, List<UpgradableListItem>>();

        if (remoteConfigManager.ConfigData != null && remoteConfigManager.ConfigData.entities != null)
        {
            foreach (var entity in remoteConfigManager.ConfigData.entities)
            {
                if (!string.IsNullOrWhiteSpace(entity.name)) // Ensure entity name is not null or whitespace
                {
                    entitiesWithUpgradables[entity.name] = entity.upgradableList;
                }
                else
                {
                    Debug.LogWarning($"An entity with ID {entity.id} is missing a name.");
                }
            }
            return entitiesWithUpgradables;
        }
        else
        {
            Debug.LogError("Remote configuration not loaded or contains no entities.");
            return null;
        }
    }

}
