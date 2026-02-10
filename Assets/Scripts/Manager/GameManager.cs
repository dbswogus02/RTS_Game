using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환(재시작 등)을 위해 필요합니다.

public class GameManager : MonoBehaviour
{
    // 1. 싱글톤 패턴: 어디서나 GameManager.Instance로 승리 처리나 재시작을 호출할 수 있게 함
    public static GameManager Instance;

    // 승리했을 때 화면에 나타날 UI 오브젝트 (Panel 등)
    public GameObject victoryUI;

    void Awake()
    {
        // 싱글톤 초기화
        Instance = this;
    }

    // 2. 승리 조건 달성 시 호출되는 함수
    // 적의 핵심 건물(본진 등)이 파괴되었을 때 호출되도록 설계되었습니다.
    public void OnEnemyBuildingDestroyed()
    {
        Debug.Log("Victory!");

        // 승리 UI가 설정되어 있다면 화면에 활성화
        if (victoryUI != null)
            victoryUI.SetActive(true);

        // [핵심] 게임의 흐름을 멈춥니다.
        // 0으로 설정하면 모든 물리 연산과 프레임 기반 동작이 일시정지됩니다.
        Time.timeScale = 0f;
    }

    // 3. 게임 재시작 함수 (승리 UI의 '다시 하기' 버튼 등에 연결)
    public void RestartGame()
    {
        // 일시정지 상태를 다시 정상 속도(1)로 복구합니다. (중요!)
        Time.timeScale = 1f;

        // 현재 활성화된 씬의 이름을 가져와서 다시 로드합니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}