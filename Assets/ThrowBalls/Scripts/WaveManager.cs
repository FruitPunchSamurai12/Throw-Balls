using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Management")]
    public LevelConfiguration currentLevel;
    public List<ThrowingBallTarget> targetPool = new List<ThrowingBallTarget>();
    
    [Header("Debug")]
    public bool autoStartWaves = true;
    
    private int currentWaveIndex = 0;
    private List<ThrowingBallTarget> activeTargets = new List<ThrowingBallTarget>();
    private Coroutine currentWaveCoroutine;
    
    public System.Action<int> OnWaveStarted;
    public System.Action<int> OnWaveCompleted;
    public System.Action OnLevelCompleted;
    
    private void Start()
    {
        if (autoStartWaves && currentLevel != null)
        {
            StartLevel();
        }
    }
    
    public void StartLevel()
    {
        if (currentLevel == null)
        {
            Debug.LogError("No level configuration assigned!");
            return;
        }
        
        currentWaveIndex = 0;
        StartNextWave();
    }
    
    public void StartNextWave()
    {
        if (currentLevel == null || currentWaveIndex >= currentLevel.GetTotalWaves())
        {
            if (currentLevel.loopWaves && currentLevel.GetTotalWaves() > 0)
            {
                currentWaveIndex = 0;
            }
            else
            {
                OnLevelCompleted?.Invoke();
                return;
            }
        }
        
        TargetWaveConfiguration wave = currentLevel.GetWave(currentWaveIndex);
        if (wave != null)
        {
            if (currentWaveCoroutine != null)
            {
                StopCoroutine(currentWaveCoroutine);
            }
            currentWaveCoroutine = StartCoroutine(ExecuteWave(wave));
        }
    }
    
    private IEnumerator ExecuteWave(TargetWaveConfiguration wave)
    {
        OnWaveStarted?.Invoke(currentWaveIndex);
        
        // Clear previous wave
        ClearActiveTargets();
        
        if (wave.spawnAllAtOnce)
        {
            // Spawn all targets at once
            for (int i = 0; i < wave.spawnData.Count; i++)
            {
                SpawnTarget(wave.spawnData[i], wave);
            }
        }
        else
        {
            // Spawn targets with delays
            List<WaveSpawnData> spawnOrder = new List<WaveSpawnData>(wave.spawnData);
            
            if (wave.randomizeSpawnOrder)
            {
                for (int i = 0; i < spawnOrder.Count; i++)
                {
                    WaveSpawnData temp = spawnOrder[i];
                    int randomIndex = Random.Range(i, spawnOrder.Count);
                    spawnOrder[i] = spawnOrder[randomIndex];
                    spawnOrder[randomIndex] = temp;
                }
            }
            
            foreach (var spawnData in spawnOrder)
            {
                yield return new WaitForSeconds(spawnData.spawnDelay + wave.timeBetweenSpawns);
                SpawnTarget(spawnData, wave);
            }
        }
        
        // Wait for wave duration
        yield return new WaitForSeconds(wave.waveDuration);
        
        OnWaveCompleted?.Invoke(currentWaveIndex);
        currentWaveIndex++;
        
        // Wait between waves
        yield return new WaitForSeconds(currentLevel.timeBetweenWaves);
        
        StartNextWave();
    }
    
    private void SpawnTarget(WaveSpawnData spawnData, TargetWaveConfiguration wave)
    {
        ThrowingBallTarget target = GetAvailableTarget();
        if (target == null)
        {
            Debug.LogWarning("No available targets in pool!");
            return;
        }
        
        // Configure target
        target.transform.position = spawnData.spawnPosition;
        
        // Set movement pattern
        TargetMovementPattern pattern = spawnData.movementPattern;
        if (pattern == null)
        {
            pattern = currentLevel.GetRandomMovementPattern();
        }
        
        if (pattern != null)
        {
            target.SetMovementPattern(pattern);
        }
        
        // Apply wave modifiers
        target.transform.localScale = Vector3.one * wave.sizeMultiplier;
        
        activeTargets.Add(target);
        target.EnableTarget();
    }
    
    private ThrowingBallTarget GetAvailableTarget()
    {
        foreach (var target in targetPool)
        {
            if (!activeTargets.Contains(target))
            {
                return target;
            }
        }
        return null;
    }
    
    private void ClearActiveTargets()
    {
        foreach (var target in activeTargets)
        {
            target.DisableTarget();
        }
        activeTargets.Clear();
    }
    
    public void SetLevel(LevelConfiguration level)
    {
        currentLevel = level;
        currentWaveIndex = 0;
    }
    
    public void StopCurrentWave()
    {
        if (currentWaveCoroutine != null)
        {
            StopCoroutine(currentWaveCoroutine);
            currentWaveCoroutine = null;
        }
        ClearActiveTargets();
    }
}
