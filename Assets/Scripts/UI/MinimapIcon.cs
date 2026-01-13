using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    void Start()
    {
        // 이 오브젝트(아이콘)의 레이어를 Minimap으로 설정
        // 메인 카메라에서는 보이지 않고 미니맵 카메라에서만 보이게 설정합니다.
        gameObject.layer = LayerMask.NameToLayer("Minimap");
    }
}