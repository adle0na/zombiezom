using System;
using System.Collections;
using UnityEngine;
using static IInteractable;

public class PlayerInteract : MonoBehaviour
{
    private static readonly int Enable = Shader.PropertyToID("_Enable");

    [Header("Interaction Settings")]
    [SerializeField] private float interactRadius = 2f;
    [SerializeField] private LayerMask interactableLayer;

    private IInteractable _closestTarget;
    private IInteractable _previousTarget;
    private float _holdTimer;
    private bool _isHolding;
    private bool _isInteracting = false; // ★★★ 상호작용 잠금 플래그 추가 ★★★
    private bool _wasHoldingCompleted = false;
    private bool _isTargetLocked;
    private IInteractable _lockedTarget;

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
        // 1. 상호작용 루틴이 실행 중이면 Update 로직을 잠급니다.
        if (_isInteracting) return; 
        
        if (_hidingDoor != null)
        {
            _closestTarget = _hidingDoor;
        }
        else if (TryGetLockedTarget(out var lockedTarget))
        {
            _closestTarget = lockedTarget;
        }
        else
        {
            _closestTarget = GetClosestInteractable();
        }

        if (_closestTarget != null && _closestTarget.IsInteractable)
            _closestTarget?.InteractableMaterial?.SetFloat(Enable, 1);
        else if (_closestTarget != null && !_closestTarget.IsInteractable)
            _closestTarget?.InteractableMaterial?.SetFloat(Enable, 0);
        
        // 가까운 대상이 바뀌었을 때만 이벤트 발행
        if (_closestTarget != _previousTarget)
        {
            _previousTarget?.InteractableMaterial?.SetFloat(Enable, 0);
            OnTargetChanged?.Invoke(_closestTarget);
            _previousTarget = _closestTarget;
            _closestTarget?.InteractableMaterial?.SetFloat(Enable, 1);
        }

        if (_closestTarget == null)
        {
            if (_isHolding)
                EndHold();
            
            // 키를 놓았는지 확인 후 상태 초기화
            if (Input.GetKeyUp(KeyCode.Space))
            {
                _wasHoldingCompleted = false; 
            }
            return;
        }
        
        // 홀드형 상호작용 (마우스)
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1))
        {
            EndHold();
            _wasHoldingCompleted = false;
            return;
        }
        
        float requiredHoldTime = GetHoldDuration(_closestTarget.HoldType);

        // 즉시형 상호작용 (requiredHoldTime <= 0f)
        if (requiredHoldTime <= 0f)
        {
            // ★★★ 수정: 즉시 상호작용이 가능한 상태에서도 완료 플래그 초기화 로직을 유지 ★★★
            if (Input.GetKeyUp(KeyCode.Space))
            {
                _wasHoldingCompleted = false;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!PlayerDataManager.Instance.canHit) return;
                
                // 숨어있는 상태에서 나갈 때는 딜레이 없이 즉시 처리
                if (IsHiding)
                {
                    _closestTarget.Interact();
                    _wasHoldingCompleted = true;
                }
                else
                {
                    // 루틴 시작 시 코루틴을 실행합니다.
                    StartCoroutine(InstantInteractRoutine(_closestTarget));
                    _wasHoldingCompleted = true; 
                }
            }
            return;
        }
        
        // ★★★ 수정: 상호작용 완료 후 키를 놓기 전에는 새로운 홀드가 시작되지 않도록 잠금 ★★★
        if (_wasHoldingCompleted)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                _wasHoldingCompleted = false; // 키를 놓았으므로 잠금 해제
            }
            return; // 홀드 또는 즉시 상호작용 완료 후 키를 놓지 않으면 여기서 리턴
        }

        // Space 키를 누르는 중 (홀드형 상호작용)
        // ★★★ 수정: 숨어있는 상태에서는 홀드 상호작용을 하지 않음 ★★★
        if (Input.GetKey(KeyCode.Space) && !IsHiding)
        {
            if (!_isHolding)
            {
                _isHolding = true;
                LockTarget(_closestTarget);
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
                CompleteHold(); 
            }
        }
        // Space 키를 놓는 순간
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            // 키를 놓았으므로 홀드 중단 및 완료 플래그 초기화
            EndHold();
            _wasHoldingCompleted = false; 
        }
    }

    private void CompleteHold()
    {
        EndHold();
        _wasHoldingCompleted = true; // 다음 상호작용 방지
    }

    private void EndHold()
    {
        _isHolding = false;
        _holdTimer = 0f;
        if (!IsHiding)
            _playerMovement.EnableMove(true);

        OnHoldEnd?.Invoke();
        
        if (!_isInteracting)
        {
            UnlockTarget();
        }
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
            case InteractHoldType.Cabinet: return 0.5f;
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

        if (_hidingDoor == null)
        {
            _isInteracting = false;
        }
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
            if (dropItemObject != null)
            {
                GameObject target = dropItemObject;

                // 좀비의 자식 콜라이더가 전달된 경우 부모 좀비 오브젝트를 제거
                if (target.GetComponent<Zombie>() == null && target.transform.parent != null)
                {
                    Zombie parentZombie = target.transform.parent.GetComponent<Zombie>();
                    if (parentZombie != null)
                    {
                        target = target.transform.parent.gameObject;
                    }
                }

                Destroy(target);
            }

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
        float duration = 0.1f;
    
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
        float duration = 0.1f;
    
        // ⬇️ 페이드 인 (Alpha를 1.0f로)
        yield return StartCoroutine(FadeRoutine(1.0f, duration)); 
    
        // 나가기 완료 후, 모든 상태를 해제합니다.
        _hidingDoor = null;
        _collider.enabled = true;
        
        // ★★★ 추가: 나가기 완료 시 강제로 타겟 락과 홀드 상태 해제 ★★★
        UnlockTarget();
        _isHolding = false;

        _isInteracting = false; // 입력 잠금 해제
        if (!Input.GetKey(KeyCode.Space))
        {
            _wasHoldingCompleted = false;
        }
        _playerMovement.EnableMove(true); // 이동 잠금 해제
        
        // ★★★ 추가: 상태 변경에 따른 UI 텍스트 갱신 (나가기 -> 숨기) ★★★
        OnTargetChanged?.Invoke(_closestTarget);
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
    
    public void ForceCancelInteraction()
    {
        if (!IsHiding)
        {
            EndHold();
        }
    }
    
    private bool TryGetLockedTarget(out IInteractable target)
    {
        if (!_isTargetLocked)
        {
            target = null;
            return false;
        }

        if (_lockedTarget == null || !IsTargetStillValid(_lockedTarget))
        {
            UnlockTarget();
            target = null;
            return false;
        }

        target = _lockedTarget;
        return true;
    }

    private bool IsTargetStillValid(IInteractable target)
    {
        if (target == null) return false;

        if (target is MonoBehaviour mono)
        {
            return mono != null && mono.isActiveAndEnabled && mono.gameObject.activeInHierarchy;
        }

        return true;
    }

    private void LockTarget(IInteractable target)
    {
        if (target == null) return;

        _lockedTarget = target;
        _isTargetLocked = true;
    }

    private void UnlockTarget()
    {
        _lockedTarget = null;
        _isTargetLocked = false;
    }
    
    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }*/
}