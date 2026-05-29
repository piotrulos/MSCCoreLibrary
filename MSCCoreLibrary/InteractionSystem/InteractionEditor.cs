#if Mini
using System;
using System.Collections.Generic;
using UnityEditor;

namespace MSCCoreLibrary.InteractionSystem;

static class GUIUtils
{

    private static GUIStyle openFoldoutStyle;
    private static GUIStyle closedFoldoutStyle;
    private static bool initted;

    private static void Init()
    {
        openFoldoutStyle = new GUIStyle(GUI.skin.FindStyle("Button"));
        openFoldoutStyle.fontStyle = FontStyle.Bold;
        openFoldoutStyle.stretchHeight = true;
        closedFoldoutStyle = new GUIStyle(openFoldoutStyle);
        openFoldoutStyle.normal = openFoldoutStyle.onNormal;
        openFoldoutStyle.active = openFoldoutStyle.onActive;
        initted = true;
    }

    public static bool Foldout(bool open, ref bool toggled, string text) { return Foldout(open, ref toggled, new GUIContent(text)); }
    public static bool Foldout(bool open, ref bool toggled, GUIContent text)
    {
        if (!initted) Init();

        if (open)
        {
            GUILayout.BeginHorizontal();
            toggled = GUILayout.Toggle(toggled, "", GUILayout.Width(30));
            if (GUILayout.Button(text, openFoldoutStyle, GUILayout.Height(20), GUILayout.ExpandWidth(true)))
            {
                GUI.FocusControl("");
                GUI.changed = false; // force change-checking group to take notice
                GUI.changed = true;
                return false;
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            toggled = GUILayout.Toggle(toggled, "", GUILayout.Width(30));
            if (GUILayout.Button(text, closedFoldoutStyle, GUILayout.Height(20)))
            {
                GUI.FocusControl("");
                GUI.changed = false; // force change-checking to take notice
                GUI.changed = true;
                return true;
            }
            GUILayout.EndHorizontal();
        }
        return open;
    }
}
public class InteractionElementDetails
{
    public bool[] showInteraction = new bool[5] { false, false, false, false, false };
}
[CustomEditor(typeof(Interaction))]
public class InteractionEditor : Editor
{
    private List<bool> showElement = new List<bool>();
    private List<InteractionElementDetails> showInteractionEventDetails = new List<InteractionElementDetails>();

    private string[] interactionTypeNames = new string[] { "Left Click", "Right Click", "Middle Click", "Scroll Wheel", "Use (F key)" };

    public override void OnInspectorGUI()
    {
        Interaction interactionComponent = (Interaction)target;
        GUIStyle helpBoxRich = GUI.skin.GetStyle("HelpBox");
        helpBoxRich.richText = true;

        Collider collider = interactionComponent.GetComponent<Collider>();
        if (collider == null)
        {
            EditorGUILayout.HelpBox("This component requires a Collider/Trigger!", MessageType.Error);
            return;
        }
        EditorGUILayout.HelpBox(string.Format("Using collider/trigger: {0} ({1})", collider.name, collider.GetType().Name), MessageType.Info);

        // Check layer
        if (interactionComponent.gameObject.layer != LayerMask.NameToLayer("Tools") &&
            interactionComponent.gameObject.layer != LayerMask.NameToLayer("HingedObjects") &&
            interactionComponent.gameObject.layer != LayerMask.NameToLayer("Dashboard"))
        {
            EditorGUILayout.HelpBox("GameObject is not in the required layer (Tools, HingedObjects or Dashboard)!", MessageType.Error);
            return;
        }

        List<InteractionConfig> interactions = interactionComponent.interactions ?? new List<InteractionConfig>();

        bool hasValidActive = interactionComponent.activeInteraction != null && interactions.Contains(interactionComponent.activeInteraction);
        if (!hasValidActive)
        {
            EditorGUILayout.HelpBox("No default active interaction is set!", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox(string.Format("Default active interaction: {0}", interactionComponent.activeInteraction.interactionName), MessageType.Info);
        }

        string[] icons = new string[] { "GUIuse", "GUIbuy", "GUIassemble", "GUIdisassemble", "GUIdrive", "GUIpassenger" };

        // Additional info
        EditorGUILayout.LabelField("Interaction Setup", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Assign functions to the UnityEvents for different interactions.");
        EditorGUILayout.LabelField("Set the interaction icon and text as needed.");

        while (showElement.Count < interactions.Count)
        {
            showElement.Add(true);
            showInteractionEventDetails.Add(new InteractionElementDetails());

        }
        while (showElement.Count > interactions.Count)
        {
            showElement.RemoveAt(showElement.Count - 1);
            showInteractionEventDetails.RemoveAt(showInteractionEventDetails.Count - 1);

        }

        GUIStyle buttonAddStyle = EditorStyles.toolbarButton;
        buttonAddStyle.richText = true;
        if (GUILayout.Button("<color=lime>Add Interaction Config</color>", buttonAddStyle))
        {
            InteractionConfig newInteractionConfig = new InteractionConfig();
            interactions.Add(newInteractionConfig);
            showElement.Add(true);
            showInteractionEventDetails.Add(new InteractionElementDetails());

            if (!hasValidActive)
            {
                interactionComponent.activeInteraction = newInteractionConfig;
                hasValidActive = true;
            }
            interactionComponent.interactions = interactions;
            serializedObject.Update();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Interactions", EditorStyles.boldLabel);

        for (int i = 0; i < interactions.Count; i++)
        {
            string header = string.IsNullOrEmpty(interactions[i].interactionName) ? "Interaction " + (i + 1) : interactions[i].interactionName;
            showElement[i] = EditorGUILayout.Foldout(showElement[i], header);


            if (showElement[i])
            {
                if (interactionComponent.activeInteraction == interactions[i])
                {
                    EditorGUILayout.TextArea("<color=lime>This is the active interaction!</color>", helpBoxRich);
                }

                InteractionConfig interactionConfig = interactions[i];

                EditorGUILayout.BeginHorizontal();
                interactionConfig.interactionName = EditorGUILayout.TextField("Interaction Name", interactionConfig.interactionName, GUILayout.Width(300));

                //set active
                if (GUILayout.Button("Active", EditorStyles.toolbarButton))
                {
                    interactionComponent.activeInteraction = interactionConfig;
                }

                if (GUILayout.Button("<color=red>X</color>", EditorStyles.toolbarButton))
                {
                    if (EditorUtility.DisplayDialog("Are you sure?", "Do you want to remove this interaction?", "Yes", "No"))
                    {

                        interactions.Remove(interactionConfig);
                        showElement.RemoveAt(i);
                        showInteractionEventDetails.RemoveAt(i);
                        i--;
                        if (interactionComponent.activeInteraction == interactionConfig)
                        {
                            interactionComponent.activeInteraction = null;
                        }
                        interactionComponent.interactions = interactions;
                        serializedObject.Update();
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Icon selection
                int selectedIcon = Array.IndexOf(icons, interactionConfig.interactionIcon);
                Texture2D iconTexture = null;

                if (selectedIcon >= 0 && selectedIcon < icons.Length)
                {
                    iconTexture = AssetDatabase.LoadAssetAtPath(string.Format("Assets/Editor/{0}.png", icons[selectedIcon]), typeof(Texture2D)) as Texture2D;
                    if (iconTexture == null)
                    {
                        EditorGUILayout.HelpBox("Icon not found in Assets/Editor!", MessageType.Warning);
                    }
                }

                GUILayout.BeginHorizontal();
                selectedIcon = EditorGUILayout.Popup("Interaction Icon", selectedIcon, icons, GUILayout.Width(300));
                if (iconTexture != null)
                {
                    GUILayout.Label(iconTexture, GUILayout.Width(32), GUILayout.Height(32));
                }
                GUILayout.EndHorizontal();

                interactionConfig.interactionIcon = icons[selectedIcon];
                interactionConfig.interactionText = EditorGUILayout.TextField("Interaction Text", interactionConfig.interactionText);

                EditorGUILayout.LabelField("Interaction Events", EditorStyles.boldLabel);


                DrawInteractionEvents(interactionConfig, i);

                EditorGUILayout.Space();
            }
        }
        if (interactionComponent.activeInteraction != null && !interactions.Contains(interactionComponent.activeInteraction))
        {
            interactionComponent.activeInteraction = null;
        }
        interactionComponent.interactions = interactions;
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawInteractionEvents(InteractionConfig interactionConfig, int interactionIndex)
    {
        for (int eventIndex = 0; eventIndex < 5; eventIndex++)
        {
            InteractionEvent interactionEvent = interactionConfig.interactionEvents[eventIndex];
            if (interactionEvent == null)
            {
                interactionEvent = new InteractionEvent();
                interactionConfig.interactionEvents[eventIndex] = interactionEvent;
            }

            DrawInteractionEventUI(interactionConfig, eventIndex, interactionEvent, interactionIndex);
        }
    }

    private void DrawInteractionEventUI(InteractionConfig interactionConfig, int eventIndex, InteractionEvent interactionEvent, int interactionIndex)
    {
        EditorGUILayout.BeginVertical("box");

        // Header with enabled toggle
        EditorGUILayout.BeginHorizontal();
        //  interactionConfig.interactions[eventIndex] = EditorGUILayout.Toggle(interactionConfig.interactions[eventIndex], GUILayout.Width(20));
        // Debug.Log("showInteractionEventDetails.Count: " + showInteractionEventDetails.Count);
        // Debug.Log("showInteractionEventDetails.Count2: " + showInteractionEventDetails[interactionIndex].showInteractionEventDetails);
        showInteractionEventDetails[interactionIndex].showInteraction[eventIndex] = GUIUtils.Foldout(showInteractionEventDetails[interactionIndex].showInteraction[eventIndex], ref interactionConfig.enabledInteractions[eventIndex], interactionTypeNames[eventIndex]);


        EditorGUILayout.EndHorizontal();

        if (showInteractionEventDetails[interactionIndex].showInteraction[eventIndex])
        {

            // Audio settings
            EditorGUILayout.LabelField("Audio Settings", EditorStyles.boldLabel);
            interactionEvent.playMasterAudioSound = EditorGUILayout.Toggle("Play Master Audio Sound", interactionEvent.playMasterAudioSound);
            if (interactionEvent.playMasterAudioSound)
            {
                interactionEvent.soundType = EditorGUILayout.TextField("Sound Type", interactionEvent.soundType);
                interactionEvent.variationName = EditorGUILayout.TextField("Variation Name", interactionEvent.variationName);
            }

            EditorGUILayout.Space();

            // Interaction-specific settings
            if (eventIndex != 3) // Not scroll wheel
            {
                EditorGUILayout.LabelField("Hold Settings", EditorStyles.boldLabel);
                interactionEvent.onHold = EditorGUILayout.Toggle("Enable Hold Functionality", interactionEvent.onHold);

                if (interactionEvent.onHold)
                {
                    interactionEvent.holdTime = EditorGUILayout.FloatField("Hold Time (seconds)", interactionEvent.holdTime);
                    if (interactionEvent.holdTime < 0)
                        interactionEvent.holdTime = 0;
                }
                EditorGUILayout.Space();
            }
            // Debug.Log(eventIndex);
            //Debug.Log(interactionIndex);

            // UnityEvent fields
            if (eventIndex == 3) // Scroll wheel
            {
                EditorGUILayout.LabelField("Scroll Event", EditorStyles.boldLabel);
                SerializedProperty onMouseScrollProp = serializedObject.FindProperty("interactions").GetArrayElementAtIndex(interactionIndex).FindPropertyRelative("interactionEvents").GetArrayElementAtIndex(eventIndex).FindPropertyRelative("OnScroll");
                EditorGUILayout.PropertyField(onMouseScrollProp);
            }
            else
            {
                if (interactionEvent.onHold)
                {

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Hold Event (OnHold)", EditorStyles.boldLabel);
                    SerializedProperty onMouseHoldProp = serializedObject.FindProperty("interactions").GetArrayElementAtIndex(interactionIndex).FindPropertyRelative("interactionEvents").GetArrayElementAtIndex(eventIndex).FindPropertyRelative("OnHold");
                    EditorGUILayout.PropertyField(onMouseHoldProp);
                    EditorGUILayout.EndVertical();

                }
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Click Event (OnClick)", EditorStyles.boldLabel);
                SerializedProperty onMouseClickProp = serializedObject.FindProperty("interactions").GetArrayElementAtIndex(interactionIndex).FindPropertyRelative("interactionEvents").GetArrayElementAtIndex(eventIndex).FindPropertyRelative("OnClick");
                EditorGUILayout.PropertyField(onMouseClickProp);
                EditorGUILayout.EndVertical();


            }


        }

        EditorGUILayout.EndVertical();
    }
    void OnInspectorUpdate()
    {
        Repaint();
    }
}

#endif