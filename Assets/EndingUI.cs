using UnityEngine;

public class EndingUI : MonoBehaviour
{
    [SerializeField] private GameObject scrollView;
    [SerializeField] private GameObject ending1;
    [SerializeField] private GameObject ending2;

    public void CheckEnding()
    {
        scrollView.SetActive(false);

        bool isFindCat = PlayerDataManager.Instance.isFindCat;

        ending1.SetActive(isFindCat);
        ending2.SetActive(!isFindCat);
    }
}
