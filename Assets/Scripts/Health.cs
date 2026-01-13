// Health.cs
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float maxHealth = 500f;
    public float currentHealth;
    public bool isBuilding = false;

    [Header("Team Settings")]
    public bool isPlayerSide = true; // 아군이면 true, 적군이면 false [추가]

    public UnityEvent OnDestroyed;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return;
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        OnDestroyed?.Invoke();
        if (isBuilding) Debug.Log(isPlayerSide ? "아군 건물 파괴!" : "적 건물 파괴!");
        Destroy(gameObject);
    }
}