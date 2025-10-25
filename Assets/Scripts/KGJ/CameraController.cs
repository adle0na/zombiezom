using Unity.Cinemachine;
using UnityEngine;
using System.Collections; // 👈 코루틴 사용을 위해 추가

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource _shake;
    private CinemachineCamera _camera;
    
    private PlayerMovement _player;

    private void Awake()
    {
        _camera = GetComponentInChildren<CinemachineCamera>();
    }

    private void Start()
    {
        StartCoroutine(WaitForPlayerAndSetTarget());
    }

    private IEnumerator WaitForPlayerAndSetTarget()
    {
        // 1. 플레이어가 생성될 때까지 기다림
        while (_player == null)
        {
            _player = FindAnyObjectByType<PlayerMovement>();
            yield return null; 
        }

        _camera.Target.TrackingTarget = _player.transform;
    }

    public void Shake()
    {
        _shake.GenerateImpulse();
    }
}