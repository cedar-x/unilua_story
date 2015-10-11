using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--阶段结束类--事件的调度
/// 设计时间：2015-09-10
/// </summary>

namespace xxstory
{
    public class StoryEndCtrl : StoryBaseCtrl
    {
        //事件相关属性
        public float _time;

        /// ////////////////功能重写部分-必须实现部分/////////////////////////////////////////////
        public override string luaName
        {
            get { return "StoryEndCtrl"; }
        }
        public override string ctrlName
        {
            get { return "结束"; }
        }
        public override void initInfo()
        {
            _time = -1;
            bWait = false;
            base.initInfo();
            expList.Add("time");
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryEndCtrl obj = new StoryEndCtrl();
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj._baseCtrl = _baseCtrl;
            //////本类事件属性赋值
            obj._time= _time;
            return obj;
        }
        public override void Execute()
        {
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
//         //修改事件内容可以重写-点击页签AddEvent-修改时调用
//         public override void ModInfo()
//         {
//         }
//         //保存存储点时可以重写-点击页签AddEvent-存储点时调用
//         public override void SavePoint()
//         {
//         }
//         //重设存储点时可以重写-点击页签AddEvent-重设时调用
//         public override void ResetPoint()
//         {
//         }
        /// //////////////////属性导出导入部分-有导入导出需求时重写///////////////////////////////////////////////
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "time":
                    lua.PushNumber(_time);
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
            _time = EditorGUILayout.FloatField("time", _time);
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////

    }
}