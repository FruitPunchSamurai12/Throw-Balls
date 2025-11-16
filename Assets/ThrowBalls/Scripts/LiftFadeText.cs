using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class LiftFadeText : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;
    public void Initialize(string textToShow, float duration, Color color, float moveY, Action<LiftFadeText> releaseAction, bool alwaysKeepSameDistanceFromCamera = false)
	{
		// Set values 
		Vector3 endValue = transform.position + new Vector3(0, moveY, 0);
        float alphaValue = 1;

        // Set text
        messageText.SetText(textToShow);
        messageText.color = color;

        // Do Transition
        if(alwaysKeepSameDistanceFromCamera)
        {
            Transform cam = Camera.main.transform;
			float dist = Vector3.Distance(cam.position,transform.position);
            DOVirtual.Vector3(transform.position, endValue, duration, (t) => 
            {
                Vector3 newPos = t;
                Vector3 dir = (newPos - cam.position).normalized;
                newPos = cam.position + dir * dist;
                transform.position = newPos;
            }).SetEase(Ease.OutSine);
        }
        else
        {
            transform.DOMove(endValue, duration).SetEase(Ease.OutSine);
        }
        DOTween.To(() => alphaValue, x => alphaValue = x, 0f, duration).OnUpdate(() => {
            transform.GetComponent<CanvasGroup>().alpha = alphaValue;
        }).OnComplete(() => {
            releaseAction?.Invoke(this);
        });
    }
}
