using UnityEngine;

public class RTSSelectionManager : MonoBehaviour
{
    public static RTSSelectionManager Instance;

    [Header("Top Level Panels")]
    public GameObject mainChoicePanel;      // [생산] [연구] 선택
    public GameObject unitProductionPanel;  // 유닛 생산 목록
    public GameObject researchMainPanel;    // 연구 종류 선택 (건물/유닛)

    [Header("Research Sub Panels")]
    public GameObject buildingResearchSubPanel;
    public GameObject unitResearchSubPanel;

    void Awake() { Instance = this; }

    public void HandleBuildingClick() // RTSController에서 호출
    {
        CloseAllUI();
        if (mainChoicePanel != null) mainChoicePanel.SetActive(true);
    }

    // --- 패널 이동 함수들 ---
    public void OpenProductionMenu() { CloseAllUI(); unitProductionPanel.SetActive(true); }
    public void OpenResearchMainMenu() { CloseAllUI(); researchMainPanel.SetActive(true); }

    public void OpenBuildingResearch() { CloseAllUI(); buildingResearchSubPanel.SetActive(true); }
    public void OpenUnitResearch() { CloseAllUI(); unitResearchSubPanel.SetActive(true); }

    // 뒤로 가기 로직
    public void BackToMainChoice() { CloseAllUI(); mainChoicePanel.SetActive(true); }
    public void BackToResearchMain() { CloseAllUI(); researchMainPanel.SetActive(true); }

    public void CloseAllUI()
    {
        mainChoicePanel.SetActive(false);
        unitProductionPanel.SetActive(false); //
        researchMainPanel.SetActive(false);
        buildingResearchSubPanel.SetActive(false);
        unitResearchSubPanel.SetActive(false);

        if (UIManager.Instance != null) UIManager.Instance.DisplayUnitInfo(null); //
    }
}