using System.Collections;
using UnityEngine;

public class DownStair : MonoBehaviour, IInteractable
{
    [SerializeField] private ApartmentFloor owner;
    [SerializeField] private GameObject downArrow;
    public IInteractable.InteractHoldType HoldType { get; } = IInteractable.InteractHoldType.Instant;
    public bool IsInteractable { get; } = true;
    
    private Coroutine moveCor;
    public void Interact()
    {
        if (moveCor != null)
        {
            StopCoroutine(moveCor);
        }

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

        return $"{owner.floorNum - 1}으로 내려가기";
    }
    
    IEnumerator DownMoveCor()
    {
        Transform playerTransform = PlayerDataManager.Instance.playerObj.transform;
        
        PlayerDataManager.Instance.playerObj.gameObject.SetActive(false);
        
        SoundManager.Instance.PlaySFX(4);
        
        UIManager.Instance.OpenFadeInUI();

        yield return new WaitForSeconds(1f);
        
        // Y값을 +8 만큼 내리기
        Vector3 newPos = playerTransform.position;
        newPos.y -= 8f;
        playerTransform.position = newPos;
            
        downArrow.SetActive(false);
        
        PlayerDataManager.Instance.playerObj.gameObject.SetActive(true);
    }
}
