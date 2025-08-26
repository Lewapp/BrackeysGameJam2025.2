using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the death screen UI and retry logic
/// </summary>
public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject[] deathScreenObjects; // UI elements to show/hide

    /// <summary>
    /// Activates or deactivates the death screen
    /// </summary>
    public void SetActiveDeathScreen(bool _enabled)
    {
        // Enable/disable all death screen objects
        foreach (var obj in deathScreenObjects)
        {
            obj.SetActive(_enabled);
        }

        if (_enabled)
        {
            // Pause game and unlock cursor
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // Resume game and lock cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Retry the current scene
    /// </summary>
    public void RetryButton()
    {
        // Reload the active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}