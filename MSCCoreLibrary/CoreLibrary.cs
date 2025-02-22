global using UnityEngine;
namespace MSCCoreLibrary;
internal static class CoreLibrary
{    
    internal static GameObject coreLibraryHelper;
    internal static void SetupCoreLibrary()
    {
        if (coreLibraryHelper != null) return;
        coreLibraryHelper = new GameObject("MSCCoreLibrary Helper", typeof(CoreLibraryHelper));
        GameObject.DontDestroyOnLoad(coreLibraryHelper);
    }
}
