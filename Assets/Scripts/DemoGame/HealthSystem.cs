using UnityEngine;
using UnityEngine.Events;

namespace DemoGame
{
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private HealthBar _healthBar;
        private int _maxHp;
        private int _currentHp;
        public UnityAction<int, int> OnHealthChange;
        public UnityAction OnDeadEvent;

        private void Awake()
        {
            if (_healthBar != null)
            {
                OnHealthChange += _healthBar.UpdateHealthBar;
                OnDeadEvent += _healthBar.DisableBar;
            }
        }
        public void SetHealth(int health)
        {
            _maxHp = health;
            _currentHp = health;
            OnHealthChange?.Invoke(_currentHp, _maxHp);
        }
        public void SetMaxHealth(int MaxHealth)
        {
            _maxHp = MaxHealth;
            _currentHp = _maxHp;
            OnHealthChange?.Invoke(_currentHp, _maxHp);
        }
        public void ApplyDamage(int damage)
        {
            if (_currentHp > 0)
            {
                _currentHp -= damage;
            }
            if (_currentHp <= 0)
            {
                _currentHp = 0;
                OnDeadEvent?.Invoke();
            }
            OnHealthChange?.Invoke(_currentHp, _maxHp);
        }

        public void Heal(int healAmount)
        {
            if (_currentHp < _maxHp)
            {
                _currentHp += healAmount;
                if (_currentHp >= _maxHp)
                {
                    _currentHp = _maxHp;
                }

                OnHealthChange?.Invoke(_currentHp, _maxHp);
            }
        }

        public void Reset()
        {
            _currentHp = _maxHp;
            OnHealthChange?.Invoke(_currentHp, _maxHp);
            _healthBar.EnableBar();
        }
    }
}