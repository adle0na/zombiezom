using UnityEngine;

public class HomePlayer : MonoBehaviour
{
    public void EndPlayerBlood()
    {
        Debug.Log("EndPlayerBlood");
        HomeUIScript home = GetComponentInParent<HomeUIScript>();
        if (home != null)
        {
            home.ClearCaredZombie();
        }
    }
}
