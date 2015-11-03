using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--ITween摄像机淡入淡出类--事件的调度
/// 设计时间：2015-08-13
/// </summary>

namespace xxstory
{
    public class StoryTweenFadeCtrl : StoryBaseCtrl
    {
        private struct paramInfo
        {
            public int type;//淡入淡出类型：0-淡入，1-淡出
            public float amount;
            public int easetype;
            public Color directColor;
            public bool bReset;
        }
        private paramInfo _saveInfo;
        private paramInfo _realInfo;
        private paramInfo _normalInfo;
        private Hashtable _paramHash;

        /// ////////////////功能重写部分-必须实现部分/////////////////////////////////////////////
        public override string luaName
        {
            get { return "StoryTweenFadeCtrl"; }
        }
        public override string ctrlName
        {
            get { return "淡入淡出:"; }
        }
        public override void initInfo()
        {
            _paramHash = new Hashtable();
            _normalInfo.amount = 1f;
            base.initInfo();
            expList.Add("directColor");
            expList.Add("time");
            expList.Add("dwType");
            expList.Add("amount");
            expList.Add("bReset");
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryTweenFadeCtrl obj = new StoryTweenFadeCtrl();
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj.time = time;
            obj._normalInfo = _normalInfo;
            return obj;
        }
        public override void Execute()
        {
            Texture2D _t2d = iTween.CameraTexture(_realInfo.directColor);
            iTween.CameraFadeAdd(_t2d);
            SmoothTarget(_realInfo);
        }
        public override void OnFinish()
        {
            if (_saveInfo.bReset == true)
                iTween.CameraFadeDestroy();
        }
        public override void ModInfo()
        {
            SavePoint();
            _realInfo = _saveInfo;
        }
        public override void SavePoint()
        {
            _saveInfo = _normalInfo;
        }
        public override void ResetPoint(bool bRealInfo)
        {
            if (bRealInfo)
                _normalInfo = _realInfo;
            else
                _normalInfo = _saveInfo;
        }
        /// //////////////////属性导出导入部分-有导入导出需求时重写///////////////////////////////////////////////
        protected override bool WidgetWriteOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "dwType":
                    _normalInfo.type = lua.L_CheckInteger(-1);
                    break;
                case "directColor":
                    _normalInfo.directColor = LuaExport.GetColor(lua, -1);
                    break;
                case "amount":
                    _normalInfo.amount = (float)lua.L_CheckNumber(-1);
                    break;
                case "bReset":
                    _normalInfo.bReset = lua.ToBoolean(-1);
                    break;
                default:
                    return base.WidgetWriteOper(lua, key);
            }
            return true;
        }
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "dwType":
                    lua.PushInteger(_realInfo.type);
                    break;
                case "directColor":
                    LuaExport.ColorToStack(lua, _realInfo.directColor);
                    break;
                case "amount":
                    lua.PushNumber(_realInfo.amount);
                    break;
                case "bReset":
                    lua.PushBoolean(_realInfo.bReset);
                    break;
                default:
                    return base.WidgetReadOper(lua, key);
            }
            return true;
        }

#if UNITY_EDITOR 
        /// ////////////////UI显示部分-AddEvent页签中创建相应事件UI显示/////////////////////////////////////////////
        public override void OnParamGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("类型 0:当前->指定,1:指定->当前");
            _normalInfo.type = EditorGUILayout.IntField(_normalInfo.type);
            EditorGUILayout.EndHorizontal();
            _normalInfo.directColor = EditorGUILayout.ColorField("directColor", _normalInfo.directColor);
            _normalInfo.amount = EditorGUILayout.FloatField("amount", _normalInfo.amount);
            _normalInfo.bReset = GUILayout.Toggle(_normalInfo.bReset, "bReset");
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////
        private void SmoothTarget(paramInfo pmInfo)
        {
            _paramHash.Clear();
            _paramHash.Add("time", time);
            _paramHash.Add("amount", pmInfo.amount);
            _paramHash.Add("easetype", iTween.EaseType.linear);
//             _paramHash.Add("oncomplete", "OnProxyFinish");
//             _paramHash.Add("oncompleteparams", this);
//             _paramHash.Add("oncompletetarget", _baseCtrl.objProxy.gameObject);
            if (pmInfo.type == 0)
                iTween.CameraFadeTo(_paramHash);
            else if(pmInfo.type == 1)
            {
                iTween.CameraFadeFrom(_paramHash);
            }
            else
            {
                Debug.Log("StoryTweenFadeCtrl not have dwType:" + pmInfo.type);
            }
        }
    }
}