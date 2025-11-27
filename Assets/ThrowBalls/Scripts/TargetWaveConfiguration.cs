using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveSpawnData
{
    public Vector3 spawnPosition;
    public float spawnDelay;
    public TargetMovementPattern movementPattern;
}

[CreateAssetMenu(fileName = "New Wave Configuration", menuName = "ThrowBalls/Wave Configuration")]
public class TargetWaveConfiguration : ScriptableObject
{
    [Header("Wave Settings")]
    public string waveName = "Wave 1";
    public float waveDuration = 30f;
    public int targetCount = 5;
    
    [Header("Spawn Configuration")]
    public List<WaveSpawnData> spawnData = new List<WaveSpawnData>();
    
    [Header("Wave Behavior")]
    public bool spawnAllAtOnce = false;
    public float timeBetweenSpawns = 2f;
    public bool randomizeSpawnOrder = false;
    
    [Header("Difficulty Modifiers")]
    [Range(0.5f, 2f)]
    public float speedMultiplier = 1f;
    [Range(0.5f, 2f)]
    public float sizeMultiplier = 1f;
    
    private void OnValidate()
    {
        // Ensure spawn data list matches target count
        while (spawnData.Count < targetCount)
        {
            spawnData.Add(new WaveSpawnData());
        }
        
        while (spawnData.Count > targetCount)
        {
            spawnData.RemoveAt(spawnData.Count - 1);
        }
    }
}
