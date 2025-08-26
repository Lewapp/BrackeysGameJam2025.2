using UnityEngine;

/// <summary>
/// Component that allows an enemy to acquire and track a focusable target via the AI Director.
/// Implements ITargetSeeker to provide a target for other systems.
/// </summary>
public class EnemyFocus : MonoBehaviour, ITargetSeeker
{
    #region Properties
    //Provides the current target transform for this enemy.
    // Returns the transform of the focus if one exists, otherwise null.
    public Transform target
    {
        get => focus ? focus.transform : null;
        set { }
    }
    #endregion

    #region Inspector Variables
    [SerializeField] private Transform focus;        // Current object being focused
    [SerializeField] private float influncePower = 1f; // How strongly this enemy reduces the influence of a focusable
    #endregion

    #region Unity Callbacks

    private void Start()
    {
        // Register this enemy with the AI Director on startup.
        if (AiDirector.instance)
        {
            AiDirector.instance.Register(this);
        }
    }

    private void Update()
    {
        if (!AiDirector.instance)
            return;

        if (!focus)
        {
            // Ask the AI Director for a focusable object
            focus = AiDirector.instance.GetFocus(this, null)?.transform;
            return;
        }
    }

    private void OnDestroy()
    {
        // Unregister this enemy from the AI Director when destroyed.
        if (AiDirector.instance)
        {
            AiDirector.instance.UnRegister(this);
        }
    }

    #endregion

    #region Interface Methods

    public void CanSwitchFocus()
    {
        // TODO: Use this method to attempt focus switch when attacked
        // Check if allowed to switch with the AiDirector
    }

    #endregion 

    #region Methods

    /// <summary>
    /// Returns this enemy's influence power for adjusting focusable influence.
    /// </summary>
    public float GetInfluencePower()
    {
        return influncePower;
    }

    /// <summary>
    /// Returns the currently focused transform
    /// </summary>
    public Transform GetFocus()
    {
        return focus;
    }

    public void RerollFocus()
    {
        focus = AiDirector.instance.GetFocus(this, focus.gameObject)?.transform;
    }

    #endregion
}