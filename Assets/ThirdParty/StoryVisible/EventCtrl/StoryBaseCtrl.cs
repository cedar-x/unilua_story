using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniLua;
using Hstj;

#if UNITY_EDITOR 
    using UnityEditor;
#endif


/// <summary>
/// 设计目的：时间管理基类-用于事件的调度
/// 设计时间：2015-08-03
/// 作者：xingxuesong
/// </summary>
/// 
namespace xxstory
{
    public class StoryBaseCtrl
    {
        public class storyUI
        {
            public UILabel talkName;
            public UILabel talkInfo;
            public GameObject backGround;
        }
        private List<StoryBaseCtrl> _listCtrl = new List<StoryBaseCtrl>();
        private int _index = 0;
        private static LuaGameCamera _gameCamera;
        private LuaAnimEvent _animEvent;
        protected StoryBaseCtrl _baseCtrl = null;
        public bool bWait = false;
        public bool bClick = false;
        protected List<string> expList;
        private static storyUI _storyUI = null;
        private StoryBaseCtrl _selectCtrl;
        //////////////////虚函数////////////////////////
        public StoryBaseCtrl()
        {
            expList = new List<string>();
            bWait = false;
            bClick = false;
            initInfo();
        }
        public virtual void initInfo()
        {
            expList.Clear();
            expList.Add("bWait");
            expList.Add("event");
            expList.Add("bClick");
        }
        //路由lua中对应功能的类名称-导出lua生成类实例或者标识类名
        public virtual string luaName
        {
            get
            {
                return "StoryEventCtrl";
            }
        }
        //用于可视化调节窗口名字显示-OnGUI窗口标识事件名称，易于理解
        public virtual string ctrlName
        {
            get
            {
                return "baseCtrl";
            }
        }
        public virtual int index
        {
            get
            {
                return _baseCtrl._listCtrl.IndexOf(this);
            }
        }
        public virtual void Execute()
        {
            
            for (int i = _index; i < _listCtrl.Count; i++)
            {
                StoryBaseCtrl childCtrl = _listCtrl[i];
//                 if (i >= 1)
//                 {
//                     StoryBaseCtrl preCtrl = _listCtrl[i - 1];
//                     if (preCtrl != null && preCtrl.bWait == false)
//                     {
//                         preCtrl.OnFinish();
//                     }
//                 }
                SetIndex(i+1);
                Debug.Log("storyBaseCtrl:Execute:" + i+":childCtrl index:"+childCtrl.index+" luaName:"+childCtrl.luaName);
                childCtrl.Execute();
                if (childCtrl.bClick == true || childCtrl.bWait == true)
                {
                    return;
                }
            }
            return;
        }
        public virtual void Update()
        {

        }
        public virtual void OnFinish()
        {
            Debug.Log("StoryBase.OnFinish index:"+index+" :"+ luaName + " bWait:"+bWait+":"+bClick+" :" + Time.time);
            //_baseCtrl.SetIndex(index + 1);
            if (bClick == true) return;
            if (bWait == true)
            {
                this._baseCtrl.Execute();
            }
        }
        public virtual void ModInfo()
        {

        }
        public virtual void SavePoint()
        {

        }
        public virtual void ResetPoint()
        {

        }
        public virtual StoryBaseCtrl CopySelf()
        {
            StoryBaseCtrl obj = new StoryBaseCtrl();
            obj.bWait = bWait;
            obj._baseCtrl = _baseCtrl;
            obj.bClick = bClick;
            return obj;
        }
#if UNITY_EDITOR
        public virtual StoryBaseCtrl selectCtrl
        {
            get
            {
                return _selectCtrl;
            }
            set
            {
                _selectCtrl = value;
            }
        }
        public virtual void OnParamGUI()
        {
            bWait = GUILayout.Toggle(bWait, "bWait");
            bClick = GUILayout.Toggle(bClick, "bClick");
        }
        public virtual void OnExampleGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(index.ToString()+":"+ctrlName);
            if (GUILayout.Button("Edit"))
            {
                ResetPoint();
                _baseCtrl.selectCtrl = this;

            }
            if (GUILayout.Button("Delete"))
            {
                Debug.Log("StoryBaseCtrl:Delete:" + index);
                RemoveEvent(index);
            }
            if (GUILayout.Button("Execute"))
            {
                Execute();
            }
            GUILayout.EndHorizontal();
        }

        public static void OnCameraInfoGUI(ref normalInfo norInfo, string expect = "", bool bCopy = true)
        {
            EditorGUILayout.BeginHorizontal();
            if (bCopy == true)
            {
                if (GUILayout.Button("P"))
                {
                    norInfo.distance = objMainCamera.distance;
                    norInfo.offSet = objMainCamera.offset;
                    norInfo.rotationLR = objMainCamera.LRAnge;
                    norInfo.rotationUD = objMainCamera.UDAngle;
                }
            }
            if (!expect.Contains("distance"))
                norInfo.distance = EditorGUILayout.FloatField("视 点 距 离", norInfo.distance);
            EditorGUILayout.EndHorizontal();
            if (!expect.Contains("offset"))
                norInfo.offSet = EditorGUILayout.Vector3Field("视 点 偏 移", norInfo.offSet); 
            if (!expect.Contains("rotationUD"))
                norInfo.rotationUD = EditorGUILayout.FloatField("仰俯 偏转角度", norInfo.rotationUD);
            if (!expect.Contains("rotationLR"))
                norInfo.rotationLR = EditorGUILayout.FloatField("水平 偏转角度", norInfo.rotationLR);
 
        }
#endif
        /// <summary>
        /// //////////////////属性导出导入与访问相关///////////////////////////////////////////////
        /// </summary>
        protected virtual bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "bWait":
                    lua.PushBoolean(bWait);
                    break;
                case "event":
                    lua.PushString(luaName);
                    break;
                case "bClick":
                    lua.PushBoolean(bClick);
                    break;
                default:
                    return false;
            }
            return true;
        }
        protected virtual bool WidgetWriteOper(ILuaState lua, string key)
        {
            int dwStackIndex = lua.GetTop();
            switch (key)
            {
                case "bWait":
                    bWait = lua.ToBoolean(3);
                    break;
                case "bClick":
                    bClick = lua.ToBoolean(3);
                    break;
                default:
                    return false;
            }
            if (dwStackIndex != lua.GetTop())
                Debug.LogWarning("StoryBaseCtrl:WidgetWriteOper stack Exception:start=" + key + ":" + dwStackIndex + " end=" + lua.GetTop());
            return true;
        }

        public virtual void ExportProperty(ILuaState lua, int index)
        {
            lua.NewTable();
            bool bSet = false;
            foreach (string key in expList)
            {
                bSet = WidgetReadOper(lua, key);
                if (bSet == true)
                {
                    lua.SetField(-2, key);
                }
            }
        }
        public virtual void ImportProperty(ILuaState lua, int index)
        {
            bool bFlag = false;
            lua.PushValue(index);
            lua.PushNil();
            while (lua.Next(-2))
            {
                string key = lua.L_CheckString(-2);
                bFlag = WidgetWriteOper(lua, key);
                if (bFlag == false)
                    Debug.LogWarning(luaName + " can't write key:" + key + " please check....");
                lua.Pop(1);
            }
            lua.Pop(1);
        }
        //////////////////公开属性////////////////////////


        //////////////////私有函数////////////////////////

        //////////////////公开函数////////////////////////
        public static LuaGameCamera objMainCamera 
        {
            get
            {
                if (_gameCamera == null)
                {
                    GameObject obj = GameObject.FindGameObjectWithTag("JQCamera");
                    _gameCamera = obj.GetComponent<LuaGameCamera>();
                }
                return _gameCamera;
            }
            set
            {
                _gameCamera = value;
            }
        }
        public static storyUI objStoryUI
        {
            get
            {
                if (_storyUI == null)
                {
                    _storyUI = new storyUI();
                    _storyUI.talkName = GameObject.Find("szStoryName").GetComponent<UILabel>();
                    _storyUI.talkInfo = GameObject.Find("szStoryTalkInfo").GetComponent<UILabel>();
                    _storyUI.backGround = GameObject.Find("storyBkGround");
                }
                return _storyUI;
            }
        }
        public LuaAnimEvent objProxy
        {
            get
            {
                return _animEvent;
            }
            set
            {
                _animEvent = value;
            }
        }
        public void AddEvent(bool bWait, StoryBaseCtrl bsCtrl, int index = -1)
        {
            bsCtrl.bWait = bWait;
            bsCtrl._baseCtrl = this;
            if (index == -1)
                _listCtrl.Add(bsCtrl);
            else
            {
                if (index < 0 || index > _listCtrl.Count)
                {
                    Debug.LogWarning("AddEvent index is out of range:count=" + _listCtrl.Count + " index=" + index);
                    return;
                }
                _listCtrl.Insert(index, bsCtrl);
            }
        }
        public void RemoveEvent(int index)
        {
            _baseCtrl._listCtrl.RemoveAt(index);
        }
        public List<StoryBaseCtrl> GetList()
        {
            return _listCtrl;
        }
        public void Clear()
        {
            _listCtrl.Clear();
            _index = 0;
        }
        public void Reset()
        {
            _index = 0;
        }
        public void SetIndex(int index)
        {
            _index = index;
        }
        public void ExecuteEditorCtrl()
        {
            StoryBaseCtrl objCtrl = _listCtrl[_index-1];
            Debug.Log("ExecuteEditorCtrl:" + _index+":"+objCtrl.bClick);
            if (objCtrl.bClick == true)
            {
                objCtrl._baseCtrl.Execute();
            }
            return;
        }
        //////////////////公开Lua调用/////////////////////

    }
}
