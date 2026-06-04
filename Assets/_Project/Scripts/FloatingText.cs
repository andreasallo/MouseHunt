using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float moveUpDistance = 80f;
    [SerializeField] private float duration = 1f;

    private TMP_Text text;
    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Color startColor;
    private float timer;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();

        startPosition = rectTransform.anchoredPosition;

        if (text != null)
        {
            startColor = text.color;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = timer / duration;

        rectTransform.anchoredPosition = Vector2.Lerp(
            startPosition,
            startPosition + Vector2.up * moveUpDistance,
            t
        );

        if (text != null)
        {
            Color newColor = startColor;
            newColor.a = Mathf.Lerp(1f, 0f, t);
            text.color = newColor;
        }

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string value)
    {
        if (text == null)
        {
            text = GetComponent<TMP_Text>();
        }

        text.text = value;
    }
}