using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject[] deathScreenObjects;

    public void SetActiveDeathScreen(bool _enabled)
    {
        foreach (var obj in deathScreenObjects)
        {
            obj.SetActive(_enabled);
        }

        if (_enabled)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Time.timeScale = 1f;
        Cursor.lockState =CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RetryButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
