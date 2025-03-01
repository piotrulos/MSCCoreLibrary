﻿using MSCLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MSCCoreLibrary;

/// <summary>
/// Class to scheduling actions to execute at specific moments during game time
/// </summary>
public class TimeScheduler : MonoBehaviour
{
    /// <summary>
    /// Event you can subscribe your actions to to execute when time is skipped in any way. Hands over an int value telling the skipped time in minutes
    /// </summary>
    public static event Action<int> OnTimeSkipped;

    static GameObject timeScheduler;
    static bool schedulerInstantiated = false;
    static bool executeActions = false;

    internal static void StartScheduler()
    {
        try
        {
            if (schedulerInstantiated) return;
            timeScheduler = new GameObject("MSCCoreLibrary Time Scheduler");
            timeScheduler.AddComponent<TimeScheduler>();
            timeScheduler.transform.SetParent(CoreLibrary.coreLibraryHelper.transform, false);
            schedulerInstantiated = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ModConsole.Error($"TimeScheduler failed to start: {e.Message}");
        }
    }


    internal static void StopScheduler()
    {

        CoreLibrary.SetupCoreLibrary();
        try
        {
            if (!schedulerInstantiated) return;
            GameObject.Destroy(timeScheduler);
            timeScheduler = null;
            ScheduledActions = new List<ScheduledAction>();
            previousMinute = previousHour = default;
            previousDay = default;
            executeActions = false;
            schedulerInstantiated = false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ModConsole.Error($"TimeScheduler failed to stop: {e.Message}");
        }
    }

    /// <summary>
    /// Class for all scheduled actions
    /// </summary>
    public class ScheduledAction
    {
        /// <summary>Hour (0-23)</summary>
        public int Hour { get; }
        /// <summary>Minute (0-59)</summary>
        public int Minute { get; }
        /// <summary>Action to invoke</summary>
        public Action Action { get; }
        /// <summary>Whether the action is only ran once and then unscheduled or not</summary>
        public bool OneTimeAction { get; }
        /// <summary>Days the action is invoked on, also can be chained together using bitwise OR</summary>
        public GameTime.Days Day { get; }

        internal ScheduledAction(int hour, int minute, Action action, GameTime.Days day, bool oneTimeAction)
        {
            Hour = hour;
            Minute = minute;
            Action = action;
            Day = day;
            OneTimeAction = oneTimeAction;
        }
    }

    /// <summary>
    /// List containing all Scheduled Actions
    /// </summary>
    public static List<ScheduledAction> ScheduledActions { get; private set; } = new List<ScheduledAction>();

    /// <summary>
    /// Method to schedule an action
    /// </summary>
    /// <param name="hour">Game Hour (0-24)</param>
    /// <param name="minute">Game Minute (0-60)</param>
    /// <param name="action">The action to execute</param>
    /// <param name="day">Days the action is invoked on, also can be chained together using bitwise OR</param>
    /// <param name="oneTimeAction">[Optional] Whether the action is ran only once (false on default)</param>
    /// <returns>Scheduled action</returns>
    public static ScheduledAction ScheduleAction(int hour, int minute, Action action, GameTime.Days day = GameTime.Days.All, bool oneTimeAction = false) 
    {
        ScheduledAction sa;
        ScheduledActions.Add(sa = new ScheduledAction(hour, minute, action, day, oneTimeAction));

        ScheduledActions = SortActions(ScheduledActions);

        return sa;
    }

    static int previousMinute;
    static int previousHour;
    static GameTime.Days previousDay;

    /// <summary>
    /// Method to un-schedule an action
    /// </summary>
    /// <param name="action">The action you want to unscheduled</param>
    /// <returns>Whether the action has been unscheduled successfully or not</returns>
    public static bool UnscheduleAction(ScheduledAction action)
    {
        if (ScheduledActions.Contains(action))
        {
            ScheduledActions.Remove(action);
            return true;
        }

        else return false;
    }

    void Update()
    {
        if (GameTime.Minute == previousMinute || !executeActions) return;

        for (int i = ScheduledActions.Count - 1; i >= 0; i--)
        {
            ScheduledAction action = ScheduledActions[i];

            if (action.Hour == GameTime.Hour && action.Minute == GameTime.Minute && ((GameTime.Day & action.Day) != 0))
            {
                action.Action.Invoke();
                if (action.OneTimeAction) ScheduledActions.Remove(action);
            }

            if (action.Hour > GameTime.Hour || action.Minute > GameTime.Minute) break;
        }

        if ((GameTime.Hour - previousHour == 0 && Math.Abs(GameTime.Minute - previousMinute) > 1) || // same hour, but minutes skipped                                                                     
            GameTime.Hour - previousHour > 1 || GameTime.Hour - previousHour < 0) // more than one hour skipped
        {
            InvokeMissedActions(previousHour, previousMinute, previousDay);
            OnTimeSkipped?.Invoke(GetTimeDifference(previousHour, previousMinute, previousDay));
        }

        previousMinute = GameTime.Minute;
        previousHour = GameTime.Hour;
        previousDay = GameTime.Day;
    }

    static int GetTimeDifference(int previousHour, int previousMinute, GameTime.Days previousDay)
    {
        int currentTotalMinutes = CalcTotalMinutes(GameTime.Hour, GameTime.Minute, GameTime.Day);
        int previousTotalMinutes = CalcTotalMinutes(previousHour, previousMinute, previousDay);

        // week rollover
        if (currentTotalMinutes < previousTotalMinutes) currentTotalMinutes += 10080;  // Adds one full week of minutes

        return currentTotalMinutes - previousTotalMinutes;
    }

    static void InvokeMissedActions(int sinceHour, int sinceMinute, GameTime.Days sinceDay)
    {
        List<ScheduledAction> missedActions = new List<ScheduledAction>();

        foreach (ScheduledAction action in ScheduledActions) if (ActionMissed(action, sinceDay, sinceHour, sinceMinute)) missedActions.Add(action);

        foreach (ScheduledAction action in missedActions)
        {
            action.Action.Invoke();
            if (action.OneTimeAction) ScheduledActions.Remove(action);
        }
    }

    static bool ActionMissed(ScheduledAction action, GameTime.Days sinceDay, int sinceHour, int sinceMinute)
    {
        int currentTotalMinutes = CalcTotalMinutes(GameTime.Hour, GameTime.Minute, GameTime.Day);
        int sinceTotalMinutes = CalcTotalMinutes(sinceHour, sinceMinute, sinceDay);

        for (GameTime.Days day = GameTime.Days.Sunday; day <= GameTime.Days.Saturday; day = (GameTime.Days)((int)day << 1))
        {
            if ((action.Day & day) != 0)
            {
                int actionTotalMinutes = CalcTotalMinutes(action.Hour, action.Minute, day);

                // Week rollover
                if (currentTotalMinutes < sinceTotalMinutes) currentTotalMinutes += 10080;  
                
                if (actionTotalMinutes < sinceTotalMinutes) actionTotalMinutes += 10080;

                if (actionTotalMinutes > sinceTotalMinutes && actionTotalMinutes <= currentTotalMinutes) return true; 
            }
        }

        return false;
    }

    static int CalcTotalMinutes(int hour, int minute, GameTime.Days day = (GameTime.Days)1) => ((int)Math.Log((int)day, 2) * 24 + hour) * 60 + minute;

    static List<ScheduledAction> SortActions(List<ScheduledAction> list)
    {
        return list
            .OrderBy(action => GetFirstSetDay(action.Day))
            .ThenBy(action => action.Hour)
            .ThenBy(action => action.Minute)
            .ToList();

        static int GetFirstSetDay(GameTime.Days day)
        {
            if ((day & GameTime.Days.Sunday) != 0) return 0;
            if ((day & GameTime.Days.Monday) != 0) return 1;
            if ((day & GameTime.Days.Tuesday) != 0) return 2;
            if ((day & GameTime.Days.Wednesday) != 0) return 3;
            if ((day & GameTime.Days.Thursday) != 0) return 4;
            if ((day & GameTime.Days.Friday) != 0) return 5;
            if ((day & GameTime.Days.Saturday) != 0) return 6;

            return int.MaxValue;
        }
    }

    internal static void SaveScheduler()
    {
        try
        {
            ES2.Save(GameTime.Hour, $"Mods.txt?tag=MSCLoader_TimeScheduler||hour");
            ES2.Save(GameTime.Minute, $"Mods.txt?tag=MSCLoader_TimeScheduler||minute");
            ES2.Save(GameTime.Day, $"Mods.txt?tag=MSCLoader_TimeScheduler||day");
        }
        catch (Exception e)
        {
            Console.WriteLine(e); 
            ModConsole.Error($"TimeScheduler failed to save: {e.Message}");
        }
    }

    internal static void LoadScheduler()
    {
        try
        {
            int hour = default;
            int minute = default;
            GameTime.Days day = default;

            bool savedata = true;
            string savepath = "Mods.txt?tag=MSCLoader_TimeScheduler||";

            if (ES2.Exists($"{savepath}hour"))
                hour = ES2.Load<int>($"{savepath}hour");
            else savedata = false;

            if (ES2.Exists($"{savepath}minute"))
                minute = ES2.Load<int>($"{savepath}minute");

            if (ES2.Exists($"{savepath}day"))
                day = ES2.Load<GameTime.Days>($"{savepath}day");

            if (savedata) InvokeMissedActions(hour, minute, day);

            timeScheduler.GetComponent<TimeScheduler>().StartCoroutine(Wait(day));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ModConsole.Error($"TimeScheduler failed to load: {e.Message}");
        }
    }

    static IEnumerator Wait(GameTime.Days day)
    {
        while (GameTime.Day != day) yield return new WaitForEndOfFrame();

        previousMinute = GameTime.Minute;
        previousHour = GameTime.Hour;
        previousDay = GameTime.Day;

        executeActions = true;

        yield break;
    }
}
