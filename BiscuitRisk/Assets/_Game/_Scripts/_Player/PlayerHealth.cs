using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private DeathScreenUI deathScreen;
    [SerializeField] private TextMeshProUGUI healthTxt;
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;

    private void Start()
    {
        deathScreen?.SetActiveDeathScreen(false);
        currentHealth = maxHealth;
        CheckHealth();
    }

    public void TakeDamage(float _amount, GameObject _source)
    {
        currentHealth -= _amount;
        CheckHealth();
    }

    private void CheckHealth()
    {
        healthTxt.text = $"Health: {currentHealth}";

        if (currentHealth > 0f)
            return;

        currentHealth = 0f;
        healthTxt.text = $"Health: {currentHealth}";

        // Handle Death
        deathScreen?.SetActiveDeathScreen(true);

    }

    //TEMP
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10f, gameObject);
        }
    }

}
