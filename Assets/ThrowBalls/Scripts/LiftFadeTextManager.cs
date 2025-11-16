using UnityEngine;
using UnityEngine.Pool;

public class LiftFadeTextManager : MonoBehaviour
{
    [SerializeField] LiftFadeText fadeTextPrefab;
    [SerializeField] float fadeDuration = 0.7f;
    [SerializeField] float textScale = 0.01f;
    [SerializeField] Color textColor = Color.white;
    [SerializeField] float floatingTextMoveY = 20f;
    [SerializeField] Transform textParent;
    ObjectPool<LiftFadeText> fadeTextPool;
    public static LiftFadeTextManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
            fadeTextPool = new ObjectPool<LiftFadeText>(() => 
            { 
                LiftFadeText text = Instantiate(fadeTextPrefab);
                text.transform.SetParent(textParent);
                return text;
            }, (t) => 
            {
                t.gameObject.SetActive(true);
                t.transform.SetParent(null);
            }
            , (t) => 
            { 
                t.gameObject.SetActive(false);
                t.transform.SetParent(textParent);
            }, (t) => Destroy(t));
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnFadeText(string text, Vector3 position, bool alwaysKeepSameDistanceFromCamera = false)
    {
        SpawnFadeText(text, position, fadeDuration, textScale, textColor, floatingTextMoveY, alwaysKeepSameDistanceFromCamera);
    }

    public void SpawnFadeText(float scale, string text, Vector3 position, bool alwaysKeepSameDistanceFromCamera = false)
    {
        SpawnFadeText(text, position, fadeDuration, scale, textColor, floatingTextMoveY, alwaysKeepSameDistanceFromCamera);
    }

    public void SpawnFadeText(float scale, string text, Vector3 position,float textMoveY, bool alwaysKeepSameDistanceFromCamera = false)
    {
        SpawnFadeText(text, position, fadeDuration, scale, textColor, textMoveY, alwaysKeepSameDistanceFromCamera);
    }

    public void SpawnFadeText(string text, Vector3 position, float duration, bool alwaysKeepSameDistanceFromCamera = false)
    {
        SpawnFadeText(text, position, duration, textScale, textColor, floatingTextMoveY, alwaysKeepSameDistanceFromCamera);
    }

    public void SpawnFadeText(string text, Vector3 position, Color color, bool alwaysKeepSameDistanceFromCamera = false)
    {
        SpawnFadeText(text, position, fadeDuration, textScale, color, floatingTextMoveY, alwaysKeepSameDistanceFromCamera);
    }

    public void SpawnFadeText(string text, Vector3 position, float duration, float scale, bool alwaysKeepSameDistanceFromCamera = false)
    {
        SpawnFadeText(text, position, duration, scale, textColor, floatingTextMoveY, alwaysKeepSameDistanceFromCamera);
    }

    public void SpawnFadeText(string text, Vector3 position, float duration, Color color, bool alwaysKeepSameDistanceFromCamera = false)
    {
        SpawnFadeText(text, position, duration, textScale, color, floatingTextMoveY, alwaysKeepSameDistanceFromCamera);
    }

    public void SpawnFadeText(string text, Vector3 position, Color color,float scale, bool alwaysKeepSameDistanceFromCamera = false)
    {
        SpawnFadeText(text, position, fadeDuration, scale, color,floatingTextMoveY, alwaysKeepSameDistanceFromCamera);
    }

    public void SpawnFadeText(string text, Vector3 position, float duration, Color color, float scale, bool alwaysKeepSameDistanceFromCamera = false)
    {
        SpawnFadeText(text, position, duration, scale, color, floatingTextMoveY, alwaysKeepSameDistanceFromCamera);
    }

    public void SpawnFadeText(string text, Vector3 position, float duration,float scale, Color color, float textMoveY, bool alwaysKeepSameDistanceFromCamera = false)
    {
        LiftFadeText fadeText = fadeTextPool.Get();
        fadeText.transform.position = position;
        fadeText.transform.localScale = Vector3.one * scale;
        fadeText.Initialize(text,duration,color, textMoveY, ReleaseText, alwaysKeepSameDistanceFromCamera);
    }

    void ReleaseText(LiftFadeText fadeText)
    {
        fadeTextPool.Release(fadeText);
    }
}