using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ConfigModel;

namespace DemoGame
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance;
        private Dictionary<string, List<UpgradableListItem>> _allEntitiesWithUpgrades = new Dictionary<string, List<UpgradableListItem>>();
        public Dictionary<string, List<UpgradableListItem>> AllEntities => _allEntitiesWithUpgrades;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            GameManager.RemoteConfig.OnConfigLoaded += OnLoaded;
        }

        private void OnLoaded()
        {
            _allEntitiesWithUpgrades = GameManager.Instance.GetAllEntitiesWithUpgradableItems();

            if (_allEntitiesWithUpgrades != null)
            {
                foreach (var entityEntry in _allEntitiesWithUpgrades)
                {
                    string entityId = entityEntry.Key;
                    List<UpgradableListItem> upgradableItems = entityEntry.Value;

                    Debug.Log($"Entity ID: {entityId}, Upgradable Items Count: {upgradableItems.Count}");
                    foreach (var item in upgradableItems)
                    {
                        Debug.Log($"\tItem Name: {item.name}, Tier: {item.upgradableObject.tier}, Damage: {item.upgradableObject.damage}");
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve entities or upgradable items.");
            }

        }
        public (bool canUpgrade, int currentTier, int maxTier) CanUpgradeItem(string itemId, int currentTier)
        {
            int maxTier = GetMaxTierForItem(itemId);

            bool canUpgrade = currentTier < maxTier;
            return (canUpgrade, currentTier, maxTier);
        }

        public UpgradableObject GetUpgradeDetails(string itemId, int tier)
        {
            if (AllEntities.TryGetValue(itemId, out List<UpgradableListItem> items))
            {
                var itemDetail = items.FirstOrDefault(item => item.upgradableObject.tier == tier);
                if (itemDetail != null)
                {
                    return itemDetail.upgradableObject;
                }
                else
                {
                    Debug.LogWarning($"No upgrade details found for Item ID '{itemId}' at Tier '{tier}'.");
                }
            }
            else
            {
                Debug.LogWarning($"Item ID '{itemId}' not found.");
            }
            return null;
        }

        public int GetMaxTierForItem(string itemId)
        {
            int maxTier = 0;
            if (AllEntities.TryGetValue(itemId, out List<UpgradableListItem> items))
            {
                maxTier = items.Max(item => item.upgradableObject.tier);
            }

            if (maxTier == 0)
            {
                Debug.LogWarning($"No upgrade details found for Item ID '{itemId}'.");
            }

            return maxTier;
        }
    }
}