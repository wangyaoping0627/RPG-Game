using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ExpController : MonoBehaviour
{
    public int currentExp = 0;
    public int maxExp = 10;
    public int level = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GainExp(2);
        }
    }
    public void GainExp(int amount)
    {
        currentExp += amount;
        if (currentExp >= maxExp)
        {
            LevelUp();
        }
    }
    public void LevelUp()
    {
        level++;
        currentExp -= maxExp;
        maxExp += level + 3;
    }
    
    
}
