using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ZombieHitArea : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true; // 데미지 영역은 트리거
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        
        // 여기서 데미지 처리
        PlayerDataManager.Instance.GetHit();
    }
}