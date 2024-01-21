using UnityEditor;
using UnityEngine;

namespace Core
{
    [CustomEditor(typeof (Actor), true)]
    public class ActorInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Actor actor = (Actor) target;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Health Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            actor.maxHealth = EditorGUILayout.FloatField("Max Health", actor.maxHealth);
            actor.startWithMaxHealth = EditorGUILayout.Toggle("Start with max health?", actor.startWithMaxHealth);
            if (!actor.startWithMaxHealth)
            {
                actor.startingHealth = EditorGUILayout.FloatField("Starting Health", actor.startingHealth);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            EditorGUILayout.LabelField("Health", actor.Health.ToString());
            EditorGUILayout.LabelField("Is Alive", actor.IsAlive.ToString());

            EditorGUILayout.Space();

            if (GUILayout.Button("Take Damage"))
            {
                ((Actor)target).TakeDamage(10f);
            }

            if (GUILayout.Button("Heal"))
            {
                ((Actor)target).Heal(10f);
            }

            if (GUILayout.Button("Die"))
            {
                ((Actor)target).Die();
            }

        }
    }
}