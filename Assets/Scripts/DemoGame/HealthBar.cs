using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _healthBarImage;
    [SerializeField] private Image _bg;
    [SerializeField] private GameObject _healthBar;
    [SerializeField] private float _speed;

    public void UpdateHealthBar(int currentHp, int maxValue)
    {
        var rate = MathUtils.GetImageFillValue(maxValue, currentHp);
        _healthBarImage.fillAmount = rate;

        if (rate < 0.35f)
        {
            _healthBarImage.color = Color.red;
        }
        else
        {
            _healthBarImage.color = Color.white;
        }

        if (gameObject.activeInHierarchy)
            StartCoroutine(Co_UpdateHealthBar(rate));
    }

    private IEnumerator Co_UpdateHealthBar(float rate)
    {
        yield return new WaitForSeconds(0.3f);
        _bg.fillAmount = rate;
    }

    public void DisableBar()
    {
        _healthBar.SetActive(false);
    }

    public void EnableBar()
    {
        _healthBar.SetActive(true);
    }
}
