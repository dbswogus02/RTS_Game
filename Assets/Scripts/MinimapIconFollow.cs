using UnityEngine;

public class MinimapIconFollow : MonoBehaviour
{
    // 유닛이 회전해도 아이콘은 회전하지 않도록 고정
    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}