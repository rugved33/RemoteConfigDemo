using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TroopInfo
{
    public GameObject soldierPrefab;
    public string settingsID;
    public string maxTroopID;
    public TeamType teamType;
}

[CreateAssetMenu(fileName = "Troop Info", menuName = "Troop Info", order = 51)]
public class TroopInfoData : ScriptableObject
{
    public TroopInfo[] troopsConfig;
}
