using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class BackSenseArea : MonoBehaviour, IInteractable
{
    private Zombie owner;
    private string playerTag;

    public Transform HintAnchorTransform => transform.parent;
    [SerializeField] private Vector3 hintOffset = new Vector3(0, 3f, 0);
    public Vector3 HintWorldOffset => hintOffset;

    public void Init(Zombie zombie, string tagToDetect)
    {
        owner = zombie;
        playerTag = tagToDetect;
        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true; // 트리거 필수
    }
    
    public IInteractable.InteractHoldType HoldType => IInteractable.InteractHoldType.Instant;
    public bool IsInteractable { get; } = true;
    public void Interact()
    {
        SoundManager.Instance.PlaySFX(8);
        PlayerDataManager.Instance.playerObj.GetComponent<PlayerMovement>().GetPlayerAnimator().SetTrigger("Dangsu");
        owner.Stun();
    }

    public string GetInteractPrompt()
    {
        return "당수";
    }
}