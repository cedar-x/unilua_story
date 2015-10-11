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
        private class paramInfo
        {
            public int dwType;//淡入淡出类型：0-淡入，1-淡出
            public float time;
            public float amount;
            public int easetype;
            public Color directColor;
            public bool bReset;
        }
        //事件相关属性
        public int _dwType;
        public float _time;
        public float _amount;
        public Color _directColor;
        public bool _bReset;

        private paramInfo _saveInfo;
        private paramInfo _realInfo;
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
            _saveInfo = new paramInfo();
            _realInfo = new paramInfo();
            _paramHash = new Hashtable();
            _amount = 1f;
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
            obj._baseCtrl = _baseCtrl;
            //////本类事件属性赋值
            obj._time = _time;
            obj._directColor = _directColor;
            obj._dwType = _dwType;
            obj._amount = _amount;
            obj._bReset = _bReset;
            return obj;
        }
        public override void Execute()
        {
            Texture2D _t2d = iTween.CameraTexture(_realInfo.directColor);
            iTween.CameraFadeAdd(_t2d);
            SmoothTarget(_realInfo);
        }
 
        /// ////////////////功能重写部分-需要保存状态时可以重写/////////////////////////////////////////////
        //修改事件内容可以重写-点击页签AddEvent-修改时调用
        public override void ModInfo()
        {
            SavePoint();
            _realInfo = _saveInfo;
        }
        //保存存储点时可以重写-点击页签AddEvent-存储点时调用
        public override void SavePoint()
        {
            _saveInfo.dwType = _dwType;
            _saveInfo.directColor = _directColor;
            _saveInfo.time = _time;
            _saveInfo.amount = _amount;
            _saveInfo.bReset = _bReset;
        }
        //重设存储点时可以重写-点击页签AddEvent-重设时调用
        public override void ResetPoint()
        {
            _dwType = _saveInfo.dwType;
            _directColor = _saveInfo.directColor;
            _time = _saveInfo.time;
            _amount = _saveInfo.amount;
            _bReset = _saveInfo.bReset;
        }
        public override void OnFinish()
        {
            if (_saveInfo.bReset==true)
                iTween.CameraFadeDestroy();
            base.OnFinish();
        }
        /// //////////////////属性导出导入部分-有导入导出需求时重写///////////////////////////////////////////////
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "dwType":
                    lua.PushInteger(_realInfo.dwType);
                    break;
                case "directColor":
                    LuaExport.ColorToStack(lua, _realInfo.directColor);
                    break;
                case "time":
                    lua.PushNumber(_realInfo.time);
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
            _dwType = EditorGUILayout.IntField(_dwType);
            EditorGUILayout.EndHorizontal();
            _directColor = EditorGUILayout.ColorField("directColor", _directColor);
            _time = EditorGUILayout.FloatField("time", _time);
            _amount = EditorGUILayout.FloatField("amount", _amount);
            _bReset = GUILayout.Toggle(_bReset, "结束销毁");
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////
        private void SmoothTarget(paramInfo pmInfo)
        {
            _paramHash.Clear();
            _paramHash.Add("time", pmInfo.time);
            _paramHash.Add("amount", pmInfo.amount);
            _paramHash.Add("easetype", iTween.EaseType.linear);
            _paramHash.Add("oncomplete", "OnProxyFinish");
            _paramHash.Add("oncompleteparams", this);
            _paramHash.Add("oncompletetarget", _baseCtrl.objProxy.gameObject);
            if (pmInfo.dwType == 0)
                iTween.CameraFadeTo(_paramHash);
            else if(pmInfo.dwType == 1)
            {
                iTween.CameraFadeFrom(_paramHash);
            }
            else
            {
                Debug.Log("StoryTweenFadeCtrl not have dwType:" + pmInfo.dwType);
            }
        }
    }
}