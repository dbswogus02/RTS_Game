using UnityEngine;

public class RTSSelectionManager : MonoBehaviour
{
    // 1. 싱글톤 패턴: 어디서나 RTSSelectionManager.Instance로 접근 가능하게 함
    public static RTSSelectionManager Instance;

    [Header("Top Level Panels")]
    public GameObject mainChoicePanel;      // [생산] 버튼과 [연구] 버튼이 있는 최상위 선택 패널
    public GameObject unitProductionPanel;  // 유닛을 생산하는 버튼들이 모인 패널
    public GameObject researchMainPanel;    // 연구의 종류(건물/유닛 등)를 고르는 패널

    [Header("Research Sub Panels")]
    public GameObject buildingResearchSubPanel; // 건물 관련 세부 연구 패널
    public GameObject unitResearchSubPanel;     // 유닛 관련 세부 연구 패널

    void Awake()
    {
        Instance = this;
    }

    // --- 핵심 로직: 유닛/건물 클릭 시 호출 ---
    public void HandleBuildingClick() // RTSController 등에서 건물을 클릭했을 때 호출됨
    {
        CloseAllUI(); // 일단 열려 있는 모든 창을 닫고
        if (mainChoicePanel != null)
            mainChoicePanel.SetActive(true); // "무엇을 하시겠습니까?" 패널을 띄움
    }

    // --- 패널 이동 함수들 (버튼에 연결하여 사용) ---

    // 유닛 생산 메뉴로 이동
    public void OpenProductionMenu()
    {
        CloseAllUI();
        unitProductionPanel.SetActive(true);
    }

    // 연구 메인 메뉴로 이동
    public void OpenResearchMainMenu()
    {
        CloseAllUI();
        researchMainPanel.SetActive(true);
    }

    // 건물 세부 연구 메뉴로 이동
    public void OpenBuildingResearch()
    {
        CloseAllUI();
        buildingResearchSubPanel.SetActive(true);
    }

    // 유닛 세부 연구 메뉴로 이동
    public void OpenUnitResearch()
    {
        CloseAllUI();
        unitResearchSubPanel.SetActive(true);
    }

    // --- 뒤로 가기(Back) 로직 ---

    // 하위 메뉴에서 다시 [생산/연구] 선택창으로 돌아감
    public void BackToMainChoice()
    {
        CloseAllUI();
        mainChoicePanel.SetActive(true);
    }

    // 세부 연구 메뉴에서 연구 메인 메뉴로 돌아감
    public void BackToResearchMain()
    {
        CloseAllUI();
        researchMainPanel.SetActive(true);
    }

    // 모든 UI 패널을 화면에서 숨기는 함수
    public void CloseAllUI()
    {
        mainChoicePanel.SetActive(false);
        unitProductionPanel.SetActive(false);
        researchMainPanel.SetActive(false);
        buildingResearchSubPanel.SetActive(false);
        unitResearchSubPanel.SetActive(false);
    }
}