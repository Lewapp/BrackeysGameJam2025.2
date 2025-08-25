using System;
using UnityEngine;

/// <summary>
/// Handles the health of a GameObject and implements the IDamageable interface.
/// Automatically destroys the object when health reaches zero.
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    #region Inspector Variables
    [SerializeField] private float currentHealth; // Current health value of the object
    [SerializeField] private float maxHealth;     // Maximum health the object can have
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        currentHealth = maxHealth; // Set current health to full
        CheckLife();               // Ensure object is alive
    }
    #endregion

    #region Interface Methods
    /// <summary>
    /// Applies damage to the object.
    /// </summary>
    public void TakeDamage(float _amount, GameObject _source)
    {
        currentHealth -= _amount; // Subtract damage from current health
        CheckLife();              // Check if object should be destroyed
    }
    #endregion

    #region Methods
    /// <summary>
    /// Checks if the object’s health has reached zero or below.
    /// If so, clamps health to zero and destroys the GameObject.
    /// </summary>
    private void CheckLife()
    {
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;     // Clamp health at zero
            Destroy(gameObject);    // Destroy the GameObject
        }
    }
    #endregion
}