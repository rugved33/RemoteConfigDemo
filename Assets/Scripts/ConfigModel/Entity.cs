using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ConfigModel
{
    [System.Serializable]
    public class Entity
    {
        public int id;
        public string name;
        public string description;
        public List<UpgradableListItem> upgradableList;
    }
}