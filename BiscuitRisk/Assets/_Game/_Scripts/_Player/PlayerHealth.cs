using TMPro;
using UnityEngine;

/// <summary>
/// Handles the player's health and death logic
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    #region Inspector Variables
    [SerializeField] private DeathScreenUI deathScreen; // Reference to death screen UI
    [SerializeField] private TextMeshProUGUI healthTxt; // UI element to display health
    [SerializeField] private float maxHealth;           // Maximum player health
    [SerializeField] private float currentHealth;       // Current player health
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        // Ensure death screen is hidden on start
        deathScreen?.SetActiveDeathScreen(false);
        currentHealth = maxHealth; // Initialise health
        CheckHealth(); // Update UI
    }
    #endregion

    #region Interface Methods
    /// <summary>
    /// Handles applying damage to health by the amount given.
    /// </summary>
    public void TakeDamage(float _amount, GameObject _source)
    {
        // Reduce health by incoming damage and check for death
        currentHealth -= _amount;
        CheckHealth();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Checks ito see if player is still alive, if not then the death screen is shown.
    /// Also updates the health UI text.
    /// </summary>
    private void CheckHealth()
    {
        // Update UI with current health
        healthTxt.text = $"Health: {currentHealth}";

        // Exit if still alive
        if (currentHealth > 0f)
            return;

        // Clamp health to zero and update UI
        currentHealth = 0f;
        healthTxt.text = $"Health: {currentHealth}";

        // Show death screen
        deathScreen?.SetActiveDeathScreen(true);
    }
    #endregion
}