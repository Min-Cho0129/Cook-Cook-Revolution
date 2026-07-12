using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DesktopMouseKeyboardController : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 desktopStartPosition = new Vector3(1.2f, 1.45f, -3f);
    public Vector3 desktopLookTarget = new Vector3(1.2f, 0.85f, -0.4f);
    public float moveSpeed = 2.5f;
    public float sprintSpeed = 5f;
    public float verticalSpeed = 2f;
    public float mouseSensitivity = 0.12f;

    [Header("Interaction")]
    public float interactDistance = 4f;
    public float minHoldDistance = 0.35f;
    public float maxHoldDistance = 3f;
    public float grabRadius = 0.18f;
    public float heldObjectFollowSpeed = 18f;
    public float throwVelocityScale = 1f;
    public float stackDropSearchRadius = 0.3f;

    Camera desktopCamera;
    Transform cameraTransform;
    Rigidbody heldRigidbody;
    Quaternion heldLocalRotation;
    bool heldWasKinematic;
    bool heldUsedGravity;
    float holdDistance = 1f;
    Vector3 lastHeldPosition;
    Vector3 heldVelocity;
    float yaw;
    float pitch;
    bool wasMenuOpen;
    float inputBlockedUntil;
    bool sceneNeedsDesktopSetup = true;
    readonly List<RaycastResult> uiRaycastResults = new();
    readonly Dictionary<Collider, Rigidbody> grabbableColliderMap = new();

    void Awake()
    {
        EnsureCamera();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneNeedsDesktopSetup = true;
        heldRigidbody = null;
        if (cameraTransform != null)
            ResetDesktopCameraPose();
    }

    void Update()
    {
        EnsureCamera();
        if (cameraTransform == null)
            return;

        if (sceneNeedsDesktopSetup)
            PrepareSceneForDesktop(desktopCamera);

        bool menuOpen = IsAnyGameMenuOpen();
        UpdateCursor(menuOpen);

        if (menuOpen)
        {
            wasMenuOpen = true;
            if (heldRigidbody != null)
                ReleaseHeldObject(false);
            HandleMenuPointerInput();
            return;
        }

        if (wasMenuOpen)
        {
            wasMenuOpen = false;
            inputBlockedUntil = Time.unscaledTime + 0.15f;
        }

        HandleLook();
        HandleMovement();
        HandleHeldObject();

        if (Time.unscaledTime >= inputBlockedUntil)
            HandleInteractionInput();
    }

    void EnsureCamera()
    {
        if (cameraTransform != null)
            return;

        desktopCamera = CreateDesktopCamera();
        cameraTransform = desktopCamera.transform;
        ResetDesktopCameraPose();
        PrepareSceneForDesktop(desktopCamera);
    }

    Camera CreateDesktopCamera()
    {
        Camera sourceCamera = Camera.main;
        GameObject cameraObject = new GameObject("Desktop Gameplay Camera");
        cameraObject.transform.SetParent(transform);

        Camera camera = cameraObject.AddComponent<Camera>();
        if (sourceCamera != null)
        {
            camera.clearFlags = sourceCamera.clearFlags;
            camera.backgroundColor = sourceCamera.backgroundColor;
            camera.cullingMask = sourceCamera.cullingMask;
            camera.nearClipPlane = sourceCamera.nearClipPlane;
            camera.farClipPlane = sourceCamera.farClipPlane;
            camera.fieldOfView = sourceCamera.fieldOfView;
            camera.allowHDR = sourceCamera.allowHDR;
            camera.allowMSAA = sourceCamera.allowMSAA;
        }

        camera.tag = "MainCamera";
        cameraObject.AddComponent<AudioListener>();
        return camera;
    }

    void PrepareSceneForDesktop(Camera activeCamera)
    {
        if (activeCamera == null)
            return;

        DisableSceneCamerasExcept(activeCamera);
        DisableTrackedPoseDrivers();
        PrepareWorldSpaceCanvases(activeCamera);
        RegisterDesktopGrabbables();
        sceneNeedsDesktopSetup = false;
    }

    void DisableSceneCamerasExcept(Camera activeCamera)
    {
        foreach (Camera camera in FindObjectsOfType<Camera>(true))
        {
            if (camera == activeCamera)
                continue;

            camera.enabled = false;
            if (camera.CompareTag("MainCamera"))
                camera.tag = "Untagged";
        }

        foreach (AudioListener listener in FindObjectsOfType<AudioListener>(true))
        {
            if (listener.gameObject != activeCamera.gameObject)
                listener.enabled = false;
        }

        activeCamera.enabled = true;
        activeCamera.tag = "MainCamera";
    }

    void DisableTrackedPoseDrivers()
    {
        foreach (Behaviour behaviour in FindObjectsOfType<Behaviour>(true))
        {
            string typeName = behaviour.GetType().Name;
            if (typeName.Contains("TrackedPoseDriver"))
                behaviour.enabled = false;
        }
    }

    void PrepareWorldSpaceCanvases(Camera mainCamera)
    {
        foreach (Canvas canvas in FindObjectsOfType<Canvas>(true))
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
                canvas.worldCamera = mainCamera;

            if (canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    void RegisterDesktopGrabbables()
    {
        grabbableColliderMap.Clear();

        foreach (XRGrabInteractable grab in FindObjectsOfType<XRGrabInteractable>(true))
        {
            Rigidbody body = grab.GetComponent<Rigidbody>();
            if (body == null)
                body = grab.GetComponentInParent<Rigidbody>();
            if (body == null)
                continue;

            Transform registrationRoot = grab.transform.root;
            foreach (Collider collider in registrationRoot.GetComponentsInChildren<Collider>(true))
            {
                if (collider == null)
                    continue;

                if (!grabbableColliderMap.ContainsKey(collider))
                    grabbableColliderMap.Add(collider, body);
            }
        }
    }

    void ResetDesktopCameraPose()
    {
        cameraTransform.position = desktopStartPosition;

        Vector3 lookDirection = desktopLookTarget - desktopStartPosition;
        if (lookDirection.sqrMagnitude < 0.0001f)
            lookDirection = Vector3.forward;

        cameraTransform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        CaptureCameraAngles();
    }

    void CaptureCameraAngles()
    {
        if (cameraTransform == null)
            return;

        Vector3 angles = cameraTransform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x > 180f ? angles.x - 360f : angles.x;
    }

    void UpdateCursor(bool menuOpen)
    {
        Cursor.lockState = menuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = menuOpen;
    }

    void HandleMenuPointerInput()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return;

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = mouse.position.ReadValue(),
            button = PointerEventData.InputButton.Left,
            clickCount = 1
        };

        uiRaycastResults.Clear();
        eventSystem.RaycastAll(pointerData, uiRaycastResults);
        if (uiRaycastResults.Count == 0)
            return;

        GameObject hitObject = uiRaycastResults[0].gameObject;
        GameObject handler = ExecuteEvents.ExecuteHierarchy(hitObject, pointerData, ExecuteEvents.pointerClickHandler);
        if (handler != null)
            return;

        Button button = hitObject.GetComponentInParent<Button>();
        if (button != null && button.IsInteractable())
            button.onClick.Invoke();
    }

    void HandleLook()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        Vector2 delta = mouse.delta.ReadValue();
        yaw += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 direction = Vector3.zero;

        if (keyboard.wKey.isPressed) direction += forward;
        if (keyboard.sKey.isPressed) direction -= forward;
        if (keyboard.dKey.isPressed) direction += right;
        if (keyboard.aKey.isPressed) direction -= right;
        if (keyboard.spaceKey.isPressed) direction += Vector3.up;
        if (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed) direction -= Vector3.up;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        float speed = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed
            ? sprintSpeed
            : moveSpeed;

        if (Mathf.Abs(direction.y) > 0.01f)
            speed = Mathf.Max(speed, verticalSpeed);

        cameraTransform.position += direction.normalized * speed * Time.unscaledDeltaTime;
    }

    void HandleInteractionInput()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;
        bool leftPressed = mouse != null && mouse.leftButton.wasPressedThisFrame;
        bool interactPressed = keyboard != null && keyboard.eKey.wasPressedThisFrame;
        bool dropPressed = keyboard != null && keyboard.gKey.wasPressedThisFrame;

        if (heldRigidbody != null && (leftPressed || dropPressed))
        {
            ReleaseHeldObject(true);
            return;
        }

        if (!leftPressed && !interactPressed)
            return;

        if (EventSystem.current != null && Cursor.lockState != CursorLockMode.Locked && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray interactionRay = GetInteractionRay();
        if (interactPressed && TryFindDesktopInteractable(interactionRay, out IDesktopInteractable desktopInteractable))
        {
            desktopInteractable.DesktopInteract();
            return;
        }

        if (leftPressed && TryFindGrabbable(interactionRay, out Rigidbody body, out float distance))
        {
            Grab(body, distance);
            return;
        }

        if (leftPressed && TryFindDesktopInteractable(interactionRay, out desktopInteractable))
            desktopInteractable.DesktopInteract();
    }

    Ray GetInteractionRay()
    {
        Camera camera = cameraTransform.GetComponent<Camera>();
        if (camera != null)
            return camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        return new Ray(cameraTransform.position, cameraTransform.forward);
    }

    IDesktopInteractable FindDesktopInteractable(Collider collider)
    {
        foreach (MonoBehaviour behaviour in collider.GetComponentsInParent<MonoBehaviour>(true))
        {
            if (behaviour is IDesktopInteractable desktopInteractable)
                return desktopInteractable;
        }

        return null;
    }

    bool TryFindDesktopInteractable(Ray ray, out IDesktopInteractable desktopInteractable)
    {
        foreach (RaycastHit hit in SortedRaycastHits(ray, 0f))
        {
            desktopInteractable = FindDesktopInteractable(hit.collider);
            if (desktopInteractable != null)
                return true;
        }

        desktopInteractable = null;
        return false;
    }

    bool TryFindGrabbable(Ray ray, out Rigidbody body, out float distance)
    {
        foreach (RaycastHit hit in SortedRaycastHits(ray, grabRadius))
        {
            body = FindGrabbableRigidbody(hit.collider);
            if (body == null)
                continue;

            if (IsBlockedFromDesktopGrab(body.gameObject))
                continue;

            distance = hit.distance;
            return true;
        }

        body = null;
        distance = 0f;
        return false;
    }

    RaycastHit[] SortedRaycastHits(Ray ray, float radius)
    {
        RaycastHit[] hits = radius > 0f
            ? Physics.SphereCastAll(ray, radius, interactDistance, ~0, QueryTriggerInteraction.Collide)
            : Physics.RaycastAll(ray, interactDistance, ~0, QueryTriggerInteraction.Collide);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        return hits;
    }

    Rigidbody FindGrabbableRigidbody(Collider collider)
    {
        if (collider == null)
            return null;

        if (grabbableColliderMap.TryGetValue(collider, out Rigidbody mappedBody) && mappedBody != null)
            return mappedBody;

        Rigidbody body = collider.attachedRigidbody;
        if (body != null && IsDesktopGrabbable(body.gameObject))
            return body;

        XRGrabInteractable grab = collider.GetComponentInParent<XRGrabInteractable>();
        if (grab != null && grab.enabled)
        {
            body = grab.GetComponent<Rigidbody>();
            if (body == null)
                body = grab.GetComponentInParent<Rigidbody>();
            if (body != null)
                return body;
        }

        body = collider.GetComponentInParent<Rigidbody>();
        if (body != null && IsDesktopGrabbable(body.gameObject))
            return body;

        return null;
    }

    bool IsDesktopGrabbable(GameObject target)
    {
        if (target == null)
            return false;

        if (target.GetComponent<XRGrabInteractable>() != null) return true;
        if (target.GetComponentInParent<XRGrabInteractable>() != null) return true;
        if (target.GetComponent<Knife>() != null) return true;
        if (target.GetComponent<FryingPan>() != null) return true;
        if (target.GetComponent<StackableIngredient>() != null) return true;
        if (target.GetComponent<ChoppableIngredient>() != null) return true;
        if (target.GetComponent<CookableIngredient>() != null) return true;
        if (target.GetComponent<Egg>() != null) return true;
        if (target.GetComponent<EggLiquid>() != null) return true;

        return false;
    }

    void Grab(Rigidbody body, float distance)
    {
        heldRigidbody = body;
        heldWasKinematic = body.isKinematic;
        heldUsedGravity = body.useGravity;
        heldLocalRotation = Quaternion.Inverse(cameraTransform.rotation) * body.transform.rotation;
        holdDistance = Mathf.Clamp(distance, minHoldDistance, maxHoldDistance);
        lastHeldPosition = body.position;
        heldVelocity = Vector3.zero;

        body.isKinematic = true;
        body.useGravity = false;
    }

    bool IsBlockedFromDesktopGrab(GameObject target)
    {
        if (IsDesktopGrabbable(target))
            return false;

        Transform current = target.transform;
        while (current != null)
        {
            if (current.CompareTag("Unmovable") || current.CompareTag("Conveyor"))
                return true;

            current = current.parent;
        }

        return false;
    }

    void HandleHeldObject()
    {
        if (heldRigidbody == null)
            return;

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 scroll = mouse.scroll.ReadValue();
            holdDistance = Mathf.Clamp(holdDistance + scroll.y * 0.0015f, minHoldDistance, maxHoldDistance);
        }

        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * holdDistance;
        Quaternion targetRotation = cameraTransform.rotation * heldLocalRotation;
        float follow = 1f - Mathf.Exp(-heldObjectFollowSpeed * Time.unscaledDeltaTime);

        Transform heldTransform = heldRigidbody.transform;
        heldTransform.position = Vector3.Lerp(heldTransform.position, targetPosition, follow);
        heldTransform.rotation = Quaternion.Slerp(heldTransform.rotation, targetRotation, follow);

        float deltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        heldVelocity = (heldRigidbody.position - lastHeldPosition) / deltaTime;
        lastHeldPosition = heldRigidbody.position;
    }

    void ReleaseHeldObject(bool allowStackDrop)
    {
        Rigidbody releasedBody = heldRigidbody;
        if (releasedBody == null)
            return;

        StackableIngredient stackable = releasedBody.GetComponent<StackableIngredient>();
        if (stackable == null) stackable = releasedBody.GetComponentInChildren<StackableIngredient>();
        if (stackable == null) stackable = releasedBody.GetComponentInParent<StackableIngredient>();
        if (allowStackDrop && TryCompleteDesktopStackDrop(stackable))
        {
            ClearHeldObject();
            return;
        }

        releasedBody.isKinematic = heldWasKinematic;
        releasedBody.useGravity = heldUsedGravity;

        if (!releasedBody.isKinematic)
            releasedBody.linearVelocity = heldVelocity * throwVelocityScale;

        ClearHeldObject();
    }

    bool TryCompleteDesktopStackDrop(StackableIngredient stackable)
    {
        if (stackable == null)
            return false;

        Collider[] nearby = Physics.OverlapSphere(
            stackable.transform.position,
            stackDropSearchRadius,
            ~0,
            QueryTriggerInteraction.Collide);

        foreach (Collider collider in nearby)
        {
            StackDropZone zone = collider.GetComponentInParent<StackDropZone>();
            if (zone != null && zone.TryCompleteDesktopDrop(stackable))
                return true;
        }

        return false;
    }

    void ClearHeldObject()
    {
        heldRigidbody = null;
        heldVelocity = Vector3.zero;
    }

    bool IsAnyGameMenuOpen()
    {
        foreach (StartScreen startScreen in FindObjectsOfType<StartScreen>())
        {
            if ((startScreen.mainPanel != null && startScreen.mainPanel.activeInHierarchy) ||
                (startScreen.rulesPanel != null && startScreen.rulesPanel.activeInHierarchy))
                return true;
        }

        foreach (PauseScreen pauseScreen in FindObjectsOfType<PauseScreen>())
        {
            if (pauseScreen.panel != null && pauseScreen.panel.activeInHierarchy)
                return true;
        }

        foreach (GameOverScreen gameOverScreen in FindObjectsOfType<GameOverScreen>())
        {
            if (gameOverScreen.panel != null && gameOverScreen.panel.activeInHierarchy)
                return true;
        }

        return false;
    }

    void OnGUI()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        const float size = 6f;
        float x = Screen.width * 0.5f - size * 0.5f;
        float y = Screen.height * 0.5f - size * 0.5f;

        Color previousColor = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }
}
