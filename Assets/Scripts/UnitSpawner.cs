using UnityEngine;
using UnityEngine.AI;

public class UnitSpawner : MonoBehaviour
{
    public Transform spawnPoint;

    public void SpawnUnit(UnitData data)
    {
        if (data == null || data.unitPrefab == null) return;

        // 자원 및 인구수 확인
        if (ResourceManager.Instance.CanProduceUnit(data.unitCost, data.unitCapacity))
        {
            ResourceManager.Instance.UseResources(data.unitCost, data.unitCapacity);

            // 1. 소환 위치 설정 (변수를 미리 선언하여 에러 방지)
            // 소환 시 유닛들이 겹치지 않도록 반지름 1.5m 이내의 랜덤 위치를 계산합니다.
            Vector2 randomCircle = Random.insideUnitCircle * 1.5f;
            Vector3 spawnOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
            Vector3 targetSpawnPos = spawnPoint.position + spawnOffset;

            // NavMesh 위에서 가장 가까운 위치를 찾습니다.
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetSpawnPos, out hit, 3.0f, NavMesh.AllAreas))
            {
                targetSpawnPos = hit.position;
            }

            // 2. 유닛 소환
            GameObject spawnedUnit = Instantiate(data.unitPrefab, targetSpawnPos, Quaternion.identity);

            // 3. 모든 자식 오브젝트의 레이어를 재설정
            SetLayerRecursive(spawnedUnit.transform, "Unit", "Minimap");

            // 4. 유닛 데이터 전달 및 초기 이동 명령
            PhysicsUnitMover mover = spawnedUnit.GetComponent<PhysicsUnitMover>();
            if (mover != null)
            {
                mover.unitData = data; // 유닛 데이터 할당
                mover.moveSpeed = data.moveSpeed;
                mover.attackRange = data.attackRange;

                // [추가] 소환되자마자 건물에서 조금 떨어진 곳(앞쪽)으로 걸어가게 합니다.
                // 이렇게 하면 뒤에 나오는 유닛과 겹치는 현상이 훨씬 줄어듭니다.
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

    private void SetLayerRecursive(Transform parent, string targetLayerName, string excludeLayerName)
    {
        int targetLayer = LayerMask.NameToLayer(targetLayerName);
        int excludeLayer = LayerMask.NameToLayer(excludeLayerName);

        if (parent.gameObject.layer != excludeLayer)
        {
            parent.gameObject.layer = targetLayer;
        }

        foreach (Transform child in parent)
        {
            SetLayerRecursive(child, targetLayerName, excludeLayerName);
        }
    }
}