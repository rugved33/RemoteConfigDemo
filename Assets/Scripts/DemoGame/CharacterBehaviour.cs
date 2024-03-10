using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.AI;

namespace DemoGame
{
    public enum TargetType
    {
        Primary,
        Secondary,
    }

    public enum TeamType
    {
        PlayerTeam,
        EnemyTeam,
    }
    public abstract class CharacterBehaviour : MonoBehaviour
    {
        private const string UNIT_TAG = "unit";
        private const float MIN_ATTACK_RANGE = 10.1f;
        private bool _isAlive = true;
        [SerializeField] protected List<CharacterBehaviour> _secondaryEnemies = new List<CharacterBehaviour>();
        [SerializeField] protected List<CharacterBehaviour> _primaryEnemies = new List<CharacterBehaviour>();
        [SerializeField] protected CharacterSettings _settings;
        [SerializeField] protected TargetType _targetType;
        [SerializeField] protected TeamType _teamType;

        [Space(15)]
        [SerializeField] protected HealthSystem _healthSystem;
        [Space(15)]
        [SerializeField] protected bool _addBonusDamage;
        protected CharacterBehaviour _currentTarget;
        protected System.Action OnAttacked;
        public System.Action OnDamageDealt;
        public UnityAction<CharacterBehaviour> OnDeadEvent;

        #region  CHARACTER STATS

        public TargetType TargetType
        {
            get => _targetType;
        }
        public TeamType TeamType
        {
            get => _teamType;
        }

        public float AttackRange
        {
            get => _settings.AttackRange;
        }
        public float RotateSpeed
        {
            get => _settings.RotateSpeed;
        }
        public int MaxHp
        {
            get => _settings.MaxHp;
        }
        public float ChaseRange
        {
            get => _settings.ChaseRange;
        }
        public int AttackDamage
        {
            get;
            set;
        }
        public bool EnableDoubleDamage
        {
            get;
            set;
        }
        #endregion

        public bool IsAlive
        {
            get => _isAlive;
            set => _isAlive = value;
        }

        public virtual void Awake()
        {
            _healthSystem.SetHealth(_settings.MaxHp);
            _healthSystem.OnDeadEvent += Kill;
        }

        protected bool CanAttack()
        {
            if (_currentTarget != null && _isAlive) { return true; }
            return false;
        }

        public virtual void ApplyDamage(int damage)
        {
            if (EnableDoubleDamage)
            {
                _healthSystem.ApplyDamage(damage * 2);
            }
            else
            {
                _healthSystem.ApplyDamage(damage);
            }
            OnDamageDealt?.Invoke();
        }

        public virtual void Kill()
        {
            if (!_isAlive) { return; }


            if (TryGetComponent(out CapsuleCollider collider))
            {
                collider.enabled = false;
            }

            OnDeadEvent?.Invoke(this);
            _isAlive = false;
            _currentTarget = null;
            StopCoroutine(Co_Attack());
        }

        protected bool IsEnemyInRange()
        {
            if (_currentTarget != null)
            {
                if (Vector3.Distance(_currentTarget.transform.position, transform.position) < AttackRange)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsPrimaryTarget()
        {
            if (_currentTarget != null)
            {
                if (_currentTarget.TargetType == TargetType.Primary && _currentTarget.TeamType != TeamType)
                {
                    return true;
                }
            }
            return false;
        }

        protected void FindEnemyTarget()
        {
            var closestDistance = Mathf.Infinity;

            if (_currentTarget != null && !IsPrimaryTarget())
            {
                return;
            }

            void UpdateClosestTarget(List<CharacterBehaviour> potentialTargets)
            {
                foreach (var potentialTarget in potentialTargets)
                {
                    float distance = Vector3.Distance(transform.position, potentialTarget.gameObject.transform.position);
                    if (distance < closestDistance && potentialTarget.IsAlive && TeamType != potentialTarget.TeamType)
                    {
                        closestDistance = distance;
                        _currentTarget = potentialTarget;
                    }
                }
            }

            UpdateClosestTarget(_secondaryEnemies);


            if (_secondaryEnemies.Count == 0 || _currentTarget == null)
            {
                UpdateClosestTarget(_primaryEnemies);
            }

            RemoveRedundantTargets();
        }


        protected void RemoveRedundantTargets()
        {
            if (_currentTarget != null)
            {
                if (!_currentTarget.IsAlive)
                {
                    if (_secondaryEnemies.Contains(_currentTarget)) { _secondaryEnemies.Remove(_currentTarget); }
                    if (_primaryEnemies.Contains(_currentTarget)) { _primaryEnemies.Remove(_currentTarget); }
                    _currentTarget = null;
                }
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(UNIT_TAG))
            {
                if (other.gameObject.TryGetComponent(out CharacterBehaviour characterBehaviour))
                {
                    if (characterBehaviour != this)
                    {
                        AddEnemy(characterBehaviour);
                    }
                }
            }
        }
        protected virtual void OnTriggerStay(Collider other)
        {
            if (other.gameObject.CompareTag(UNIT_TAG))
            {
                if (other.gameObject.TryGetComponent(out CharacterBehaviour characterBehaviour))
                {
                    if (characterBehaviour != this)
                    {
                        AddEnemy(characterBehaviour);
                    }
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag(UNIT_TAG))
            {
                if (other.gameObject.TryGetComponent(out CharacterBehaviour characterBehaviour))
                {
                    if (characterBehaviour != this)
                        RemoveEnemy(characterBehaviour);
                }
            }
        }
        private void AddEnemy(CharacterBehaviour target)
        {
            if (!_secondaryEnemies.Contains(target) && target.IsAlive)
            {
                if (target.TargetType == TargetType.Secondary && target.TeamType != TeamType)
                    _secondaryEnemies.Add(target);
            }

            if (!_primaryEnemies.Contains(target) && target.IsAlive)
            {
                if (target.TargetType == TargetType.Primary && target.TeamType != TeamType)
                    _primaryEnemies.Add(target);
            }
        }


        private void RemoveEnemy(CharacterBehaviour target)
        {
            if (_secondaryEnemies.Contains(target))
            {
                if (target.TargetType == TargetType.Secondary && target.TeamType != TeamType)
                {
                    _secondaryEnemies.Remove(target);

                    if (_currentTarget == target)
                    {
                        _currentTarget = null;
                    }
                }
            }

            if (_primaryEnemies.Contains(target))
            {
                if (target.TargetType == TargetType.Primary && target.TeamType != TeamType)
                {
                    _primaryEnemies.Remove(target);

                    if (_currentTarget == target)
                    {
                        _currentTarget = null;
                    }
                }
            }
        }
        protected bool IsOutOfSight()
        {
            if (_currentTarget != null)
            {
                if (Vector3.Distance(transform.position, _currentTarget.transform.position) > ChaseRange)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void ResetCharacter()
        {
            _secondaryEnemies.Clear();
            _primaryEnemies.Clear();
            gameObject.SetActive(true);
            _isAlive = true;
            _healthSystem.Reset();
        }


        protected void AttackCurrentTarget()
        {
            if (_currentTarget != null)
            {
                if (Vector3.Distance(transform.position, _currentTarget.transform.position) <= MIN_ATTACK_RANGE)
                {
                    _currentTarget.ApplyDamage(AttackDamage);
                }
            }
        }

        protected IEnumerator Co_Attack()
        {
            while (_currentTarget != null)
            {
                yield return new WaitForSeconds(1 / (_settings.AttackRate / 60));
                AttackCurrentTarget();
            }
            yield return null;
        }

        public void AttackEventListener()
        {
            AttackCurrentTarget();
        }

        protected void DisableComponents()
        {
            if (gameObject.TryGetComponent(out CapsuleCollider collider))
            {
                if (collider) { collider.enabled = false; }
            }
            if (gameObject.TryGetComponent(out SphereCollider sphereCollider))
            {
                if (sphereCollider) { sphereCollider.enabled = false; }
            }
            if (gameObject.TryGetComponent(out NavMeshAgent navMeshAgent))
            {
                if (navMeshAgent) { navMeshAgent.enabled = false; }
            }
        }

        private void ClearEnemies()
        {
            _currentTarget = null;
            _primaryEnemies.Clear();
            _secondaryEnemies.Clear();
        }

        public virtual void LoadCharacterLevelData(CharacterSettings settings)
        {
            _settings = settings;
        }
        public virtual void RegisterAttacker(CharacterBehaviour characterBehaviour)
        {

        }
    }
}