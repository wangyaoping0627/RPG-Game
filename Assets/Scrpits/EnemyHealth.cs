 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int currentHealth=10;
    public int maxHealth=10;
    public Image Image;
    public GameObject BloodUI;
    private void Start()
    {
        currentHealth = maxHealth;
    }
    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        else if (currentHealth <= 0)
        {
            currentHealth = 0;
            gameObject.SetActive(false);
            BloodUI.gameObject.SetActive(false);
        }
        //血量UI的百分比
        Image.fillAmount = (float)currentHealth / maxHealth;
    }

}
