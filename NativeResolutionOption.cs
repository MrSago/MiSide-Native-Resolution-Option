using HarmonyLib;
using Il2CppInterop.Runtime;
using Mod.Properties;
using UnityEngine;
#if ML
using Il2Cpp;
#elif BIE
using BepInEx.IL2CPP;
#endif

namespace Mod;

public static class NativeResolutionOption
{
    private static HarmonyLib.Harmony _harmony = null!;

    public static void Init()
    {
        _harmony = new(BuildInfo.PACKAGE);
        _harmony.PatchAll(typeof(Patch));

        ModCore.Log("Initialized");
    }

    [HarmonyPatch]
    private static class Patch
    {
        [HarmonyPatch(typeof(ButtonMouseClick), "OnPointerDown")]
        private static void Postfix(ButtonMouseClick __instance)
        {
            if (
                __instance.name != "Button Option Graphics"
                && __instance.name != "Button Options Graphics"
            )
            {
                return;
            }

            AddNativeResolutionOption();
        }

        private static void AddNativeResolutionOption()
        {
            var menuCaseOption = Resources
                .FindObjectsOfTypeAll(Il2CppType.Of<MenuCaseOption>())
                ?.FirstOrDefault(x => x.name == "Button Resolution")
                ?.Cast<MenuCaseOption>();
            if (menuCaseOption == null)
            {
                ModCore.LogError("MenuCaseOption not found");
                return;
            }

            Resolution resolution = GetNativeResolution();
            string buttonText =
                $"{resolution.width}x{resolution.height}@{resolution.refreshRate}Hz";

            ModCore.Log($"Native resolution: {buttonText}");

            foreach (var buttonInfo in menuCaseOption.scrIccb)
            {
                if (buttonInfo.buttonText == buttonText)
                {
                    ModCore.Log("Option is already exists");
                    return;
                }
            }

            var newButtonInfo = new Interface_ChangeScreenButton_Class_ButtonInfo()
            {
                buttonText = buttonText,
            };

            int index = menuCaseOption.resolutions.IndexOf(resolution);
            newButtonInfo.value_int = index >= 0 ? index : menuCaseOption.resolutions.Count - 1;

            menuCaseOption.scrIccb?.Add(newButtonInfo);

            ModCore.Log("Option successfully added");
        }

        private static Resolution GetNativeResolution()
        {
            Display primaryDisplay = Display.main;
            int nativeWidth = primaryDisplay.systemWidth;
            int nativeHeight = primaryDisplay.systemHeight;

            int maxRefreshRate = Screen
                .resolutions.Where(r => r.width == nativeWidth && r.height == nativeHeight)
                .Max(r => r.refreshRate);

            return new Resolution
            {
                width = nativeWidth,
                height = nativeHeight,
                refreshRate = maxRefreshRate,
            };
        }
    }
}
