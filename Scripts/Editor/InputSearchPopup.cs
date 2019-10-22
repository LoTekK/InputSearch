using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;

public class InputSearchPopup : PopupWindowContent
{
    public InputSearchPopup(string filter, ref SerializedProperty prop)
    {
        m_Filter = filter;
        m_Property = prop;
    }

    SerializedProperty m_Property;
    Object inputManagerAsset;
    string[] m_Axes;
    string[] m_Filtered;
    string m_Filter;
    int m_Index;
    Vector2 m_ScrollPos;
    float m_ScrollHeight;
    float m_ItemHeight;
    GUIStyle highlight;

    public override void OnOpen()
    {
        inputManagerAsset = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/InputManager.asset");
        var inputManagerObject = new SerializedObject(inputManagerAsset);
        var m_AxesProp = inputManagerObject.FindProperty("m_Axes");
        m_Axes = new string[m_AxesProp.arraySize];
        for (int i = 0; i < m_Axes.Length; i++)
        {
            m_Axes[i] = m_AxesProp.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name").stringValue;
        }
        UpdateFilter();
        m_ItemHeight = EditorGUIUtility.singleLineHeight + GUI.skin.textField.padding.top;
        editorWindow.wantsMouseMove = true;
    }

    private void UpdateFilter()
    {
        m_Filtered = m_Axes.Where(s => Regex.IsMatch(s, Regex.Replace(m_Filter, "\\s", "(.*)?"), RegexOptions.IgnoreCase)).ToArray();
        m_Index = 0;
    }

    private void Select()
    {
        if (m_Index < m_Filtered.Length)
        {
            m_Property.stringValue = m_Filtered[m_Index];
            m_Property.serializedObject.ApplyModifiedProperties();
        }
        editorWindow.Close();
    }

    public override void OnGUI(Rect rect)
    {
        if (highlight == null)
        {
            highlight = new GUIStyle(GUI.skin.label);
            highlight.stretchWidth = true;
            highlight.normal.background = EditorGUIUtility.whiteTexture;
        }
        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                case KeyCode.UpArrow:
                    m_Index--;
                    e.Use();
                    break;
                case KeyCode.DownArrow:
                    m_Index++;
                    e.Use();
                    break;
                case KeyCode.KeypadEnter:
                case KeyCode.Return:
                    Select();
                    break;
                case KeyCode.Escape:
                    if (string.IsNullOrEmpty(m_Filter))
                    {
                        editorWindow.Close();
                    }
                    else
                    {
                        m_Filter = string.Empty;
                        m_Filtered = m_Axes;
                        m_Index = 0;
                    }
                    break;
            }
            m_Index = Mathf.Clamp(m_Index, 0, m_Filtered.Length - 1);
            var selectedPos = m_Index * m_ItemHeight;
            if (selectedPos < m_ScrollPos.y)
            {
                m_ScrollPos.y = selectedPos;
            }
            if (selectedPos > m_ScrollPos.y + m_ScrollHeight - m_ItemHeight)
            {
                m_ScrollPos.y = selectedPos;
            }
        }
        GUI.SetNextControlName("m_Filter");
        using (var scope = new EditorGUI.ChangeCheckScope())
        {
            m_Filter = EditorGUILayout.TextField(m_Filter);
            if (scope.changed)
            {
                UpdateFilter();
            }
        }
        using (var scrollScope = new EditorGUILayout.ScrollViewScope(m_ScrollPos))
        {
            m_ScrollPos = scrollScope.scrollPosition;
            for (int i = 0; i < m_Filtered.Length; i++)
            {
                EditorGUILayout.LabelField(m_Filtered[i]);
                Rect r = GUILayoutUtility.GetLastRect();
                if (i == m_Index)
                {
                    Color c = GUI.color;
                    GUI.color = Color.white * 0.5f;
                    EditorGUI.LabelField(r, "", highlight);
                    GUI.color = c;
                }
                if (r.Contains(e.mousePosition))
                {
                    if (e.type == EventType.MouseMove)
                    {
                        m_Index = i;
                        editorWindow.Repaint();
                    }
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        Select();
                    }
                }
            }
        }
        if (e.type == EventType.Repaint)
        {
            m_ScrollHeight = GUILayoutUtility.GetLastRect().height;
        }
        EditorGUI.FocusTextInControl("m_Filter");
    }
}
