using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class that manages AI targeting and influence between enemies and focusable objects.
/// </summary>
public class AiDirector : MonoBehaviour
{
    #region Public Variables
    // Singleton instance
    public static AiDirector instance;
    #endregion

    #region Private Variables
    private Dictionary<EnemyFocus, GameObject> activeEnemies;    // Dictionary of active enemies and the object they are currently focusing on
    private Dictionary<GameObject, FocusableInfo> focusables;    // Dictionary of focusable objects and their influence info
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (instance && instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }
        instance = this; // Set the singleton instance

        activeEnemies = new Dictionary<EnemyFocus, GameObject>();
        focusables = new Dictionary<GameObject, FocusableInfo>();
    }
    #endregion

    #region Registration Methods

    /// <summary>
    /// Register a new enemy with the AI system
    /// </summary>
    public void Register(EnemyFocus _enemy)
    {
        if (!activeEnemies.ContainsKey(_enemy))
            activeEnemies[_enemy] = null; // Initially not focusing on anything
    }

    /// <summary>
    /// Register a new focusable object with default influence values
    /// </summary>
    public void Register(GameObject _focusable)
    {
        if (!focusables.ContainsKey(_focusable))
            focusables[_focusable] = new FocusableInfo(1f, 1f); // default influence
    }

    /// <summary>
    /// Unregister an enemy and restore influence to its last focused object
    /// </summary>
    public void UnRegister(EnemyFocus _enemy)
    {
        GameObject _focusObj = _enemy?.GetFocus()?.gameObject;

        RestoreInfluence(_focusObj, _enemy);

        // Remove the enemy from the active enemies dictionary
        activeEnemies.Remove(_enemy);
    }

    /// <summary>
    /// Unregister a focusable object from the system
    /// </summary>
    public void UnRegister(GameObject _focusable)
    {
        if (focusables.ContainsKey(_focusable))
            focusables.Remove(_focusable);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Restores influence to a focusable object when an enemy is no longer targeting it.
    /// </summary>
    private void RestoreInfluence(GameObject _focusedObject, EnemyFocus _enemy)
    {
        // Check that the focusable object exists and is tracked by the AI system
        if (_focusedObject != null && focusables.ContainsKey(_focusedObject))
        {
            // Get a copy of the FocusableInfo struct for this object
            FocusableInfo _info = focusables[_focusedObject];

            // Restore influence based on the enemy's influence power
            _info.influence += _info.initialInfluence * _enemy.GetInfluencePower();

            // Write the updated struct back into the dictionary
            focusables[_focusedObject] = _info;
        }
    }

    /// <summary>
    /// Determine which focusable object an enemy should target
    /// </summary>
    public GameObject GetFocus(EnemyFocus _enemy)
    {
        if (!activeEnemies.ContainsKey(_enemy))
        {
            Debug.LogWarning("Unregistered enemy object attempted to get a focus object.");
            return null;
        }

        // Sum total current influence of all focusables
        float _totalInfluence = 0f;
        foreach (var _info in focusables.Values)
        {
            _totalInfluence += _info.influence;
        }

        GameObject _keyFocus = null;

        if (_totalInfluence <= 0f)
        {
            // If all influence is zero, pick a random focusable
            List<GameObject> _keyList = new List<GameObject>(focusables.Keys);
            int _randomKey = UnityEngine.Random.Range(0, _keyList.Count);
            _keyFocus = _keyList[_randomKey];
        }
        else
        {
            // Weighted random selection based on influence values
            float _randomPoint = UnityEngine.Random.Range(0, _totalInfluence);
            float _runningSum = 0f;

            foreach (var _kvp in focusables)
            {
                _runningSum += _kvp.Value.influence;

                if (_randomPoint <= _runningSum)
                {
                    _keyFocus = _kvp.Key;
                    break;
                }
            }
        }

        // Reduce influence on the selected focusable using the enemy's influence power
        if (_keyFocus != null && focusables.ContainsKey(_keyFocus))
        {
            FocusableInfo info = focusables[_keyFocus];
            info.influence -= info.initialInfluence * _enemy.GetInfluencePower(); // InfluencePower acts as multiplier
            focusables[_keyFocus] = info; // Write back to dictionary
        }

        // Update the current focus of this enemy
        activeEnemies[_enemy] = _keyFocus;

        return _keyFocus;
    }

    #endregion

    #region Structs

    /// <summary>
    /// Stores initial and current influence values for a focusable object
    /// </summary>
    [Serializable]
    public struct FocusableInfo
    {
        public float initialInfluence;  // Original influence value
        public float influence;         // Current influence value

        public FocusableInfo(float initialInfluence, float influence)
        {
            this.initialInfluence = initialInfluence;
            this.influence = influence;
        }
    }

    #endregion
}