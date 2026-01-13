using UnityEngine;

public enum ResearchType { BuildingHealth, MeleeAttack, RangeAttack, CavalryAttack }

[CreateAssetMenu(fileName = "NewResearch", menuName = "RTS/Research Data")]
public class ResearchData : ScriptableObject
{
    public string researchName;
    public ResearchType type;
    public int goldCost;
    public float upgradeValue; // 증가할 수치 (체력량 또는 공격력)
    public bool isCompleted = false;
}