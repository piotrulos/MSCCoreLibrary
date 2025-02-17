using HutongGames.PlayMaker;
using MSCLoader;
using System;

namespace MSCCoreLibrary;

/// <summary>
/// Class to read the current in-game time and day
/// </summary>    
public static class GameTime
{
    /// <summary>
    /// Enum representing the days of the week from sunday to saturday, also containing <b>Week</b>, <b>Weekend</b>, and <b>All</b>.
    /// </summary>       
    [Flags] public enum Days
    {
        /// <summary>
        /// Monday
        /// </summary>
        Monday = 1 << 1,
        /// <summary>
        /// Tuesday
        /// </summary>
        Tuesday = 1 << 2,
        /// <summary>
        /// Wednesday
        /// </summary>
        Wednesday = 1 << 3,
        /// <summary>
        /// Thursday
        /// </summary>
        Thursday = 1 << 4,
        /// <summary>
        /// Friday
        /// </summary>
        Friday = 1 << 5,
        /// <summary>
        /// Saturday
        /// </summary>
        Saturday = 1 << 6,
        /// <summary>
        /// Sunday
        /// </summary>
        Sunday = 1 << 7,

        /// <summary>
        /// All days from Monday to Friday
        /// </summary>
        Week = Monday | Tuesday | Wednesday | Thursday | Friday,
        /// <summary>
        /// All days of the weekend (Saturday and Sunday)
        /// </summary>
        Weekend = Saturday | Sunday,
        /// <summary>
        /// All days of the week
        /// </summary>
        All = Week | Weekend
    }

    static FsmInt fsm_time;
    static FsmFloat fsm_minutes;
    static FsmInt fsm_day;
    static PlayMakerFSM colorFsm;
    static bool initialized = false;

    /// <summary>
    /// Event to invoke when the day changes
    /// </summary>
    /// <param name="day"></param>
    public delegate void NextDayEventHandler(Days day);
    /// <summary>
    /// Event to invoke when the day changes
    /// </summary>
    public static event NextDayEventHandler OnNextDay;

    internal static void Initialize()
    {
        if (initialized) return;

        GameObject sun = GameObject.Find("MAP").transform.Find("SUN/Pivot/SUN").gameObject;
        if (sun == null) return; 

        colorFsm = sun.GetPlayMaker("Color");

        fsm_time = colorFsm.GetVariable<FsmInt>("Time");
        fsm_minutes = colorFsm.GetVariable<FsmFloat>("Minutes");

        fsm_day = PlayMakerGlobals.Instance.Variables.GetFsmInt("GlobalDay");

        initialized = (fsm_time != null && fsm_minutes != null && fsm_day != null);
        colorFsm.FsmInject("00-02", delegate
        {
            Days day = Day;
            OnNextDay?.Invoke(day);
            ModConsole.Warning($"[GameTime] Day changed to {day}");

        }, index: 0);
    }
    internal static void Reset() => initialized = false;
    /// <summary>
    /// Current hour from 0 to 24
    /// </summary>
    public static int Hour
    {
        get
        {
            if (!initialized) return 0;
            int hour = fsm_time.Value == 24 ? 0 : fsm_time.Value;
            if (fsm_minutes.Value > 60f) hour++;
            return hour;
        }
    }

    /// <summary>
    /// Current minute from 0 to 60
    /// </summary>
    public static int Minute
    {
        get
        {
            if (!initialized) return 0;
            return Mathf.Clamp(Mathf.FloorToInt(fsm_minutes.Value) % 60, 0, 59);
        }
    }

    /// <summary>
    /// Current day
    /// </summary>
    public static Days Day
    {
        get
        {
            if (!initialized) return 0;
            return (Days)(1 << fsm_day.Value);
        }
    }
}
