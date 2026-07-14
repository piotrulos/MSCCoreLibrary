using HutongGames.PlayMaker;

namespace MSCCoreLibrary;

/// <summary>
/// Helper class for reading global GUI values.
/// </summary>
public static class GUIPlaymakerGlobals
{
    static readonly FsmBool assemble = FsmVariables.GlobalVariables.FindFsmBool("GUIassemble");
    static readonly FsmBool buy = FsmVariables.GlobalVariables.FindFsmBool("GUIbuy");
    static readonly FsmBool disassemble = FsmVariables.GlobalVariables.FindFsmBool("GUIdisassemble");
    static readonly FsmBool drive = FsmVariables.GlobalVariables.FindFsmBool("GUIdrive");
    static readonly FsmBool passenger = FsmVariables.GlobalVariables.FindFsmBool("GUIpassenger");
    static readonly FsmBool use = FsmVariables.GlobalVariables.FindFsmBool("GUIuse");
    static readonly FsmString gear = FsmVariables.GlobalVariables.FindFsmString("GUIgear");
    static readonly FsmString interaction = FsmVariables.GlobalVariables.FindFsmString("GUIinteraction");
    static readonly FsmString subtitle = FsmVariables.GlobalVariables.FindFsmString("GUIsubtitle");

    /// <summary>
    /// Set cached variable by string
    /// </summary>
    /// <param name="variable">global variable name</param>
    /// <param name="value">value</param>
    public static void SetGUIVariable(string variable, bool value)
    {
        switch (variable)
        {
            case "GUIassemble": assemble.Value = value; break;
            case "GUIbuy": buy.Value = value; break;
            case "GUIdisassemble": disassemble.Value = value; break;
            case "GUIdrive": drive.Value = value; break;
            case "GUIpassenger": passenger.Value = value; break;
            case "GUIuse": use.Value = value; break;
        }
    }
    /// <summary>
    /// GUIassemble
    /// </summary>
    public static bool GUIassemble { get => assemble.Value; set => assemble.Value = value; }

    /// <summary>
    /// GUIbuy
    /// </summary>
    public static bool GUIbuy { get => buy.Value; set => buy.Value = value; }

    /// <summary>
    /// GUIdisassemble
    /// </summary>
    public static bool GUIdisassemble { get => disassemble.Value; set => disassemble.Value = value; }

    /// <summary>
    /// GUIdrive
    /// </summary>
    public static bool GUIdrive  { get => drive.Value; set => drive.Value = value; }

    /// <summary>
    /// GUIpassenger
    /// </summary>
    public static bool GUIpassenger { get => passenger.Value; set => passenger.Value = value; }

    /// <summary>
    /// GUIuse
    /// </summary>
    public static bool GUIuse { get => use.Value; set => use.Value = value; }

    /// <summary>
    /// GUIgear
    /// </summary>
    public static string GUIgear { get => gear.Value; set => gear.Value = value; }

    /// <summary>
    /// GUIinteraction
    /// </summary>
    public static string GUIinteraction { get => interaction.Value; set => interaction.Value = value; }

    /// <summary>
    /// GUIsubtitle
    /// </summary>
    public static string GUIsubtitle { get => subtitle.Value; set => subtitle.Value = value; }
}
