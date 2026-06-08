using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsUI : MonoBehaviour
{
    public GameObject[] statsSlots;
    public CanvasGroup canvasGroup;

    private void Start()
    {
        UpdateAllStats();
        
    }
    private void Update()
    {
        if (Input.GetButtonDown("OpenMenu")&& canvasGroup.alpha == 0)
        {
            Time.timeScale = 0;
            UpdateAllStats();
            canvasGroup.alpha = 1;
        }
        else if(Input.GetButtonDown("OpenMenu") && canvasGroup.alpha == 1)
        {
            Time.timeScale = 1;
            UpdateAllStats();
            canvasGroup.alpha = 0;
        }
    }
    public void UpdateMaxHealth()
    {
        statsSlots[0].GetComponentInChildren<TMP_Text>().text = "Damage: " + StatsManager.Instance.maxHealth;
    }
    public void UpdateDamage()
    {
        statsSlots[1].GetComponentInChildren<TMP_Text>().text = "Damage: " + StatsManager.Instance.damage;
    }
    public void UpdateSpeed()
    {
        statsSlots[2].GetComponentInChildren<TMP_Text>().text = "Speed: " + StatsManager.Instance.speed;
    }
    public void UpdateAllStats()
    {
        UpdateMaxHealth();
        UpdateDamage();
        UpdateSpeed();
    }
}
