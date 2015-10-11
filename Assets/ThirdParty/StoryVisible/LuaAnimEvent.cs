using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UniLua;
using xxstory;

/// <summary>
/// 设计目的：剧情时间轴实例、可以挂载各种事件
/// 设计时间：2015-08-03
/// 作者：xingxuesong
/// </summary>

namespace Hstj
{

    public class LuaAnimEvent : LuaExport
    {
        public StoryBaseCtrl _bsCtrl;
        public List<StoryBaseCtrl> _listCtrl;
        //public Dictionary<string, StoryActorInfo> _actorList = new Dictionary<string, StoryActorInfo>();
        private UIButton _btnStoryNext;
        // Use this for initialization
        public override void Init()
        {
            _bsCtrl = new StoryBaseCtrl();
            _bsCtrl.objProxy = this;
            _listCtrl = _bsCtrl.GetList();
            _btnStoryNext = GameObject.Find("btnStoryNext").GetComponent<UIButton>();
            if (_btnStoryNext != null)
            {
                ILuaState lua = Game.LuaApi;
                lua.GetGlobal("UISotryNoNext");
                lua.PushBoolean(true);
                if (lua.PCall(1, 0, 0) != 0)
                {
                    Debug.LogWarning(lua.ToString(-1));
                    lua.Pop(-1);
                }
                EventDelegate.Add(_btnStoryNext.onClick, onEditorAnimNext);
            }
        }
        private void onEditorAnimNext()
        {
            _bsCtrl.ExecuteEditorCtrl();
        }
        void Update()
        {
            if (_listCtrl == null || _listCtrl.Count == 0) return;
            for (int i = 0; i < _listCtrl.Count; i++)
            {
                StoryBaseCtrl obj = _listCtrl[i];
                obj.Update();
            }
        }
        public void OnProxyFinish(StoryBaseCtrl objCtrl)
        {
            objCtrl.OnFinish();
        }
        /// <summary>
        /// //////////////////属性导出导入与访问相关///////////////////////////////////////////////
        /// </summary>
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "StoryTimeCtrl":
                    
                    break;
                default:
                    return base.WidgetReadOper(lua, key);
            }
            return true;
        }
        protected override bool WidgetWriteOper(ILuaState lua, string key)
        {
            int dwStackIndex = lua.GetTop();
            switch (key)
            {
                case "cookieSize":
                    
                    break;
                default:
                    return base.WidgetWriteOper(lua, key);
            }
            if (dwStackIndex != lua.GetTop())
                Debug.LogWarning("LuaLight:WidgetWriteOper stack Exception:start=" + key + ":" + dwStackIndex + " end=" + lua.GetTop());
            return true;
        }
        private StoryBaseCtrl InstanceEventCtrl(string key)
        {
            switch (key)
            {
                case "StoryTimeCtrl":
                    return new StoryTimeCtrl();
                case "StoryCameraLookCtrl":
                    return new StoryCameraLookCtrl();
            }
            Debug.Log("there is no EventCtrl:"+key);
            return null;
        }
        public override void ImportProperty(int dwIndex)
        {
            //bool bFlag = false;
            _fileLua.PushValue(dwIndex);
            int len = _fileLua.L_Len(-1);
            for (int i = 1; i <= len; i++)
            {
                _fileLua.PushNumber(i);
                _fileLua.GetTable(-2);
                _fileLua.PushString("event");
                _fileLua.GetTable(-2);
                string eventName = _fileLua.L_CheckString(-1);
                _fileLua.Pop(1);
                StoryBaseCtrl objCtrl = InstanceEventCtrl(eventName);
                if (objCtrl != null)
                    objCtrl.ImportProperty(_fileLua, -1);
                _fileLua.Pop(1);
                _bsCtrl.Clear();
                _bsCtrl.AddEvent(objCtrl.bWait, objCtrl);
            }
        }
        public override string ExportProperty(string[] strProperty)
        {
            if (_listCtrl.Count == 0)
            {
                Debug.LogWarning("LuaAnimEvent:there is no any event....");
                return "";
            }
            _fileLua.NewTable();
            for (int i = 0; i < _listCtrl.Count; i++)
            {
                _fileLua.PushNumber(i + 1);
                StoryBaseCtrl objCtrl = _listCtrl[i];
                objCtrl.ExportProperty(_fileLua, -1);
                _fileLua.SetTable(-3);
            }
            string strResult = SerializeTable(-1);
            _fileLua.Pop(1);
            return strResult;
        }
        /// <summary>
        /// /////////////////////////////////////////////////////////////////
        /// </summary>
#if UNITY_EDITOR 
        private string _camTargetName = "null";
        void OnGUI()
        {
            GUI.color = new UnityEngine.Color(255, 0, 0);
            if (_listCtrl != null)
            {
                for (int i = 0; i < _listCtrl.Count; i++)
                {
                    StoryBaseCtrl objCtrl = _listCtrl[i];
                    objCtrl.OnExampleGUI();
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Execute"))
            {
                _bsCtrl.Reset();
                _bsCtrl.Execute();
            }
            if (GUILayout.Button("Clear"))
            {
                _bsCtrl.Clear();
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("ImportLua"))
            {
                ILuaState LuaApi = Game.LuaApi;
                LuaApi.GetGlobal("StoryEventImport");
                if (LuaApi.Type(-1) != LuaType.LUA_TFUNCTION)
                {
                    Debug.LogWarning("not found main function in lua script. can't start game.");
                    LuaApi.Pop(1);
                    return;
                }
                if (LuaApi.PCall(0, 1, 0) != 0)
                {
                    Debug.LogWarning(LuaApi.ToString(-1));
                    LuaApi.Pop(1); 
                }
                if (LuaApi.Type(-1) == LuaType.LUA_TNIL)
                {
                    Debug.LogWarning("importLua failed, there is no event....");
                    return;
                }
                ImportProperty(-1);
                LuaApi.Pop(1);

            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("剧情摄像机目标：");
            if (StoryBaseCtrl.objMainCamera.target == null)
                _camTargetName = "null";
            else
                _camTargetName = StoryBaseCtrl.objMainCamera.target.name;
            GUILayout.Label(_camTargetName);
            GUILayout.EndHorizontal();
        }
#endif
    }
}
