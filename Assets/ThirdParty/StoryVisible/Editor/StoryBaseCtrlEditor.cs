using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Inspector 显示LuaGameCamera设置参数，方便可视化配置参数
/// </summary>
[CustomEditor(typeof(Hstj.LuaAnimEvent))]
public class StoryBaseCtrlEditor : Editor
{
    Hstj.LuaAnimEvent model;
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        model = target as Hstj.LuaAnimEvent;
        if (model._listCtrl != null)
        {
            for (int i = 0; i < model._listCtrl.Count; i++)
            {
                xxstory.StoryBaseCtrl objCtrl = model._listCtrl[i];
                objCtrl.OnExampleGUI();
            }
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Execute"))
        {
            model._bsCtrl.Reset();
            model._bsCtrl.Execute();
        }
        if (GUILayout.Button("Clear"))
        {
            model._bsCtrl.Clear();
        }
        GUILayout.EndHorizontal();
    }
}
