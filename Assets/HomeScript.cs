using UnityEngine;

public class HomeScript : MonoBehaviour, IInteractable
{
    public IInteractable.InteractHoldType HoldType { get; } = IInteractable.InteractHoldType.Instant;
    public bool IsInteractable { get; } = PlayerDataManager.Instance.playerFloor == 1;
    public void Interact()
    {
        UIManager.Instance.OpenHomeUI();
        
        PlayerDataManager.Instance.playerObj.SetActive(false);
    }

    public string GetInteractPrompt()
    {
        //throw new System.NotImplementedException();
        return "집으로";
    }
}
