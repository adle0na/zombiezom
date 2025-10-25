using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ZombieState { Walk, Chase, Stunned }

// ⬇️ IInteractable 인터페이스를 구현합니다.
public class Zombie : MonoBehaviour, IInteractable
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

    [Title("앞 센서 설정")]
    [SerializeField, LabelText("앞 센서")] private ZombieHitArea frontSense;   // 자식에 붙인 스크립트
    [SerializeField, LabelText("앞 센서 기본 거리")] private float frontSensorDistance = 0.8f;
    [SerializeField, LabelText("앞 센서 Y오프셋")] private float frontSensorYOffset = 0f;
    
    [SerializeField, LabelText("뒤 감지 센서")] private BackSenseArea backSense; // 자식에 붙인 스크립트
    [SerializeField, LabelText("뒤 센서 기본 거리")] private float backSensorDistance = 0.8f;
    [SerializeField, LabelText("뒤 센서 Y오프셋")] private float backSensorYOffset = 0f;
    
    private Transform sensedPlayer; // 감지된 플레이어(옵션)
    
    private Transform player;
    public ZombieState state = ZombieState.Walk;
    private int dir = 1;                       // +1 오른쪽, -1 왼쪽
    private Coroutine turnRoutine;
    private Coroutine stunRoutine;
    public ZombieData ZombieData => zombieData;
    private Collider2D _stunCollider; // 좀비가 주울 수 있는 상태가 되었을 때 활성화될 콜라이더
    
    // ⬇️ 아이템 줍기 요청 이벤트
    public static event Action<ItemCsvRow, GameObject> OnItemPickupRequested; 
    
    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        StunCollider stunComponent = GetComponentInChildren<StunCollider>();
        
        if (stunComponent != null)
        {
            _stunCollider = stunComponent.gameObject.GetComponent<Collider2D>();
            
            if (_stunCollider != null)
            {
                _stunCollider.enabled = false;
            }
        }
    }

    void OnEnable()
    {
        dir = (zombieData != null && zombieData.isLeftSpawn) ? 1 : -1;

        UpdateSensorsPosition();
        
        // 🔹 뒤 감지 센서 초기화(자식 트리거에 붙은 BackSenseArea)
        if (backSense != null)
            backSense.Init(this, playerTag);

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
        if (state == ZombieState.Stunned) return;
        
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        if (spriteRenderer) spriteRenderer.flipX = (direction > 0);
        UpdateSensorsPosition();
    }

    private int GetChaseDirection()
    {
        if (player == null) player = FindPlayer();
        if (!player) return dir;

        float dx = player.position.x - transform.position.x;
        return (dx >= 0f) ? +1 : -1;
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
        
        // ⬇️ 센서 및 스턴 콜라이더 활성화/비활성화 통합 처리
        bool sensorActive = (next != ZombieState.Stunned);
        if (frontSense) frontSense.gameObject.SetActive(sensorActive);
        if (backSense) backSense.gameObject.SetActive(sensorActive);
        if (_stunCollider) _stunCollider.enabled = !sensorActive; // Stunned 상태일 때만 true

        switch (state)
        {
            case ZombieState.Walk:
                if (!string.IsNullOrEmpty(isWalkingParam))
                    animator.SetBool(isWalkingParam, true);
                animator.CrossFade(walkState, 0.1f, 0);
                animator.speed = 1f; // 정상 속도
                break;

            case ZombieState.Chase:
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

    // ---------- 외부에서 스턴 호출 ----------
    public void Stun()
    {
        if (state == ZombieState.Stunned) return; // 이미 기절 상태면 무시
        if (animator == null) return; // 애니메이터 없으면 로직 진행 불가
        
        if (rb) rb.linearVelocity = Vector2.zero;

        // ⬇️ SetState를 사용하여 모든 상태 로직 통합
        SetState(ZombieState.Stunned); 
        
        if (stunRoutine != null)
        {
            StopCoroutine(stunRoutine);
        }
        stunRoutine = StartCoroutine(StunCor());
    }

    IEnumerator StunCor()
    {
        yield return new WaitForSeconds(4);
        
        // ⬇️ 상태를 복구할 때는 SetState를 사용합니다.
        SetState(ZombieState.Walk); 
        stunRoutine = null;
    }
    
    private void UpdateSensorsPosition()
    {
        float facing = (spriteRenderer != null && spriteRenderer.flipX) ? 1f : -1f;

        // 🔹 앞 센서
        if (frontSense != null)
        {
            var tf = frontSense.transform;
            Vector3 local = tf.localPosition;
            local.x = facing * Mathf.Abs(frontSensorDistance);
            local.y = frontSensorYOffset;
            tf.localPosition = local;
        }

        // 🔹 뒤 센서
        if (backSense != null)
        {
            var tb = backSense.transform;
            Vector3 local = tb.localPosition;
            local.x = -facing * Mathf.Abs(backSensorDistance);
            local.y = backSensorYOffset;
            tb.localPosition = local;
        }
    }
    
    // ---------- 디버그 ----------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }

    // ===================================
    // 💀 IInteractable 구현
    // ===================================

    public IInteractable.InteractHoldType HoldType => IInteractable.InteractHoldType.Short; 
    
    // 🚨 상호작용 가능 조건: 기절 상태이면서 특정 타입이 아닐 때
    public bool IsInteractable => (state == ZombieState.Stunned) && (zombieData?.zombieType != ZombieType.DisCureZombie);

    public void Interact()
    {
        if (!IsInteractable) return;

        // 기절 루틴이 진행 중이었다면 중단
        if (stunRoutine != null)
        {
            StopCoroutine(stunRoutine);
            stunRoutine = null;
        }
        
        int index = 21;
        // ⬇️ 좀비 데이터가 null일 경우 대비
        if (zombieData != null)
        {
            if (zombieData.zombieType == ZombieType.SuaZombie)
            {
                index += 5;
            }
            else
            {
                int floor = PlayerDataManager.Instance.PlayerFloor;
                index += floor;
            }
        }
        
        ItemCsvRow zombieItem = ItemDataManager.Instance.GetItemByIndex(index);
        
        // PlayerInteract의 HandleItemPickup에서 Destroy를 담당하도록 이벤트를 발행
        OnItemPickupRequested?.Invoke(zombieItem, gameObject); 
    }

    public string GetInteractPrompt()
    {
        return "납치하기";
    }
}