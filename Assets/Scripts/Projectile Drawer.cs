using UnityEditor;
using UnityEngine;
using IMVR.BodyArcade.Neuromania.Input;
using IMVR.BodyArcade.Neuromania.Signals;

namespace IMVR.BodyArcade.Neuromania.Editor
{
    [CustomPropertyDrawer(typeof(ProjectileSpawnManager.Projectile))]
    internal class ProjectileDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var splineContainer = property.FindPropertyRelative("_splineContainer");
            var signalPrefab = property.FindPropertyRelative("_signalPrefab");
            var button = property.FindPropertyRelative("_button");
            var buttonEventType = property.FindPropertyRelative("_buttonEventType");
            var spawnRate = property.FindPropertyRelative("_spawnRate");
            var minSpawnRate = property.FindPropertyRelative("_minSpawnRate");
            var maxSpawnRate = property.FindPropertyRelative("_maxSpawnRate");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float y = position.y;

            EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), splineContainer);
            y += lineHeight + spacing;

            EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), signalPrefab);
            y += lineHeight + spacing;

            EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), button);
            y += lineHeight + spacing;

            EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), buttonEventType);
            y += lineHeight + spacing;

            var eventType = (ButtonEventType)buttonEventType.enumValueIndex;

            switch (eventType)
            {
                case ButtonEventType.OnHeld:
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), spawnRate);
                    break;

                case ButtonEventType.OnHeldWithPercentage:
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), minSpawnRate);
                    y += lineHeight + spacing;
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), maxSpawnRate);
                    break;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var buttonEventType = property.FindPropertyRelative("_buttonEventType");
            var eventType = (ButtonEventType)buttonEventType.enumValueIndex;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            int extraLines = eventType switch
            {
                ButtonEventType.OnHeld => 1,
                ButtonEventType.OnHeldWithPercentage => 2,
                _ => 0
            };

            // 4 base fields: splineContainer, signalPrefab, button, buttonEventType
            return (4 + extraLines) * (lineHeight + spacing);
        }
    }
}