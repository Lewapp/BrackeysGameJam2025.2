using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player weapon input and continuous shooting behaviour.
/// Uses the Unity Input System to detect button presses and releases.
/// </summary>
public class PlayerWeapon : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] private Weapon equippedWeapon; // The currently equipped weapon to fire
    #endregion

    #region Private Variables
    private bool isShooting = false; // Tracks whether the player is holding down the fire button
    #endregion

    #region Unity Callbacks
    private void Update()
    {
        if (isShooting)
            equippedWeapon.Shoot(); // Fire weapon if input is active
    }
    #endregion

    #region Input Actions 
    /// <summary>
    /// Input System callback for shooting.
    /// Detects when the fire button is pressed or released.
    /// </summary>
    public void ShootIA(InputAction.CallbackContext context)
    {
        if (!equippedWeapon)
            return; // Do nothing if no weapon is equipped

        // Button has just been pressed
        if (context.started)
            isShooting = true;

        // Button has just been released
        else if (context.canceled)
            isShooting = false;
    }
    #endregion
}