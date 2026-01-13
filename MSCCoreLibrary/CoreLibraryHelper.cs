using System.Collections;
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
                StartCoroutine(WaitForGame());
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

    //aka OnLoad
    void GameFullyLoaded()
    {

    }

    IEnumerator WaitForGame()
    {
        while (GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera") == null)
            yield return null;
        GameFullyLoaded();
    }
}
