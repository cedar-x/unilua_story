using UnityEngine;
using System;
using System.Collections;
using UniLua;

//author: 
//Desc: 封装Camera路径运动

namespace Hstj
{
//     [RequireComponent(typeof(CameraPathAnimator))]
//     [RequireComponent(typeof(CameraPath))]
    public class LuaPathCamera : LuaExport 
    {
        protected CameraPath _cameraPath;
        protected CameraPathAnimator _cameraAnimator;
        public override void Awake()
        {
            base.Awake();
        }
        public override void Init()
        {
            _cameraPath = gameObject.GetComponent<CameraPath>();
            if (_cameraPath == null)
                _cameraPath = gameObject.AddComponent<CameraPath>();
            _cameraAnimator = gameObject.GetComponent<CameraPathAnimator>();
            if (_cameraAnimator == null)
            {
                _cameraAnimator = gameObject.AddComponent<CameraPathAnimator>();
                _cameraAnimator.playOnStart = false;
            }
        }
        ////////////////////////////////////////////////////////////////////////
        public override string[] PropertyString
        {
            get
            {
                return new string[] { "interpolation", "animMode", "speed", "controlpoints", "Orientations", "dwType", "isLoop", "speedArr", "localPosition" };
            }
        }
        protected override void ExtraRefLua()
        {
            base.ExtraRefLua();
            Game.Lua.SetTableFunction(-1, "Clear", new CSharpFunctionDelegate(Lua_Clear));
            Game.Lua.SetTableFunction(-1, "AddPoint", new CSharpFunctionDelegate(Lua_AddPoint));
            Game.Lua.SetTableFunction(-1, "Play", new CSharpFunctionDelegate(Lua_Play));
            Game.Lua.SetTableFunction(-1, "Stop", new CSharpFunctionDelegate(Lua_Stop));
            Game.Lua.SetTableFunction(-1, "SetAnimTarget", new CSharpFunctionDelegate(Lua_SetAnimTarget));
        }
        protected override bool WidgetWriteOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "isLoop":
                    _cameraPath.loop = (bool)lua.ToBoolean(3);
                    break;
                case "interpolation":
                    _cameraPath.interpolation = (CameraPath.Interpolation)lua.ToInteger(3);
                    break;
                case "animtarget":
                    LuaObject obj = LuaObject.GetLuaObject(lua, 3);
                    SetAnimTarget(obj.transform);
                    break;
                case "animMode":
                    _cameraAnimator.animationMode = (CameraPathAnimator.animationModes)lua.ToInteger(3);
                    break;
                case "speed":
                    _cameraAnimator.pathSpeed = (float)lua.ToNumber(3);
                    break;
                case "controlpoints":
                    lua.PushValue(3);
                    int count = lua.L_Len(-1);
                    _cameraPath.Clear();
                    for (int i = 1; i <= count; i++)
                    {
                        lua.PushNumber(i);
                        lua.GetTable(-2);

                        lua.PushString("controlpoint");
                        lua.GetTable(-2);
                        Vector3 controlPoint = GetVector3(lua, -1);
                        lua.Pop(1);
                        lua.PushString("forwardControlPoint");
                        lua.GetTable(-2);
                        Vector3 forwardControlPoint = GetVector3(lua, -1);
                        lua.Pop(1);
                        lua.PushString("backwardControlPoint");
                        lua.GetTable(-2);
                        Vector3 backwardControlPoint = GetVector3(lua, -1);
                        lua.Pop(1);

                        lua.Pop(1);
                        _cameraPath.AddPoint(controlPoint);
                        _cameraPath[i-1].forwardControlPoint = forwardControlPoint;
                        _cameraPath[i-1].backwardControlPoint = backwardControlPoint;
                    }
                    lua.Pop(1);
                    break;
                case "speedArr": //lsy add
                    //Debug.Log("-----------SPEED-------WRITE----------");
                    lua.PushValue(3);
                    int count2 = lua.L_Len(-1);
                    _cameraPath.speedList.Clear();
                    for (int i = 1; i <= count2; i++) {
                        lua.PushNumber(i);
                        lua.GetTable(-2);
                        Quaternion pointRotation = GetQuaternion(lua, -1);
                       //------------------------------------------
                        int point = -1, cpointA = -1, cpointB = -1;
                        float curvePercentage = 0f;
                        int speed = 0;

                        //free or fixedToPoint
                        lua.PushString("positionModes");
                        lua.GetTable(-2);
                        CameraPathPoint.PositionModes positionModes = (CameraPathPoint.PositionModes)lua.ToInteger(-1);
                        lua.Pop(1);

                        lua.PushString("percent");
                        lua.GetTable(-2);
                        float percent = (float)lua.ToNumber(-1);
                        lua.Pop(1);

                        lua.PushString("point");
                        lua.GetTable(-2);
                        if (lua.Type(-1) != LuaType.LUA_TNIL)
                            point = lua.ToInteger(-1);
                        lua.Pop(1);

                        lua.PushString("cpointA");
                        lua.GetTable(-2);
                        if (lua.Type(-1) != LuaType.LUA_TNIL)
                            cpointA = lua.ToInteger(-1);
                        lua.Pop(1);

                        lua.PushString("cpointB");
                        lua.GetTable(-2);
                        if (lua.Type(-1) != LuaType.LUA_TNIL)
                            cpointB = lua.ToInteger(-1);
                        lua.Pop(1);

                        lua.PushString("curvePercentage");
                        lua.GetTable(-2);
                        if (lua.Type(-1) != LuaType.LUA_TNIL)
                            curvePercentage = (float)lua.ToNumber(-1);
                        lua.Pop(1);

                        lua.PushString("speed");
                        lua.GetTable(-2);
                        if (lua.Type(-1) != LuaType.LUA_TNIL)
                            speed = lua.ToInteger(-1);
                        lua.Pop(1);

                        lua.Pop(1);
                        CameraPathSpeed spd = AddSpeedPoint(positionModes, point,cpointA,cpointB,curvePercentage);
                        spdNeeds(spd, i-1, percent, curvePercentage, speed);

                    }
                    lua.Pop(1);
                    break;
                case "Orientations":
                    lua.PushValue(3);
                    int count1 = lua.L_Len(-1);
                    _cameraPath.orientationList.Clear();
                    for (int i = 1; i <= count1; i++)
                    {
                        lua.PushNumber(i);
                        lua.GetTable(-2);
                        Quaternion pointRotation = GetQuaternion(lua, -1);

                        lua.PushString("positionModes");
                        lua.GetTable(-2);
                        CameraPathPoint.PositionModes positionModes = (CameraPathPoint.PositionModes)lua.ToInteger(-1);
                        lua.Pop(1);

                        lua.PushString("percent");
                        lua.GetTable(-2);
                        float percent= (float)lua.ToNumber(-1);
                        lua.Pop(1);
                        int point = -1, cpointA = -1, cpointB = -1;
                        float curvepercentage = 0f;
                        lua.PushString("point");
                        lua.GetTable(-2);
                        if (lua.Type(-1)!= LuaType.LUA_TNIL)
                            point = lua.ToInteger(-1);
                        lua.Pop(1);

                        lua.PushString("cpointA");
                        lua.GetTable(-2);
                        if (lua.Type(-1) != LuaType.LUA_TNIL)
                            cpointA= lua.ToInteger(-1);
                        lua.Pop(1);

                        lua.PushString("cpointB");
                        lua.GetTable(-2);
                        if (lua.Type(-1) != LuaType.LUA_TNIL)
                            cpointB = lua.ToInteger(-1);
                        lua.Pop(1);

                        lua.PushString("curvePercentage");
                        lua.GetTable(-2);
                        if (lua.Type(-1) != LuaType.LUA_TNIL)
                            curvepercentage = (float)lua.ToNumber(-1);
                        lua.Pop(1);

                        lua.Pop(1);

                        CameraPathOrientation oriQrien = AddOrientationsPoint(positionModes, pointRotation, point, cpointA, cpointB, curvepercentage);
                        fromLua(oriQrien, i - 1, percent, curvepercentage, pointRotation);
                    }
                    lua.Pop(1);
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
                case "speedArr"://lsy add
                    //Debug.Log("-----------SPEED------READ-----------");
                    CameraPathSpeedList slist = _cameraPath.speedList;
                    lua.NewTable();
                    for (int i = 1; i <= slist.realNumberOfPoints; i++)
                    {     
                        CameraPathSpeed spd = slist[i - 1];
                        lua.PushNumber(i);
                        lua.NewTable();

                        lua.PushInteger((int)spd.positionModes);
                        lua.SetField(-2, "positionModes");//free or FixedToPoint

                        lua.PushNumber(spd.percent);
                        lua.SetField(-2, "percent");

                        lua.PushNumber(spd.speed);
                        lua.SetField(-2, "speed");

                        lua.PushNumber(spd.curvePercentage);
                        lua.SetField(-2, "curvePercentage");
                        if (spd.point != null)
                        {
                            lua.PushInteger(spd.point.index);
                            lua.SetField(-2, "point");
                        }    
                        if (spd.cpointA != null)
                        {
                            lua.PushInteger(spd.cpointA.index);
                            lua.SetField(-2, "cpointA");
                        }
                        if (spd.cpointB != null)
                        {
                            lua.PushInteger(spd.cpointB.index);
                            lua.SetField(-2, "cpointB");
                        }
                     
                        lua.SetTable(-3);
                    }
                    break;
                case "isLoop"://
                    lua.PushBoolean(_cameraPath.loop);
                    break;
                case "dwType":
                    lua.PushInteger(2);
                    break;
                case "interpolation":
                    lua.PushInteger((int)_cameraPath.interpolation);
                    break;
                case "animMode":
                    lua.PushInteger((int)_cameraAnimator.animationMode);
                    break;
                case "speed":
                    lua.PushNumber(_cameraAnimator.pathSpeed);
                    break;
                case "controlpoints":
                    lua.NewTable();
                    for (int i = 1; i <= _cameraPath.realNumberOfPoints; i++)
                    {
                        lua.PushNumber(i);
                        lua.NewTable();
                        Vector3ToStack(lua, _cameraPath[i-1].localPosition);
                        lua.SetField(-2, "controlpoint");
                        Vector3ToStack(lua, _cameraPath[i - 1].forwardControlPoint);
                        lua.SetField(-2, "forwardControlPoint");
                        Vector3ToStack(lua, _cameraPath[i - 1].backwardControlPoint);
                        lua.SetField(-2, "backwardControlPoint");
                        lua.SetTable(-3);
                    }
                    break;
                case "Orientations":
                    CameraPathOrientationList ori = _cameraPath.orientationList;
                    lua.NewTable();
                    for (int i = 1; i <= ori.realNumberOfPoints; i++)
                    {
                        CameraPathOrientation pathOri = ori[i-1];
                        lua.PushNumber(i);
                        QuaternionToStack(lua, pathOri.rotation);
                        lua.PushInteger((int)pathOri.positionModes);
                        lua.SetField(-2, "positionModes");
                        lua.PushNumber(pathOri.percent);
                        lua.SetField(-2, "percent");
                        lua.PushNumber(pathOri.curvePercentage);
                        lua.SetField(-2, "curvePercentage");
                        
                        if (pathOri.point != null)
                        {
                            lua.PushInteger(pathOri.point.index);
                            lua.SetField(-2, "point");
                        }
                        if (pathOri.cpointA != null)
                        {
                            lua.PushInteger(pathOri.cpointA.index);
                            lua.SetField(-2, "cpointA");
                        }
                        if (pathOri.cpointB != null)
                        {
                            lua.PushInteger(pathOri.cpointB.index);
                            lua.SetField(-2, "cpointB");
                        }
                        lua.SetTable(-3);
                    }
                    break;
               
                default:
                    return base.WidgetReadOper(lua, key);
            }
            return true;
        }
        /// <summary>
        /// ///////////////////////////////////////////////////////
        /// </summary>
        public void Clear()
        {
            _cameraPath.Clear();
        }
        public void AddPoint(Vector3 pt)
        {
            _cameraPath.AddPoint(pt);
        }
        public void Play()
        {
            _cameraAnimator.Play();
        }
        public void Stop()
        {
            _cameraAnimator.Stop();
        }
      
        public CameraPathOrientation AddOrientationsPoint(CameraPathPoint.PositionModes positionModes, Quaternion pointRotation, int pointIndex, int cpointA, int cpointB, float curvepercentage)
        {
           // Debug.Log("AddOrientationsPoint:" + positionModes + ":" + pointIndex + ":" + cpointA + ":" + cpointB + ":" + curvepercentage);
            CameraPathOrientation newCameraPathPoint = _cameraPath.gameObject.AddComponent<CameraPathOrientation>();//CreateInstance<CameraPathOrientation>();
            newCameraPathPoint.hideFlags = HideFlags.HideInInspector;
            //CameraPathPoint.PositionModes positionModes = positionModes;
            switch (positionModes)
            {
                case CameraPathPoint.PositionModes.Free:
                    CameraPathControlPoint cPointA = _cameraPath[cpointA];
                    CameraPathControlPoint cPointB = _cameraPath[cpointB];
                    float curvePercentage = curvepercentage;
                    _cameraPath.orientationList.AddPoint(newCameraPathPoint, cPointA, cPointB, curvePercentage);
                    break;
                case CameraPathPoint.PositionModes.FixedToPoint:
                    CameraPathControlPoint point = _cameraPath[pointIndex];
                    _cameraPath.orientationList.AddPoint(newCameraPathPoint, point);
                    break;
            }
            
            //newCameraPathPoint.rotation = pointRotation;
            return newCameraPathPoint;
        }

        public void fromLua(CameraPathOrientation pathOrientation, int index, float percent, float curvePercentage, Quaternion quater)
        {
            pathOrientation.index = index;
            pathOrientation.percent = percent;
            pathOrientation.curvePercentage = curvePercentage;
            pathOrientation.rotation = quater;
            
        }

        //lsy add
        public CameraPathSpeed AddSpeedPoint(CameraPathPoint.PositionModes positionModes, int pointIndex, int curvePointA, int curvePointB, float curvePercentage)
        {
            //Debug.Log("------------AddSpeedPoint--------------");
            CameraPathSpeed newSpeedPoint = _cameraPath.gameObject.AddComponent<CameraPathSpeed>();//CreateInstance<CameraPathSpeed>();
            newSpeedPoint.hideFlags = HideFlags.HideInInspector;
            ////////AddPoint(point, curvePointA, curvePointB, Mathf.Clamp01(curvePercetage));
            ////////RecalculatePoints();
            switch (positionModes)
            {
                case CameraPathPoint.PositionModes.Free:
                    CameraPathControlPoint cPointA = _cameraPath[curvePointA];
                    CameraPathControlPoint cPointB = _cameraPath[curvePointB];
                    float CurvePercentage = curvePercentage;
                    _cameraPath.speedList.AddPoint(newSpeedPoint, cPointA, cPointB, Mathf.Clamp01(CurvePercentage));
                    break;
                case CameraPathPoint.PositionModes.FixedToPoint:
                    CameraPathControlPoint point = _cameraPath[pointIndex];
                    _cameraPath.speedList.AddPoint(newSpeedPoint, point);
                    break;
                default:
                    break;
            }
            return newSpeedPoint;
        }

        //lsy add
        public void spdNeeds(CameraPathSpeed speed, int index, float percent, float curvePercentage, int mySpeed)
        {
            //Debug.Log("-----------spdNeeds--------------");
            speed.index = index;
            speed.percent = percent;
            speed.curvePercentage = curvePercentage;
            speed.speed = mySpeed;
        }
        public void SetAnimTarget(Transform trans)
        {
            _cameraAnimator.animationObject = trans;
        }
        /// <summary>
        /// ////////////////////////////////////////////////////
        /// </summary>
        private int Lua_Clear(ILuaState lua)
        {
            Clear();
            return 0;
        }
        private int Lua_AddPoint(ILuaState lua)
        {
            Vector3 pt = LuaItween.GetVector3(lua, 2);
            AddPoint(pt);
            return 0;
        }
        public int Lua_Play(ILuaState lua)
        {
            Play();
            return 0;
        }
        public int Lua_Stop(ILuaState lua)
        {
            Stop();
            return 0;
        }
        public int Lua_SetAnimTarget(ILuaState lua)
        {
            LuaObject obj = LuaObject.GetLuaObject(lua, 2);
            if (obj == null)
            {
                Debug.LogWarning("LuaPahtCamera  Lua SetTarget is not LuaObject...");
                return 0;
            }
            _cameraAnimator.animationObject = obj.transform;
            return 0;
        }
	}
}