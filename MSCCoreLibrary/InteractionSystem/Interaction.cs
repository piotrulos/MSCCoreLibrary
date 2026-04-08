using System;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MSCCoreLibrary.InteractionSystem;
[Serializable]
public class MouseClickUnityEvent : UnityEvent<int, bool, int>
{
}
[Serializable]
public class InteractionConfig
{
    public string interactionName = "";
    public string interactionIcon = "GUIuse";
    public string interactionText = "";
    public MouseClickUnityEvent onMouseClick; //0=left, 1=right, 2=middle | bool= held | int=held time in seconds
    //public UnityEvent onRightClick;
    public UnityEvent<bool> onScroll; //true=up, false=down
    public UnityEvent onUse; //cinput "Use"
}

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class Interaction : MonoBehaviour
{
    //Active    //
    public InteractionConfig activeInteraction;
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
