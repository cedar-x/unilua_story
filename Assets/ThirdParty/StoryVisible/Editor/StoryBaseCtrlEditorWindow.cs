using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniLua;
using xxstory;
using Hstj;

/// <summary>
/// 剧情事件添加界面管理接口
/// </summary>
/// 
public class StoryBaseCtrlEditorWindow : EditorWindow
{
    /// <summary>
    /// 打开剧情动画编辑器 用于以某个时间轴为基准的事件窗口显示
    /// </summary>
    /// <param name="bsCtrl">时间轴实例，对bsCtrl进行的事件添加</param>
    [MenuItem("Story Export/StoryWindow")]
    private static void OnStoryWindow()
    {
        StoryBaseCtrlEditorWindow window = (StoryBaseCtrlEditorWindow)EditorWindow.GetWindow(typeof(StoryBaseCtrlEditorWindow), false, "AddEvent");
        window.ShowTab();
    }
    
    //当前时间轴Ctrl和当前选中事件Ctrl
    private static StoryBaseCtrl _selectCtrl;
    private Hstj.LuaAnimEvent _animEvent;

    private int _dwScriptID;
    private bool mbCameraFolderOut;
    private bool mbExportFolderOut;
    private bool _bHaveOption;//---------------------
    private int _insertIndex = -1;//使用此参数代表当前时间添加到哪个位置
    private string szEditorState = "实例参数"; //使用此参数代表当前是待添加事件还是修改事件
    //初始化一个时间轴、目前单个时间轴
    private void initAnimEvent()
    {
        if (_animEvent != null)
        {
            Debug.LogWarning("StoryBaseCtrlEditorWindow: already have a AnimEvent....");
            return;
        }
        GameObject obj = new GameObject("New Anim Event");
        _animEvent = obj.AddComponent<Hstj.LuaAnimEvent>();
        _animEvent.InitMemeber();
    }
    private void drawLine()
    {
        GUILayout.Label("-------------------------------------------------------------------------------------");
    }
    ////////////////////////////////////////////////////////
    void OnInspectorUpdate()
    {
        this.Repaint(); // 刷新Inspector
    }
    void OnGUI()
    {
        OnEventWindow();
    }
    private void OnEventWindow()
    {
        OnExportSetting();
        if (!Application.isPlaying) return;
        if (_animEvent == null)
        {
            if (GUILayout.Button("Create Time Ctrl"))
            {
                initAnimEvent();
            }
        }
        else
        {
            CameraSetting();
            if (_animEvent.objEditorShotCtrl == null)
            {
                GUILayout.Label("please choose storyShot in LuaAnimEvent.");
                return;
            }
            GUILayout.Label("Event Target:"+_animEvent.objEditorShotCtrl.actorName);
            drawLine();
            SingleSetting();
            EventSettting();
        }
    }
    
    public void OnExportSetting()
    {
        mbExportFolderOut = EditorGUILayout.Foldout(mbExportFolderOut, "导出剧情至lua脚本");
        if (mbExportFolderOut == true)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("剧情ID：");
            _dwScriptID = EditorGUILayout.IntField(_dwScriptID);
            if (_animEvent != null)
            {
                bool isClickNext = GUILayout.Toggle(_bHaveOption, "Click");
                if (isClickNext != _bHaveOption)
                {
                    _bHaveOption = isClickNext;
                    ILuaState lua = Game.LuaApi;
                    lua.GetGlobal("UISotryNoNext");
                    lua.PushBoolean(isClickNext);
                    if (lua.PCall(1, 0, 0) != 0)
                    {
                        Debug.LogWarning(lua.ToString(-1));
                        lua.Pop(-1);
                    }
                    //                     if (isClickNext == true)
                    //                     {
                    //                         _animEvent.OnInitAnimNext();
                    //                     }
                    //                     else
                    //                     {
                    //                         _animEvent.OnExitAnimNext();
                    //                     }
                }
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("导出"))
            {
                if (_animEvent == null)
                {
                    Debug.LogWarning("there is no Create Time Ctrl...");
                    return;
                }
                ExportToScriptFile();
            }
        }
    }
    //摄像机参数设置
    private void CameraSetting()
    {
        GUILayout.Label(_animEvent.gameObject.name);
        LuaGameCamera objCamera = StoryBaseCtrl.objMainCamera;
        mbCameraFolderOut = EditorGUILayout.Foldout(mbCameraFolderOut, "摄像机参数实时设置:(目标:"+((objCamera.target!=null)?objCamera.target.name:"nil")+")");
        if (mbCameraFolderOut)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("特写镜头"))
            {
                objCamera.distance = 5;
                objCamera.offset = new Vector3(0f, 1f, 0f);
                objCamera.UDAngle = 0;
                objCamera.LRAnge = 0;
                objCamera.calculatePos(false);
            }
            if (GUILayout.Button("中景镜头"))
            {
                objCamera.distance = 5;
                objCamera.offset = new Vector3(0f, 1f, 0f);
                objCamera.UDAngle = 0;
                objCamera.LRAnge = 0;
                objCamera.calculatePos(false);
            }
            if (GUILayout.Button("远景镜头"))
            {
                objCamera.distance = 5;
                objCamera.offset = new Vector3(0f, 1f, 0f);
                objCamera.UDAngle = 0;
                objCamera.LRAnge = 0;
                objCamera.calculatePos(false);
            }
            GUILayout.EndHorizontal();
            float distance = EditorGUILayout.FloatField("视 点 距 离", objCamera.distance);
            objCamera.offset = EditorGUILayout.Vector3Field("视 点 偏 移", objCamera.offset);
            float UDAngle = EditorGUILayout.FloatField("仰俯 偏转角度", objCamera.UDAngle);
            float LRAngle = EditorGUILayout.FloatField("水平 偏转角度", objCamera.LRAnge);
            if (objCamera.distance != distance)
            {
                objCamera.distance = distance;
                objCamera.calculatePos(false);
            }
            if (objCamera.LRAnge != LRAngle)
            {
                objCamera.LRAnge = LRAngle;
                objCamera.calculatePos(false);
            }
            if (objCamera.UDAngle != UDAngle)
            {
                objCamera.UDAngle = UDAngle;
                objCamera.calculatePos(false);
            }
        }
        drawLine();
    }
    //时间编辑参数
    private void SingleSetting()
    {
        GUILayout.Label("----"+szEditorState+"----");
        if (_animEvent.objEditorShotCtrl._objEditorEventCtrl != null)
        {
            
            _selectCtrl = _animEvent.objEditorShotCtrl._objEditorEventCtrl;
            szEditorState = "修改事件:"+_animEvent.objEditorShotCtrl.actorName+":"+_animEvent.objEditorShotCtrl.indexOf(_selectCtrl);
            //_animEvent.objEditorShotCtrl._objEditorEventCtrl = null;
            _selectCtrl.OnParamGUI();
        }
        else if (_selectCtrl != null)
        {
            szEditorState = "待添加事件";
            _selectCtrl.OnParamGUI();
        }
        drawLine();
        GUILayout.BeginHorizontal();
        _insertIndex = EditorGUILayout.IntField(_insertIndex, GUILayout.Width(30));
        if (GUILayout.Button("添加"))
        {
            if (_selectCtrl == null) return;
            StoryBaseCtrl objCtrl = _selectCtrl.CopySelf();
            objCtrl.ModInfo();
            _animEvent.objEditorShotCtrl.Add(objCtrl, _insertIndex);
            NGUITools.SetDirty(_animEvent);
            _insertIndex = -1;
        }
        if (GUILayout.Button("修改"))
        {
            if (_selectCtrl == null) return;
            _selectCtrl.ModInfo();
            NGUITools.SetDirty(_animEvent);
        }
        if (GUILayout.Button("存储点"))
        {
            if (_selectCtrl == null) return;
            _selectCtrl.SavePoint();
        }
        if (GUILayout.Button("重设"))
        {
            if (_selectCtrl == null) return;
            _selectCtrl.ResetPoint(false);
        }
        if (GUILayout.Button("放弃"))
        {
            if (_selectCtrl != null)
                _selectCtrl.ResetPoint(false);
            _selectCtrl = null;
            if (_animEvent.objEditorShotCtrl != null)
                _animEvent.objEditorShotCtrl._objEditorEventCtrl = null;
        }
        GUILayout.EndHorizontal();
        drawLine();
    }
    //事件列表区
    private void EventSettting()
    {
        int btnWidth = 200;
        GUILayout.Label("人物相关");
        if (GUILayout.Button("位置", GUILayout.Width(btnWidth)))
        {
            StoryPositionCtrl objCtrl = new StoryPositionCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("走动", GUILayout.Width(btnWidth)))
        {
            StoryMoveCtrl objCtrl = new StoryMoveCtrl();
            _selectCtrl = objCtrl;

        }
        if (GUILayout.Button("动作", GUILayout.Width(btnWidth)))
        {
            StoryAnimCtrl objCtrl = new StoryAnimCtrl();
            _selectCtrl = objCtrl;

        }
        GUILayout.Label("界面");
        if (GUILayout.Button("对话", GUILayout.Width(btnWidth)))
        {
            StoryTalkCtrl objCtrl = new StoryTalkCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("图片", GUILayout.Width(btnWidth)))
        {
            StoryPictureCtrl objCtrl = new StoryPictureCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("描述", GUILayout.Width(btnWidth)))
        {
            StoryUIDescCtrl objCtrl = new StoryUIDescCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("背景控制", GUILayout.Width(btnWidth)))
        {
            StoryUIBackCtrl objCtrl = new StoryUIBackCtrl();
            _selectCtrl = objCtrl;
        }
        
//         if (GUILayout.Button("选项", GUILayout.Width(btnWidth)))
//         {
//             StoryOptionCtrl objCtrl = new StoryOptionCtrl();
//             _selectCtrl = objCtrl;
//         }
        GUILayout.Label("效果");
        if (GUILayout.Button("特效", GUILayout.Width(btnWidth)))
        {
            StoryEffectCtrl objCtrl = new StoryEffectCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("音效", GUILayout.Width(btnWidth)))
        {
            StoryMusicCtrl objCtrl = new StoryMusicCtrl();
            _selectCtrl = objCtrl;
        }

        GUILayout.Label("摄像机");
        if (GUILayout.Button("分离", GUILayout.Width(btnWidth)))
        {
            StorySeparateCtrl objCtrl = new StorySeparateCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("合并", GUILayout.Width(btnWidth)))
        {
            StoryCombineCtrl objCtrl = new StoryCombineCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("目标", GUILayout.Width(btnWidth)))
        {
            StoryCameraLookCtrl objCtrl = new StoryCameraLookCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("缓动", GUILayout.Width(btnWidth)))
        {
            StoryCameraSmoothCtrl objCtrl = new StoryCameraSmoothCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("震屏", GUILayout.Width(btnWidth)))
        {
            StoryCameraShakeCtrl objCtrl = new StoryCameraShakeCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("广角设置", GUILayout.Width(btnWidth)))
        {
            StoryCameraFovCtrl objCtrl = new StoryCameraFovCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("淡入淡出", GUILayout.Width(btnWidth)))
        {
            StoryTweenFadeCtrl objCtrl = new StoryTweenFadeCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("画面去色", GUILayout.Width(btnWidth)))
        {
            StoryGrayscaleCtrl objCtrl = new StoryGrayscaleCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("蒙太奇", GUILayout.Width(btnWidth)))
        {
            StoryMontageCtrl objCtrl = new StoryMontageCtrl();
            _selectCtrl = objCtrl;
        }
        GUILayout.Label("缓动变换相关");
        if (GUILayout.Button("直线变换", GUILayout.Width(btnWidth)))
        {
            StoryTweenMoveCtrl objCtrl = new StoryTweenMoveCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("旋转变换", GUILayout.Width(btnWidth)))
        {
            StoryTweenRotateCtrl objCtrl = new StoryTweenRotateCtrl();
            _selectCtrl = objCtrl;
        }
        GUILayout.Label("时间");
        if (GUILayout.Button("等待", GUILayout.Width(btnWidth)))
        {
            _selectCtrl = new StoryTimeCtrl();
        }
    }

    #region StoryExportInfo
    public int dwScriptID
    {
        get
        {
            return _dwScriptID;
        }
        set
        {
            _dwScriptID = value;
        }
    }
    public string GetBasicString()
    {
        return _animEvent.ExportBasicInfo();
    }
    public string GetActorString()
    {
        return _animEvent.ExportActorInfo();
    }
    public string GetEventString()
    {
        string eventString = "tabEventCtrl = {};";
        if (_animEvent != null)
        {
            eventString = string.Format("tabEventCtrl = {{{0}\n}};", _animEvent.ExportProperty(null).Replace("[", "\n["));
        }
        return eventString;
    }
    public string GetEnvString()
    {
        return "tabEnv = {};";
    }
    public string GetLightString()
    {
        return "tabLight = {};";
    }
    public string GetClassString()
    {
        return string.Format("_G.StoryScript_{0} = StoryBaseSystem:Create();\nStoryScript_{0}.storyConfig = StoryConfig;\nfunction StoryScript_{0}:OnAfterInitStory() end;\nfunction StoryScript_{0}:OnBeforeEndStory() end;", _dwScriptID);
    }
    public void ExportToScriptFile()
    {
        string scriptString = string.Format("local StoryConfig = {{\n{0}\n{1}\n{2}\n{3}\n{4}\n}}\n{5}",
            GetBasicString(),GetActorString(), GetEventString(), GetEnvString(), GetLightString(), GetClassString());
        string path = Application.streamingAssetsPath + "//ScriptConfig//StoryConfig//StoryScript_" + dwScriptID.ToString() + ".lua";
        if (!File.Exists(path))
        {
            FileStream fstream = new FileStream(path, FileMode.CreateNew);
            StreamWriter sw = new StreamWriter(fstream);
            sw.WriteLine(scriptString);
            sw.Close();
            sw.Dispose();
            Debug.Log("剧情脚本" + dwScriptID.ToString() + "已经导出完毕......");
        }
        else
        {
            if (EditorUtility.DisplayDialog("文件已经存在", "是否覆盖", "确认", "取消"))
            {
                FileStream fstream = new FileStream(path, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fstream);
                sw.WriteLine(scriptString);
                sw.Close();
                sw.Dispose();
                Debug.Log("文件覆盖已经完成：");
            }
        }
    }
    #endregion

    /*
    //更新
    void Update()
    {

    }

    void OnFocus()
    {
        Debug.Log("当窗口获得焦点时调用一次");
    }

    void OnLostFocus()
    {
        Debug.Log("当窗口丢失焦点时调用一次");
    }

    void OnHierarchyChange()
    {
        Debug.Log("当Hierarchy视图中的任何对象发生改变时调用一次");
    }

    void OnProjectChange()
    {
        Debug.Log("当Project视图中的资源发生改变时调用一次");
    }

    void OnInspectorUpdate()
    {
        //Debug.Log("窗口面板的更新");
        //这里开启窗口的重绘，不然窗口信息不会刷新
        this.Repaint();
    }

    void OnSelectionChange()
    {
        //当窗口出去开启状态，并且在Hierarchy视图中选择某游戏对象时调用
        foreach (Transform t in Selection.transforms)
        {
            //有可能是多选，这里开启一个循环打印选中游戏对象的名称
            Debug.Log("OnSelectionChange" + t.name);
        }
    }

    void OnDestroy()
    {
        Debug.Log("当窗口关闭时调用");
    }
     * */
}
