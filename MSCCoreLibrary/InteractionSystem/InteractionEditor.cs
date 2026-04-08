#if Mini
using System;
using System.Collections.Generic;

using UnityEditor;

namespace MSCCoreLibrary.InteractionSystem
{
    [CustomEditor(typeof(Interaction))]
    public class InteractionEditor : Editor
    {
        private List<bool> showElement = new List<bool>();

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
            EditorGUILayout.HelpBox(string.Format("Using collider/trigger: {0}", collider.name), MessageType.Info);
            // Check layer
            if (interactionComponent.gameObject.layer != LayerMask.NameToLayer("Tools") && interactionComponent.gameObject.layer != LayerMask.NameToLayer("HingedObjects") && interactionComponent.gameObject.layer != LayerMask.NameToLayer("Dashboard"))
            {
                EditorGUILayout.HelpBox("GameObject is not in the required layer (Tools, HingedObjects or Dashboard)!", MessageType.Error);
                return;
            }
            if (interactionComponent.activeInteraction == null)
            {
                EditorGUILayout.HelpBox("No default active interaction is set!", MessageType.Warning);
            }
            string[] icons = new string[] { "GUIuse", "GUIbuy", "GUIassemble", "GUIdisassemble", "GUIdrive", "GUIpassenger" };

            // Additional info
            EditorGUILayout.LabelField("Interaction Setup", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Assign functions to the UnityEvents for different interactions.");
            EditorGUILayout.LabelField("Set the interaction icon and text as needed.");

            List<InteractionConfig> interactions = interactionComponent.interactions;
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
                if (interactionComponent.activeInteraction == null)
                {
                    interactionComponent.activeInteraction = newInteractionConfig;
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

                    EditorGUILayout.LabelField("Mouse Click Events", EditorStyles.boldLabel);
                    int mouseClickType = EditorGUILayout.Popup("Mouse Button", 0, new string[] { "Left Click", "Right Click", "Middle Click" });
                    bool mouseClickHeld = EditorGUILayout.Toggle("Held", false);
                    int mouseClickHeldTime = EditorGUILayout.IntField("Held Time (seconds)", 0);

                    EditorGUILayout.LabelField("Add Functions to Mouse Click Event", EditorStyles.boldLabel);
                    // SerializedProperty elementProp = interactionsProp.GetArrayElementAtIndex(i);
                    SerializedProperty onMouseClickProp = serializedObject.FindProperty("interactions").GetArrayElementAtIndex(i).FindPropertyRelative("onMouseClick");
                    EditorGUILayout.PropertyField(onMouseClickProp);
                }
            }

            interactionComponent.interactions = interactions;

            serializedObject.ApplyModifiedProperties();



        }
        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
#endif