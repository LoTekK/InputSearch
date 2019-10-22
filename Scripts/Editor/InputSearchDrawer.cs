using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(InputSearchAttribute))]
public class InputSearchDrawer : PropertyDrawer
{
    Object inputManagerAsset;
    string[] m_Axes;
    GUIStyle buttonStyle;

    string Init(string filter)
    {
        inputManagerAsset = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/InputManager.asset");
        var inputManagerObject = new SerializedObject(inputManagerAsset);
        var m_AxesProp = inputManagerObject.FindProperty("m_Axes");
        m_Axes = new string[m_AxesProp.arraySize];
        for (int i = 0; i < m_Axes.Length; i++)
        {
            m_Axes[i] = m_AxesProp.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name").stringValue;
        }
        if (string.IsNullOrEmpty(filter))
        {
            return m_Axes[0];
        }
        return filter;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!inputManagerAsset)
        {
            property.stringValue = Init(property.stringValue);
            property.serializedObject.ApplyModifiedProperties();
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.alignment = TextAnchor.MiddleLeft;
        }
        Rect labelRect = new Rect(position);
        labelRect.width = EditorGUIUtility.labelWidth;
        EditorGUI.LabelField(labelRect, property.displayName);
        Rect fieldRect = new Rect(position);
        fieldRect.xMin = EditorGUIUtility.labelWidth;
        if (GUI.Button(fieldRect, property.stringValue, buttonStyle))
        {
            PopupWindow.Show(fieldRect, new InputSearchPopup(property.stringValue, ref property));
        }
    }
}
