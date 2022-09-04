using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public Slider healthSlider;

    [SerializeField]
    private float health = 100;

    // Start is called before the first frame update
    void Start()
    {
        healthSlider.maxValue = health;
        healthSlider.value = health;
    }

    void UpdateHealthSlider()
    {
        healthSlider.value = health;
    }

    public void TakeDamage()
    {
        health -= 25;
        UpdateHealthSlider();

        if (health <= 0)
        {
            Die();
        }

        Client.Instance.SendDecimal(health);
    }

    public void SetHealth(float healthToSet)
    {
        health = healthToSet;

        if (health <= 0)
        {
            Die();
        }

        UpdateHealthSlider();
    }

    public void Die()
    {
        Client.Instance.HandleDisconnection();
        Debug.Log(gameObject.name + " died. So sad :(");
        //Destroy(gameObject);
    }
}
