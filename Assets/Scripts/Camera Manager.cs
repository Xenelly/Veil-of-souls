using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [SerializeField] private CinemachineVirtualCamera[] allVirtualCameras;

    [Header("Settings for lerping the Y damping during jumping/falling")]
    [SerializeField] private float fallPanAmount = 0.25f;
    [SerializeField] private float fallPanTime = 0.35f;
    public float fallSpeedYDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine lerpYPanCoroutine;

    private CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer framingTransposer;

    private float normYPanAmount;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        for (int i = 0; i < allVirtualCameras.Length; i++)
        {
            if (allVirtualCameras[i].enabled)
            {
                currentCamera = allVirtualCameras[i];
                framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                break;
            }
        }

        if (framingTransposer != null)
        {
            normYPanAmount = framingTransposer.m_YDamping;
        }
    }

    public void LerpYDamping(bool IsPlayerFalling)
    {
        if (lerpYPanCoroutine != null)
        {
            StopCoroutine(lerpYPanCoroutine);
        }
        lerpYPanCoroutine = StartCoroutine(LerpYAction(IsPlayerFalling));
    }

    private IEnumerator LerpYAction(bool IsPlayerFalling)
    {
        IsLerpingYDamping = true;

        float startDampAmount = framingTransposer.m_YDamping;
        float endDampAmount = IsPlayerFalling ? fallPanAmount : normYPanAmount;

        if (IsPlayerFalling)
        {
            LerpedFromPlayerFalling = true;
        }

        float elapsedTime = 0f;
        while (elapsedTime < fallPanTime)
        {
            elapsedTime += Time.deltaTime;
            framingTransposer.m_YDamping = Mathf.Lerp(startDampAmount, endDampAmount, elapsedTime / fallPanTime);
            yield return null;
        }

        IsLerpingYDamping = false;
    }
}
