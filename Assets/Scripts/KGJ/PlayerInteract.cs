using System;
using System.Collections;
using UnityEngine;
using static IInteractable;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRadius = 2f;
    [SerializeField] private LayerMask interactableLayer;

    private IInteractable _closestTarget;
    private IInteractable _previousTarget;
    private float _holdTimer;
    private bool _isHolding;
    private bool _isInteracting = false; // ★★★ 상호작용 잠금 플래그 추가 ★★★

    private PlayerMovement _playerMovement;
    private PlayerInventory _playerInventory;
    private SpriteRenderer _sr;

    public bool IsHiding => _hidingDoor != null;
    private Door _hidingDoor;
    private BoxCollider2D _collider;

    // 이벤트 정의
    public static event Action<Transform> OnHoldStart;
    public static event Action<float> OnHoldProgress;
    public static event Action OnHoldEnd;
    public static event Action<IInteractable> OnTargetChanged;

    void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _collider = GetComponent<BoxCollider2D>();
        _sr = GetComponent<SpriteRenderer>();
        _playerInventory = PlayerInventory.Instance; 
    }

    void Update()
    {
        // ★★★ 1. 상호작용 루틴이 실행 중이면 Update 로직을 잠급니다. ★★★
        if (_isInteracting) return; 
        
        if (_hidingDoor != null)
        {
            _closestTarget = _hidingDoor;
        }
        else
        {
            _closestTarget = GetClosestInteractable();
        }

        // 가까운 대상이 바뀌었을 때만 이벤트 발행
        if (_closestTarget != _previousTarget)
        {
            OnTargetChanged?.Invoke(_closestTarget);
            _previousTarget = _closestTarget;
        }

        if (_closestTarget == null)
        {
            if (_isHolding)
                EndHold();
            return;
        }
        
        // 홀드형 상호작용
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1))
        {
            EndHold();
            return;
        }
        
        float requiredHoldTime = GetHoldDuration(_closestTarget.HoldType);

        // 즉시형 상호작용은 키를 누른 순간 1회만 실행
        if (requiredHoldTime <= 0f)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // ★★★ 루틴 시작 시 코루틴을 실행합니다. ★★★
                StartCoroutine(InstantInteractRoutine(_closestTarget));
            }
            return;
        }
        
        if (Input.GetKey(KeyCode.Space))
        {
            if (!_isHolding)
            {
                _isHolding = true;
                _playerMovement.EnableMove(false);
                _holdTimer = 0f;

                OnHoldStart?.Invoke((_closestTarget as MonoBehaviour)?.transform);
            }

            _holdTimer += Time.deltaTime;
            float ratio = Mathf.Clamp01(_holdTimer / requiredHoldTime);
            OnHoldProgress?.Invoke(ratio);

            if (_holdTimer >= requiredHoldTime)
            {
                _closestTarget.Interact();
                EndHold();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            EndHold();
        }
    }

    private void EndHold()
    {
        _isHolding = false;
        _holdTimer = 0f;
        if (!IsHiding)
            _playerMovement.EnableMove(true);

        OnHoldEnd?.Invoke();
    }

    private IInteractable GetClosestInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactableLayer);
        IInteractable closest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<IInteractable>();
            if (interactable == null) continue;
            if (!interactable.IsInteractable) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = interactable;
            }
        }

        return closest;
    }

    private float GetHoldDuration(InteractHoldType type)
    {
        switch (type)
        {
            case InteractHoldType.Instant: return 0f;
            case InteractHoldType.Short: return 1f;
            case InteractHoldType.Long: return 2f;
            default: return 1f;
        }
    }
    
    private IEnumerator InstantInteractRoutine(IInteractable target)
    {
        // ★★★ 2. 루틴 시작 시 잠금 설정 ★★★
        _isInteracting = true; 

        // 대상이 유효한지 최종적으로 검사
        if (target is MonoBehaviour mono && mono.gameObject.activeInHierarchy)
        {
            _playerMovement.EnableMove(false); 
            yield return new WaitForSeconds(0.1f); 
            
            // Interact 호출 (이후 DropItem이 파괴될 수 있음)
            target.Interact(); 
            
            if (!IsHiding)
                _playerMovement.EnableMove(true);
        }
        else
        {
             // 대상이 파괴되었거나 유효하지 않으면 조용히 종료
             Debug.LogWarning("Target object was destroyed before interaction start.");
        }

        // ★★★ 3. 루틴 종료 시 잠금 해제 ★★★
        _isInteracting = false; 
    }

    private void OnEnable()
    {
        // DropItem이 발생시키는 아이템 줍기 요청 이벤트를 구독합니다.
        DropItem.OnItemPickupRequested += HandleItemPickup;
        StunCollider.OnItemPickupRequested += HandleItemPickup;
    }

    private void OnDisable()
    {
        DropItem.OnItemPickupRequested -= HandleItemPickup;
        StunCollider.OnItemPickupRequested -= HandleItemPickup;
    }

    // 아이템 줍기 요청을 처리하는 함수
    private void HandleItemPickup(ItemCsvRow item, GameObject dropItemObject)
    {
        if (_playerInventory == null) return;

        bool success = _playerInventory.AddItem(item);

        if (success)
        {
            // 2. 성공 시 아이템 오브젝트 파괴
            Destroy(dropItemObject);
            
            UI_Popup.OnShowPopupRequested?.Invoke($"'{item.itemName}' 획득!");
        }
        else
        {
            UI_Popup.OnShowPopupRequested?.Invoke("인벤토리가 가득 찼습니다!");
        }
    }

    public void InteractDoor(Door door)
    {
        if (_hidingDoor == null)
        {
            _playerMovement.EnableMove(false);
            HideDoor(door);
        }
        else
        {
            Show(door);
        }
    }
    private void HideDoor(Door door)
    {
        _hidingDoor = door;
        _isInteracting = true;
        
        _collider.enabled = false;
        StartCoroutine(Hiding());
        
        SoundManager.Instance.PlaySFX(0);
    }

    private void Show(Door door)
    {
        _isInteracting = true;
        StartCoroutine(Showing());
    }

    private IEnumerator Hiding()
    {
        // 0.5초 간 서서히 _hidingDoor의 중심으로 이동하며 투명해짐
        float duration = 0.5f;
    
        // ⬇️ 페이드 아웃 (Alpha를 0.0f로)
        yield return StartCoroutine(FadeRoutine(0.0f, duration)); 
        
        // 플레이어의 실제 위치 변경 (선택적: 문 안으로 이동)
        transform.position = _hidingDoor.transform.position + Vector3.down * 2f;
            
        _isInteracting = false; 
        
        OnTargetChanged?.Invoke(_closestTarget);
    
        // **참고:** 페이드 아웃 후 Door 객체에 플레이어 비활성화 요청이 이미 되어 있어야 합니다.
        // (HideDoor 메서드에서 door.Hide(this.gameObject) 호출)
    }

    private IEnumerator Showing()
    {
        // 0.5초 간 서서히 플레이어의 투명도가 낮아짐.
        float duration = 0.5f;
    
        // 🚨 플레이어 객체는 Door.ExitHide()에서 이미 활성화(SetActive(true)) 되었다고 가정합니다.
    
        // ⬇️ 페이드 인 (Alpha를 1.0f로)
        yield return StartCoroutine(FadeRoutine(1.0f, duration)); 
    
        // 나가기 완료 후, 모든 상태를 해제합니다.
        _hidingDoor = null;
        _collider.enabled = true;
        _isInteracting = false; // 입력 잠금 해제
        _playerMovement.EnableMove(true); // 이동 잠금 해제
    }
    
    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        if (_sr == null) yield break;

        Color startColor = _sr.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            _sr.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        _sr.color = targetColor;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}