using System.Collections;
using UnityEngine;

public class DownStair : MonoBehaviour, IInteractable
{
    [SerializeField] private ApartmentFloor owner;
    [SerializeField] private GameObject downArrow;
    public IInteractable.InteractHoldType HoldType { get; } = IInteractable.InteractHoldType.Instant;
    public bool IsInteractable { get; } = true;
    public Material InteractableMaterial => null;
    private Coroutine moveCor;
    
    public bool isGoing;
    public void Interact()
    {
        // 1층에서 하강 불가능
        if (PlayerDataManager.Instance.playerFloor <= 1) return;
        
        if (moveCor != null)
        {
            StopCoroutine(moveCor);
        }

        if(!isGoing)
            StartCoroutine(DownMoveCor());
    }

    public string GetInteractPrompt()
    {
        if (owner.floorNum - 1 <= 0)
        {
            downArrow.SetActive(false);
            return null;
        }
        else
        {
            downArrow.SetActive(true);
        }

        return $"{owner.floorNum - 1}층 내려가기";
    }
    
    IEnumerator DownMoveCor()
    {
        isGoing = true;
        
        Transform playerTransform = PlayerDataManager.Instance.playerObj.transform;
        
        SoundManager.Instance.PlaySFX(4);
        
        UIManager.Instance.OpenFadeInUI();
        
        PlayerDataManager.Instance.playerObj.gameObject.SetActive(false);

        PlayerDataManager.Instance.playerFloor--;
        
        yield return new WaitForSeconds(1f);
        
        // Y값을 +8 만큼 내리기
        Vector3 newPos = playerTransform.position;
        newPos.y -= 8f;
        playerTransform.position = newPos;
            
        downArrow.SetActive(false);
        
        PlayerDataManager.Instance.playerObj.gameObject.SetActive(true);

        isGoing = false;
    }
}
