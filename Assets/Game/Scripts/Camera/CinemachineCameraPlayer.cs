using UnityEngine;
using Unity.Cinemachine;

public class CinemachineCameraPlayer : MonoBehaviour
{
    private CinemachineCamera _cinemachineCamera;

    private void Start()
    {
        _cinemachineCamera = GetComponent<CinemachineCamera>();
    }

    public void SetTarget(Transform newTarget)
    {
        _cinemachineCamera.Target.TrackingTarget = newTarget;
    }
}
