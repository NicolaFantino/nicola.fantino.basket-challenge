using UnityEngine;
using UnityEngine.SceneManagement; // Fondamentale per cambiare scena

public class MainMenuManager : MonoBehaviour {

    [SerializeField] private string gameSceneName = "GameplayScene";

    public void PlayGame() {
        // Carica la scena di gioco
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame() {
        Debug.Log("Uscita dall'applicazione...");
        Application.Quit();
    }
}
