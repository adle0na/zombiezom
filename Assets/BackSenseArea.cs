using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class BackSenseArea : MonoBehaviour, IInteractable
{
    private Zombie owner;
    private string playerTag;

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