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

        private class paramInfo
        {
            public string AnimName;
            public float speed;
            public int nState;
        }

        private paramInfo _realInfo;
        private paramInfo _saveInfo;

        //事件相关属性
        public string _AnimName;
        public float _speed;
        public int _nState;
        public Actor target;

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
            _speed = 1.0f;
            _saveInfo = new paramInfo();
            _realInfo = new paramInfo();
            base.initInfo();
            expList.Add("AnimName");
            expList.Add("szTarget");
            expList.Add("speed");
            expList.Add("nState");

        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryAnimCtrl obj = new StoryAnimCtrl();
            obj.bWait = bWait;
            obj._baseCtrl = _baseCtrl;
            //////本类事件属性赋值
            obj.target = target; 
            obj._AnimName = _AnimName;
            obj._speed = _speed;
            obj._nState = _nState;
            return obj;
        }
        public override void Execute()
        {
            if (target == null)
            {
                Debug.LogWarning("StoryAnimCtrl Execute not have target");
                return;
            }
            Animator anim = target.GetAnimator();
            if (anim && _realInfo.nState != 0)
            {
                anim.SetInteger("nState", _realInfo.nState);
            }
            target.PlayAnim(_realInfo.AnimName, _realInfo.speed);
        }
 
        /// ////////////////功能重写部分-需要保存状态时可以重写/////////////////////////////////////////////

        /// 每帧执行一次调用-与MonoBehaviour调用频率相同
//         public override void Update()
//         {
//         }
//         //事件等待结束，结束有特殊需求可以重写-用于bWait=true的事件的结束调用
//         public override void OnFinish()
//         {
//         }
        //修改事件内容可以重写-点击页签AddEvent-修改时调用
        public override void ModInfo()
        {
            SavePoint();
            _realInfo = _saveInfo;
        }
        //保存存储点时可以重写-点击页签AddEvent-存储点时调用
        public override void SavePoint()
        {
            _saveInfo.AnimName = _AnimName;
            _saveInfo.speed = _speed;
            _saveInfo.nState = _nState;
        }
        //重设存储点时可以重写-点击页签AddEvent-重设时调用
        public override void ResetPoint()
        {
            _AnimName = _saveInfo.AnimName;
            _speed = _saveInfo.speed;
            _nState = _saveInfo.nState;
        }
        /// //////////////////属性导出导入部分-有导入导出需求时重写///////////////////////////////////////////////
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "szTarget":
                    lua.PushString(target.name);
                    break;
                case "AnimName":
                    lua.PushString(_realInfo.AnimName);
                    break;
                case "speed":
                    lua.PushNumber(_realInfo.speed);
                    break;
                case "nState":
                    lua.PushInteger(_realInfo.nState);
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
            target = EditorGUILayout.ObjectField(target, typeof(Actor)) as Actor;
            _AnimName = EditorGUILayout.TextField("AnimName", _AnimName);
            _nState = EditorGUILayout.IntField("nState", _nState);
            _speed = EditorGUILayout.FloatField("Speed", _speed);
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////

    }
}