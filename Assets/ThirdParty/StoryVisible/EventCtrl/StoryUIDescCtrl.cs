using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--UI文字描述--事件的调度
/// 设计时间：2015-10-10
/// </summary>

namespace xxstory
{
    public class StoryUIDescCtrl : StoryBaseCtrl
    {
        //事件相关属性
        public string _DescName = "UIStory/storyShi";
        public string _chidName = "grid";
        public float _time;
        public Vector3 _position;
        public float dwStart;
        private HUIShowPicture mPicture;

        public List<Texture2D> _textures = new List<Texture2D>();

        /// ////////////////功能重写部分-必须实现部分/////////////////////////////////////////////
        public override string luaName
        {
            get { return "StoryUIDescCtrl"; }
        }
        public override string ctrlName
        {
            get { return "描述"; }
        }
        public override void initInfo()
        {
            bWait = false;
            base.initInfo();
            expList.Add("descName");
            expList.Add("childName");
            expList.Add("localPosition");
            expList.Add("time");
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryUIDescCtrl obj = new StoryUIDescCtrl();
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj._baseCtrl = _baseCtrl;
            //////本类事件属性赋值
            obj._DescName = _DescName;
            obj._chidName = _chidName;
            obj._position = _position;
            obj._time = _time;
            return obj;
        }
        public override void Execute()
        {
            dwStart = Time.time;
            if (mPicture == null)
            {
//                 GameObject objRes = ResManager.LoadResource("UI/" + _DescName) as GameObject;
//                 if (objRes == null)
//                 {
//                     Debug.LogWarning("can't find HUIShowPicture in path:" + _DescName);
//                     return;
//                 }
//                 GameObject pChild = NGUITools.AddChild(objStoryUI.backGround.gameObject, objRes);
//                 pChild.gameObject.name = "StoryDesc";
//                 pChild.transform.localPosition = _position;
//                 mPicture = pChild.gameObject.FindInChildren(_chidName).GetComponent<HUIShowPicture>();
                mPicture.Reposition();
            }
            mPicture.showPicture();
        }
        public override void Update()
        {
            if (dwStart == 0) return;
            if (Time.time - dwStart > _time)
            {
                //HideTalkInfo();
                dwStart = 0f;
                OnFinish();
            }
        }
        /// //////////////////属性导出导入部分-有导入导出需求时重写///////////////////////////////////////////////
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "descName":
                    lua.PushString(_DescName);
                    break;
                case "childName":
                    lua.PushString(_chidName);
                    break;
                case "localPosition":
                    LuaExport.Vector3ToStack(lua, _position);
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
            _DescName = EditorGUILayout.TextField("ResName", _DescName);
            _chidName = EditorGUILayout.TextField("childName", _chidName);
            _position = EditorGUILayout.Vector3Field("localPosition", _position);
            _time = EditorGUILayout.FloatField("time", _time);
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////

    }
}