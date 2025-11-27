using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Configuration", menuName = "ThrowBalls/Level Configuration")]
public class LevelConfiguration : ScriptableObject
{
    [Header("Level Info")]
    public string levelName = "Level 1";
    public int levelNumber = 1;
    [TextArea(3, 5)]
    public string levelDescription = "Description of the level";
    
    [Header("Level Settings")]
    public int targetScore = 100;
    public float timeLimit = 60f;
    public bool hasTimeLimit = true;
    
    [Header("Wave Configurations")]
    public List<TargetWaveConfiguration> waves = new List<TargetWaveConfiguration>();
    
    [Header("Level Progression")]
    public bool loopWaves = false;
    public float timeBetweenWaves = 5f;
    
    [Header("Default Movement Patterns")]
    public TargetMovementPattern defaultMovementPattern;
    public List<TargetMovementPattern> availablePatterns = new List<TargetMovementPattern>();
    
    public int GetTotalWaves()
    {
        return waves.Count;
    }
    
    public TargetWaveConfiguration GetWave(int index)
    {
        if (index >= 0 && index < waves.Count)
        {
            return waves[index];
        }
        return null;
    }
    
    public TargetMovementPattern GetRandomMovementPattern()
    {
        if (availablePatterns.Count > 0)
        {
            return availablePatterns[Random.Range(0, availablePatterns.Count)];
        }
        return defaultMovementPattern;
    }
}
