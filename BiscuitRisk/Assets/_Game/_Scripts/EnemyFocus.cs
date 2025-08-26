using UnityEngine;

/// <summary>
/// Component that allows an enemy to acquire and track a focusable target via the AI Director.
/// Implements ITargetSeeker to provide a target for other systems.
/// </summary>
public class EnemyFocus : MonoBehaviour, ITargetSeeker, IDamageable
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

[Header("Irritation Settings")]
[SerializeField] private bool willResetAfterCooldown = true; // If true, enemy can reset to original focus after irritation cooldown
[SerializeField] private GameObject preIrritationFocus;      // Stores the enemy's focus before being redirected by irritation
[SerializeField] private float irritationLevel = 0f;         // Current irritation value (0–1), increases when taking damage
[SerializeField] private float irrtationMultiplier = 0.02f;  // Multiplier for irritation gained when damaged
[SerializeField] private float irritationCooldownPower = 0.2f; // Rate at which irritation decreases over time
[SerializeField] private float resetCooldownThreshold = 0.5f; // Irritation level below which the enemy may reset focus
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
        // Gradually reduce irritation level over time
        irritationLevel -= Time.deltaTime * irritationCooldownPower;
        irritationLevel = Mathf.Max(irritationLevel, 0f);

        // Check if irritation has cooled down enough to restore pre-irritation focus
        ManageIrritationCooldown();

        // If there's no AI Director in the scene, skip further logic
        if (!AiDirector.instance)
            return;

        // If this enemy currently has no focus assigned
        if (!focus)
        {
            // Request a new focusable target from the AI Director
            // (passing null indicates no current focus to consider)
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

    /// <summary>
    /// When the enemy is damage, it increases irritation, 
    /// possibly causing a focus shift.
    /// </summary>
    public void TakeDamage(float _amount, GameObject _source)
    {
        // Ignore if AiDirector is missing OR the current focus is already the damage source
        if (!AiDirector.instance || focus.gameObject == _source)
            return;

        // Increase irritation level by damage amount scaled with irritation multiplier
        irritationLevel += _amount * irrtationMultiplier;

        // Clamp irritation level to a maximum of 1
        irritationLevel = Mathf.Min(1f, irritationLevel);

        // If irritation hasn’t maxed out yet, no focus change occurs
        if (irritationLevel < 1f)
            return;

        // Store current focus so it can potentially be restored after cooldown
        preIrritationFocus = focus.gameObject;

        // If AiDirector allows, switch focus to the damage source
        if (AiDirector.instance.ForceChangeFocus(this, _source))
        {
            focus = _source.transform;
        }
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

    /// <summary>
    /// Requests a new focus target for this enemy from the AI Director.
    /// </summary>
    public void RerollFocus()
    {
        // Ask the AiDirector for a new focus
        focus = AiDirector.instance.GetFocus(this, focus.gameObject)?.transform;
    }

    /// <summary>
    /// Handles logic for resetting an enemy's focus after irritation cooldown,
    /// restoring their pre-irritation focus.
    /// </summary>
    private void ManageIrritationCooldown()
    {
        // Skip entirely if reset behaviour is disabled or irritation is still above threshold
        if (!willResetAfterCooldown || irritationLevel >= resetCooldownThreshold)
            return;

        // No focus stored to restore
        if (!preIrritationFocus)
            return;

        // Already focusing on the pre-irritation object
        if (focus == preIrritationFocus)
            return;

        // Attempt to restore previous focus
        if (AiDirector.instance.ForceChangeFocus(this, preIrritationFocus))
            focus = preIrritationFocus.transform;

        // Clear stored focus reference (restored or attempted)
        preIrritationFocus = null;
    }
    
    #endregion
}