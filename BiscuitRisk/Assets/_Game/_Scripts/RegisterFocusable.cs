using UnityEngine;

/// <summary>
/// Automatically registers this GameObject with the AI Director as a focusable object.
/// Also provides a method to unregister itself when needed.
/// </summary>
public class RegisterFocusable : MonoBehaviour
{
    #region Unity Callbacks
    private void Start()
    {
        // Register this object with the AI Director
        if (AiDirector.instance)
        {
            AiDirector.instance.Register(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unregister this object when it is destroyed
        if (AiDirector.instance)
        {
            UnRegisterSelf();
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Unregisters this GameObject from the AI Director.
    /// Can be called manually if needed.
    /// </summary>
    public void UnRegisterSelf()
    {
        if (AiDirector.instance)
        {
            AiDirector.instance.UnRegister(gameObject);
        }
    }
    #endregion
}