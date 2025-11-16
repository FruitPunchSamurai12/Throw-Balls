using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingBallTarget : MonoBehaviour
{
    [SerializeField] int points = 10;
    [SerializeField] Vector2 minMaxForce = new Vector2(0.5f, 1.5f);

    [Header("Appear Animation")]
    [SerializeField] float appearYOffset = 1f;
    [SerializeField] float appearDuration = 0.75f;
    [SerializeField] Ease appearEase = Ease.OutBack;
    [SerializeField] Ease hideEase = Ease.InBack;

    [Header("Move Animation")]
    [SerializeField] float moveXOffset = 1f;
    [SerializeField] float moveDuration = 4f;
    [SerializeField] Ease moveEase = Ease.Linear;

    [Header("Wave Animation")]
    [SerializeField] float waveAmplitude = 1f;
    [SerializeField] float waveFrequency = 1f;
    [SerializeField] float waveDuration = 3f;
    [SerializeField] Ease waveEase = Ease.InOutSine;

    [Header("Toggle Direction Animation")]
    [SerializeField] Transform dyno;
    [SerializeField] float toggleDirectionDuration = 0.5f;
    [SerializeField] Ease toggleDirectionEase = Ease.Linear;

    [Header("Get Hit Animation")]
    [SerializeField] float getHitAnimationDuration = 0.5f;
    [SerializeField] float getHitRotateAmount = 720f;
    [SerializeField] Ease getHitEase = Ease.OutQuad;


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
        transform.localPosition = initPos;
        transform.rotation = initRot;
        dyno.transform.localScale = initScale;
        col.enabled = false;
        appearTween = transform.DOLocalMoveY(initPos.y + appearYOffset, appearDuration).SetEase(appearEase).OnComplete(() => 
        {
            col.enabled = true;
            moveTween = transform.DOLocalMoveX(initPos.x + moveXOffset, moveDuration).SetEase(moveEase).SetLoops(-1, LoopType.Yoyo).OnStepComplete(() => 
            {
                Vector3 dynoScale = dyno.localScale;
                float z = dynoScale.z;
                float rot = z > 0 ? 90 : -90;
                dyno.DOLocalRotate(new Vector3(0, rot, 0), toggleDirectionDuration * 0.5f, RotateMode.Fast).SetEase(toggleDirectionEase).OnComplete(() => 
                {
                    dynoScale.z = -z;
                    dyno.localScale = dynoScale;
                    dyno.DOLocalRotate(Vector3.zero, toggleDirectionDuration * 0.5f, RotateMode.Fast).SetEase(toggleDirectionEase);
                });
            });

            float initY = initPos.y + appearYOffset;
            waveTween = DOVirtual.Float(0, moveDuration, waveDuration, (t) =>
            {
                // Calculate the new y position using the sine function and apply the shift to our og y
                float newY = waveAmplitude * Mathf.Sin(t * waveFrequency * Mathf.PI * 2);
                transform.localPosition = new Vector3(transform.localPosition.x, initY + newY, transform.localPosition.z);
            }).SetEase(waveEase).SetLoops(-1, LoopType.Yoyo);


        });
    }

    public void DisableTarget()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
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
        appearTween = transform.DOLocalMoveY(initPos.y, appearDuration).SetEase(hideEase);
    }

    public void ResetTarget()
    {
        transform.localPosition = initPos;
        transform.rotation = initRot;
        dyno.transform.localScale = initScale;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
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

    void GetHitAnimation(bool left)
    {
        moveTween.Pause();
        waveTween.Pause();
        float rot = left ? getHitRotateAmount : -getHitRotateAmount;
        transform.DOBlendableLocalRotateBy(new Vector3(0, rot, 0), getHitAnimationDuration, RotateMode.FastBeyond360).SetEase(getHitEase).OnComplete(() =>
        {
            moveTween.Play();
            waveTween.Play();
            hasBeenHit = false;
            col.enabled = true;
        });
    }

}
