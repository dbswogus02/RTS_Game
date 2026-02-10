using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    void Start()
    {
        // 1. 이 오브젝트(아이콘 전용 메시/이미지)의 레이어를 "Minimap"으로 강제 변경합니다.
        // 유니티 인스펙터 창 상단에 있는 Layer 설정을 코드로 실행하는 것입니다.
        gameObject.layer = LayerMask.NameToLayer("Minimap");

        // [작동 원리]
        // - 메인 카메라는 'Minimap' 레이어를 제외(Culling Mask)하고 렌더링합니다. (실제 게임 화면)
        // - 미니맵 카메라는 'Minimap' 레이어만 포함하여 렌더링합니다. (우측 하단 미니맵)
    }
}