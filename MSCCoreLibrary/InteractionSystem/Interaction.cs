using System;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MSCCoreLibrary.InteractionSystem;
[Serializable]
public class MouseScrollUnityEvent : UnityEvent<bool>
{
}
public enum InteractionType
{
    LeftClick = 0,
    RightClick = 1,
    MiddleClick = 2,
    ScrollWheel = 3,
    Use = 4
}
[Serializable]
public class InteractionEvent
{
    // public InteractionType interactionType = InteractionType.LeftClick;
    public bool playMasterAudioSound = false;
    public string soundType = "";
    public string variationName = "";
    public UnityEvent OnClick = null;
    public bool onHold = false;
    public float holdTime = 0.5f;
    public UnityEvent OnHold = null;
    public MouseScrollUnityEvent OnScroll = null;

}
[Serializable]
public class InteractionConfig
{
    public string interactionName = "";
    public string interactionIcon = "GUIuse";
    public string interactionText = "";
    public bool[] interactions = new bool[5] { false, false, false, false, false };
    public InteractionEvent[] interactionEvents = new InteractionEvent[5];
}

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class Interaction : MonoBehaviour
{
    //Active    //
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
