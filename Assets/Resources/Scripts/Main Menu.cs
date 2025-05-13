using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu Components")]
    [SerializeField] private Image mainBackground; // Background image
    [SerializeField] private Button playButton; // Play button
    [SerializeField] private Button quitButton; // Quit button

    [Header("Levels Panel Components")]
    [SerializeField] private GameObject levelsPanel; // Panel for level selection
    [SerializeField] private Button backButton; // Back button to return to main menu

    [Header("Background Sprites")]
    [SerializeField] private Sprite mainMenuWithTitle; // Background with title
    [SerializeField] private Sprite mainMenuClean; // Clean background without title

    #region Unity Methods

    private void Start()
    {
        // Initialize UI state
        ReturnToMainMenu();

        // Setup button listeners
        playButton.onClick.AddListener(ShowLevelsPanel);
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(ReturnToMainMenu);
    }

    #endregion

    #region Panel Methods

    private void ShowLevelsPanel()
    {
        // Switch to clean background
        mainBackground.sprite = mainMenuClean;

        // Toggle visibility
        SetMainMenuElements(false);
        levelsPanel.SetActive(true);
    }

    public void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }

    private void ReturnToMainMenu()
    {
        // Restore original background
        mainBackground.sprite = mainMenuWithTitle;

        // Toggle visibility
        SetMainMenuElements(true);
        levelsPanel.SetActive(false);
    }

    #endregion

    #region Main Menu Methods

    private void SetMainMenuElements(bool state)
    {
        playButton.gameObject.SetActive(state);
        quitButton.gameObject.SetActive(state);
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    #endregion

}