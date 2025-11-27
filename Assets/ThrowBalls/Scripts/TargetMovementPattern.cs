using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "New Movement Pattern", menuName = "ThrowBalls/Target Movement Pattern")]
public class TargetMovementPattern : ScriptableObject
{
    [Header("Appear Animation")]
    public float appearYOffset = 1f;
    public float appearDuration = 0.75f;
    public Ease appearEase = Ease.OutBack;
    public Ease hideEase = Ease.InBack;

    [Header("Move Animation")]
    public float moveXOffset = 1f;
    public float moveDuration = 4f;
    public Ease moveEase = Ease.Linear;
    public bool enableHorizontalMovement = true;

    [Header("Wave Animation")]
    public float waveAmplitude = 1f;
    public float waveFrequency = 1f;
    public float waveDuration = 3f;
    public Ease waveEase = Ease.InOutSine;
    public bool enableWaveMovement = true;

    [Header("Toggle Direction Animation")]
    public float toggleDirectionDuration = 0.5f;
    public Ease toggleDirectionEase = Ease.Linear;
    public bool enableDirectionToggle = true;

    [Header("Get Hit Animation")]
    public float getHitAnimationDuration = 0.5f;
    public float getHitRotateAmount = 720f;
    public Ease getHitEase = Ease.OutQuad;
}
