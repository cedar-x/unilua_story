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
    [MenuItem("Story Export/Create Story Path")]
    private static void OnCreatePath()
    {
        GameObject obj = new GameObject("New Story Path");
        LuaPathCamera objPath = obj.AddComponent<LuaPathCamera>();
        objPath.Init();
    }
    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    //当前时间轴Ctrl和当前选中事件Ctrl
    private static StoryBaseCtrl _selectCtrl;

    private Hstj.LuaAnimEvent _animEvent;
    private Hstj.LuaAnimEvent _proxy;
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
        _animEvent.Init();
        _proxy = _animEvent;
    }
    //OnGUI功能函数
    private void ShowParam()
    {
        if (_selectCtrl == null) return;
        _selectCtrl.OnParamGUI();
    }
    private void SetCurrent(StoryBaseCtrl objCtrl)
    {
        _selectCtrl = objCtrl;
    }
    ////////////////////////////////////////////////////////
    void OnInspectorUpdate()
    {
        this.Repaint(); // 刷新Inspector
    }
    
    void OnGUI()
    {
   
        OnResetGui(200);
    }

    private bool _bCameraFoldout;
    private int _insertIndex = -1;//使用此参数代表当前时间添加到哪个位置
    public void OnResetGui(int btnWidth)
    {
        //GUILayout.BeginArea(new Rect(0, 0, btnWidth, 500));
        OnExportSetting();
        GUILayout.BeginVertical();
        if (Application.isPlaying)
        {
            if (_proxy != null)
            {
                GUILayout.Label(_proxy.gameObject.name);
                _bCameraFoldout = EditorGUILayout.Foldout(_bCameraFoldout, "摄像机参数实时设置");
                if (_bCameraFoldout)
                {
                    LuaGameCamera objCamera = StoryBaseCtrl.objMainCamera;
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
            }
            else
            {
                GUILayout.Label("请添加实例时间轴");
                if (GUILayout.Button("实例化时间轴"))
                {
                    initAnimEvent();
                }
            }
        }
        else
        {
            GUILayout.Label("请在运行状态下进行相关事件的添加");
            OnInitActor();
            return;
        }

        if (_proxy == null)
            return;
        GUILayout.Label("实例参数");
        if (_proxy._bsCtrl.selectCtrl != null)
        {
            _selectCtrl = _proxy._bsCtrl.selectCtrl;
            _proxy._bsCtrl.selectCtrl = null;
            _selectCtrl.OnParamGUI();
        }
        else if (_selectCtrl != null)
            _selectCtrl.OnParamGUI();
        
        GUILayout.BeginHorizontal();
        _insertIndex = EditorGUILayout.IntField(_insertIndex, GUILayout.Width(30));
        if (GUILayout.Button("添加"))
        {
            StoryBaseCtrl objCtrl = _selectCtrl.CopySelf();
            objCtrl.ModInfo();
            _proxy._bsCtrl.AddEvent(objCtrl.bWait, objCtrl, _insertIndex);
            _selectCtrl = null;
            _insertIndex = -1;
        }
        if (GUILayout.Button("修改"))
        {
            _selectCtrl.ModInfo();
        }
        if (GUILayout.Button("存储点"))
        {
            _selectCtrl.SavePoint();
        }
        if (GUILayout.Button("重设"))
        {
            _selectCtrl.ResetPoint();
        }
        if (GUILayout.Button("放弃"))
        {
            _selectCtrl = null;
            //_proxy._bsCtrl.selectCtrl = null;
        }
        GUILayout.EndHorizontal();
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
        if (GUILayout.Button("交通工具选择", GUILayout.Width(btnWidth)))
        {

        }
        if (GUILayout.Button("交通工具移除", GUILayout.Width(btnWidth)))
        {

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
        if (GUILayout.Button("设置结束", GUILayout.Width(btnWidth)))
        {
            StoryEndCtrl objCtrl = new StoryEndCtrl();
            _selectCtrl = objCtrl;
        }
        if (GUILayout.Button("选项", GUILayout.Width(btnWidth)))
        {
            StoryOptionCtrl objCtrl = new StoryOptionCtrl();
            _selectCtrl = objCtrl;
        }
        GUILayout.Label("效果");
        if (GUILayout.Button("特效", GUILayout.Width(btnWidth)))
        {
            StoryEffectCtrl objCtrl = new StoryEffectCtrl();
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
            SetCurrent(new StoryTimeCtrl());
        }
        GUILayout.EndVertical();
        //GUILayout.EndArea();
    }

    private bool _bExportSetting;
    public void OnExportSetting()
    {
        _bExportSetting = EditorGUILayout.Foldout(_bExportSetting, "导出剧情至lua脚本");
        if (_bExportSetting == true)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("剧情ID：");
            _dwScriptID = EditorGUILayout.IntField(_dwScriptID);
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
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("导出"))
            {
                ExportToScriptFile();
            }
        }
    }

    public class StoryActorInfo
    {
        public string name;
        public string szRoleName;
        public int dwType;
        public string skeleton;
        public Vector3 localPosition;
        public Vector3 localEulerAngles;
        public Vector3 localScale;
        public int dwModelID;
        public bool bFoldout;//没有意义-控制GUI的状态
    }
    
    public Transform _actorPosInfo;
    //public Dictionary<string, StoryActorInfo> _actorList = new Dictionary<string, StoryActorInfo>();
    [SerializeField]
    public List<StoryActorInfo> _actorList = new List<StoryActorInfo>();
    public bool _bFoldout = false; 
    private int _actorIndex = 1;
    [SerializeField]
    private string _actorString = "";
    public void OnInitActor()
    {
        GUILayout.Label("剧情人物初始设定");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("npc添加"))
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogWarning("you have to select at least one object in hierarchy....");
                return;
            }
            foreach (Transform obj in Selection.transforms)
            {
                SetActorInfo(obj);
            }
        }
        if(GUILayout.Button("清空"))
        {
            _actorList.Clear();
            _actorIndex = 1;
        }
        if (GUILayout.Button("保存"))
        {
            //保存人物信息
            SaveActorInfo();
            //导出脚本信息

        }

        GUILayout.EndHorizontal();
        foreach (StoryActorInfo objActor in _actorList)
        {
            EditorGUILayout.BeginHorizontal();
            objActor.bFoldout = EditorGUILayout.Foldout(objActor.bFoldout, objActor.name);
            GUILayout.Space(120);
            if (GUILayout.Button("reset"))
            {
                objActor.localPosition = Selection.activeTransform.localPosition;
                objActor.localEulerAngles = Selection.activeTransform.localEulerAngles;
                objActor.localScale = Selection.activeTransform.localScale;
            }
            if (GUILayout.Button("delete"))
            {
                _actorList.Remove(objActor);
            }
            EditorGUILayout.EndHorizontal();
            if (objActor.bFoldout == true)
            {
                objActor.localPosition = EditorGUILayout.Vector3Field("localPosition", objActor.localPosition);
                objActor.localEulerAngles = EditorGUILayout.Vector3Field("localEulaerAngles", objActor.localEulerAngles);
                objActor.localScale = EditorGUILayout.Vector3Field("localScale", objActor.localScale);
                objActor.dwType = EditorGUILayout.IntField("dwType", objActor.dwType);
                objActor.dwModelID = EditorGUILayout.IntField("dwModelID", objActor.dwModelID);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("name");
                objActor.name = EditorGUILayout.TextField(objActor.name);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("szRoleName");
                objActor.szRoleName = EditorGUILayout.TextField(objActor.szRoleName);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("skeleton");
                objActor.skeleton = EditorGUILayout.TextField(objActor.skeleton);
                EditorGUILayout.EndHorizontal();
            }

        }

  
    }

    public void SetActorInfo(Transform trans)
    {
        StoryActorInfo objActor = new StoryActorInfo();
        objActor.localPosition = trans.localPosition;
        objActor.localEulerAngles = trans.localEulerAngles;
        objActor.localScale = trans.localScale;
        objActor.skeleton = trans.name;
        objActor.dwType = 0;
        if (Application.isPlaying)
            objActor.name = trans.name;
        else
        {
            objActor.name = "Actor" + _actorIndex; 
            ++_actorIndex;
        }
        objActor.szRoleName = "角色名字";
        _actorList.Add(objActor);
    }
    public void AddEmptyActorInfo()
    {
        StoryActorInfo objActor = new StoryActorInfo();
        objActor.localPosition = Vector3.zero;
        objActor.localEulerAngles = Vector3.zero;
        objActor.localScale = Vector3.zero;
        objActor.skeleton = "";
        objActor.dwType = 3;
        objActor.name = "obj_Empty";
        objActor.szRoleName = "";
    }
    public void SaveActorInfo()
    {
        if (_actorList.Count == 0)
        {
            Debug.LogWarning("this is no Actor Info..");
            return;
        }
        _actorString = "tabActor = {\n    {name='obj_Empty', szRoleName = '--', dwType = 3, dwModelID = 0, skeleton = '', localPosition={x=0, y=0, z=0}, localEulerAngles={x=0, y=0, z=0}, localScale={x=1, y=1, z=1} };";
        foreach (StoryActorInfo objActor in _actorList)
        {
            _actorString += string.Format("\n    {{name=\"{0}\", szRoleName = \"{1}\", dwType = {2}, dwModelID = {13}, skeleton = \"{3}\", localPosition={{x={4:N2}, y={5:N2}, z={6:N2}}}, localEulerAngles={{x={7:N2}, y={8:N2}, z={9:N2}}}, localScale={{x={10:N2}, y={11:N2}, z={12:N2}}} }};",
                objActor.name, objActor.szRoleName, objActor.dwType, objActor.skeleton, 
                objActor.localPosition.x, objActor.localPosition.y, objActor.localPosition.z,
                objActor.localEulerAngles.x, objActor.localEulerAngles.y, objActor.localEulerAngles.z,
                objActor.localScale.x, objActor.localScale.y, objActor.localScale.z, objActor.dwModelID);
        }
        _actorString += "\n};";
    } 
    private int _dwScriptID;
    private bool _bHaveOption;
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

#region 剧情脚本各个状态读取
    public string GetBasicString()
    {
        return @"tabBasic = {
    szStageName = '';
    bNewSceneState = true;
    bNewLightState = true;
    dwNextTalkTime = false;
    dwNextTargetTime = false;
    dwType = 1;
    dwVersion = 2;
};";
    }
    public string GetOptionString()
    {
        return "tabOption={};";
    }
    public string GetContentString()
    {
        string con =  @"tabContent = {
    {name = '', dwCamType = 1, dwOpt = 0, dwNextOpt = 0, dwSmooth = 0, dwEventCtrl = 1,target = 'obj_Empty', info = ''};
};";

        return con;
    }
    public string GetCameraString()
    {
        return @"tabCamera = {
    [1] = { distance = 8; offset = {x=0,y=0,z=0}; UDAngle = 0; LRAngle = 90;};
};";
    }
    public string GetSmoothString()
    {
        return "tabSmooth={};";
    }
    public string GetActorString()
    {
        return _actorString;
    }
    public string GetEventString()
    {
        string eventString = "tabEventCtrl = {};";
        if (_animEvent != null)
        {
            eventString = string.Format("tabEventCtrl = {{\n{0}\n}};", _animEvent.ExportProperty(null).Replace("[", "\n["));
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
        return string.Format("_G.StoryScript_{0} = StoryBaseSystem:Create();\nStoryScript_{0}.storyConfig = StoryConfig;", _dwScriptID);
    }
#endregion

#region 剧情整体或者部分导出到文件
    public void ExportToScriptFile()
    {
        string scriptString = string.Format("local StoryConfig = {{\n{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\n{8}\n}}\n{9}",
            GetBasicString(), GetOptionString(),GetContentString(),GetCameraString(), GetSmoothString(),
            GetActorString(), GetEventString(), GetEnvString(), GetLightString(), GetClassString()
                );
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
