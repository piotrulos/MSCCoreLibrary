using System;
using System.Collections.Generic;
using UnityEngine.Events;
using static MSCCoreLibrary.InteractionSystem.Interaction;

namespace MSCCoreLibrary.InteractionSystem;
[Serializable]
public class InteractionEvent
{
    public bool playMasterAudioSound = false;
    public string soundType = "";
    public string variationName = "";
    public UnityEvent OnClick = null;
    public bool onHold = false;
    public float holdTime = 0.5f;
    public UnityEvent OnHold = null;
    public UnityEvent OnScrollUp = null;
    public UnityEvent OnScrollDown = null;

    public InteractionEvent()
    {
        OnClick = new UnityEvent();
        OnHold = new UnityEvent();
        OnScrollUp = new UnityEvent();
        OnScrollDown = new UnityEvent();
    }
}

[Serializable]
public class InteractionConfig
{
    public string interactionName = "";
    public string interactionIcon = "GUIuse";
    public string interactionText = "";
    public bool[] enabledInteractions = new bool[5] { false, false, false, false, false };
    public InteractionEvent[] interactionEvents = new InteractionEvent[5];

    public InteractionConfig()
    {
        enabledInteractions = new bool[5] { false, false, false, false, false };
        interactionEvents = new InteractionEvent[5];
        for (int i = 0; i < 5; i++)
        {
            interactionEvents[i] = new InteractionEvent();
        }
    }
}
public enum InteractionType
{
    LeftClick = 0,
    RightClick = 1,
    MiddleClick = 2,
    ScrollWheel = 3,
    Use = 4
}

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class Interaction : MonoBehaviour
{



    public InteractionConfig activeInteraction = null;
    public List<InteractionConfig> interactions = new List<InteractionConfig>();

    public void SetActiveInteraction(int index)
    {
        if (index >= 0 && index < interactions.Count)
            activeInteraction = interactions[index];
        else
            activeInteraction = interactions[0];
    }

    public void SetActiveInteraction(string interactionName)
    {
        for (int i = 0; i < interactions.Count; i++)
        {
            if (interactions[i].interactionName == interactionName)
            {
                activeInteraction = interactions[i];
                return;
            }
        }
        Debug.LogError("Interaction " + interactionName + " not found!");
        activeInteraction = interactions[0];
    }
}




