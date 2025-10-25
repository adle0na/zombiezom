using System;
using UnityEngine;

public class StunCollider : MonoBehaviour, IInteractable
{
    public static event Action<ItemCsvRow, GameObject> OnItemPickupRequested;
        
    private Zombie _zombie;
    public IInteractable.InteractHoldType HoldType => IInteractable.InteractHoldType.Instant;
    public bool IsInteractable => (!PlayerDataManager.Instance.IsZombieInHome) && (!PlayerInventory.Instance.HaveZombie) && (_zombie.state == ZombieState.Stunned) && (_zombie.ZombieData.zombieType != ZombieType.DisCureZombie);

    private void Start()
    {
        _zombie = GetComponentInParent<Zombie>();
    }
    
    public void Interact()
    {
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
        return "납치하기";
    }

}
