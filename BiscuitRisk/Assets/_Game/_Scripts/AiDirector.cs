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

    #region Inspector Variables

    [Header("Enemy Types")]
    [SerializeField] private GameObject defaultEnemy;

    [Header("Initial Spawn Settings")]
    [SerializeField] private int maxSpawns = 30;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Vector2Int spawnAmountRange;
    [SerializeField] private Vector2 spawnTimeRange;

    [Header("Wave Settings")]
    [SerializeField] private float waveSpawnsLeft;
    [SerializeField] private float spawnsPerWave;
    [SerializeField] private float spawnsPerWaveIncrease;
    [SerializeField] private float spawnTimeDecrease;

    [Header("Refocus Settings")]
    [SerializeField] private float checkDelay = 5.0f;
    [SerializeField] private float focusThreshold = 1.25f;
    [SerializeField] private float switchPercentage = 0.3f;
    [SerializeField] private int maxCandidatesPerCheck = 5;

    #endregion

    #region Private Variables
    private Dictionary<EnemyFocus, GameObject> activeEnemies;    // Dictionary of active enemies and the object they are currently focusing on
    private Dictionary<GameObject, FocusableInfo> focusables;    // Dictionary of focusable objects and their influence info

    private float timeSinceLastSpawn = 0f;
    private float instanceSpawnTime = 0f;
    private float timeSinceLastCheck = 0f;
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

    private void Update()
    {
        if (spawnPoints.Length <= 0)
            return;

        ManageWave();
    }

    private void LateUpdate()
    {
        if (focusables.Count <= 0)
            return;

        timeSinceLastCheck += Time.deltaTime;
        if (timeSinceLastCheck >= checkDelay)
        {
            timeSinceLastCheck = 0f;
            BalanceFocuses();
        }
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
            _info.influence = Mathf.Min(1f, _info.influence);

            // Write the updated struct back into the dictionary
            focusables[_focusedObject] = _info;
        }
    }

    /// <summary>
    /// Determine which focusable object an enemy should target
    /// </summary>
    public GameObject GetFocus(EnemyFocus _enemy, GameObject _oldFocus)
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
            FocusableInfo _info = focusables[_keyFocus];
            _info.influence -= _info.initialInfluence * _enemy.GetInfluencePower(); // InfluencePower acts as multiplier
            _info.influence = Mathf.Max(0f, _info.influence);
            focusables[_keyFocus] = _info; // Write back to dictionary
        }

        // Update the current focus of this enemy
        activeEnemies[_enemy] = _keyFocus;
        RestoreInfluence(_oldFocus, _enemy);

        return _keyFocus;
    }

    /// <summary>
    /// Handles enemy spawning and wave progression.
    /// </summary>
    private void ManageWave()
    {
        // If no spawns are left in this wave
        if (waveSpawnsLeft <= 0)
        {
            // If all active enemies are cleared, the wave is finished
            if (activeEnemies.Count <= 0)
            {
                // End Wave 
            }

            return; // Stop processing until the next wave is triggered
        }

        // Track elapsed time since the last spawn
        timeSinceLastSpawn += Time.deltaTime;

        // Wait until the spawn cooldown has passed, 
        // and ensure we don't exceed the maximum allowed active enemies
        if (timeSinceLastSpawn < instanceSpawnTime || activeEnemies.Count >= maxSpawns)
            return;

        // Choose a new random spawn delay for the next enemy batch
        instanceSpawnTime = UnityEngine.Random.Range(spawnTimeRange.x, spawnTimeRange.y);
        timeSinceLastSpawn = 0f;

        // Select a random spawn point and a random number of enemies to spawn
        int _randSpawnPoint = UnityEngine.Random.Range(0, spawnPoints.Length - 1);
        int _randSpawnAmount = UnityEngine.Random.Range(spawnAmountRange.x, spawnAmountRange.y);

        // Spawn enemies at the chosen point
        for (int i = 0; i < _randSpawnAmount; i++)
        {
            if (activeEnemies.Count >= maxSpawns)
                break; 

            Instantiate(defaultEnemy, spawnPoints[_randSpawnPoint].position, Quaternion.identity);
            waveSpawnsLeft--; // Decrease the remaining spawns for this wave
        }
    }

    /// <summary>
    /// Rebalances enemy focuses when one focusable object is overwhelmingly more influential than the rest.
    /// </summary>
    private void BalanceFocuses()
    {
        float _highestInfluence = float.MinValue;
        GameObject _underwhelmedObj = null;

        // Find the focusable object with the highest influence
        foreach (var _focusable in focusables)
        {
            if (_focusable.Value.influence > _highestInfluence)
            {
                _highestInfluence = _focusable.Value.influence;
                _underwhelmedObj = _focusable.Key;
            }
        }

        // Exit if no valid focusable was found
        if (_underwhelmedObj == null)
            return;

        bool _isOverwhelmed = true;

        // Check if the highest influence is significantly larger than all others
        foreach (var _focusable in focusables)
        {
            if (_focusable.Key == _underwhelmedObj) continue; // Skip the most influential one

            if (_highestInfluence < _focusable.Value.influence * focusThreshold)
            {
                _isOverwhelmed = false; // Not dominant enough
                break;
            }
        }

        if (!_isOverwhelmed)
            return;

        // Collect enemies that are not already focused on the underwhelmed object
        List<EnemyFocus> _candidates = new List<EnemyFocus>();
        int _candidatesAdded = 0;
        foreach (var _enemy in activeEnemies)
        {
            if (_enemy.Value != _underwhelmedObj)
            {
                _candidatesAdded++;
                _candidates.Add(_enemy.Key);
            }

            if (_candidatesAdded >= maxCandidatesPerCheck)
                break;

        }

        // Exit if there are no candidates to switch
        if (_candidates.Count <= 0)
            return;

        // Determine how many enemies to switch based on percentage
        int _numToSwitch = Mathf.CeilToInt(_candidates.Count * switchPercentage);

        for (int i = 0; i < _numToSwitch; i++)
        {
            // Randomly select an enemy to switch focus
            EnemyFocus _chosen = _candidates[UnityEngine.Random.Range(0, _candidates.Count)];
            _chosen.RerollFocus();
            _candidates.Remove(_chosen); // Prevent choosing the same enemy twice
        }
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