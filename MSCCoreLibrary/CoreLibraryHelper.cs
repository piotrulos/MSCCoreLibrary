using MSCLoader;

namespace MSCCoreLibrary;

class CoreLibraryHelper : MonoBehaviour
{
    private void OnLevelWasLoaded(int level)
    {
        switch (Application.loadedLevelName)
        {
            case "MainMenu":
                GameTime.Reset();
                break;
            case "Intro":
                GameTime.Reset();
                break;
            case "GAME":
                GameSceneLoaded();
                break;
            case "Ending":
                GameTime.Reset();
                break;
        }
    }

    //aka PreLoad
    void GameSceneLoaded()
    {
        if (ModLoader.CurrentGame == Game.MySummerCar) GameTime.InitializeMSC();
        else if (ModLoader.CurrentGame == Game.MyWinterCar) GameTime.InitializeMWC();
    }
}
