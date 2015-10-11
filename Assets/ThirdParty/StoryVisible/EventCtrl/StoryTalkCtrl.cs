using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--对话类--事件的调度
/// 设计时间：2015-08-14
/// </summary>

namespace xxstory
{
    public class StoryTalkCtrl : StoryBaseCtrl
    {
        //事件相关属性
        public string _talkName;
        public string _talkInfo;
        public float _time;
        public float dwStart;

        /// ////////////////功能重写部分-必须实现部分/////////////////////////////////////////////
        public override string luaName
        {
            get { return "StoryTalkCtrl"; }
        }
        public override string ctrlName
        {
            get { return "对话"; }
        }
        public override void initInfo()
        {
            bWait = false;
            base.initInfo();
            expList.Add("talkName");
            expList.Add("talkInfo");
            expList.Add("time");
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryTalkCtrl obj = new StoryTalkCtrl();
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj._baseCtrl = _baseCtrl;
            //////本类事件属性赋值
            obj._talkName = _talkName;
            obj._talkInfo = _talkInfo;
            obj._time = _time;
            return obj;
        }
        public override void Execute()
        {
            dwStart = Time.time;
            ShowTalkInfo();
        }
        public override void Update()
        {
            if (dwStart == 0) return;
            if (Time.time - dwStart > _time)
            {
                HideTalkInfo();
                dwStart = 0f;
                OnFinish();
            }
        }
        /// //////////////////属性导出导入部分-有导入导出需求时重写///////////////////////////////////////////////
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "talkInfo":
                    lua.PushString(_talkInfo);
                    break;
                case "talkName":
                    lua.PushString(_talkName);
                    break;
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
            _talkName = EditorGUILayout.TextField("talkName-人名", _talkName);
            _talkInfo = EditorGUILayout.TextField("talkInfo-内容", _talkInfo);
            _time = EditorGUILayout.FloatField("time-显示时间", _time);
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////
        public void ShowTalkInfo()
        {
            objStoryUI.talkName.text = _talkName;
            objStoryUI.talkInfo.text = _talkInfo;
        }
        public void HideTalkInfo()
        {
            objStoryUI.talkName.text = "";
            objStoryUI.talkInfo.text = "";
        }
    }
}