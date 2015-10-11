using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--ITween旋转类--事件的调度
/// 设计时间：2015-08-14
/// </summary>

namespace xxstory
{
    public class StoryTweenRotateCtrl : StoryBaseCtrl
    {
        private class paramInfo
        {
            public float time;
            public int easetype;
            public Vector3 localEulerAngles;
            public Vector3 directRotate;
            public bool bInitRatate;
        }
        //事件相关属性
        public Transform target;
        public float _time;
        public Vector3 _localEulerAngles;
        public bool _bInitRatate;

        private paramInfo _saveInfo;
        private paramInfo _realInfo;
        private Hashtable _paramHash;

        /// ////////////////功能重写部分-必须实现部分/////////////////////////////////////////////
        public override string luaName
        {
            get { return "StoryTweenRotateCtrl"; }
        }
        public override string ctrlName
        {
            get { return "ITweenRotate:"; }
        }
        public override void initInfo()
        {
            _saveInfo = new paramInfo();
            _realInfo = new paramInfo();
            _paramHash = new Hashtable();
            base.initInfo();
            expList.Add("szTarget");
            expList.Add("directRotate");
            expList.Add("localEulerAngles");
            expList.Add("time");
            expList.Add("bInitRatate");
            
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryTweenRotateCtrl obj = new StoryTweenRotateCtrl();
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj._baseCtrl = _baseCtrl;
            //////本类事件属性赋值
            obj.target= target;
            obj._time = _time;
            obj._localEulerAngles = _localEulerAngles;
            obj._bInitRatate = _bInitRatate;
            return obj;
        }
        public override void Execute()
        {
            if (_realInfo.bInitRatate == true)
            {
                target.localEulerAngles = _realInfo.localEulerAngles;
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
            _saveInfo.localEulerAngles = _localEulerAngles;
            _saveInfo.directRotate = target.localEulerAngles;
            _saveInfo.time = _time;
            _saveInfo.bInitRatate = _bInitRatate;
        }
        //重设存储点时可以重写-点击页签AddEvent-重设时调用
        public override void ResetPoint()
        {
            _localEulerAngles = _saveInfo.localEulerAngles;
            target.localEulerAngles = _saveInfo.directRotate;
            _time = _saveInfo.time;
            _bInitRatate = _saveInfo.bInitRatate;
        }
        /// //////////////////属性导出导入部分-有导入导出需求时重写///////////////////////////////////////////////
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "szTarget":
                    lua.PushString(target.name);
                    break;
                case "directRotate":
                    LuaExport.Vector3ToStack(lua, _realInfo.directRotate);
                    break;
                case "localEulerAngles":
                    if (_realInfo.bInitRatate == false)
                        return false;
                    LuaExport.Vector3ToStack(lua, _realInfo.localEulerAngles);
                    break;
                case "time":
                    lua.PushNumber(_realInfo.time);
                    break;
                case "bInitRatate":
                    lua.PushBoolean(_realInfo.bInitRatate);
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

                target.localEulerAngles = EditorGUILayout.Vector3Field("directRotate", target.localEulerAngles);
                
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("c", new GUILayoutOption[]{GUILayout.Height(20), GUILayout.Width(20)}))
            {
                _localEulerAngles = target.transform.localEulerAngles;
            }

            _localEulerAngles = EditorGUILayout.Vector3Field("localRotate", _localEulerAngles);
            GUILayout.EndHorizontal();
            _time = EditorGUILayout.FloatField("time", _time);
            _bInitRatate = GUILayout.Toggle(_bInitRatate, "bInitRatate");
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////
        private void SmoothTarget(Transform sTarget, paramInfo pmInfo)
        {
            _paramHash.Clear();
            _paramHash.Add("time", pmInfo.time);
            _paramHash.Add("rotation", pmInfo.directRotate);
            _paramHash.Add("easetype", iTween.EaseType.linear);
            _paramHash.Add("oncomplete", "OnProxyFinish");
            _paramHash.Add("oncompleteparams", this);
            _paramHash.Add("oncompletetarget", _baseCtrl.objProxy.gameObject);
            
            iTween.RotateTo(sTarget.gameObject, _paramHash);
        }
    }
}