using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    [SerializeField] private string sceneName;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    private Material materialInstance;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            materialInstance = targetRenderer.material;
            materialInstance.color = normalColor;
        }
    }

    public void SetHover(bool isHovering)
    {
        if (materialInstance != null)
        {
            materialInstance.color = isHovering ? hoverColor : normalColor;
        }
    }

    public void Select()
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("MenuButton: Scene Name is empty on " + gameObject.name);
            return;
        }

        Debug.Log("Loading scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}