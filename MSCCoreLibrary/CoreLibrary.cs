global using UnityEngine;
namespace MSCCoreLibrary;
internal static class CoreLibrary
{    
    internal static GameObject coreLibraryHelper;
    private static bool init = false;
    internal static void SetupCoreLibrary()
    {
        if (init) return;
        coreLibraryHelper = new GameObject("MSCCoreLibrary Helper", typeof(CoreLibraryHelper));
        GameObject.DontDestroyOnLoad(coreLibraryHelper);
        init = true;
    }
}
