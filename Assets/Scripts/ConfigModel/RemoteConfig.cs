using System.Collections.Generic;

namespace ConfigModel
{
    [System.Serializable]
    public class RemoteConfig
    {
        public List<Entity> entities;
        public Dictionary<string, object> global = new Dictionary<string, object>();
    }
}

