#if UNITY_EDITOR
using Npcs.Sensors;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PatrolPointManager<>), true)]
    public class MultiTargetTypeSensorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);

            if (GUILayout.Button("Spawn Target", GUILayout.Height(40)))
            {
                var sensor = target as MonoBehaviour;
                var method = sensor.GetType().GetMethod("SpawnTarget");
                method?.Invoke(sensor, null);
            }
        }
    }
}
#endif