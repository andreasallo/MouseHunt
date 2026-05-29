using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuButtom : MonoBehaviour, IPointerClickHandler
{
    public int levelIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameSettings.selectedLevel = levelIndex;
        SceneManager.LoadScene("SampleScene");
    }
}