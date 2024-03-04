
using UnityEngine;
using ConfigModel;

public class UpgradableEntityItem
{
    public string Id { get; private set; }
    public int CurrentTier { get; private set; }
    public int MaxTier { get; private set; }
    private UpgradeManager upgradeManager;

    public UpgradableEntityItem(string id, int initialTier, UpgradeManager manager)
    {
        Id = id;
        CurrentTier = initialTier;
        upgradeManager = manager;
    }

    public void Upgrade(out UpgradableObject upgradableObject)
    {
        upgradableObject = null;
        var (canUpgrade, currentTier, maxTier) = CanUpgrade();
        if (canUpgrade)
        {
            CurrentTier++;
            var upgradeDetails = upgradeManager.GetUpgradeDetails(Id, CurrentTier);
            if (upgradeDetails != null)
            {
                upgradableObject = upgradeDetails;
                Debug.Log($"Item {Id} upgraded to Tier {CurrentTier} with Damage: {upgradeDetails.damage}");
            }
            else
            {
                Debug.LogWarning($"Upgrade details for Item {Id} at Tier {CurrentTier} not found.");
            }
        }
        else
        {
            Debug.LogWarning($"Item {Id} cannot be upgraded further. It's already at the maximum tier or upgrade not allowed.");
        }
    }
    public void GetDefault(out UpgradableObject upgradableObject)
    {
        upgradableObject = null;

        var upgradeDetails = upgradeManager.GetUpgradeDetails(Id, CurrentTier);
        if (upgradeDetails != null)
        {
            // Here, you can update the item's properties, e.g., damage
            upgradableObject = upgradeDetails;
            Debug.Log($"Item {Id} upgraded to Tier {CurrentTier} with Damage: {upgradeDetails.damage}");
        }
        else
        {
            Debug.LogWarning($"Upgrade details for Item {Id} at Tier {CurrentTier} not found.");
        }
    }

    public (bool canUpgrade, int currentTier, int maxTier) CanUpgrade()
    {
        return upgradeManager.CanUpgradeItem(Id, CurrentTier);
    }
}