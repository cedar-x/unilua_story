﻿using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--图片显示类--事件的调度
/// 设计时间：2015-10-19
/// </summary>

namespace xxstory
{
    public class StoryPictureCtrl : StoryBaseCtrl
    {
        private struct paramInfo
        {
            public string atlas;
            public string sprite;
            public string texture;
            public int depth;
            public Vector3 size;
            public Vector3 localPosition;
        }
        private paramInfo _saveInfo;
        private paramInfo _realInfo;
        private paramInfo _normalInfo;
        
        private UITexture mTexture;
        private UISprite mSprite;
        /// ////////////////功能重写部分-必须实现部分/////////////////////////////////////////////
        public override string luaName
        {
            get { return "StoryPictureCtrl"; }
        }
        public override string ctrlName
        {
            get { return "图片"; }
        }
        public override void initInfo()
        {
            _normalInfo.size = new Vector2(100, 100);
            _normalInfo.depth = 5;
            _normalInfo.atlas = "";
            _normalInfo.sprite = "";
            _normalInfo.texture = "";
            base.initInfo();
            expList.Add("atlas");
            expList.Add("sprite");
            expList.Add("texture");
            expList.Add("depth");
            expList.Add("size");
            expList.Add("localPosition");
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryPictureCtrl obj = new StoryPictureCtrl();
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj.time = time;
            obj._normalInfo = _normalInfo;
            return obj;
        }
        public override void Execute()
        {
            ShowPicture();
        }
        public override void OnFinish()
        {
            HidePicture();
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
                case "atlas":
                    _normalInfo.atlas = lua.L_CheckString(-1);
                    break;
                case "sprite":
                    _normalInfo.sprite = lua.L_CheckString(-1);
                    break;
                case "texture":
                    _normalInfo.texture = lua.L_CheckString(-1);
                    break;
                case "localPosition":
                    _normalInfo.localPosition = LuaExport.GetVector3(lua, -1);
                    break;
                case "size":
                    _normalInfo.size = LuaExport.GetVector2(lua, -1);
                    break;
                case "depth":
                    _normalInfo.depth = lua.L_CheckInteger(-1);
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
                case "atlas":
                    lua.PushString(_realInfo.atlas);
                    break;
                case "sprite":
                    lua.PushString(_realInfo.sprite);
                    break;
                case "texture":
                    lua.PushString(_realInfo.texture);
                    break;
                case "localPosition":
                    LuaExport.Vector3ToStack(lua, _realInfo.localPosition);
                    break;
                case "size":
                    LuaExport.Vector2ToStack(lua, _realInfo.size);
                    break;
                case "depth":
                    lua.PushInteger(_realInfo.depth);
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
            _normalInfo.atlas = EditorGUILayout.TextField("Atlas", _normalInfo.atlas);
            _normalInfo.sprite = EditorGUILayout.TextField("Sprite", _normalInfo.sprite);
            _normalInfo.texture = EditorGUILayout.TextField("Texture", _normalInfo.texture);
            _normalInfo.depth = EditorGUILayout.IntField("depth", _normalInfo.depth);
            _normalInfo.size = EditorGUILayout.Vector2Field("size", _normalInfo.size);
            _normalInfo.localPosition = EditorGUILayout.Vector3Field("localPosition", _normalInfo.localPosition);
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////
        public void ShowPicture()
        {
            Debug.Log("ShowPicture:" + _realInfo.texture + ":" + _realInfo.atlas + ":" + _realInfo.sprite);
            if(_realInfo.texture != "")
            {
                if (mTexture == null)
                {
                    mTexture = NGUITools.AddChild<UITexture>( objStoryUI.backGround);
                }
                mTexture.transform.localPosition = _realInfo.localPosition;
                NGUITools.AdjustDepth(mTexture.gameObject, _realInfo.depth);
                mTexture.width = (int)_realInfo.size.x;
                mTexture.height = (int)_realInfo.size.y;
                mTexture.mainTexture = HUIManager.LoadTexture(_realInfo.texture);
            }
            else
            {
                if (mSprite == null)
                {
                    mSprite = NGUITools.AddChild<UISprite>(objStoryUI.backGround);
                    mSprite.gameObject.name = "storyPicture";
                }
                mSprite.transform.localPosition = _realInfo.localPosition;
                NGUITools.AdjustDepth(mSprite.gameObject, _realInfo.depth);
                mSprite.width = (int)_realInfo.size.x;
                mSprite.height = (int)_realInfo.size.y;
                mSprite.atlas = HUIManager.LoadAtlas(_realInfo.atlas);
                mSprite.spriteName = _realInfo.sprite;
            }
        }
        public void HidePicture()
        {
            if (mSprite != null)
                GameObject.DestroyObject(mSprite.gameObject);
            else if (mTexture != null)
                GameObject.DestroyImmediate(mTexture.gameObject);
            return;
        }
    }
}