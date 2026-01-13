using UnityEngine;
using UnityEngine.UI; // 레거시 UI Text를 사용하기 위해 필요

public class UnitInfoUI : MonoBehaviour
{
    public static UnitInfoUI Instance;

    public GameObject infoPanel;   // 하단 중앙 정보 패널
    public Text nameText;         // 기존 UI Text 컴포넌트
    public Text healthText;       // 기존 UI Text 컴포넌트

    void Awake()
    {
        Instance = this;
    }

    // 정보를 갱신하고 패널을 활성화하는 함수
    public void ShowInfo(string unitName, float health)
    {
        if (infoPanel != null) infoPanel.SetActive(true);

        if (nameText != null) nameText.text = "이름: " + unitName;
        if (healthText != null) healthText.text = "체력: " + health.ToString("F0");
    }

    // 패널을 숨기는 함수
    public void HideInfo()
    {
        if (infoPanel != null) infoPanel.SetActive(false);
    }
}