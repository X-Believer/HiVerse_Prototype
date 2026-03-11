using System;
using StarterAssets;
using UnityEngine;
using System.Collections;
using Cinemachine;

public enum CameraMode
{
    PlayerFollow,
    NPC,
    TopDown
}

public class CameraManager : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineVirtualCamera playerFollowCamera;
    public CinemachineVirtualCamera npcCamera;
    public CinemachineVirtualCamera topDownCamera;

    [Header("Player")]
    public GameObject playerCharacter;
    public GameObject playerCameraRoot;
    private ThirdPersonController _playerController;

    private Transform _npcTarget;
    private Transform _npcCameraRoot;

    [Header("TopDown Settings")]
    public float topDownHeight = 10f;
    public float topDownSpeed = 20f;
    public float scrollSpeed = 20f;
    public float minHeight = 50f;
    public float maxHeight = 120f;
    public float transitionDuration = 0.25f;
    [Header("TopDown World Bounds")]
    public Vector3 minWorldBounds = new Vector3(-60f, 0f, -50f);
    public Vector3 maxWorldBounds = new Vector3(0f, 0f, 100f);

    private CinemachineVirtualCamera activeCamera;
    private CinemachineVirtualCamera lastActiveCamera;
    private Transform topDownPivot;
    private bool isTopDownTransitioning = false;
    
    public CameraMode cameraMode { get; private set; }
    
    public static event Action<CameraMode> OnCameraModeChanged;
    
    public static CameraManager Instance;

    private void Awake()
    {
        Instance = this;
        _playerController = playerCharacter.GetComponent<ThirdPersonController>();
    }

    private void Start()
    {
        // 默认玩家视角
        activeCamera = playerFollowCamera;
        playerFollowCamera.Follow = playerCameraRoot.transform;
        SetActiveCamera(activeCamera);
        cameraMode = CameraMode.PlayerFollow;

        // 创建 TopDownPivot 用于自由移动
        topDownPivot = new GameObject("TopDownPivot").transform;
        topDownPivot.position = playerFollowCamera.transform.position;
    }

    private void Update()
    {
        // 未激活摄像机跟随当前视角
        if (activeCamera != npcCamera) npcCamera.transform.position = activeCamera.transform.position;
        if (activeCamera != topDownCamera && !isTopDownTransitioning)
        {
            topDownCamera.transform.position = activeCamera.transform.position;
            topDownCamera.transform.rotation = activeCamera.transform.rotation;
        }
        if (activeCamera != npcCamera) npcCamera.transform.rotation = activeCamera.transform.rotation;

        // TopDown自由控制
        if (activeCamera == topDownCamera && !isTopDownTransitioning)
            TopDownMove();
    }

    private void TopDownMove()
    {
        // WASD 移动
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        topDownPivot.position += input * topDownSpeed * Time.deltaTime;

        // 使用世界坐标限制
        topDownPivot.position = new Vector3(
            Mathf.Clamp(topDownPivot.position.x, minWorldBounds.x, maxWorldBounds.x),
            topDownPivot.position.y, // Y 由高度控制，不限制
            Mathf.Clamp(topDownPivot.position.z, minWorldBounds.z, maxWorldBounds.z)
        );

        // 滚轮控制高度
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        topDownHeight -= scroll * scrollSpeed;

        if (Input.GetKey(KeyCode.E)) topDownHeight += scrollSpeed * Time.deltaTime * 10f;
        if (Input.GetKey(KeyCode.Q)) topDownHeight -= scrollSpeed * Time.deltaTime * 10f;

        topDownHeight = Mathf.Clamp(topDownHeight, minHeight, maxHeight);

        topDownCamera.transform.position = topDownPivot.position + Vector3.up * topDownHeight;
        topDownCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    // 一键切换逻辑
    public void ToggleCamera()
    {
        if (activeCamera == playerFollowCamera)
        {
            topDownPivot.position = activeCamera.transform.position;
            SwitchToTopDown();
        }
        else if (activeCamera == topDownCamera)
        {
            if (lastActiveCamera == npcCamera)
            {
                SwitchToNPCCamera(_npcTarget);
            }
            else
            {
                SwitchToPlayerCamera();
            }
        }
        else if (activeCamera == npcCamera)
        {
            topDownPivot.position = activeCamera.transform.position;
            SwitchToTopDown();
        }
    }

    private void SetActiveCamera(CinemachineVirtualCamera cam)
    {
        lastActiveCamera = activeCamera;
        activeCamera = cam;

        playerFollowCamera.Priority = (cam == playerFollowCamera) ? 20 : 10;
        npcCamera.Priority = (cam == npcCamera) ? 20 : 10;
        topDownCamera.Priority = (cam == topDownCamera) ? 20 : 10;

        if (_playerController != null)
            _playerController.enabled = (cam == playerFollowCamera);

        CameraMode newMode = cameraMode;

        if (cam == playerFollowCamera) newMode = CameraMode.PlayerFollow;
        else if (cam == npcCamera) newMode = CameraMode.NPC;
        else if (cam == topDownCamera) newMode = CameraMode.TopDown;

        if (newMode != cameraMode)
        {
            cameraMode = newMode;
            OnCameraModeChanged?.Invoke(cameraMode);
        }

    }

    public void SwitchToPlayerCamera()
    {
        if (activeCamera == playerFollowCamera) return;
        
        Vector3 targetPos = playerFollowCamera.transform.position;
        Quaternion targetRot = playerFollowCamera.transform.rotation;

        StartCoroutine(SmoothCameraSwitch(activeCamera, playerFollowCamera, targetPos, targetRot));
    }

    public void SwitchToNPCCamera(Transform npcTarget)
    {
        _npcTarget = npcTarget;
        _npcCameraRoot = npcTarget.transform.Find("NPCCameraRoot");
        
        npcCamera.Follow = _npcCameraRoot;
        npcCamera.LookAt = _npcCameraRoot;

        Vector3 targetPos = npcCamera.transform.position;
        Quaternion targetRot = npcCamera.transform.rotation;

        StartCoroutine(SmoothCameraSwitch(activeCamera, npcCamera, targetPos, targetRot));
    }

    public void SwitchToTopDown()
    {
        Vector3 targetPos = topDownPivot.position + Vector3.up * topDownHeight;
        Quaternion targetRot = Quaternion.Euler(90f, 0f, 0f);

        StartCoroutine(SmoothCameraSwitch(activeCamera, topDownCamera, targetPos, targetRot));
    }

    /// <summary>
    /// 通用平滑切换：从 fromCamera 平滑移动到 toCamera 的位置/旋转，同时切换优先级
    /// </summary>
    private IEnumerator SmoothCameraSwitch(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera, Vector3 targetPos, Quaternion targetRot)
    {
        _playerController.enabled = false;

        if (activeCamera == playerFollowCamera)
        {
            activeCamera.Follow = null;
            activeCamera.LookAt = playerCameraRoot.transform;
        }
        else if (activeCamera == npcCamera)
        {
            activeCamera.Follow = null;
            activeCamera.LookAt = _npcCameraRoot;
        }
        
        // 统一切换摄像机状态
        SetActiveCamera(toCamera);

        Vector3 startPos = fromCamera.transform.position;
        Quaternion startRot = fromCamera.transform.rotation;

        bool topDownFlag = (toCamera == topDownCamera);
        if (topDownFlag)
            isTopDownTransitioning = true;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

            toCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            toCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        toCamera.transform.position = targetPos;
        toCamera.transform.rotation = targetRot;

        if (topDownFlag)
            isTopDownTransitioning = false;

        if (activeCamera == playerFollowCamera)
        {
            activeCamera.Follow = playerCameraRoot.transform;
            activeCamera.LookAt = null;
        }
        else if (activeCamera == npcCamera)
        {
            activeCamera.Follow = _npcCameraRoot;
            activeCamera.LookAt = null;
        }
    }
}