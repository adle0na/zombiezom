using UnityEngine;

public class InfoUI : MonoBehaviour
{
    public void CloseInfoPopup()
    {
        UIManager.Instance.AllPopupClear();
    }
}
