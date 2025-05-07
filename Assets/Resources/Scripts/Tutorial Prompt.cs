using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialPrompt : MonoBehaviour
{
    [Header("Tutorial Panel")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI infoText;

    private bool tutorialVisible;

    #region Prompt Methods

    public void ShowTutorialMessage(string message)
    {
        tutorialPanel.SetActive(true);
        infoPanel.SetActive(false);
        tutorialText.text = message;
        tutorialVisible = true;
    }

    public void ShowObjectInfo(string info)
    {
        tutorialPanel.SetActive(false);
        infoPanel.SetActive(true);
        infoText.text = info;
        tutorialVisible = false;
    }

    public void HideAllPanels()
    {
        tutorialPanel.SetActive(false);
        infoPanel.SetActive(false);
        tutorialVisible = false;
    }

    public bool IsTutorialHidden() => !tutorialVisible;

    #endregion

}