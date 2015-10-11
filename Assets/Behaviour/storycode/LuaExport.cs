using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using UniLua;

//环境光属性
namespace Hstj
{
    public class LuaExport :LuaObject
    {
        protected string m_szResult = "";

        void Start()
        {
            Init();
        }
        public virtual void Init()
        {
        }
        //==================public property function==============================
        public ILuaState _fileLua
        {
            get
            {
                return Game.LuaApi;
            }
        }
        public string ResultString
        {
            set
            {
                m_szResult = value;
//                 _fileLua.L_DoString("return" + value);
//                 ImportProperty();
//                 _fileLua.Pop(1);
            }
            get
            {
                return m_szResult;
            }
        }
        public virtual string[] PropertyString
        {
            get
            {
                return new string[] { "ambientLight", "ambientIntensity", "fog", "fogColor", "fogDensity", "fogEndDistance", "fogMode", "fogStartDistance"};
            }
        }
        //==================public function=======================================
        protected override void ExtraRefLua()
        {
            Game.Lua.SetTableFunction(-1, "ImportProperty", new CSharpFunctionDelegate(Lua_ImportProperty));
        }
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "name":
                    lua.PushString(gameObject.name);
                    break;
                case "fDir":
                    lua.PushNumber(transform.localEulerAngles.y);
                    break;
                case "position":
                    Vector3ToStack(lua, transform.position);
                    break;
                case "localPosition":
                    Vector3ToStack(lua, transform.localPosition);
                    break;
                case "localRotate":
                    Vector3ToStack(lua, transform.localEulerAngles);
                    break;
                case "localScale":
                    Vector3ToStack(lua, transform.localScale);
                    break;
                case "ambientLight":
                    ColorToStack(lua, RenderSettings.ambientLight);
                    break;
                case "ambientIntensity":
#if UNITY_4_6
                    lua.PushNumber(1.0);
#else
                    lua.PushNumber(RenderSettings.ambientIntensity);
#endif
                    break;
                case "fog":
                    lua.PushBoolean(RenderSettings.fog);
                    break;
                case "fogColor":
                    ColorToStack(lua, RenderSettings.fogColor);
                    break;
                case "fogDensity":
                    lua.PushNumber(RenderSettings.fogDensity);
                    break;
                case "fogEndDistance":
                    lua.PushNumber(RenderSettings.fogEndDistance);
                    break;
                case "fogMode":
                    lua.PushInteger((int)RenderSettings.fogMode);
                    break;
                case "fogStartDistance":
                    lua.PushNumber(RenderSettings.fogStartDistance);
                    break;
                default:
                    return false;
            }
            return true;
        }
        protected override bool WidgetWriteOper(ILuaState lua, string key)
        {
            //Debug.Log("LuaExport:StramImportOper:"+key);
            int dwStackIndex = lua.GetTop();
            switch (key)
            {
                case "name":
                    gameObject.name = lua.L_CheckString(3);
                    break;
                case "fDir":
                    transform.localEulerAngles = new Vector3(0, (float)lua.L_CheckNumber(3), 0);
                    break;
                case "position":
                    transform.position = GetVector3(lua, 3);
                    break;
                case "localPosition":
                    transform.localPosition = GetVector3(lua, 3);
                    break;
                case "localRotate":
                    transform.localEulerAngles = GetVector3(lua, 3);
                    break;
                case "localScale":
                    transform.localScale = GetVector3(lua, 3);
                    break;
                case "ambientLight":
                    RenderSettings.ambientLight = GetColor(lua, 3);
                    break;
                case "ambientIntensity":
#if UNITY_4_6
                    
#else
                    RenderSettings.ambientIntensity = (float)lua.ToNumber(3);
#endif
                    break;
                case "fog":
                    RenderSettings.fog = lua.ToBoolean(3);
                    break;
                case "fogColor":
                    RenderSettings.fogColor = GetColor(lua, 3);
                    break;
                case "fogDensity":
                    RenderSettings.fogDensity = (float)lua.ToNumber(3);
                    break;
                case "fogEndDistance":
                    RenderSettings.fogEndDistance = (float)lua.ToNumber(3);
                    break;
                case "fogMode":
                    RenderSettings.fogMode = (FogMode)lua.ToInteger(3);;
                    break;
                case "fogStartDistance":
                    RenderSettings.fogStartDistance = (float)lua.ToNumber(3);
                    break;
                default:
                    return false;
            }
            if (dwStackIndex != lua.GetTop())
                Debug.LogWarning("LuaExport:WidgetWriteOper stack Exception:start=" + key + ":" + dwStackIndex + " end=" + lua.GetTop());
            return true;
        }
        /// <summary>
        /// 将Table序列化成为字符串写入文件
        /// </summary>
        public string SerializeTable(int index)
        {
            //获取LibCore中_SerializeTable函数，然后串行化table， 以后要重写，从而不依赖LibCore（因为有在非运行状态下获取内容）
            if (_fileLua.Type(index) != LuaType.LUA_TTABLE)
            {
                Debug.LogWarning("LuaExport:SerializeTable param must a table.. please check");
                return "";
            }
            int dwStackIndex = _fileLua.GetTop();
            _fileLua.PushValue(index);
            _fileLua.GetGlobal("_SerializeTable");
            _fileLua.Insert(-2);
            _fileLua.PushBoolean(false);
            if (_fileLua.PCall(2, 1, 0) != 0)
            {
                Debug.LogWarning(_fileLua.ToString(-1) + " in SerializeTable");
                _fileLua.Pop(-1);
                return "";
            }
            m_szResult = _fileLua.ToString(-1);
            //Debug.Log(m_szResult);
            _fileLua.Pop(1);
            if (dwStackIndex != _fileLua.GetTop())
                Debug.LogWarning("LuaExport:SerializeTable stack Exception:start=" + dwStackIndex + " end=" + _fileLua.GetTop());
            return m_szResult;
        }
        /// <summary>
        /// 获取文件大小
        /// </summary>
        public long GetFileSize(string path)
        {
            FileStream fstream = File.OpenRead(path);
            long len = fstream.Seek(0, SeekOrigin.End);
            fstream.Close();
            return len;
        }
        /// <summary>
        /// 
        /// </summary>
        public static Vector3 GetVector2(ILuaState lua, int dwIndex)
        {
            Vector2 v = Vector2.zero;
            lua.PushValue(dwIndex);
            lua.PushString("x");
            lua.GetTable(-2);
            v.x = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("y");
            lua.GetTable(-2);
            v.y = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.Pop(1);

            return v;
        }
        public static Vector3 GetVector3(ILuaState lua, int dwIndex)
        {
            return LuaItween.GetVector3(lua, dwIndex);
        }
        public static Vector3[] GetPath(ILuaState lua, int dwIndex)
        {
            return LuaItween.GetPath(lua, dwIndex);
        }
        public static Quaternion GetQuaternion(ILuaState lua, int dwIndex)
        {
            Quaternion v = new Quaternion();
            lua.PushValue(dwIndex);
            lua.PushString("x");
            lua.GetTable(-2);
            v.x = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("y");
            lua.GetTable(-2);
            v.y = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("z");
            lua.GetTable(-2);
            v.z = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("w");
            lua.GetTable(-2);
            v.w = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.Pop(1);
            return v;
        }
        public static Color GetColor(ILuaState lua, int dwIndex)
        {
            Color v = Color.black;
            lua.PushValue(dwIndex);
            lua.PushString("r");
            lua.GetTable(-2);
            v.r = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("g");
            lua.GetTable(-2);
            v.g = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("b");
            lua.GetTable(-2);
            v.b = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("a");
            lua.GetTable(-2);
            v.a = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.Pop(1);
            return v;

        }
        public static normalInfo GetNormalInfo(ILuaState lua, int dwIndex)
        {
            normalInfo nInfo = new normalInfo();
            lua.PushValue(dwIndex);
            lua.PushString("LRAngle");
            lua.GetTable(-2);
            nInfo.rotationLR = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("UDAngle");
            lua.GetTable(-2);
            nInfo.rotationUD = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("distance");
            lua.GetTable(-2);
            nInfo.distance = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("offset");
            lua.GetTable(-2);
            nInfo.offSet = GetVector3(lua, -1);
            lua.Pop(1);

            lua.Pop(1);
            return nInfo;

        }
        public static Keyframe GetKeyframe(ILuaState lua, int index)
        {
            Keyframe key = new Keyframe();
            lua.PushValue(index);
            lua.PushString("time");
            lua.GetTable(-2);
            key.time = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("value");
            lua.GetTable(-2);
            key.value = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("inTangent");
            lua.GetTable(-2);
            key.inTangent = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.PushString("outTangent");
            lua.GetTable(-2);
            key.outTangent = (float)lua.ToNumber(-1);
            lua.Pop(1);

            lua.Pop(1);
            return key;

        }
        public static AnimationCurve GetAnimationCurve(ILuaState lua, int dwIndex)
        {
            AnimationCurve animCurve = new AnimationCurve();
            lua.PushValue(dwIndex);
            int count = lua.L_Len(-1);

            for (int i = 1; i <= count; i++)
            {
                lua.PushInteger(i);
                lua.GetTable(-2);
                Keyframe key = GetKeyframe(lua, -1);
                animCurve.AddKey(key);
                lua.Pop(1);
            }
            lua.Pop(1);
            return animCurve;
        }
        public static void Vector2ToStack(ILuaState lua, Vector2 v3)
        {
            lua.NewTable();
            lua.PushNumber(v3.x);
            lua.SetField(-2, "x");
            lua.PushNumber(v3.y);
            lua.SetField(-2, "y");
        }
        public static void Vector3ToStack(ILuaState lua, Vector3 v3)
        {
            lua.NewTable();
            lua.PushNumber(v3.x);
            lua.SetField(-2, "x");
            lua.PushNumber(v3.y);
            lua.SetField(-2, "y");
            lua.PushNumber(v3.z);
            lua.SetField(-2, "z");
        }
        public static void QuaternionToStack(ILuaState lua, Quaternion qat)
        {
            lua.NewTable();
            lua.PushNumber(qat.x);
            lua.SetField(-2, "x");
            lua.PushNumber(qat.y);
            lua.SetField(-2, "y");
            lua.PushNumber(qat.z);
            lua.SetField(-2, "z");
            lua.PushNumber(qat.w);
            lua.SetField(-2, "w");
        }
        public static void ColorToStack(ILuaState lua, Color cr)
        {
            lua.NewTable();
            lua.PushNumber(cr.r);
            lua.SetField(-2, "r");
            lua.PushNumber(cr.g);
            lua.SetField(-2, "g");
            lua.PushNumber(cr.b);
            lua.SetField(-2, "b");
            lua.PushNumber(cr.a);
            lua.SetField(-2, "a");
        }
        public static void NormalInfoToStack(ILuaState lua, normalInfo nor)
        {
            lua.NewTable();
            lua.PushNumber(nor.distance);
            lua.SetField(-2, "distance");
            lua.PushNumber(nor.rotationLR);
            lua.SetField(-2, "LRAngle");
            lua.PushNumber(nor.rotationUD);
            lua.SetField(-2, "UDAngle");
            Vector3ToStack(lua, nor.offSet);
            lua.SetField(-2, "offset");
        }
        public static void KeyframeToStack(ILuaState lua, Keyframe keys)
        {
            lua.NewTable();
            lua.PushNumber(keys.time);
            lua.SetField(-2, "time");
            lua.PushNumber(keys.value);
            lua.SetField(-2, "value");
            lua.PushNumber(keys.inTangent);
            lua.SetField(-2, "inTangent");
            lua.PushNumber(keys.outTangent);
            lua.SetField(-2, "outTangent");
        }
        public static void AnimationCurveToStack(ILuaState lua, AnimationCurve aniCurve)
        {
            lua.NewTable();
            for (int i = 0; i < aniCurve.keys.Length;i++ )
            {
                lua.PushNumber(i+1);
                Keyframe key = aniCurve.keys[i];
                KeyframeToStack(lua, key);
                lua.SetTable(-3);
            }
        }
        //===================virtual function====================================

        /// <summary>
        /// 导出组件属性到文件   //, FileStream可以导出transform基本属性，如position, localPosition, localRotate, localScale
        /// </summary>
        public virtual string ExportProperty(string[] strProperty)
        {
            string[] listProperty = null;
            if (strProperty !=null)
                listProperty = strProperty;
            else
                listProperty = strProperty;
            _fileLua.NewTable();
            bool bSet = false;
            foreach (string key in listProperty)
            {
                bSet = WidgetReadOper(_fileLua, key);
                if (bSet == true)
                {
                    _fileLua.SetField(-2, key);
                }
            }   
            string strResult = SerializeTable(-1);
            //_fileLua.Pop(1);
            return strResult;
        }
        /// <summary>
        /// 导入： 从文件或者表中读取属性，生成新的组件或改变现有组件属性
        /// </summary>
        public virtual void ImportProperty(int dwIndex)
        {
            bool bFlag = false;
            _fileLua.PushValue(dwIndex);
            _fileLua.PushNil();
            while (_fileLua.Next(-2))
            {
                string key = _fileLua.L_CheckString(-2);
                _fileLua.Insert(3);
                bFlag = WidgetWriteOper(_fileLua, key);
                _fileLua.Remove(3);
                if (bFlag == false)
                {
                   Debug.Log("LuaExport ImportProperty can't find Property key[:"+key+":] check....");
                }
                
            }
            _fileLua.Pop(1);
        }
        /////////////////////////////////////////////////////////////
        int Lua_ImportProperty(ILuaState lua)
        {
            if (lua.Type(2) != LuaType.LUA_TTABLE)
            {
                Debug.LogWarning("LuaExport:ImportProperty param1 is not table...");
                return 0;
            }
            Debug.Log("Lua_ImportProperty:"+lua.GetTop()+":"+lua.Type(-1));
            ImportProperty(2);
            Debug.Log("Lua_ImportProperty:"+lua.GetTop()+":"+lua.Type(-1));
            return 0;
        }
    }
}
