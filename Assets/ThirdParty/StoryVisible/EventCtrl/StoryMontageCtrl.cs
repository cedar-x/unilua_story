using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--蒙太奇效果类--事件的调度
/// 设计时间：2015-08-03
/// </summary>

namespace xxstory
{
    public class StoryMontageCtrl : StoryBaseCtrl
    {
        private class paramInfo
        {
            public float time;
            public string spName;
            public float distance;
        }
        private paramInfo _saveInfo;
        private paramInfo _realInfo;
        //事件相关属性
        public float _time;
        public string _spName;
        public float _distance;
        private LuaMeshImage _meshImage;

        private float _dwStart = 0f;
        /// ////////////////功能重写部分-必须实现部分/////////////////////////////////////////////
        public override string luaName
        {
            get { return "StoryMontageCtrl"; }
        }
        public override string ctrlName
        {
            get { return "蒙太奇"; }
        }
        public override void initInfo()
        {
            _saveInfo = new paramInfo();
            _realInfo = new paramInfo();
            _distance = 10f;
            base.initInfo();
            expList.Add("time");
            expList.Add("spName");
            expList.Add("distance");
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryMontageCtrl obj = new StoryMontageCtrl();
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj._baseCtrl = _baseCtrl;
            //////本类事件属性赋值
            obj._time = _time;
            obj._spName = _spName;
            obj._distance = _distance;
            return obj;
        }
        public override void Execute()
        {
            _meshImage = objMainCamera.gameObject.GetComponentInChildren<LuaMeshImage>();
            _meshImage.transform.localPosition = new Vector3(0f, 0f, _distance);
            _meshImage.init(_spName, objMainCamera.fieldOfView);
            _dwStart = Time.time;
        }
        /// ////////////////功能重写部分-需要保存状态时可以重写/////////////////////////////////////////////

        /// 每帧执行一次调用-与MonoBehaviour调用频率相同
        public override void Update()
        {
            if (_dwStart == 0f) return;
            if (Time.time -_dwStart > _time)
            {
                Debug.Log("storyMontageCtrl end:" + Time.time);
                _dwStart = 0f;
                _meshImage.Clear();
                OnFinish();
            }
        }
        //事件等待结束，结束有特殊需求可以重写-用于bWait=true的事件的结束调用
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
            _saveInfo.time = _time;
            _saveInfo.spName = _spName;
            _saveInfo.distance = _distance;
        }
        //重设存储点时可以重写-点击页签AddEvent-重设时调用
        public override void ResetPoint()
        {
            _time = _saveInfo.time;
            _spName = _saveInfo.spName;
            _distance = _saveInfo.distance;
        }
        /// //////////////////属性导出导入部分-有导入导出需求时重写///////////////////////////////////////////////
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "time":
                    lua.PushNumber(_time);
                    break;
                case "spName":
                    lua.PushString(_spName);
                    break;
                case "distance":
                    lua.PushNumber(_distance);
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
            _spName = EditorGUILayout.TextField("sprite", _spName);
            _distance = EditorGUILayout.FloatField("distance", _distance);
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////

    }
}