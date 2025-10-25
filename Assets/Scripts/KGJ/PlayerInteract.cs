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

    // 이벤트 정의
    public static event Action<Transform> OnHoldStart;
    public static event Action<float> OnHoldProgress;
    public static event Action OnHoldEnd;
    public static event Action<IInteractable> OnTargetChanged;

    void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        // PlayerInventory 싱글톤이 Ready 상태인지 확인하거나, Null 체크를 위해 Start에서 Instance에 접근하는 것이 더 안전합니다.
        _playerInventory = PlayerInventory.Instance; 
    }

    void Update()
    {
        // ★★★ 1. 상호작용 루틴이 실행 중이면 Update 로직을 잠급니다. ★★★
        if (_isInteracting) return; 

        _closestTarget = GetClosestInteractable();

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
            if (Input.GetKeyDown(KeyCode.E))
            {
                // ★★★ 루틴 시작 시 코루틴을 실행합니다. ★★★
                StartCoroutine(InstantInteractRoutine(_closestTarget));
            }
            return;
        }
        
        if (Input.GetKey(KeyCode.E))
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
        else if (Input.GetKeyUp(KeyCode.E))
        {
            EndHold();
        }
    }

    private void EndHold()
    {
        //if (!_isHolding) return;

        _isHolding = false;
        _holdTimer = 0f;
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
    }

    private void OnDisable()
    {
        DropItem.OnItemPickupRequested -= HandleItemPickup;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}