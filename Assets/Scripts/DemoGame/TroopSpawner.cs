using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using ConfigModel;

namespace DemoGame
{
    public class TroopSpawner : MonoBehaviour
    {
        public TeamType teamType;
        public TroopInfoData troopInfoSO;
        public float radius = 2f;
        public UnityAction<int, int> OnTroopUpdated;
        public UnityAction<bool> OnDoubleDamageUpdated;
        public UnityAction<int> OnTierUpdated;
        private TroopInfo _currentTroopInfo;
        private int _maxTroopAmount;
        private int _troopAmount = 0;
        private int _attackDamage;
        private bool _enableDoubleDamage;
        private int _currentUpgradeTier;
        private UpgradableEntityItem _troopUpgradableItem;
        private List<CharacterBehaviour> _troops = new List<CharacterBehaviour>();

        private void Awake()
        {
            for (int i = troopInfoSO.troopsConfig.Length - 1; i >= 0; i--)
            {
                if (teamType == troopInfoSO.troopsConfig[i].teamType)
                {
                    _currentTroopInfo = troopInfoSO.troopsConfig[i];
                }
            }

            GameManager.OnConfigLoaded += OnRemoteConfigLoaded;
        }

        private void OnRemoteConfigLoaded()
        {
            _troopUpgradableItem = new UpgradableEntityItem(_currentTroopInfo.settingsID, 1, UpgradeManager.Instance);
            _maxTroopAmount = (int)GameManager.Instance.GetGlobalValue<Int64>(_currentTroopInfo.maxTroopID);

            OnTroopUpdated?.Invoke(_troopAmount, _maxTroopAmount);

            UpgradableObject details = null;
            _troopUpgradableItem.GetDefault(out details);  //load default configuration
            UpdateConfig(details);

        }

        public void SpawnTroop()
        {
            if (_troopAmount >= _maxTroopAmount)
            {
                Debug.Log("Reached Max Troop Amount");
                return;
            }

            GameObject bot = Instantiate(_currentTroopInfo.soldierPrefab, GetRandomSpawnPos(), Quaternion.identity);

            if (bot.TryGetComponent(out CharacterBehaviour component))
            {
                component.OnDeadEvent += RemoveTroop;
                component.AttackDamage = _attackDamage;
                component.EnableDoubleDamage = _enableDoubleDamage;

                _troops.Add(component);
                _troopAmount += 1;

                OnTroopUpdated?.Invoke(_troopAmount, _maxTroopAmount);
            }
        }
        public void UpgradeTroopSpawner()
        {
            UpgradableObject details = null;
            _troopUpgradableItem.Upgrade(out details);
            UpdateConfig(details);
        }

        public void RemoveTroop(CharacterBehaviour characterBehaviour)
        {
            if (_troops.Contains(characterBehaviour))
            {
                _troops.Remove(characterBehaviour);
                _troopAmount -= 1;

                OnTroopUpdated?.Invoke(_troopAmount, _maxTroopAmount);
            }
        }

        private void UpdateConfig(UpgradableObject upgradableObject)
        {
            if (upgradableObject != null)
            {
                _attackDamage = upgradableObject.damage;
                _enableDoubleDamage = upgradableObject.enableDoubleDamage;
                _currentUpgradeTier = upgradableObject.tier;

                for (int i = _troops.Count - 1; i >= 0; i--)
                {
                    _troops[i].AttackDamage = _attackDamage;
                    _troops[i].EnableDoubleDamage = _enableDoubleDamage;
                }

                OnTierUpdated?.Invoke(_currentUpgradeTier);
                OnDoubleDamageUpdated?.Invoke(_enableDoubleDamage);
            }
        }

        private Vector3 GetRandomSpawnPos()
        {
            Vector3 randomPos = UnityEngine.Random.insideUnitSphere * radius;
            randomPos += transform.position;
            randomPos.y = 0f;

            Vector3 direction = randomPos - transform.position;
            direction.Normalize();

            float dotProduct = Vector3.Dot(transform.forward, direction);
            float dotProductAngle = Mathf.Acos(dotProduct / transform.forward.magnitude * direction.magnitude);

            randomPos.x = Mathf.Cos(dotProductAngle) * radius + transform.position.x;
            randomPos.z = Mathf.Sin(dotProductAngle * (UnityEngine.Random.value > 0.5f ? 1f : -1f)) * radius + transform.position.z;

            return randomPos;
        }
    }
}
