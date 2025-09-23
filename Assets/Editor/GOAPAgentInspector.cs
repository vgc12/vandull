using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(GoapAgent))]
public class GOAPAgentInspector : UnityEditor.Editor {
    public override void OnInspectorGUI() {
        GoapAgent agent = (GoapAgent)target;

        EditorGUILayout.Space();
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (agent.CurrentGoal != null) {
            EditorGUILayout.LabelField("Current Goal:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.LabelField(agent.CurrentGoal.Name);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        // Show current action
        if (agent.CurrentAction != null) {
            EditorGUILayout.LabelField("Current Action:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.LabelField(agent.CurrentAction.Name);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        // Show current plan
        if (agent.ActionPlan != null) {
            EditorGUILayout.LabelField("Plan Stack:", EditorStyles.boldLabel);
            foreach (var a in agent.ActionPlan.Actions) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.LabelField(a.Name);
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();

        // Show beliefs
        EditorGUILayout.LabelField("Beliefs:", EditorStyles.boldLabel);
        if (agent.Beliefs != null) {
            foreach (var belief in agent.Beliefs){
                if (belief.Key is "Nothing" or "Something") continue;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.LabelField(belief.Key + ": " + belief.Value.Evaluate());
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();
    }
}