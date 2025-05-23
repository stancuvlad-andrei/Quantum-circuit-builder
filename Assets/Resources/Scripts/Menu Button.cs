using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public void BackToMainMenu()
    {
        // Destroy GridBuildingSystem to reset static data
        if (GridBuildingSystem.current != null)
        {
            Destroy(GridBuildingSystem.current.gameObject);
        }
        SceneManager.LoadScene(0);
    }
}
