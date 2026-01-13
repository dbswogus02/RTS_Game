using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject victoryUI;

    void Awake() { Instance = this; }

    public void OnEnemyBuildingDestroyed()
    {
        Debug.Log("Victory!");
        if (victoryUI != null) victoryUI.SetActive(true);
        Time.timeScale = 0f; // 게임 일시정지
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}