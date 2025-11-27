using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingBallTarget : MonoBehaviour
{
    [SerializeField] int points = 10;
    [SerializeField] Vector2 minMaxForce = new Vector2(0.5f, 1.5f);
    
    [Header("Movement Configuration")]
    [SerializeField] TargetMovementPattern movementPattern;
    
    [Header("Target Components")]
    [SerializeField] Transform dyno;


    bool hasBeenHit = false;
    public event Action<int,Vector3> onKnockedDown;

    Rigidbody rb;
    Vector3 initPos;
    Quaternion initRot;
    Vector3 initScale;
    Collider col;
    DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> appearTween;
    DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> moveTween;
    Tweener waveTween;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initPos = transform.localPosition;
        initRot = transform.rotation;
        initScale = dyno.transform.localScale;
        col = GetComponent<Collider>();
        col.enabled = false;
    }

    public void EnableTarget()
    {
        if (movementPattern == null)
        {
            Debug.LogWarning("No movement pattern assigned to target!");
            return;
        }

        transform.localPosition = initPos;
        transform.rotation = initRot;
        dyno.transform.localScale = initScale;
        col.enabled = false;
        
        appearTween = transform.DOLocalMoveY(initPos.y + movementPattern.appearYOffset, movementPattern.appearDuration)
            .SetEase(movementPattern.appearEase).OnComplete(() => 
        {
            col.enabled = true;
            
            // Horizontal movement
            if (movementPattern.enableHorizontalMovement)
            {
                moveTween = transform.DOLocalMoveX(initPos.x + movementPattern.moveXOffset, movementPattern.moveDuration)
                    .SetEase(movementPattern.moveEase).SetLoops(-1, LoopType.Yoyo);
                
                if (movementPattern.enableDirectionToggle)
                {
                    moveTween.OnStepComplete(() => 
                    {
                        Vector3 dynoScale = dyno.localScale;
                        float z = dynoScale.z;
                        float rot = z > 0 ? 90 : -90;
                        dyno.DOLocalRotate(new Vector3(0, rot, 0), movementPattern.toggleDirectionDuration * 0.5f, RotateMode.Fast)
                            .SetEase(movementPattern.toggleDirectionEase).OnComplete(() => 
                        {
                            dynoScale.z = -z;
                            dyno.localScale = dynoScale;
                            dyno.DOLocalRotate(Vector3.zero, movementPattern.toggleDirectionDuration * 0.5f, RotateMode.Fast)
                                .SetEase(movementPattern.toggleDirectionEase);
                        });
                    });
                }
            }

            // Wave movement
            if (movementPattern.enableWaveMovement)
            {
                float initY = initPos.y + movementPattern.appearYOffset;
                waveTween = DOVirtual.Float(0, movementPattern.moveDuration, movementPattern.waveDuration, (t) =>
                {
                    float newY = movementPattern.waveAmplitude * Mathf.Sin(t * movementPattern.waveFrequency * Mathf.PI * 2);
                    transform.localPosition = new Vector3(transform.localPosition.x, initY + newY, transform.localPosition.z);
                }).SetEase(movementPattern.waveEase).SetLoops(-1, LoopType.Yoyo);
            }
        });
    }

    public void DisableTarget()
    {
        //rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
        hasBeenHit = false;
        if (appearTween != null)
        {
            appearTween.Kill();
        }
        if (moveTween != null)
        {
            moveTween.Kill();
        }
        if (waveTween != null)
        {
            waveTween.Kill();
        }
        col.enabled = false;
        if (movementPattern != null)
        {
            appearTween = transform.DOLocalMoveY(initPos.y, movementPattern.appearDuration).SetEase(movementPattern.hideEase);
        }
    }

    public void ResetTarget()
    {
        transform.localPosition = initPos;
        transform.rotation = initRot;
        dyno.transform.localScale = initScale;
        //rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
        hasBeenHit = false;
        if(appearTween!=null)
        {
            appearTween.Kill();
        }
        if (moveTween != null)
        {
            moveTween.Kill();
        }
        if (waveTween != null)
        {
            waveTween.Kill();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponent<ThrowingBall>();
        if(ball!=null && !hasBeenHit)
        {
            //AudioManager.Instance.PlaySound2D("ThrowBalls_HitTarget");
            hasBeenHit = true;
            col.enabled = false;
            Vector3 pos = other.transform.position;

            Vector3 hitPointLocal = transform.InverseTransformPoint(pos);
            //Debug.Log(hitPointLocal);
            GetHitAnimation(hitPointLocal.x > 0);
            onKnockedDown?.Invoke(points,pos);
        }
    }

    public void SetMovementPattern(TargetMovementPattern pattern)
    {
        movementPattern = pattern;
    }
    
    void GetHitAnimation(bool left)
    {
        if (movementPattern == null) return;
        
        if (moveTween != null) moveTween.Pause();
        if (waveTween != null) waveTween.Pause();
        
        float rot = left ? movementPattern.getHitRotateAmount : -movementPattern.getHitRotateAmount;
        transform.DOBlendableLocalRotateBy(new Vector3(0, rot, 0), movementPattern.getHitAnimationDuration, RotateMode.FastBeyond360)
            .SetEase(movementPattern.getHitEase).OnComplete(() =>
        {
            if (moveTween != null) moveTween.Play();
            if (waveTween != null) waveTween.Play();
            hasBeenHit = false;
            col.enabled = true;
        });
    }

}
