using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--动作类--事件的调度
/// 设计时间：2015-08-28
/// </summary>

namespace xxstory
{
    public class StoryAnimCtrl : StoryBaseCtrl
    {
        private struct paramInfo
        {
            public string AnimName;
            public float speed;
            public int nState;
            public int dwMMethod;
        }
        private paramInfo _realInfo;
        private paramInfo _saveInfo;
        private paramInfo _normalInfo;

        /// ////////////////功能重写部分-必须实现部分/////////////////////////////////////////////
        public override string luaName
        {
            get { return "StoryAnimCtrl"; }
        }
        public override string ctrlName
        {
            get { return "动作"; }
        }
        public override void initInfo()
        {
            _normalInfo.speed = 1.0f;
            _normalInfo.nState = 1;
            base.initInfo();
            expList.Add("AnimName");
            expList.Add("speed");
            expList.Add("nState");
            expList.Add("dwMMethod");
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryAnimCtrl obj = new StoryAnimCtrl();
            obj.time = time;
            obj.bWait = bWait;
            obj.bClick = bClick;
            //////本类事件属性赋值
            obj._normalInfo = _normalInfo;
            return obj;
        }
        public override void Execute()
        {
            Actor target = _shotCtrl.actor.target.GetComponent<Actor>();
            if (target == null)
            {
                Debug.LogWarning("StoryAnimCtrl Execute not have target");
                return;
            }
            Animator anim = target.GetAnimator();
            if (anim)
            {
                anim.SetInteger("nState", _realInfo.nState);
            }
            target.MoveMethod = _realInfo.dwMMethod;
            target.PlayAnim(_realInfo.AnimName, _realInfo.speed, 0, 0);
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
            if (bRealInfo == true)
                _normalInfo = _realInfo;
            else
                _normalInfo = _saveInfo; 
        }
        /// //////////////////属性导出导入部分-有导入导出需求时重写///////////////////////////////////////////////
        protected override bool WidgetWriteOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "AnimName":
                    _normalInfo.AnimName = lua.L_CheckString(-1);
                    break;
                case "speed":
                    _normalInfo.speed = (float)lua.L_CheckNumber(-1);
                    break;
                case "nState":
                    _normalInfo.nState = lua.L_CheckInteger(-1);
                    break;
                case "dwMMethod":
                    _normalInfo.dwMMethod = lua.L_CheckInteger(-1);
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
                case "AnimName":
                    lua.PushString(_realInfo.AnimName);
                    break;
                case "speed":
                    lua.PushNumber(_realInfo.speed);
                    break;
                case "nState":
                    lua.PushInteger(_realInfo.nState);
                    break;
                case "dwMMethod":
                    lua.PushInteger(_realInfo.dwMMethod);
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
            _normalInfo.AnimName = EditorGUILayout.TextField("AnimName", _normalInfo.AnimName);
            _normalInfo.nState = EditorGUILayout.IntField("nState", _normalInfo.nState);
            if (_normalInfo.nState != 1 && _normalInfo.nState != 50)
            {
                _normalInfo.nState = 1;
            }
            _normalInfo.dwMMethod = EditorGUILayout.IntField("dwMMethod", _normalInfo.dwMMethod);
            _normalInfo.speed = EditorGUILayout.FloatField("Speed", _normalInfo.speed);
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////

    }
}