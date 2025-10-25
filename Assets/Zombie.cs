using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public enum ZombieState { Walk, Chase, Stunned }

public class Zombie : MonoBehaviour
{
    [Title("Refs")]
    [SerializeField, LabelText("애니메이터")] private Animator animator;
    [SerializeField, LabelText("스프라이트")] private SpriteRenderer spriteRenderer;
    [SerializeField, LabelText("리짓바디2D")] private Rigidbody2D rb;

    [Title("데이터")]
    [SerializeField, LabelText("좀비 데이터")] private ZombieData zombieData;

    [Title("이동 설정")]
    [SerializeField, LabelText("기본 속도")] private float moveSpeed = 2f;
    [SerializeField, LabelText("추적 배율(속도/애니속도)")] private float chaseMultiplier = 1.1f;
    [SerializeField, LabelText("방향전환 최소간격(초)")] private float turnMin = 1f;
    [SerializeField, LabelText("방향전환 최대간격(초)")] private float turnMax = 3f;

    [Title("탐지 설정")]
    [SerializeField, LabelText("탐지 반경")] private float detectRadius = 6f;
    [SerializeField, LabelText("플레이어 태그")] private string playerTag = "Player";

    [Title("애니메이터 파라미터/상태명")]
    [SerializeField, LabelText("Walk 상태명")] private string walkState = "Walk";
    [SerializeField, LabelText("Stun 트리거명")] private string stunTrigger = "Stun";
    [SerializeField, LabelText("isWalking(bool) - 옵션")] private string isWalkingParam = "isWalking";

    private Transform player;
    private ZombieState state = ZombieState.Walk;
    private int dir = 1;                       // +1 오른쪽, -1 왼쪽
    private Coroutine turnRoutine;
    private float stunEndTime = -1f;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void OnEnable()
    {
        // 스폰 방향 기준 초깃값
        dir = (zombieData != null && zombieData.isLeftSpawn) ? 1 : -1;

        SetState(ZombieState.Walk);

        if (turnRoutine != null) StopCoroutine(turnRoutine);
        turnRoutine = StartCoroutine(Co_AutoTurn());
    }

    void OnDisable()
    {
        if (turnRoutine != null) StopCoroutine(turnRoutine);
        if (animator) animator.speed = 1f; // 안전 복구
    }
    
    void FixedUpdate()
    {
        if (!rb) return;
    
        if (state == ZombieState.Stunned)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
    
        switch (state)
        {
            case ZombieState.Walk:
                Move(dir, moveSpeed);
                break;
    
            case ZombieState.Chase:
                Move(GetChaseDirection(), moveSpeed * chaseMultiplier);
                break;
        }
    }

    // ---------- 이동/방향 ----------
    private void Move(int direction, float speed)
    {
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        // 시선(보통 왼쪽=flipX true, 오른쪽=false)
        if (spriteRenderer) spriteRenderer.flipX = (direction > 0);
    }

    private int GetChaseDirection()
    {
        if (player == null) player = FindPlayer();
        if (!player) return dir;

        float dx = player.position.x - transform.position.x;
        return (dx >= 0f) ? +1 : -1;
    }

    private bool IsPlayerInRange()
    {
        if (player == null) player = FindPlayer();
        if (!player) return false;

        return Vector2.Distance(player.position, transform.position) <= detectRadius;
    }

    private Transform FindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        return go ? go.transform : null;
    }

    // ---------- 자동 방향 전환 ----------
    private IEnumerator Co_AutoTurn()
    {
        while (true)
        {
            if (state == ZombieState.Walk)
            {
                float wait = Random.Range(turnMin, turnMax);
                yield return new WaitForSeconds(wait);
                dir = (Random.value < 0.5f) ? -1 : 1;
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    // ---------- 상태/애니 ----------
    private void SetState(ZombieState next)
    {
        state = next;

        if (!animator) return;

        // 공통 초기화
        if (!string.IsNullOrEmpty(isWalkingParam))
            animator.SetBool(isWalkingParam, false);
        animator.ResetTrigger(stunTrigger);

        switch (state)
        {
            case ZombieState.Walk:
                if (!string.IsNullOrEmpty(isWalkingParam))
                    animator.SetBool(isWalkingParam, true);
                animator.CrossFade(walkState, 0.1f, 0);
                animator.speed = 1f; // 정상 속도
                break;

            case ZombieState.Chase:
                // 애니메이션은 Walk 유지, 재생 속도만 가속
                if (!string.IsNullOrEmpty(isWalkingParam))
                    animator.SetBool(isWalkingParam, true);
                animator.CrossFade(walkState, 0.1f, 0);
                animator.speed = chaseMultiplier; // 1.1배
                break;

            case ZombieState.Stunned:
                animator.speed = 1f;
                animator.SetTrigger(stunTrigger);
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
        {
            PlayerDataManager.Instance.GetHit();
        }
    }
    
    // ---------- 외부에서 스턴 호출 ----------
    public void Stun(float duration = 1.5f)
    {
        stunEndTime = Time.time + duration;
        if (rb) rb.linearVelocity = Vector2.zero;
        SetState(ZombieState.Stunned);
    }

    // ---------- 디버그 ----------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
