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
        openFoldoutStyle.fontStyle = (FontStyle)1;
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

[CustomEditor(typeof(Interaction))]
public class InteractionEditor : Editor
{
    private List<bool> showElement = new List<bool>();
    private bool[] interactionEvents = new bool[5] { false, false, false, false, false };
    int mouseClickType = 0;
    public override void OnInspectorGUI()
    {
        Interaction interactionComponent = (Interaction)target;
        GUIStyle helpBoxRich = GUI.skin.GetStyle("HelpBox");
        helpBoxRich.richText = true;
        //DrawDefaultInspector();
        //private int interactionLayerMask = LayerMask.GetMask("Tools", "HingedObjects", "Dashboard");

        Collider collider = interactionComponent.GetComponent<Collider>();
        if (collider == null)
        {
            EditorGUILayout.HelpBox("This component requires a Collider/Trigger!", MessageType.Error);
            return;
        }
        EditorGUILayout.HelpBox(string.Format("Using collider/trigger: {0} ({1})", collider.name, collider.GetType().Name), MessageType.Info);

        // Check layer
        if (interactionComponent.gameObject.layer != LayerMask.NameToLayer("Tools") && interactionComponent.gameObject.layer != LayerMask.NameToLayer("HingedObjects") && interactionComponent.gameObject.layer != LayerMask.NameToLayer("Dashboard"))
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
            showElement.Add(true);
        while (showElement.Count > interactions.Count)
            showElement.RemoveAt(showElement.Count - 1);

        GUIStyle buttonAddStyle = EditorStyles.toolbarButton;
        buttonAddStyle.richText = true;
        if (GUILayout.Button("<color=lime>Add Interaction Config</color>", buttonAddStyle))
        {
            InteractionConfig newInteractionConfig = new InteractionConfig();
            interactions.Add(newInteractionConfig);
            showElement.Add(true);
            // If there is no valid active interaction, set the newly added one as active.
            if (!hasValidActive)
            {
                interactionComponent.activeInteraction = newInteractionConfig;
                hasValidActive = true;
            }
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
                        i--;
                        if (interactionComponent.activeInteraction == interactionConfig)
                        {
                            interactionComponent.activeInteraction = null;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                int selectedIcon = Array.IndexOf(icons, interactionConfig.interactionIcon);
                Texture2D iconTexture = null;

                if (selectedIcon >= 0 && selectedIcon < icons.Length)
                {
                    iconTexture = AssetDatabase.LoadAssetAtPath(string.Format("Assets/Editor/{0}.png", icons[selectedIcon]), typeof(Texture2D)) as Texture2D;
                    if (iconTexture != null)
                    {
                        //   interaction.interactionIcon = icons[selectedIcon];
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Icon not found in Assets/Editor!", MessageType.Warning);
                    }
                }
                GUILayout.BeginHorizontal();

                selectedIcon = EditorGUILayout.Popup("Interaction Icon", selectedIcon, icons, GUILayout.Width(300));

                GUILayout.Label(iconTexture, GUILayout.Width(32), GUILayout.Height(32));
                GUILayout.EndHorizontal();

                interactionConfig.interactionIcon = icons[selectedIcon];
                interactionConfig.interactionText = EditorGUILayout.TextField("Interaction Text", interactionConfig.interactionText);

                EditorGUILayout.LabelField("Interaction Events", EditorStyles.boldLabel);
                //   mouseClickType = EditorGUILayout.Popup("Add Interaction Event", mouseClickType, new string[] { "Left Click", "Right Click", "Middle Click", "Scroll Wheel", "Use (F key)" }, GUILayout.Width(300));
                //  bool flag = EditorGUILayout.Toggle(false, "ShurikenToggle");

                interactionEvents[0] = GUIUtils.Foldout(interactionEvents[0], ref interactionConfig.interactions[0], "Left Click");
                interactionEvents[1] = GUIUtils.Foldout(interactionEvents[1], ref interactionConfig.interactions[1], "Right Click");
                interactionEvents[2] = GUIUtils.Foldout(interactionEvents[2], ref interactionConfig.interactions[2], "Middle Click");
                interactionEvents[3] = GUIUtils.Foldout(interactionEvents[3], ref interactionConfig.interactions[3], "Scroll Wheel");
                interactionEvents[4] = GUIUtils.Foldout(interactionEvents[4], ref interactionConfig.interactions[4], "Use (F key)");


                //   interactionConfig.interactionEvents[4].onHold = EditorGUILayout.Toggle("Held", interactionConfig.interactionEvents[4].onHold);
                /*int mouseClickHeldTime = EditorGUILayout.IntField("Held Time (seconds)", 0);*/

                //   EditorGUILayout.LabelField("Add Functions to Mouse Click Event", EditorStyles.boldLabel); 
                // SerializedProperty elementProp = interactionsProp.GetArrayElementAtIndex(i);
                // SerializedProperty onMouseClickProp = serializedObject.FindProperty("interactions").GetArrayElementAtIndex(i).FindPropertyRelative("onMouseClick");
                // EditorGUILayout.PropertyField(onMouseClickProp);
            }
        }

        if (interactionComponent.activeInteraction != null && !interactions.Contains(interactionComponent.activeInteraction))
        {
            interactionComponent.activeInteraction = null;
        }

        interactionComponent.interactions = interactions;

        serializedObject.ApplyModifiedProperties();
        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}

#endif