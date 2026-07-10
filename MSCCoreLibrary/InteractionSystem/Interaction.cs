using MSCLoader;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MSCCoreLibrary.InteractionSystem;
[Serializable]
public class InteractionEvent
{
    public bool playMasterAudioSound = false;
    public string soundType = "";
    public string variationName = "";
    public UnityEvent OnClick = null;
    public bool onHold = false;
    public float holdDelay = 0.5f;
    public UnityEvent OnHold = null;
    public UnityEvent OnRelease = null;
    public UnityEvent OnScrollUp = null;
    public UnityEvent OnScrollDown = null;


    internal float holdTime = 0.5f;

    public InteractionEvent()
    {
        OnClick = new UnityEvent();
        OnHold = new UnityEvent();
        OnRelease = new UnityEvent();
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
[Serializable]
public class Interaction : MonoBehaviour
{
    public List<InteractionConfig> interactions = new List<InteractionConfig>();
    public int activeInteractionIndex = -1; 
    private InteractionConfig activeInteraction = null;
    private Collider interactionCollder = null;
  //  private float holdTime = 0f;
    private void Start()
    {
        interactionCollder = GetComponent<Collider>();
        if (activeInteraction == null && activeInteractionIndex >= 0)
            SetActiveInteraction(activeInteractionIndex);
        ModConsole.Warning((activeInteraction == null).ToString());
        ModConsole.Warning(interactions.Count.ToString());

    }

    public void SetActiveInteraction(int index)
    {

        if (index >= 0 && index < interactions.Count)
        {
            activeInteraction = interactions[index];
            activeInteractionIndex = index;
        }
        else
        { 
            activeInteraction = interactions[0];
            activeInteractionIndex = 0;
        }
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
        
        ModConsole.Error("Interaction " + interactionName + " not found!");
        activeInteraction = interactions[0];
    }

    private void MouseInputInteractionEvents(InteractionEvent interactionEvent, int mouseButton)
    {
        if (interactionEvent.onHold)
        {
            if (Input.GetMouseButton(mouseButton))
            {
                interactionEvent.holdTime += Time.deltaTime;
                if (interactionEvent.holdTime >= interactionEvent.holdDelay)
                    interactionEvent.OnHold?.Invoke();
            }
            if (Input.GetMouseButtonUp(mouseButton))
            {
                if (interactionEvent.holdTime >= interactionEvent.holdDelay)
                {
                    interactionEvent.OnRelease?.Invoke();
                }
                else
                {
                    interactionEvent.OnClick?.Invoke();
                }
                interactionEvent.holdTime = 0f;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(mouseButton))
                interactionEvent.OnClick?.Invoke();
        }
    }
    private void Update()
    {
        if(activeInteraction == null) return;
        if (UnifiedRaycast.GetHitInteraction(interactionCollder))
        {
            for (int i = 0; i < activeInteraction.enabledInteractions.Length; i++)
            {
                if (activeInteraction.enabledInteractions[i])
                {
                    switch (i)
                    {
                        case 0:                          
                        case 1:
                        case 2:
                            MouseInputInteractionEvents(activeInteraction.interactionEvents[i], i);
                            break;
                        case 3:
                            if (Input.mouseScrollDelta.y > 0)
                                activeInteraction.interactionEvents[i].OnScrollUp?.Invoke();
                            else if (Input.mouseScrollDelta.y < 0)
                                activeInteraction.interactionEvents[i].OnScrollDown?.Invoke();
                            break;
                        case 4:
                            if (cInput.GetButtonDown("Use"))
                                activeInteraction.interactionEvents[i].OnClick?.Invoke();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}




