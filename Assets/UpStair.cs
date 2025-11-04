using System.Collections;
using UnityEngine;

public class UpStair : MonoBehaviour, IInteractable
{
    [SerializeField] private ApartmentFloor owner;
    [SerializeField] private GameObject upArrow;

    private Coroutine moveCor;
    public IInteractable.InteractHoldType HoldType { get; } = IInteractable.InteractHoldType.Instant;
    public bool IsInteractable { get; } = true;
    public Material InteractableMaterial => null;

    public bool isGoing;
    public void Interact()
    {
        // 최대층 5층 설정
        if (PlayerDataManager.Instance.playerFloor >= 5) return;
        
        if (moveCor != null)
        {
            StopCoroutine(moveCor);
        }

        if(!isGoing)
            StartCoroutine(UpMoveCor());
    }

    public string GetInteractPrompt()
    {
        if (owner.floorNum >= 5)
        {
            upArrow.SetActive(false);
            return null;
        }
        else
        {
            upArrow.SetActive(true);
        }

        return $"{owner.floorNum + 1}층 올라가기";
    }

    IEnumerator UpMoveCor()
    {
        isGoing = true;
        
        Transform playerTransform = PlayerDataManager.Instance.playerObj.transform;
        
        SoundManager.Instance.PlaySFX(4);
        
        UIManager.Instance.OpenFadeInUI();

        PlayerDataManager.Instance.playerObj.gameObject.SetActive(false);

        PlayerDataManager.Instance.playerFloor++;
        
        yield return new WaitForSeconds(1f);
        
        // Y값을 +8 만큼 올리기
        Vector3 newPos = playerTransform.position;
        newPos.y += 8f;
        playerTransform.position = newPos;
        
        upArrow.SetActive(false);
        
        PlayerDataManager.Instance.playerObj.gameObject.SetActive(true);

        PlayerDataManager.Instance.MoveStair();
        
        isGoing = false;
    }
}
