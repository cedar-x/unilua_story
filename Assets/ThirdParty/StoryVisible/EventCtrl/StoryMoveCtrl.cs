using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--Actor移动--事件的调度
/// 设计时间：2015-08-03
/// </summary>

namespace xxstory
{
    public class StoryMoveCtrl : StoryBaseCtrl
    {
        private class paramInfo
        {
            public Actor target;
            public Vector3 localPosition;
            public Vector3 directPos;
            public float speed;
            public int dwMMethod;
            public bool bInitPosition;
        }

        private paramInfo _realInfo;
        private paramInfo _saveInfo;

        public Actor target;
        public Vector3 _localPosition;
        public float _speed;
        public int _dwMMethod;
        public bool _bInitPosition;

        public override void initInfo()
        {
            _speed = 1;
            _bInitPosition = false;
            _realInfo = new paramInfo();
            _saveInfo = new paramInfo();

            base.initInfo();
            expList.Add("szTarget");
            expList.Add("speed");
            expList.Add("localPosition");
            expList.Add("directPos");
            expList.Add("bInitPosition");
            expList.Add("dwMMethod");
        }
        public override string luaName
        {
            get
            {
                return "StoryMoveCtrl";
            }
        }
        public override string ctrlName
        {
            get
            {
                return "走动";
            }
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryMoveCtrl obj = new StoryMoveCtrl();
            EventDelegate.Add(target.onMoveStop, obj.OnFinish);
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj._baseCtrl = _baseCtrl;
            obj.target = target;
            obj._localPosition = _localPosition;
            obj._speed = _speed;
            obj._bInitPosition = _bInitPosition;
            obj._dwMMethod = _dwMMethod;
            return obj;
        }
        /// ///////////////////////////////////////////////////////////
        public override void ModInfo()
        {
            SavePoint();
            _realInfo = _saveInfo;
        }
        public override void SavePoint()
        {
            _saveInfo.localPosition = _localPosition;
            _saveInfo.directPos = target.transform.localPosition;
            _saveInfo.speed = _speed;
            _saveInfo.bInitPosition = _bInitPosition;
            _saveInfo.dwMMethod = _dwMMethod;
        }
        public override void ResetPoint()
        {
            target.transform.localPosition = _saveInfo.directPos;
            _speed = _saveInfo.speed;
            _localPosition = _saveInfo.localPosition;
            _bInitPosition = _saveInfo.bInitPosition;
            _dwMMethod = _saveInfo.dwMMethod;
        }
        public override void OnFinish()
        {
            base.OnFinish();
        }
        public override void Execute()
        {
            if (_realInfo.bInitPosition == true)
                target.transform.localPosition = _realInfo.localPosition;
            //target.MoveMethod = _realInfo.dwMMethod;
            target.SetSpeed(_realInfo.speed);
            target.MoveTo(_realInfo.directPos.x, _realInfo.directPos.y, _realInfo.directPos.z, false, 0, 0, 0);
            
        }
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "szTarget":
                    lua.PushString(target.name);
                    break;
                case "speed":
                    lua.PushNumber(_realInfo.speed);
                    break;
                case "dwMMethod":
                    lua.PushInteger(_realInfo.dwMMethod);
                    break;
                case "bInitPosition":
                    lua.PushBoolean(_realInfo.bInitPosition);
                    break;
                case "localPosition":
                    LuaExport.Vector3ToStack(lua, _realInfo.localPosition);
                    break;
                case "directPos":
                    LuaExport.Vector3ToStack(lua, _realInfo.directPos);
                    break;
                default:
                    return base.WidgetReadOper(lua, key);
            }
            return true;
        }
#if UNITY_EDITOR 
        public override void OnParamGUI()
        {
            target = EditorGUILayout.ObjectField(target, typeof(Actor)) as Actor;
            if (target != null)
            {
                target.transform.localPosition = EditorGUILayout.Vector3Field("directPos", target.transform.localPosition);
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("c", new GUILayoutOption[] { GUILayout.Height(20), GUILayout.Width(20) }))
            {
                _localPosition = target.transform.localPosition;
            }
            if (GUILayout.Button("s", new GUILayoutOption[] { GUILayout.Height(20), GUILayout.Width(20) }))
            {
                target.transform.localPosition = _localPosition;
            }
            _localPosition = EditorGUILayout.Vector3Field("localPosition", _localPosition);
            EditorGUILayout.EndHorizontal();
            _speed = EditorGUILayout.FloatField("speed", _speed);
            _dwMMethod = EditorGUILayout.IntField("dwMMethod", _dwMMethod);
            _bInitPosition = GUILayout.Toggle(_bInitPosition, "bInitPosition");
            base.OnParamGUI();
        }
#endif
    }
}
