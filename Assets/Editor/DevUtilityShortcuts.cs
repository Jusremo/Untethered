using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[InitializeOnLoad]
public class DevUtilityShortcuts 
{

#if UNITY_EDITOR

    private static EditorInputActions input;
    private static float shift;
    private static bool _devModeActive;

    static DevUtilityShortcuts() 
    {
        input = new EditorInputActions();
        input.Editor.SwitchFullscreenMode.canceled += SwitchFullscreenModeInput;
        input.Editor.TestButton.performed += TestButtonInput;
        input.Editor.Enable();
    }

    private static void TestButtonInput(InputAction.CallbackContext obj)
    {
        _devModeActive = !_devModeActive;
        Time.timeScale = _devModeActive ? 0.2f : 1;
    }

    private static void SwitchFullscreenModeInput(InputAction.CallbackContext obj)
    {
        Assembly assembly = typeof(EditorWindow).Assembly;
        Type type = assembly.GetType("UnityEditor.GameView");

        if(EditorWindow.focusedWindow.GetType() == type) 
            EditorWindow.focusedWindow.maximized = !EditorWindow.focusedWindow.maximized;
    }

#endif

}