using UnityEngine;
using UnityEngine.XR;

public static class DesktopInputBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateDesktopControls()
    {
        if (Object.FindObjectOfType<DesktopMouseKeyboardController>() != null)
            return;

#if !UNITY_EDITOR
        if (XRSettings.isDeviceActive)
            return;
#endif

        GameObject controllerObject = new GameObject("Desktop Mouse Keyboard Controls");
        Object.DontDestroyOnLoad(controllerObject);
        controllerObject.AddComponent<DesktopMouseKeyboardController>();
    }
}
