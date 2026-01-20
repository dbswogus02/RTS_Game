using UnityEngine;

public class MapGridDrawer : MonoBehaviour
{
    public Material lineMaterial; // 선에 사용할 재질 (간단한 Unlit Color 추천)
    private float mapSize = 250f;
    private int gridX = 26; // A-Z
    private int gridZ = 10; // 1-10

    void Start()
    {
        DrawGrid();
    }

    void DrawGrid()
    {
        float halfSize = mapSize / 2f;

        // 가로선 그리기 (Z축 방향 구분)
        for (int i = 0; i <= gridZ; i++)
        {
            float z = -halfSize + (mapSize / gridZ) * i;
            CreateLine(new Vector3(-halfSize, 0.1f, z), new Vector3(halfSize, 0.1f, z));
        }

        // 세로선 그리기 (X축 방향 구분)
        for (int i = 0; i <= gridX; i++)
        {
            float x = -halfSize + (mapSize / gridX) * i;
            CreateLine(new Vector3(x, 0.1f, -halfSize), new Vector3(x, 0.1f, halfSize));
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.SetParent(this.transform);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.material = lineMaterial ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.2f;
        lr.endWidth = 0.2f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startColor = new Color(1, 1, 1, 0.3f); // 반투명 흰색
        lr.endColor = new Color(1, 1, 1, 0.3f);
        lr.useWorldSpace = true;
    }
}