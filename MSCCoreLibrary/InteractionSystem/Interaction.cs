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

    internal float holdTime = 0f;

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
public class Interaction : MonoBehaviour, ISerializationCallbackReceiver
{
    public List<InteractionConfig> interactions = new List<InteractionConfig>();
    public int activeInteractionIndex = -1;
    private InteractionConfig activeInteraction = null;
    private Collider interactionCollder = null;

    // --- Custom Serialization Fields ---
    [HideInInspector, SerializeField] private List<string> _sNames = new List<string>();
    [HideInInspector, SerializeField] private List<string> _sIcons = new List<string>();
    [HideInInspector, SerializeField] private List<string> _sTexts = new List<string>();
    [HideInInspector, SerializeField] private List<bool> _sEnabled = new List<bool>();

    [HideInInspector, SerializeField] private List<bool> _ePlayAudio = new List<bool>();
    [HideInInspector, SerializeField] private List<string> _eSoundType = new List<string>();
    [HideInInspector, SerializeField] private List<string> _eVarName = new List<string>();
    [HideInInspector, SerializeField] private List<bool> _eOnHold = new List<bool>();
    [HideInInspector, SerializeField] private List<float> _eHoldDelay = new List<float>();

    [HideInInspector, SerializeField] private List<UnityEvent> _eOnClick = new List<UnityEvent>();
    [HideInInspector, SerializeField] private List<UnityEvent> _eOnHoldEv = new List<UnityEvent>();
    [HideInInspector, SerializeField] private List<UnityEvent> _eOnRelease = new List<UnityEvent>();
    [HideInInspector, SerializeField] private List<UnityEvent> _eOnScrollUp = new List<UnityEvent>();
    [HideInInspector, SerializeField] private List<UnityEvent> _eOnScrollDown = new List<UnityEvent>();

    [HideInInspector] public bool _isDirty = false; 
 
    // Unity calls this before saving the object (Editor/AssetBundle Export)
    public void OnBeforeSerialize()
    {
        //Seems like don't need that in game only in editor
#if Mini
        if(_isDirty) return; //Workaround for editor (only serialize when applying prefab changes)

        _sNames.Clear(); _sIcons.Clear(); _sTexts.Clear(); _sEnabled.Clear();
        _ePlayAudio.Clear(); _eSoundType.Clear(); _eVarName.Clear(); _eOnHold.Clear(); _eHoldDelay.Clear();
        _eOnClick.Clear(); _eOnHoldEv.Clear(); _eOnRelease.Clear(); _eOnScrollUp.Clear(); _eOnScrollDown.Clear();

        if (interactions == null) return;

        foreach (var config in interactions)
        {
            _sNames.Add(config.interactionName);
            _sIcons.Add(config.interactionIcon);
            _sTexts.Add(config.interactionText);

            for (int i = 0; i < 5; i++)
            {
                _sEnabled.Add(config.enabledInteractions[i]);

                var ev = config.interactionEvents[i];
                if (ev == null) ev = new InteractionEvent();

                _ePlayAudio.Add(ev.playMasterAudioSound);
                _eSoundType.Add(ev.soundType);
                _eVarName.Add(ev.variationName);
                _eOnHold.Add(ev.onHold);
                _eHoldDelay.Add(ev.holdDelay);

                _eOnClick.Add(ev.OnClick ?? new UnityEvent());
                _eOnHoldEv.Add(ev.OnHold ?? new UnityEvent());
                _eOnRelease.Add(ev.OnRelease ?? new UnityEvent());
                _eOnScrollUp.Add(ev.OnScrollUp ?? new UnityEvent());
                _eOnScrollDown.Add(ev.OnScrollDown ?? new UnityEvent());
            }
        }
#endif
    }

    // Unity calls this after loading the object (Editor/Runtime bundle load)
    public void OnAfterDeserialize()
    { 

#if !Mini
        // Do not override if Unity successfully deserialized (or list is empty)
        if (_sNames == null || _sNames.Count == 0) return;

        interactions = new List<InteractionConfig>();
        for (int i = 0; i < _sNames.Count; i++)
        {
            var config = new InteractionConfig();
            config.interactionName = _sNames[i];
            config.interactionIcon = _sIcons[i];
            config.interactionText = _sTexts[i];

            for (int j = 0; j < 5; j++)
            {
                int flatIndex = i * 5 + j;
                if (flatIndex < _sEnabled.Count) 
                {
                    config.enabledInteractions[j] = _sEnabled[flatIndex];

                    var ev = config.interactionEvents[j];
                    ev.playMasterAudioSound = _ePlayAudio[flatIndex];
                    ev.soundType = _eSoundType[flatIndex];
                    ev.variationName = _eVarName[flatIndex];
                    ev.onHold = _eOnHold[flatIndex];
                    ev.holdDelay = _eHoldDelay[flatIndex];

                    ev.OnClick = _eOnClick[flatIndex];
                    ev.OnHold = _eOnHoldEv[flatIndex];
                    ev.OnRelease = _eOnRelease[flatIndex];
                    ev.OnScrollUp = _eOnScrollUp[flatIndex];
                    ev.OnScrollDown = _eOnScrollDown[flatIndex];
                }
            }
            interactions.Add(config);
        }
#endif
    }

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
        if (activeInteraction == null) return;

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
