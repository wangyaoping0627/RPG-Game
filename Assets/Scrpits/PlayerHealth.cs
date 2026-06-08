using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public TMP_Text healthUI;
    public Animator healthUpdate;
    private void Start()
    {
        healthUI.text = "HP:" + StatsManager.Instance.currentHealth + "/" + StatsManager.Instance.maxHealth;
    }
    public void ChangeHealth(int amount)
    {
        StatsManager.Instance.currentHealth += amount;
        if (StatsManager.Instance.currentHealth > StatsManager.Instance.maxHealth) StatsManager.Instance.currentHealth = StatsManager.Instance.maxHealth;
        else if (StatsManager.Instance.currentHealth <= 0) {
            StatsManager.Instance.currentHealth = 0;
            gameObject.SetActive(false);
        }
        healthUpdate.Play("HealthUpdate");
        healthUI.text = "HP:" + StatsManager.Instance.currentHealth + "/" + StatsManager.Instance.maxHealth;
    }
}
