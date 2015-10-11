using UnityEngine;
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
        //事件相关属性
        public string _atlas="";
        public string _img="";
        public string _texture = "";
        public float _time;
        public Vector2 _size;
        public Vector3 _position;
        public float dwStart;
        
        
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
            _size = new Vector2(100, 100);
            bWait = false;
            base.initInfo();
            expList.Add("atlas");
            expList.Add("img");
            expList.Add("texture");
            expList.Add("time");
            expList.Add("size");
            expList.Add("localPosition");
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryPictureCtrl obj = new StoryPictureCtrl();
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj._baseCtrl = _baseCtrl;
            //////本类事件属性赋值
            obj._atlas = _atlas;
            obj._img = _img;
            obj._texture = _texture;
            obj._time = _time;
            obj._size = _size;
            obj._position = _position;
            return obj;
        }
        public override void Execute()
        {
            dwStart = Time.time;
            ShowPicture();
        }
        public override void Update()
        {
            if (dwStart == 0) return;
            if (Time.time - dwStart > _time)
            {
                HidePicture();
                dwStart = 0f;
                OnFinish();
            }
        }
        /// //////////////////属性导出导入部分-有导入导出需求时重写///////////////////////////////////////////////
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "atlas":
                    lua.PushString(_atlas);
                    break;
                case "img":
                    lua.PushString(_img);
                    break;
                case "texture":
                    lua.PushString(_texture);
                    break;
                case "time":
                    lua.PushNumber(_time);
                    break;
                case "localPosition":
                    LuaExport.Vector3ToStack(lua, _position);
                    break;
                case "size":
                    LuaExport.Vector2ToStack(lua, _size);
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
            _atlas = EditorGUILayout.TextField("atlas", _atlas);
            _img = EditorGUILayout.TextField("img", _img);
            _texture = EditorGUILayout.TextField("texture", _texture);
            _time = EditorGUILayout.FloatField("time", _time);
            _size = EditorGUILayout.Vector2Field("size", _size);
            _position = EditorGUILayout.Vector3Field("localPosition", _position);
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////
        public void ShowPicture()
        {
            Debug.Log("StoryPictureCtrl:ShowPicture:"+_texture+":"+ _atlas+":"+ _img);
            if(_texture != "")
            {
                if (mTexture == null)
                {
                    mTexture = NGUITools.AddChild<UITexture>( objStoryUI.backGround);
                    //mTexture.localSize = _size;
                }
                mTexture.transform.localPosition = _position;
                NGUITools.AdjustDepth(mTexture.gameObject, 2);
                mTexture.width = (int)_size.x;
                mTexture.height = (int)_size.y;
                //mTexture.mainTexture = HUIManager.LoadTexture(_texture);
            }
            else
            {
                if (mSprite == null)
                {
                    mSprite = NGUITools.AddChild<UISprite>( objStoryUI.backGround);
                    mSprite.gameObject.name = "storyPicture";
                }
                mSprite.transform.localPosition = _position;
                NGUITools.AdjustDepth(mSprite.gameObject, 2);
                mSprite.width = (int)_size.x;
                mSprite.height = (int)_size.y;
                //mSprite.atlas = HUIManager.LoadAtlas(_atlas);
                mSprite.spriteName = _img;
            }
        }
        public void HidePicture()
        {
            if (mSprite != null)
                GameObject.DestroyObject(mSprite.gameObject);
            else if (mTexture != null)
                GameObject.DestroyObject(mTexture.gameObject);
            return;
        }
    }
}