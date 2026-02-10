using UnityEngine;
using UnityEngine.UI; // 유니티의 기본(Legacy) UI 컴포넌트인 Text 등을 제어하기 위해 필수입니다.

public class UnitInfoUI : MonoBehaviour
{
    // 1. 싱글톤 패턴: 어디서나 UnitInfoUI.Instance.ShowInfo(...)로 접근하여 UI를 갱신할 수 있게 함
    public static UnitInfoUI Instance;

    [Header("UI References")]
    public GameObject infoPanel;   // 정보가 표시될 UI의 부모 패널 (전체를 끄고 켤 때 사용)
    public Text nameText;          // 유닛의 이름을 표시할 텍스트 컴포넌트
    public Text healthText;        // 유닛의 체력을 표시할 텍스트 컴포넌트

    void Awake()
    {
        // 인스턴스 초기화
        Instance = this;
    }

    // 2. 유닛의 정보를 UI에 갱신하고 화면에 보여주는 함수
    // unitName: 유닛의 이름, health: 현재 체력 값
    public void ShowInfo(string unitName, float health)
    {
        // 정보 패널이 꺼져 있다면 활성화합니다.
        if (infoPanel != null) infoPanel.SetActive(true);

        // 텍스트 컴포넌트에 유닛 정보를 할당합니다.
        if (nameText != null)
            nameText.text = "이름: " + unitName;

        if (healthText != null)
            // "F0"는 소수점 첫째 자리에서 반올림하여 정수 형태로만 표시하겠다는 뜻입니다.
            healthText.text = "체력: " + health.ToString("F0");
    }

    // 3. UI를 화면에서 숨기는 함수 (유닛 선택 해제 시 호출)
    public void HideInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }
}