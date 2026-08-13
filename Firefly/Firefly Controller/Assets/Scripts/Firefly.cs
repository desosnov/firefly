using UnityEngine;

namespace Firefly
{
    /// <summary>
    /// Port of Firefly.cpp — the entry point.
    ///
    /// The C++ main() constructed a FireflyController with the window title, size,
    /// serial port and stage type, then called start(). Unity owns the window, so
    /// what's left is creating the controller and handing it the stage type and
    /// animation duration range.
    ///
    /// RuntimeInitializeOnLoadMethod means there is nothing to set up in the editor:
    /// open the project and press Play. See Port Notes.
    /// </summary>
    public static class FireflyMain
    {
        public const string WINDOW_TITLE = "Firefly Controller";
        public const PixelStageOption STAGE_TYPE = PixelStageOption.FIREFLY_V2_CYLINDER;
        public const double ANIM_MIN_DURATION = 10.0;
        public const double ANIM_MAX_DURATION = 75.0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Main()
        {
            if (Object.FindFirstObjectByType<FireflyController>() != null) return;

            GameObject host = new GameObject(WINDOW_TITLE);
            Object.DontDestroyOnLoad(host);

            FireflyController ffc = host.AddComponent<FireflyController>();
            ffc.stageType = STAGE_TYPE;
            ffc.animMinDuration = ANIM_MIN_DURATION;
            ffc.animMaxDuration = ANIM_MAX_DURATION;
        }
    }
}
