using UnityEngine;
using UnityEngine.AI; // NavMesh를 사용하여 유닛 위치를 잡기 위해 필요

public class UnitSpawner : MonoBehaviour
{
    public Transform spawnPoint; // 유닛이 실제로 생성될 기준 위치

    // 유닛 데이터를 받아 소환을 실행하는 함수
    public void SpawnUnit(UnitData data)
    {
        // 유효성 검사: 데이터나 프리팹이 없으면 중단
        if (data == null || data.unitPrefab == null) return;

        // 1. 자원 및 인구수 확인 (ResourceManager 이용)
        if (ResourceManager.Instance.CanProduceUnit(data.unitCost, data.unitCapacity))
        {
            // 자원 및 인구수 실제 차감
            ResourceManager.Instance.UseResources(data.unitCost, data.unitCapacity);

            // 2. 소환 위치 계산 (유닛 겹침 방지)
            // 반지름 1.5m 원 안의 랜덤한 위치를 구함
            Vector2 randomCircle = Random.insideUnitCircle * 1.5f;
            Vector3 spawnOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
            Vector3 targetSpawnPos = spawnPoint.position + spawnOffset;

            // NavMesh(길찾기 맵) 위에서 가장 가까운 유효한 위치를 찾음 (소환 위치 보정)
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetSpawnPos, out hit, 3.0f, NavMesh.AllAreas))
            {
                targetSpawnPos = hit.position;
            }

            // 3. 실제 유닛 오브젝트 생성
            GameObject spawnedUnit = Instantiate(data.unitPrefab, targetSpawnPos, Quaternion.identity);

            // 4. 레이어 설정 (재귀 함수 호출)
            // 일반 유닛은 "Unit" 레이어로, 미니맵 아이콘 등은 그대로 "Minimap" 레이어로 유지
            SetLayerRecursive(spawnedUnit.transform, "Unit", "Minimap");

            // 5. 유닛 데이터 초기화 및 이동 명령
            PhysicsUnitMover mover = spawnedUnit.GetComponent<PhysicsUnitMover>();
            if (mover != null)
            {
                mover.unitData = data;       // 유닛 고유 데이터 할당
                mover.moveSpeed = data.moveSpeed;
                mover.attackRange = data.attackRange;

                // 랠리 포인트(Rally Point) 설정: 소환 직후 건물 앞쪽으로 2m 걸어가게 함
                // 이를 통해 건물 소환 지점에 유닛이 계속 뭉쳐있는 것을 방지합니다.
                Vector3 rallyPoint = targetSpawnPos + transform.forward * 2f;
                mover.SetTarget(rallyPoint);
            }

            Debug.Log($"<color=white>{data.unitName}</color> 소환 완료! (위치: {targetSpawnPos})");
        }
        else
        {
            Debug.Log($"<color=yellow>[자원 부족]</color> {data.unitName} 소환 실패!");
        }
    }

    // 모든 자식 오브젝트의 레이어를 계층적으로 변경하는 함수
    // targetLayerName으로 바꾸되, excludeLayerName(미니맵 아이콘 등)은 건드리지 않음
    private void SetLayerRecursive(Transform parent, string targetLayerName, string excludeLayerName)
    {
        int targetLayer = LayerMask.NameToLayer(targetLayerName);
        int excludeLayer = LayerMask.NameToLayer(excludeLayerName);

        // 현재 오브젝트가 제외 대상 레이어가 아니라면 변경
        if (parent.gameObject.layer != excludeLayer)
        {
            parent.gameObject.layer = targetLayer;
        }

        // 자식 오브젝트들에게도 동일한 로직 적용 (재귀)
        foreach (Transform child in parent)
        {
            SetLayerRecursive(child, targetLayerName, excludeLayerName);
        }
    }
}