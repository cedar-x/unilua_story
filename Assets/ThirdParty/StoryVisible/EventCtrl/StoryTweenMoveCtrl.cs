using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--ITween移动类--事件的调度
/// 设计时间：2015-08-13
/// </summary>

namespace xxstory
{
    public class StoryTweenMoveCtrl : StoryBaseCtrl
    {
        private class paramInfo
        {
            public float time;
            public int easetype;
            public Vector3 localPosition;
            public Vector3 directPos;
            public bool bInitPosition;
        }
        //事件相关属性
        public Transform target;
        public float _time;
        public Vector3 _localPosition;
        public bool _bInitPosition;

        private paramInfo _saveInfo;
        private paramInfo _realInfo;
        private Hashtable _paramHash;

        /// ////////////////功能重写部分-必须实现部分/////////////////////////////////////////////
        public override string luaName
        {
            get { return "StoryTweenMoveCtrl"; }
        }
        public override string ctrlName
        {
            get { return "ITweenMove:"; }
        }
        public override void initInfo()
        {
            _saveInfo = new paramInfo();
            _realInfo = new paramInfo();
            _paramHash = new Hashtable();
            base.initInfo();
            expList.Add("szTarget");
            expList.Add("directPos");
            expList.Add("localPosition");
            expList.Add("time");
            expList.Add("bInitPosition");
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryTweenMoveCtrl obj = new StoryTweenMoveCtrl();
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj._baseCtrl = _baseCtrl;
            //////本类事件属性赋值
            obj.target= target;
            obj._time = _time;
            obj._localPosition = _localPosition;
            obj._bInitPosition = _bInitPosition;
            return obj;
        }
        public override void Execute()
        {
            if (_realInfo.bInitPosition == true)
            {
                target.localPosition = _realInfo.localPosition;
            }
            SmoothTarget(target, _realInfo);
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
            _saveInfo.localPosition = _localPosition;
            _saveInfo.directPos = target.localPosition;
            _saveInfo.time = _time;
            _saveInfo.bInitPosition = _bInitPosition;
        }
        //重设存储点时可以重写-点击页签AddEvent-重设时调用
        public override void ResetPoint()
        {
            _localPosition = _saveInfo.localPosition;
            target.localPosition = _saveInfo.directPos;
            _time = _saveInfo.time;
            _bInitPosition = _saveInfo.bInitPosition;
        }
        /// //////////////////属性导出导入部分-有导入导出需求时重写///////////////////////////////////////////////
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "szTarget":
                    lua.PushString(target.name);
                    break;
                case "directPos":
                    LuaExport.Vector3ToStack(lua, _realInfo.directPos);
                    break;
                case "localPosition":
                    if (_realInfo.bInitPosition == false)
                        return false;
                    LuaExport.Vector3ToStack(lua, _realInfo.localPosition);
                    break;
                case "time":
                    lua.PushNumber(_realInfo.time);
                    break;
                case "bInitPosition":
                    lua.PushBoolean(_realInfo.bInitPosition);
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
            target = EditorGUILayout.ObjectField(target, typeof(Transform)) as Transform;
            if (target != null)
            {

                target.localPosition = EditorGUILayout.Vector3Field("directPos", target.localPosition);
                
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("c", new GUILayoutOption[]{GUILayout.Height(20), GUILayout.Width(20)}))
            {
                _localPosition = target.transform.localPosition;
            }
            
            _localPosition = EditorGUILayout.Vector3Field("localPosition", _localPosition);
            GUILayout.EndHorizontal();
            _time = EditorGUILayout.FloatField("time", _time);
            _bInitPosition = GUILayout.Toggle(_bInitPosition, "bInitPosition");
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////
        private void SmoothTarget(Transform sTarget, paramInfo pmInfo)
        {
            _paramHash.Clear();
            _paramHash.Add("time", pmInfo.time);
            _paramHash.Add("position", pmInfo.directPos);
            _paramHash.Add("easetype", iTween.EaseType.linear);
            _paramHash.Add("oncomplete", "OnProxyFinish");
            _paramHash.Add("oncompleteparams", this);
            _paramHash.Add("oncompletetarget", _baseCtrl.objProxy.gameObject);
            
            iTween.MoveTo(sTarget.gameObject, _paramHash);
        }
    }
}