using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--Camera缓动--事件的调度
/// 设计时间：2015-08-13
/// </summary>

namespace xxstory
{
    public class StoryCameraSmoothCtrl : StoryBaseCtrl
    {
        private class paramInfo 
        {
            public normalInfo norInfo;
            public float time;
            public int dwType;
        }

        public Transform target;
        public LuaPathCamera _pathTarget;
        public int _dwType;
        public normalInfo _cameraParam;
        public float _time;

        private paramInfo _saveInfo;
        private paramInfo _realInfo;
        /// ////////////////功能重写部分-必须实现部分/////////////////////////////////////////////
        public override string luaName
        {
            get { return "StoryCameraSmoothCtrl"; }
        }
        public override string ctrlName
        {
            get { return "Camera缓动"; }
        }
        public override void initInfo()
        {
            _cameraParam = new normalInfo();
            _saveInfo = new paramInfo();
            _realInfo = new paramInfo();
            base.initInfo();
            expList.Add("szTarget");
            expList.Add("cameraParam");
            expList.Add("time");
            expList.Add("dwType");
            expList.Add("pathParam");
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryCameraSmoothCtrl obj = new StoryCameraSmoothCtrl();
            EventDelegate.Add(objMainCamera.onTweenStop, obj.OnFinish);
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj._baseCtrl = _baseCtrl;
            obj.target = target;
            obj._pathTarget = _pathTarget;
            obj._cameraParam = _cameraParam;
            obj._time = _time;
            obj._dwType = _dwType;
            return obj;
        }
        public override void Execute()
        {
            OnCameraSmooth(_realInfo);
        }
        //修改事件内容可以重写-点击页签AddEvent-修改时调用
        public override void ModInfo()
        {
            SavePoint();

            _realInfo = _saveInfo;
        }
        //保存存储点时可以重写-点击页签AddEvent-存储点时调用
        public override void SavePoint()
        {
            _saveInfo.norInfo = _cameraParam;
            _saveInfo.time = _time;
            _saveInfo.dwType = _dwType;

        }
        //重设存储点时可以重写-点击页签AddEvent-重设时调用
        public override void ResetPoint()
        {
            _cameraParam = _saveInfo.norInfo;
            _time = _saveInfo.time;
            _dwType = _saveInfo.dwType;

            //恢复视角
            objMainCamera.LRAnge -= _cameraParam.rotationLR;
            objMainCamera.distance -= _cameraParam.distance;
            objMainCamera.UDAngle -= _cameraParam.rotationUD;
            objMainCamera.calculatePos(false);
        }
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "szTarget":
                    if (target == null)
                        return false;
                    lua.PushString(target.name);
                    break;
                case "cameraParam":
                    LuaExport.NormalInfoToStack(lua, _realInfo.norInfo);
                    break;
                case "time":
                    lua.PushNumber(_realInfo.time);
                    break;
                case "dwType":
                    lua.PushInteger(_realInfo.dwType);
                    break;
                case "pathParam":
                    if (_realInfo.dwType != 1)
                        return false;
                    if (_pathTarget == null)
                    {
                        Debug.LogWarning("StoryCameraSmoothCtrl don't set Story Path Target.....");
                        return false;
                    }
                    _pathTarget.ExportProperty(null);
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
            _dwType = EditorGUILayout.IntField("dwType", _dwType);
            if (_dwType == 0)
            {
                StoryBaseCtrl.OnCameraInfoGUI(ref _cameraParam);
                _time = EditorGUILayout.FloatField("time", _time);
                if (GUILayout.Button("Smooth"))
                {
                    _saveInfo.norInfo = _cameraParam;
                    _saveInfo.time = _time;
                    OnCameraSmooth(_saveInfo);
                }
            }
            else if (_dwType == 1 || _dwType == 2)
            {
                _pathTarget = EditorGUILayout.ObjectField(_pathTarget, typeof(LuaPathCamera)) as LuaPathCamera;
            }

            
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////
        private void OnCameraSmooth(paramInfo pInfo)
        {
            if (pInfo.dwType == 0)
            {
                objMainCamera.distance += pInfo.norInfo.distance;
                objMainCamera.UDAngle += pInfo.norInfo.rotationUD;
                objMainCamera.LRAnge += pInfo.norInfo.rotationLR;
                objMainCamera.ClearParam();
                objMainCamera.AddParam("time", pInfo.time);
                objMainCamera.AddParam("oncomplete", "oncomplete");
                objMainCamera.AddParam("oncompleteparams", new LuaObjWithCallFun());
                objMainCamera.AddParam("easetype", iTween.EaseType.linear);
                //EventDelegate.Add(objMainCamera.onTweenStop, OnFinish);
                objMainCamera.calculatePos(true);
            }
            else if(pInfo.dwType == 1)
            {
                objMainCamera.StopTween();
                _pathTarget.Stop();
                _pathTarget.SetAnimTarget(objMainCamera.transform);
                _pathTarget.Play();
            }
            else if (pInfo.dwType == 2)
            {
                objMainCamera.target = null;
                objMainCamera.StopTween();
                objMainCamera.transform.localPosition = Vector3.zero;
                objMainCamera.transform.localEulerAngles = Vector3.zero;
                _pathTarget.Stop();
                _pathTarget.SetAnimTarget(objMainCamera.proxyParent);
                _pathTarget.Play();
            }
        }
    }
}