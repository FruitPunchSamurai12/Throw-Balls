using UnityEngine;
using DG.Tweening;
using TMPro;

public class ThrowBallsManager : GenericDragAndDrop3D_Manager
{
    [SerializeField] ThrowingBallTarget[] targets;
    [SerializeField] ThrowingBall[] balls;
    [SerializeField] Vector2 minMaxPower = new Vector2(0.5f, 10f);
    [SerializeField] Vector2 minMaxTime = new Vector2(0.1f, 2f);
    [SerializeField] Transform forwardTR;
    [SerializeField] float distanceThreshold = 0.1f;
    [SerializeField] GameObject pointsUI;
    [SerializeField] TMP_Text pointsText;
    [SerializeField] TMP_Text durationText;
    [SerializeField] TMP_Text highscoreText;
    [SerializeField] float fadeTextPosOffset = 0.2f;
    [SerializeField] float spawnTextDuration = 1.5f;
    [SerializeField] float textScale = 0.004f;
    [SerializeField] float textMoveY = 1f;
    [SerializeField] GameObject impactEffect;
    [SerializeField] int scoreToWin = 300;
    [SerializeField] RectTransform tutPanel;
    [SerializeField] RectTransform tutIn;
    [SerializeField] RectTransform tutOut;
    [SerializeField] float tutAnimDuration = 0.75f;
    [SerializeField] Ease tutInEase = Ease.OutBack;
    [SerializeField] Ease tutOutEase = Ease.InBack;

    [Header("Trajectory Visualization")]
    [SerializeField] LineRenderer line;
    [SerializeField] int numOfPoints = 25;
    [SerializeField] float timeBetweenPoints = 0.1f;
    [SerializeField] LayerMask ballTargetsLayer;

    [Header("Game Sequence")]
    [SerializeField] float gameDuration = 20f;
    [SerializeField] float[] delaysToEnableTargets;

    float timeSpentDragging = 0;
    bool gameOngoing = false;
    Tweener timeTween;
    int totalPoints = 0;
    int highscore = 0;

    private void Awake()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].onKnockedDown += TargetDown;
        }
    }

    private void OnEnable()
    {
        totalPoints = 0;
        pointsUI.SetActive(true);
        pointsText.SetText(totalPoints.ToString());
        durationText.SetText(gameDuration.ToString("F1"));
        highscoreText.SetText(highscore.ToString());
        DOVirtual.DelayedCall(1f, () =>
        {
            tutPanel.DOAnchorPos(tutIn.anchoredPosition, tutAnimDuration).SetEase(tutInEase);
            //StartGame();
        });
    }

    public void StartGame()
    {
        //AudioManager.Instance.PlayMusic("ThrowBallsMusic");
        tutPanel.DOAnchorPos(tutOut.anchoredPosition, tutAnimDuration).SetEase(tutOutEase);
        float delayToEnableTarget = 0;
        gameOngoing = true;
        for (int i = 0; i < targets.Length; i++)
        {
            if (i < delaysToEnableTargets.Length)
                delayToEnableTarget = delaysToEnableTargets[i];

            var target = targets[i];
            DOVirtual.DelayedCall(delayToEnableTarget, () => { target.EnableTarget(); });
        }
        timeTween = DOVirtual.Float(gameDuration, 0, gameDuration, (t) =>
        {
            durationText.SetText(t.ToString("F1"));
        }).SetEase(Ease.Linear).OnComplete(() =>
        {
            gameOngoing = false;
            if (isDragging && currentDraggableObject != null)
            {
                EndDragging();
            }
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].DisableTarget();
            }
            if (totalPoints > highscore)
            {
                highscore = totalPoints;
            }
            DOVirtual.DelayedCall(1, () =>
            {
                if (totalPoints >= scoreToWin)
                {
                    Debug.Log("won enough");
                }
                else
                {
					Debug.Log("won");
				}
            });
        });
    }

    private void OnDisable()
    {
        if(timeTween!=null)
        {
            timeTween.Kill();
        }
        //AudioManager.Instance.PlayBackgroundMusic();
        pointsUI.SetActive(false);
        gameOngoing = false;
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].ResetTarget();
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].onKnockedDown -= TargetDown;
        }
    }

    void TargetDown(int points,Vector3 pos)
    {
        if (!gameOngoing)
            return;
        totalPoints += points;
        if (totalPoints > highscore)
        {
            highscore = totalPoints;
            highscoreText.SetText(highscore.ToString());
        }
        pointsText.SetText(totalPoints.ToString());
        impactEffect.transform.position = pos;
        impactEffect.SetActive(true);
        LiftFadeTextManager.Instance.SpawnFadeText($"+{points}", pos + Vector3.up * fadeTextPosOffset, spawnTextDuration, textScale, Color.green, textMoveY);
    }

    override protected void BeginDragging()
    {
        if (!gameOngoing)
            return;
        ThrowingBall ball = hoverDraggableObject as ThrowingBall;
        if (ball == null || ball.IsThrowned)
            return;
        currentDraggableObject = hoverDraggableObject;
        timeSpentDragging = 0;
        if (currentDraggableObject != null)
        {
            initialMousePosition = GetInputPosition();
            initialWorldPosition = currentDraggableObject.transform.position;
            isDragging = true;
            initialWorldPosition = currentDraggableObject.transform.position;
            currentDraggableObject.OnBeginDrag();
        }
    }

    override protected void OnDragging()
    {
        timeSpentDragging += Time.deltaTime;
        Vector3 currentMousePos = GetInputPosition();
        currentMousePos.z = mainCamera.WorldToScreenPoint(movementPlanePosition.position).z;
        Vector3 newPosition = mainCamera.ScreenToWorldPoint(currentMousePos);
        Vector3 worldMovement = (newPosition - initialWorldPosition).normalized;
        DrawTrajectoryVisualization(worldMovement);
    }

   
    override protected void EndDragging()
    {
        Vector3 currentMousePos = GetInputPosition();
        currentMousePos.z = mainCamera.WorldToScreenPoint(movementPlanePosition.position).z;
        Vector3 newPosition = mainCamera.ScreenToWorldPoint(currentMousePos);      

        Vector3 worldMovement = (newPosition - initialWorldPosition).normalized;
        Vector3 mouseMovement = newPosition - initialWorldPosition;

        float dist = Vector3.SqrMagnitude(mouseMovement);
       // Debug.Log($"dist  {dist}");
        if (dist>distanceThreshold)
        {
            ThrowingBall ball = currentDraggableObject as ThrowingBall;
            ball.Throw(CalculateInverseLerpPower(), worldMovement);
        }

        line.enabled = false;
        currentDraggableObject.OnEndDrag();
        currentDraggableObject = null;
        hoverDraggableObject = null;
        isDragging = false;
    }

    float CalculateInverseLerpPower()
    {
        float clampedTime = Mathf.Clamp(timeSpentDragging, minMaxTime.x, minMaxTime.y);
        float normalizedTime = Mathf.InverseLerp(minMaxTime.x, minMaxTime.y, clampedTime);

        // Apply exponential curve (adjust exponent to change curve shape)
        float inverseTime = Mathf.Pow(1f - normalizedTime, 0.5f);

        return Mathf.Lerp(minMaxPower.x, minMaxPower.y, inverseTime);
    }

    private void DrawTrajectoryVisualization(Vector3 direction)
    {
        line.enabled = true;
        line.positionCount = Mathf.CeilToInt(numOfPoints / timeBetweenPoints) + 1;
        Vector3 startPosition = initialWorldPosition;
        Vector3 startVelocity = CalculateInverseLerpPower() * direction;
        int i = 0;
        line.SetPosition(i, startPosition);
        for (float time = 0; time < numOfPoints; time += timeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            line.SetPosition(i, point);

            Vector3 lastPosition = line.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit, (point - lastPosition).magnitude, ballTargetsLayer))
            {
                line.SetPosition(i, hit.point);
                line.positionCount = i + 1;
                return;
            }
        }
    }
}

