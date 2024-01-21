using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CBG {
    [CustomEditor(typeof(FPSMaterialController), true)]
    public class FPSMaterialControllerEditor : Editor {
        public override void OnInspectorGUI() {
            FPSMaterialController controller = (FPSMaterialController)target;

            DrawDefaultInspector();

            // Don't show management controls during play mode
            if (Application.isPlaying) {
                return;
            }

            if (controller.RendererDataIsStale()) {
                EditorGUILayout.HelpBox("Renderer information is out of date. Please update it.", MessageType.Warning);
                if (GUILayout.Button("Update Renderer Information")) {
                    controller.BuildRendererList();
                    Debug.Log("Renderer information updated.");
                }
            }
        }

    }
}