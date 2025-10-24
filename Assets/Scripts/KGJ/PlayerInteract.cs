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
        _playerInventory = PlayerInventory.Instance;
    }

    void Update()
    {
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
        
        float requiredHoldTime = GetHoldDuration(_closestTarget.HoldType);

        // 즉시형 상호작용은 키를 누른 순간 1회만 실행
        if (requiredHoldTime <= 0f)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(InstantInteractRoutine(_closestTarget));
            }
            return;
        }

        // 홀드형 상호작용
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
        if (!_isHolding) return;

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
        _playerMovement.EnableMove(false); // 잠깐 멈추고
        yield return new WaitForSeconds(0.1f); // 한 프레임~0.1초 정도만 대기
        target.Interact(); // 즉시 실행
        _playerMovement.EnableMove(true); // 다시 이동 가능
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
    private void HandleItemPickup(ItemData item, GameObject dropItemObject)
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
