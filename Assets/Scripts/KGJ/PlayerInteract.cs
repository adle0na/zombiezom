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
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayerInventory.Instance.AddItem(ItemDataManager.Instance.GetItemByIndex(5));
            PlayerInventory.Instance.AddItem(ItemDataManager.Instance.GetItemByIndex(9));
            PlayerInventory.Instance.AddItem(ItemDataManager.Instance.GetItemByIndex(5));
            PlayerInventory.Instance.AddItem(ItemDataManager.Instance.GetItemByIndex(22));
        }
        // 1. 상호작용 루틴이 실행 중이면 Update 로직을 잠급니다.
        if (_isInteracting) return; 
        
        if (_hidingDoor != null)
        {
            _closestTarget = _hidingDoor;
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
                
                // 루틴 시작 시 코루틴을 실행합니다.
                StartCoroutine(InstantInteractRoutine(_closestTarget));
                
                // ★★★ 추가: 키를 누르는 순간, 바로 _wasHoldingCompleted를 설정하여 
                // 다음 프레임에서 홀드 상호작용이 시작되지 않도록 합니다. 
                // 키를 놓을 때까지 이 상태를 유지합니다. ★★★
                _wasHoldingCompleted = true; 
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
        // 0.3초 간 서서히 플레이어의 투명도가 낮아짐.
        float duration = 0.3f;
    
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
    
    public void ForceCancelInteraction()
    {
        if (!IsHiding)
        {
            EndHold();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}