using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class ThrowingBall : GenericDragAndDrop3D_GameObject
{
    [SerializeField] ParticleSystem throwEffe;
    [SerializeField] float hoverAnimDuration = 0.25f;
    [SerializeField] Vector3 shakeStrength = new Vector3(-0.1f,0.1f,-0.1f);
    [SerializeField] float grabAnimDuration = 0.25f;
    [SerializeField] Ease shakeEase = Ease.InOutSine;
    [SerializeField] GameObject trail;
    Rigidbody rb;
    Vector3 initPos;
    Quaternion initRot;
    Vector3 initScale;
    bool isThrowned = false;
    public Vector3 Velocity => rb.linearVelocity;
    DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> shakeTween;
    public bool IsThrowned => isThrowned;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initPos = transform.position;
        initRot = transform.rotation;
        initScale = transform.localScale;
        trail.SetActive(false);
    }


    public override void OnHover(bool isHovering)
    {
        if (isHovering)
        {
            if (!isThrowned)
            {
                if(shakeTween!=null)
                {
                    shakeTween.Kill();
                }
                Vector3 str = initScale + shakeStrength;
                shakeTween = transform.DOScale(str, hoverAnimDuration).SetEase(shakeEase).SetLoops(-1, LoopType.Yoyo);           
            }
        }
        else
        {
            if (shakeTween != null)
            {
                shakeTween.Kill();
            }
            transform.localScale = initScale;
        }
    }

    public override void OnBeginDrag()
    {
        base.OnBeginDrag();
        if (shakeTween != null)
        {
            Vector3 str = initScale + shakeStrength;
            shakeTween.ChangeValues(initScale, str, grabAnimDuration);
        }
    }

    public void Throw(float power, Vector3 movement)
    {
        //AudioManager.Instance.PlaySound2D("ThrowBall");
        trail.SetActive(true);
        GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        s.transform.position = transform.position + movement*power;
        rb.AddForce(movement * power, ForceMode.Impulse);
        if (shakeTween != null)
        {
            shakeTween.Kill();
        }
        transform.localScale = initScale;
        throwEffe.Play();
        isThrowned = true;
        DOVirtual.DelayedCall(5, () =>
        {
            isThrowned = false;
            trail.SetActive(false);
            throwEffe.Stop();
            transform.position = initPos;
            transform.rotation = initRot;
            //rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            DOVirtual.DelayedCall(0.01f, () => rb.isKinematic = false);
        });
    }
}
