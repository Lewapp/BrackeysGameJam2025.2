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
    [SerializeField] private GameObject defaultEnemy;               // Default enemy prefab used for spawning

    [Header("Initial Spawn Settings")]
    [SerializeField] private int maxSpawns = 30;                    // Maximum number of enemies allowed in the scene
    [SerializeField] private Transform[] spawnPoints;               // Spawn points where enemies can appear
    [SerializeField] private Vector2 spawnAmountRange;           // Randomised range for number of enemies per spawn
    [SerializeField] private Vector2 spawnTimeRange;                // Randomised range for time between spawns

    [Header("Wave Settings")]
    [SerializeField] private float currentWave = 1f;                // The current wave we are at
    [SerializeField] private int waveSpawnsLeft;                  // Remaining spawns in the current wave
    [SerializeField] private float spawnsPerWave;                   // Number of enemies spawned per wave
    [SerializeField] private float spawnsPerWaveIncrease;           // Increment added to spawns per wave after each wave
    [SerializeField] private float spawnTimeDecrease;               // Decrease applied to spawn time each wave (speeds up spawning)

    [Header("Refocus Settings")]
    [SerializeField] private float checkDelay = 5.0f;               // Time between focus balancing checks
    [SerializeField] private float focusThreshold = 1.25f;          // Threshold for deciding if focus should be redistributed
    [SerializeField] private float switchPercentage = 0.3f;         // Percentage chance for an enemy to switch focus during balancing
    [SerializeField] private int maxCandidatesPerCheck = 5;         // Maximum number of candidates considered per balancing check

    [Header("Risk and Reward Settings")]
    [SerializeField] private RiskRewardScreen riskRewardScreen;
    [SerializeField] private int waveRrScreenTrigger = 3;

    #endregion

    #region Private Variables
    private Dictionary<EnemyFocus, GameObject> activeEnemies;       // Maps active enemies to their current focus targets
    private Dictionary<GameObject, FocusableInfo> focusables;       // Maps focusable objects to their influence data

    private float timeSinceLastSpawn = 0f;                          // Timer since the last enemy spawn
    private float instanceSpawnTime = 0f;                           // Current randomised spawn interval
    private float timeSinceLastCheck = 0f;                          // Timer since the last focus balancing check
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (instance && instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance (singleton enforcement)
            return;
        }
        instance = this; // Assign singleton instance

        activeEnemies = new Dictionary<EnemyFocus, GameObject>();
        focusables = new Dictionary<GameObject, FocusableInfo>();
    }

    private void Update()
    {
        if (spawnPoints.Length <= 0) // Skip if no spawn points defined
            return;

        ManageWave(); // Handle wave spawning logic
    }

    private void LateUpdate()
    {
        if (focusables.Count <= 0) // Skip if no focusable objects exist
            return;

        timeSinceLastCheck += Time.deltaTime;
        if (timeSinceLastCheck >= checkDelay)
        {
            timeSinceLastCheck = 0f;
            BalanceFocuses(); // Periodically redistribute enemy focus
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
    /// Reduces the influence value of a given focusable object when an enemy targets it,
    /// based on the enemy's influence power.
    /// </summary>
    private void RemoveInfluence(GameObject _focusedObject, EnemyFocus _enemy)
    {
        // Reduce influence on the selected focusable using the enemy's influence power
        if (_focusedObject != null && focusables.ContainsKey(_focusedObject))
        {
            FocusableInfo _info = focusables[_focusedObject];
            _info.influence -= _info.initialInfluence * _enemy.GetInfluencePower(); // InfluencePower acts as multiplier
            _info.influence = Mathf.Max(0f, _info.influence);
            focusables[_focusedObject] = _info; // Write back to dictionary
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

            foreach (var _focusable in focusables)
            {
                _runningSum += _focusable.Value.influence;

                if (_randomPoint <= _runningSum)
                {
                    _keyFocus = _focusable.Key;
                    break;
                }
            }
        }

        RemoveInfluence(_keyFocus, _enemy);

        // Update the current focus of this enemy
        activeEnemies[_enemy] = _keyFocus;
        RestoreInfluence(_oldFocus, _enemy);

        return _keyFocus;
    }

    /// <summary>
    /// Attempts to forcefully change an enemy's current focus target to a new desired focusable.
    /// </summary>
    public bool ForceChangeFocus(EnemyFocus _enemy, GameObject _desiredFocus)
    {
        // Check if the desired focus object is registered as a valid focusable
        if (!focusables.ContainsKey(_desiredFocus))
        {
            Debug.LogWarning($"Attempting to force change focus for {_enemy.name} to an unfocusable object.");
            return false;
        }

        // Check if the enemy is registered in the active enemies dictionary
        if (!activeEnemies.ContainsKey(_enemy))
        {
            Debug.LogWarning("Unregistered enemy object attempted to get a focus object.");
            return false;
        }

        // Restore influence back to the previous focus object before changing target
        RestoreInfluence(_enemy.GetFocus().gameObject, _enemy);

        // Assign the new focus target for this enemy
        activeEnemies[_enemy] = _desiredFocus;

        // Reduce the influence of the newly focused object since it is being targeted
        RemoveInfluence(_desiredFocus, _enemy);

        return true;
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
                // Start wave
                spawnTimeRange.y *= spawnTimeDecrease;
                spawnsPerWave *= spawnsPerWaveIncrease;
                waveSpawnsLeft = (int)spawnsPerWave;
                currentWave++;

                // Show Risk Reward Screen
                if ((currentWave % waveRrScreenTrigger) == 0)
                    riskRewardScreen.enabled = true;

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
        int _randSpawnPoint = UnityEngine.Random.Range(0, spawnPoints.Length);
        int _randSpawnAmount = UnityEngine.Random.Range((int)spawnAmountRange.x, (int)spawnAmountRange.y);

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