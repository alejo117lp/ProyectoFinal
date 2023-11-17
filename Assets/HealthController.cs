using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthController : MonoBehaviour
{
    public int Health = 3;
    private int currentHealth;

    public GameObject[] healthObjects;

    private void Start()
    {
        currentHealth = Health;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        for (int i = 0; i < healthObjects.Length; i++)
        {
            healthObjects[i].SetActive(i < currentHealth);
        }

        if (currentHealth <= 0)
        {
            Debug.Log("Game Over");
        }
    }

    public void TakeDamage()
    {
        if (currentHealth > 0)
        {
            currentHealth--;
            UpdateHealthUI();
        }
    }
}
