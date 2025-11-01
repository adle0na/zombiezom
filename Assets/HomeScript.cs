using UnityEngine;

public class HomeScript : MonoBehaviour, IInteractable
{
    public IInteractable.InteractHoldType HoldType { get; } = IInteractable.InteractHoldType.Instant;
    public bool IsInteractable { get; } = true;
    public Material InteractableMaterial => null;
    
    public void Interact()
    {
        UIManager.Instance.sceneController.GetComponent<ApartSceneController>().IntoHome();
    }

    public string GetInteractPrompt()
    {
        //throw new System.NotImplementedException();
        return "집으로";
    }
}
