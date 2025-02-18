using HutongGames.PlayMaker;
using MSCLoader;
using System;

namespace MSCCoreLibrary;

/// <summary>
/// Helper class for reading the game time values.
/// </summary>    
public static class GameTime
{
    /// <summary>
    /// Enum representing the days of the week from sunday to saturday, also containing <b>Week</b>, <b>Weekend</b>, and <b>All</b>.
    /// </summary>       
    [Flags] public enum Days
    {
        /// <summary>
        /// Invalid day
        /// </summary>
        Invalid = 0,
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

    private static FsmInt fsm_time;
    private static FsmFloat fsm_minutes;
    private static FsmInt fsm_day;
    private static PlayMakerFSM colorFsm;
    private static bool initialized = false;
    private static Days oldDay = Days.Invalid;
    /// <summary>
    /// Event to invoke when the day changes
    /// </summary>
    /// <param name="day">Current day (that it changed to)</param>
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
        oldDay = (Days)fsm_day.Value;
        initialized = (fsm_time != null && fsm_minutes != null && fsm_day != null);
        
        //Normal day change (midnight)
        colorFsm.FsmInject("00-02", delegate
        {
            Days day = Day;
            oldDay = Day;
            OnNextDay?.Invoke(day);
            Console.WriteLine($"[GameTime] Day changed to {day}");

        }, index: 0);

        //Wakeup event (after sleeping)
        colorFsm.FsmInject("State 3", delegate
        {
            Days day = Day;
            if (day != oldDay) //check if day changed
            {
                OnNextDay?.Invoke(day);
                Console.WriteLine($"[GameTime] Day changed to {day}");
            }
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

    /// <summary>
    /// Whether the current day is a weekend
    /// </summary>
    public static bool IsWeekend => Day.Equals(Days.Weekend);

    /// <summary>
    /// Whether the current day is a weekday
    /// </summary>
    public static bool IsWeekday => Day.Equals(Days.Week);

    /// <summary>
    /// Current time as 24 hour format
    /// </summary>
    public static string CurrentTime => $"{Hour:00}:{Minute:00}";
}
