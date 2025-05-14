using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hexa.NET.ImGui.MelonLoader
{
    [HarmonyPatch(typeof(Input), nameof(Input.mousePosition), MethodType.Getter)]
    internal class PatchMousePosition
    {
        private static bool Prefix(ref Vector3 __result)
        {
            if (Core.BlockInput)
            {
                __result = default;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButton), MethodType.Normal)]
    internal class PatchGetMouseButton
    {
        private static bool Prefix(int button, ref bool __result)
        {
            if (Core.BlockInput)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonDown), MethodType.Normal)]
    internal class PatchGetMouseButtonDown
    {
        private static bool Prefix(int button, ref bool __result)
        {
            if (Core.BlockInput)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonUp), MethodType.Normal)]
    internal class PatchGetMouseButtonUp
    {
        private static bool Prefix(int button, ref bool __result)
        {
            if (Core.BlockInput)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.mouseScrollDelta), MethodType.Getter)]
    internal class PatchMouseScrollDelta
    {
        private static bool Prefix(ref Vector2 __result)
        {
            if (Core.BlockInput)
            {
                __result = Vector2.zero;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKey), [typeof(KeyCode)])]
    internal class PatchGetKey
    {
        private static bool Prefix(KeyCode key, ref bool __result)
        {
            if (Core.BlockInput)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), [typeof(KeyCode)])]
    internal class PatchGetKeyDown
    {
        private static bool Prefix(KeyCode key, ref bool __result)
        {
            if (Core.BlockInput)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUp), [typeof(KeyCode)])]
    internal class PatchGetKeyUp
    {
        private static bool Prefix(KeyCode key, ref bool __result)
        {
            if (Core.BlockInput)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKey), [typeof(string)])]
    internal class PatchGetKeyString
    {
        private static bool Prefix(string name, ref bool __result)
        {
            if (Core.BlockInput)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), [typeof(string)])]
    internal class PatchGetKeyDownString
    {
        private static bool Prefix(string name, ref bool __result)
        {
            if (Core.BlockInput)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUp), [typeof(string)])]
    internal class PatchGetKeyUpString
    {
        private static bool Prefix(string name, ref bool __result)
        {
            if (Core.BlockInput)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    /* Causes lag.
    [HarmonyPatch(typeof(Input), nameof(Input.inputString), MethodType.Getter)]
    internal class PatchInputString
    {
        private static bool Prefix(ref string __result)
        {
            if (Core.BlockInput)
            {
                __result = string.Empty;
                return false;
            }

            return true;
        }
    }
    */

    [HarmonyPatch(typeof(Input), nameof(Input.GetAxis))]
    internal class PatchGetAxis
    {
        private static bool Prefix(string axisName, ref float __result)
        {
            if (Core.BlockInput)
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetAxisRaw))]
    internal class PatchGetAxisRaw
    {
        private static bool Prefix(string axisName, ref float __result)
        {
            if (Core.BlockInput)
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }

    public static class InputManagerBlocker
    {
        private static Il2CppArrayBase<InputActionAsset> inputActionAssets;
        private static bool blocking;

        static InputManagerBlocker()
        {
        }

        public static void ReloadInputActions()
        {
            inputActionAssets = UnityEngine.Object.FindObjectsOfType<InputActionAsset>();
            foreach (var map in inputActionAssets)
            {
                Console.WriteLine($"Found input action map: {map.name}");
            }
        }

        internal static void SwitchInput(bool state)
        {
            if (state)
            {
                BlockInput();
            }
            else
            {
                UnblockInput();
            }
        }

        internal static void BlockInput()
        {
            if (blocking) return;
            foreach (var map in inputActionAssets)
            {
                try
                {
                    map.Disable();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            blocking = true;
        }

        internal static void UnblockInput()
        {
            if (!blocking) return;
            foreach (var map in inputActionAssets)
            {
                try
                {
                    map.Enable();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            blocking = false;
        }
    }
}