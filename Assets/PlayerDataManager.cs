using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : GenericSingleton<PlayerDataManager>
{
    [SerializeField] private List<ItemData> playerInven;
}
