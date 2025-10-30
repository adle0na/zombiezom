using UnityEngine;

public interface IInteractable
{
    public InteractHoldType HoldType { get; }
    public bool IsInteractable { get; }
    public Transform HintAnchorTransform => (this as MonoBehaviour)?.transform;
    public Vector3 HintWorldOffset => new Vector3(0f, 1.5f, 0f);
    
    /// <summary>
    /// 플레이어가 상호작용 키를 눌렀을 때 실행되는 함수
    /// </summary>
    public void Interact();

    /// <summary>
    /// UI 표시용 이름이나 설명을 반환할 때 사용 (선택사항)
    /// </summary>
    public string GetInteractPrompt();
    
    public enum InteractHoldType
    {
        Instant,   // 즉시 실행
        Cabinet,   // 0.5초
        Short,     // 1초
        Long       // 2초
    }
}