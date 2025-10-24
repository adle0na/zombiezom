using UnityEngine;
using static IInteractable;

public class TestInteratable : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractHoldType holdType = InteractHoldType.Instant;
    
    public InteractHoldType HoldType => holdType;
    public bool IsInteractable { get; private set; } = true;

    public void Interact()
    {
        Debug.Log($"Interact with {gameObject.name}");
        IsInteractable = false;
    }

    public string GetInteractPrompt()
    {
        return "Open";
    }
}
