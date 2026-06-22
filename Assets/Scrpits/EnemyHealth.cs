using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int currentHealth = 10;
    public int maxHealth = 10;
    public int expValue = 3;
    public Image Image;
    public GameObject BloodUI;

    public event Action<int> OnDeath;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath?.Invoke(expValue);
            gameObject.SetActive(false);
            BloodUI.gameObject.SetActive(false);
        }

        Image.fillAmount = (float)currentHealth / maxHealth;
    }
}
