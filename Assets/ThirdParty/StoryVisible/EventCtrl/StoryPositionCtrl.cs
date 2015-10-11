using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--Transform坐标设置--事件的调度
/// 设计时间：2015-08-04
/// </summary>

namespace xxstory
{
    public class StoryPositionCtrl : StoryBaseCtrl
    {    
        private class smoothInfo
        {
            public normalInfo norInfo;
//             public float distance;
//             public float rotationUD;
//             public float rotationLR;
//             public Vector3 offSet;
            public float time;
            public int step;
        } 
        private class paramInfo
        {
            public Transform target;
            public Vector3 localPosition = Vector3.zero;
            public Vector3 localEulerAngles = Vector3.zero;
            public Vector3 localScale = Vector3.one;
            public bool bCamera = false;
            public normalInfo cameraParam;
            public bool bSmooth = false;
            public smoothInfo cameraSmooth;
        }
 

        private paramInfo _realInfo;
        private paramInfo _saveInfo;
        private normalInfo _cameraParam;
        private smoothInfo _cameraSmooth;

        public Transform target;
        public bool _bCamera;
        public bool _bSmooth;

        public override string luaName
        {
            get
            {
                return "StoryPositionCtrl";
            }
        }
        public override string ctrlName
        {
            get
            {
                return "设置坐标";
            }
        }
        public override void initInfo()
        {
            _realInfo = new paramInfo();
            _saveInfo = new paramInfo();
            _cameraParam = new normalInfo();
            _cameraSmooth = new smoothInfo();
            _bCamera = false;
            _bSmooth = false;

            base.initInfo();
            expList.Add("szTarget");
            expList.Add("localPosition");
            expList.Add("localEulerAngles");
            expList.Add("localScale");
            expList.Add("bCamera");
            expList.Add("cameraParam");
            expList.Add("bSmooth");
            expList.Add("cameraSmooth");
        }
        public override StoryBaseCtrl CopySelf()
        {
            
            StoryPositionCtrl obj = new StoryPositionCtrl();
            EventDelegate.Add(objMainCamera.onTweenStop, obj.OnFinish);
            obj.bClick = bClick;
            obj.bWait = bWait;
            obj._baseCtrl = _baseCtrl;
            obj.target = target;
            obj._bCamera = _bCamera;
            obj._cameraParam = _cameraParam;
            obj._bSmooth = _bSmooth;
            obj._cameraSmooth = _cameraSmooth;
            return obj;
        }
        public override void Execute()
        {
            //如果是人物则停止人物移动
            Actor objActor = target.GetComponent<Actor>();
            if (objActor != null)
            {
                objActor.MoveStop(false);
            }
            //停止摄像机运动

            objMainCamera.StopTween();
            target.localPosition = _realInfo.localPosition;
            target.localEulerAngles = _realInfo.localEulerAngles;
            target.localScale = _realInfo.localScale;
            if (_realInfo.bCamera == true)
            {
                objMainCamera.UseParam(_realInfo.cameraParam);
                objMainCamera.LookTarget(target, false);
            }
            if (_realInfo.bSmooth == true)
            {
                SmoothCamera(_realInfo.cameraSmooth);
            }
        }
        public override void ModInfo()
        {
            SavePoint();
            _realInfo = _saveInfo;
        }
        public override void SavePoint()
        {
            //if (target == null) return;
            _saveInfo.localPosition = target.localPosition;
            _saveInfo.localEulerAngles = target.localEulerAngles;
            _saveInfo.localScale = target.localScale;
            _saveInfo.bCamera = _bCamera;
            _saveInfo.cameraParam = _cameraParam;
            _saveInfo.bSmooth = _bSmooth;
            _saveInfo.cameraSmooth = _cameraSmooth;
        }
        public override void ResetPoint()
        {
            target.localPosition = _saveInfo.localPosition;
            target.localEulerAngles = _saveInfo.localEulerAngles;
            target.localScale = _saveInfo.localScale;
            _bCamera = _saveInfo.bCamera;
            _cameraParam = _saveInfo.cameraParam;
            _bSmooth = _saveInfo.bSmooth;
            _cameraSmooth = _saveInfo.cameraSmooth;
            if (_bCamera == true)
            {
                objMainCamera.UseParam(_cameraParam);
                objMainCamera.LookTarget(target, false);
            }
        }

        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "szTarget":
                    lua.PushString(target.name);
                    break;
                case "localPosition":
                    LuaExport.Vector3ToStack(lua, _realInfo.localPosition);
                    break;
                case "localEulerAngles":
                    LuaExport.Vector3ToStack(lua, _realInfo.localEulerAngles);
                    break;
                case "localScale":
                    LuaExport.Vector3ToStack(lua, _realInfo.localScale);
                    break;
                case "bCamera":
                    lua.PushBoolean(_realInfo.bCamera);
                    break;
                case "cameraParam":
                    if (_realInfo.bCamera == false)
                        return false;
                    LuaExport.NormalInfoToStack(lua, _realInfo.cameraParam);
                    break;
                case "bSmooth":
                    lua.PushBoolean(_realInfo.bSmooth);
                    break;
                case "cameraSmooth":
                    if (_realInfo.bSmooth == false)
                        return false;
                    LuaExport.NormalInfoToStack(lua, _realInfo.cameraSmooth.norInfo);
                    lua.PushNumber(_realInfo.cameraSmooth.time);
                    lua.SetField(-2, "time");
                    lua.PushInteger(_realInfo.cameraSmooth.step);
                    lua.SetField(-2, "step");
                    break;
                default:
                    return base.WidgetReadOper(lua, key);
            }
            return true;
        }
        public override void OnFinish()
        {
            base.OnFinish();
            //EventDelegate.Remove(objMainCamera.onTweenStop, OnFinish);
        }
#if UNITY_EDITOR 
        public override void OnParamGUI()
        {
            target = EditorGUILayout.ObjectField(target, typeof(Transform)) as Transform;
            if (target != null)
            {
                target.localPosition = EditorGUILayout.Vector3Field("localPosition", target.localPosition);
                target.localEulerAngles = EditorGUILayout.Vector3Field("localRotate", target.localEulerAngles);
                target.localScale = EditorGUILayout.Vector3Field("localScale", target.localScale);
                _bCamera = EditorGUILayout.BeginToggleGroup("Camera角度设置", _bCamera);
                StoryBaseCtrl.OnCameraInfoGUI(ref _cameraParam);
                if (GUILayout.Button("Flush"))
                {
                    objMainCamera.UseParam(_cameraParam);
                    objMainCamera.LookTarget(target, false);        
                }
                _bSmooth = EditorGUILayout.BeginToggleGroup("Camera缓动效果", _bSmooth);
                StoryBaseCtrl.OnCameraInfoGUI(ref _cameraSmooth.norInfo, "offset", false);
                _cameraSmooth.time = EditorGUILayout.FloatField("time", _cameraSmooth.time);
                _cameraSmooth.step = EditorGUILayout.IntField("step", _cameraSmooth.step);
                if (GUILayout.Button("Smooth"))
                {
                    SmoothCamera(_cameraSmooth);
                }
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndToggleGroup();
            }

            EditorGUILayout.Space();
            base.OnParamGUI();
        }
#endif

        /// <summary>
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////
        /// </summary>
        private void SmoothCamera(smoothInfo smInfo)
        {
            objMainCamera.ClearParam();
            objMainCamera.AddParam("time", smInfo.time);
            objMainCamera.AddParam("oncomplete", "oncomplete");
            objMainCamera.AddParam("oncompleteparams", new LuaObjWithCallFun());
            objMainCamera.AddParam("easetype", iTween.EaseType.linear);
            //EventDelegate.Add(objMainCamera.onTweenStop, OnFinish);
            if (smInfo.step > 0)
            {
                float lrRange = (smInfo.norInfo.rotationUD) / smInfo.step;
                float udRange = (smInfo.norInfo.rotationUD) / smInfo.step;
                Vector3[] path = new Vector3[smInfo.step];
                for (int i = 1; i <= smInfo.step; i++)
                {
                    float lr = objMainCamera.LRAnge + i * lrRange;
                    float ud = objMainCamera.UDAngle + i * udRange;
                    path[i - 1] = objMainCamera.virtualPos(objMainCamera.distance, lr, ud);
                }
                objMainCamera.AddParam("path", path);
            }
            objMainCamera.LRAnge += smInfo.norInfo.rotationLR;
            objMainCamera.UDAngle += smInfo.norInfo.rotationUD;
            objMainCamera.distance += smInfo.norInfo.distance;
            objMainCamera.calculatePos(true);
        }
    }
}
