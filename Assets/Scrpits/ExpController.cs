using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpController : MonoBehaviour
{
    public Image expBar;
    public TMPro.TMP_Text levelText;

    private void Start()
    {
        EnemyHealth[] allEnemies = FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth enemy in allEnemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                enemy.OnDeath += OnEnemyKilled;
            }
        }
        RefreshUI();
    }

    private void OnEnemyKilled(int expValue)
    {
        StatsManager.Instance.GainExp(expValue);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (expBar != null)
            expBar.fillAmount = (float)StatsManager.Instance.currentExp / StatsManager.Instance.maxExp;
        if (levelText != null)
            levelText.text = "Lv." + StatsManager.Instance.level;
    }
}
