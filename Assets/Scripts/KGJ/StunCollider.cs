using System;
using UnityEngine;

public class StunCollider : MonoBehaviour, IInteractable
{
    public static event Action<ItemCsvRow, GameObject> OnItemPickupRequested;
        
    private Zombie _zombie;
    public IInteractable.InteractHoldType HoldType => IInteractable.InteractHoldType.Instant;
    public bool IsInteractable => (_zombie.state == ZombieState.Stunned) && (_zombie.ZombieData.zombieType != ZombieType.DisCureZombie);

    public Transform HintAnchorTransform => transform.parent;
    public Vector3 HintWorldOffset => _hintOffset;

    public Material InteractableMaterial => getInteractableMaterial();

    private Material getInteractableMaterial()
    {
        if (this == null) return null;
        return GetComponentInParent<SpriteRenderer>().material;
    }
    
    [SerializeField] private Vector3 _hintOffset = new Vector3(0, 2.5f, 0);

    private void Start()
    {
        _zombie = GetComponentInParent<Zombie>();
    }
    
    public void Interact()
    {
        if (PlayerDataManager.Instance.IsZombieInHome || PlayerInventory.Instance.HaveZombie)
        {
            UI_Popup.OnShowPopupRequested?.Invoke("더 데려올 수 없어..");
            return;
        }
        
        int index = 21;
        if (_zombie.ZombieData.zombieType == ZombieType.SuaZombie)
        {
            index += 5;
        }
        else
        {
            int floor = PlayerDataManager.Instance.PlayerFloor;
            index += floor;
        }

        ItemCsvRow zombieItem = ItemDataManager.Instance.GetItemByIndex(index);
        if (gameObject != null)
            OnItemPickupRequested?.Invoke(zombieItem, gameObject);
        Destroy(transform.parent.gameObject);
    }

    public string GetInteractPrompt()
    {
        if (PlayerDataManager.Instance.IsZombieInHome || PlayerInventory.Instance.HaveZombie)
        {
            return "";
        }
        return "납치하기";
    }

}
