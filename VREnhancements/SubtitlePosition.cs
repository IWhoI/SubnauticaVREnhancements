using HarmonyLib;

namespace VREnhancements
{
    class SubtitlePosition
    {
        [HarmonyPatch(typeof(Subtitles), nameof(Subtitles.Start))]
        class SubtitlesPosition_Patch
        {//Bring up the subtitles into view while in VR
            public static void Postfix(Subtitles __instance)
            {
                __instance.popup.oy = AdditionalVROptions.subtitleYPos;//higher values means higher on the screen. Consider using screen height percentage instead of pixel value.
            }
        }
    }
}
