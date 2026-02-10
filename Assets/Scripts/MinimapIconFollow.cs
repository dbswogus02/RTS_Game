using UnityEngine;

public class MinimapIconFollow : MonoBehaviour
{
    // LateUpdate는 모든 Update 함수가 실행된 후 마지막에 호출됩니다.
    // 유닛의 이동이나 회전이 완료된 시점에 아이콘의 회전을 다시 보정하기에 가장 적합한 시점입니다.
    void LateUpdate()
    {
        // 아이콘의 회전값을 (90, 0, 0)으로 강제 고정합니다.
        // X축을 90도 회전시키면 아이콘 평면이 하늘(Y축 위쪽)을 바라보게 되어,
        // 위에서 아래를 내려다보는 미니맵 카메라에 가장 선명하게 보입니다.
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}