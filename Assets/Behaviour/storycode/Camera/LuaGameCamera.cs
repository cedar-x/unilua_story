using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UniLua;

//author: 
//Desc: 包装Camera跟随与运动

namespace Hstj
{
    //摄像机类型
    public enum CameraCastType
    {   
        //游戏场景中摄像机设置：transform.parent.localEulerAngles = Vector3.zero;支持左右箭头旋转视角
        Normal = 0, 
        //固定：固定于一个点，固定于一个transform身上：transform.parent.localEulerAngles = Vector3.zero;
        Fixed = 1,   
        // 跟踪：随目标点朝向更改而更改：transform.parent.localEulerAngles = m_Target.transform.localEulerAngles;
        Tracking = 2,
        // 交互：暂未想好
        Interactive = 3, 
    }
    public struct pointNode
    {
        public bool bUseful;
        public Vector3 position;
        public Quaternion rotation;
        public float fieldView;
        public CameraCastType type;
        public float distance;
        public float rotationUD;
        public float rotationLR;
        public Transform target;
    }
    public struct normalInfo
    {
        public float distance;
        public float rotationUD;//==rotationX
        public float rotationLR;//==rotationY
        public Vector3 offSet;
        public normalInfo(int dis)
        {
            this.distance = 10.5f;
            this.rotationUD = 30f;
            this.rotationLR = 0f;
            this.offSet = new Vector3(0f, 0.6f, 0f);
        }
    }
    //左右上下拉近拉远
    public struct adjustView
    {
        //标识俯视旋转是否正在进行
        public bool bUDBack;
        //标识水平旋转是否正在进行
        public bool bLRBack;
        public Vector2 disRange;//distance range
        public Vector2 udAngle; //up and down angle range
        public Vector2 lrAngle; //left and right angle range
        public int LRSensitivity;
        public float UDSensitivity;
        public int LRStep;
        public float UDStep;
        public adjustView(bool bActivity)
        {
            bUDBack = false;
            bLRBack = false;
            disRange = new Vector2(20f, 20f);
            udAngle = new Vector2(15f, 35f);
            lrAngle = new Vector2(-360f, 360f);
            LRSensitivity = 0;
            UDSensitivity = 0f;
            LRStep = 5;
            UDStep = 1.2f;
        }
    }
    //shake 参数
    public struct shakeParam
    {
        public bool bShake;
        public float intensity;
        public float durTime;
        public float decay;
        public float fStart;
        public Vector3 originalPos;
        public Quaternion originalRotation;
        public shakeParam(bool bShake)
        {
            this.bShake = bShake;
            intensity = 0;
            durTime = 0;
            decay = 0;
            originalPos = Vector3.zero;
            originalRotation = new Quaternion();
            fStart = 0;
        }
    }
	public class LuaGameCamera : LuaCamera {
	   
        protected CameraCastType m_Type = CameraCastType.Normal;
        [SerializeField]protected Transform m_Target = null;
        protected pointNode m_SavePoint;
        [SerializeField]protected normalInfo m_normalInfo = new normalInfo(0);
        protected adjustView m_sensitivity = new adjustView(false);
        protected shakeParam m_shakeParam = new shakeParam(false);
        protected LuaObjWithCallFun m_callBackFun = new LuaObjWithCallFun();
        //临时变量
        private Vector3 ml_Vtmp;
        private Transform ml_Target = null;
        private Hashtable ml_paramTab = new Hashtable();
        public float ml_dis;
        public float mo_time = 0.12f;
        public int shake_intensity = 10;
        public float shake_time = 0.1f;
        public float shake_rate = 0.1f;

        public override void Awake()
        {
            base.Awake();
        }
        void Start()
        {
            if (transform.parent != null)
                ml_Target = transform.parent;
            m_SavePoint.bUseful = false;
            //ml_paramTab.Add("islocal", true);
            if (ml_Target != null)
            {
                ml_Target.localEulerAngles = Vector3.zero;
                calculatePos(false);
            }
        }
        public Transform proxyParent
        {
            get
            {
                return ml_Target;
            }
        }
        public void setProxyParent()
        {
            if (transform.parent != null)
                ml_Target = transform.parent;
        }
		protected override void ExtraRefLua()
		{
			base.ExtraRefLua();
            Game.Lua.SetTableFunction(-1, "SetLRAngle", new CSharpFunctionDelegate(Lua_SetLRAngle));
            Game.Lua.SetTableFunction(-1, "SetUDAngle", new CSharpFunctionDelegate(Lua_SetUDAngle));
            Game.Lua.SetTableFunction(-1, "LookTarget", new CSharpFunctionDelegate(Lua_LookTarget));
            Game.Lua.SetTableFunction(-1, "SavePoint", new CSharpFunctionDelegate(Lua_SavePoint));
            Game.Lua.SetTableFunction(-1, "BackPoint", new CSharpFunctionDelegate(Lua_BackPoint));
            Game.Lua.SetTableFunction(-1, "Flush", new CSharpFunctionDelegate(Lua_Flush));
            Game.Lua.SetTableFunction(-1, "SetOffSet", new CSharpFunctionDelegate(Lua_SetOffSet));
            Game.Lua.SetTableFunction(-1, "virtualPos", new CSharpFunctionDelegate(Lua_virtualPos));
            Game.Lua.SetTableFunction(-1, "UseParam", new CSharpFunctionDelegate(Lua_UseParam));
            Game.Lua.SetTableFunction(-1, "StopTween", new CSharpFunctionDelegate(Lua_StopTween));
            Game.Lua.SetTableFunction(-1, "SeparateParent", new CSharpFunctionDelegate(Lua_SeparateParent));
            Game.Lua.SetTableFunction(-1, "CombineParent", new CSharpFunctionDelegate(Lua_CombineParent));
            Game.Lua.SetTableFunction(-1, "ResetParam", new CSharpFunctionDelegate(Lua_ResetParam));
            Game.Lua.SetTableFunction(-1, "DoShake", new CSharpFunctionDelegate(Lua_DoShake));
            Game.Lua.SetTableFunction(-1, "GetMeshImage", new CSharpFunctionDelegate(Lua_GetMeshImage));
            Game.Lua.SetTableFunction(-1, "StoryGrayscaleExcute", new CSharpFunctionDelegate(Lua_StoryGrayscaleExcute));
            Game.Lua.SetTableFunction(-1, "StoryGrayscaleFinish", new CSharpFunctionDelegate(Lua_StoryGrayscaleFinish));

            //Game.Lua.SetTableFunction(-1, "RevolveAround", new CSharpFunctionDelegate(Lua_Flush));
		}
        protected override bool WidgetWriteOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "target":
                    LuaObject obj = LuaObject.GetLuaObject(lua, 3);
                    if (obj != null)
                        target = obj.transform;
                    else
                        target = null;
                    break;
                case "type":
                    type = (CameraCastType)lua.ToInteger(3);
                    return true;
                case "depth":
                    _camera.depth = lua.ToInteger(3);
                    return true;
                case "distance":
                    m_normalInfo.distance = lua.ToInteger(3);
                    return true;
                case "LRAngle":
                    SetLRAngle((float)lua.L_CheckNumber(3));
                    return true;
                case "UDAngle":
                    SetUDAngle((float)lua.L_CheckNumber(3));
                    return true;
                case "offsetX":
                    m_normalInfo.offSet.x = (float)lua.L_CheckNumber(3);
                    return true;
                case "offsetY":
                    m_normalInfo.offSet.y = (float)lua.L_CheckNumber(3);
                    return true;
                case "offsetZ":
                    m_normalInfo.offSet.z = (float)lua.L_CheckNumber(3);
                    return true;
                case "offset":
                    {
                        if (lua.Type(3) != LuaType.LUA_TTABLE)
                        {
                            Debug.LogWarning("LuaGameCamera set offset attribute parm table excepted.");
                            return true;
                        }
                        Vector3 ve = m_normalInfo.offSet;

                        lua.GetField(3, "x");
                        ve.x = (float)lua.L_CheckNumber(-1);
                        lua.Pop(1);

                        lua.GetField(3, "y");
                        ve.y = (float)lua.L_CheckNumber(-1);
                        lua.Pop(1);

                        lua.GetField(3, "z");
                        ve.z = (float)lua.L_CheckNumber(-1);
                        lua.Pop(1);

                        m_normalInfo.offSet= ve;
                    }
                    return true;
            }
            //return false;
            return base.WidgetWriteOper(lua, key);
        }
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "type":
                    lua.PushInteger((int)type);
                    return true;
                case "depth":
                    lua.PushInteger((int)_camera.depth);
                    return true;
                case "distance":
                    lua.PushNumber(m_normalInfo.distance);
                    return true;
                case "parent":
                    LuaObject pParent = transform.parent.GetComponent<LuaObject>();
                    pParent.RefLua();
                    pParent.PushThis(lua);
                    return true;
                case "ml_target":
                    LuaObject mpParent = ml_Target.GetComponent<LuaObject>();
                    mpParent.RefLua();
                    mpParent.PushThis(lua);
                    return true;
                case "target":
                    if (m_Target != null)
                    {
                        LuaObject tar = m_Target.GetComponent<LuaObject>();
                        if (tar != null)
                            tar.PushThis(lua);
                        return true;
                    }
                    return false;
                case "LRAngle":
                    lua.PushNumber(m_normalInfo.rotationLR);
                    return true;
                case "UDAngle":
                    lua.PushNumber(m_normalInfo.rotationUD);
                    return true;
                case "offsetX":
                    lua.PushNumber(m_normalInfo.offSet.x);
                    return true;
                case "offsetY":
                    lua.PushNumber(m_normalInfo.offSet.y);
                    return true;
                case "offsetZ":
                    lua.PushNumber(m_normalInfo.offSet.z);
                    return true;
                case "offset":
                    lua.NewTable();
                    lua.PushNumber(m_normalInfo.offSet.x);
                    lua.SetField(-2, "x");
                    lua.PushNumber(m_normalInfo.offSet.y);
                    lua.SetField(-2, "y");
                    lua.PushNumber(m_normalInfo.offSet.z);
                    lua.SetField(-2, "z");
                    return true;
            }
            //return false;
            return base.WidgetReadOper(lua, key);
        }

        void Update()
        {
            if (ml_Target != null)
            {
                ml_dis = Vector3.Distance(transform.localPosition, Vector3.zero);
                //transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
            }
            if (type == CameraCastType.Normal)
            {
                //左右箭头可旋转主角视角
                if (UIInput.selection == null)
                {
                    //缓动旋转
                    if (Input.GetKey(KeyCode.LeftArrow))
                    {
                        m_sensitivity.bLRBack = false;
                        m_sensitivity.LRSensitivity += m_sensitivity.LRStep;
                        m_sensitivity.LRSensitivity = (int)Mathf.Clamp((float)m_sensitivity.LRSensitivity, m_sensitivity.lrAngle.x, m_sensitivity.lrAngle.y);
                        calculatePos(false);
                    }
                    if (Input.GetKey(KeyCode.RightArrow))
                    {
                        m_sensitivity.bLRBack = false;
                        m_sensitivity.LRSensitivity -= m_sensitivity.LRStep;
                        m_sensitivity.LRSensitivity = (int)Mathf.Clamp((float)m_sensitivity.LRSensitivity, m_sensitivity.lrAngle.x, m_sensitivity.lrAngle.y);
                        calculatePos(false);
                    }
                }
                if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
                {
                    //开始回旋
                    m_sensitivity.bLRBack = true;
                }
                if (m_sensitivity.bLRBack == true)
                {
                    //缓动回旋，然后置零
                    if (m_sensitivity.LRSensitivity == 0)
                    {
                        m_sensitivity.bLRBack = false;
                    }
                    else
                    {
                        float lrStart = m_sensitivity.LRSensitivity;
                        m_sensitivity.LRSensitivity = (int)Mathf.Lerp(lrStart, 0, Time.deltaTime);
                        calculatePos(false);
                    }
                }
                //鼠标中键滚动拉近放远摄像机
//                 if (m_sensitivity.bUDBack == false)
//                 {
//                     if (Util.IsMouseOnUI() == false)
//                     {
//                         float wheelAxis = Input.GetAxis("Mouse ScrollWheel");
//                         if (wheelAxis != 0)
//                         {
//                             if (wheelAxis < 0)
//                             {
//                                 m_sensitivity.UDStep = -Mathf.Abs(m_sensitivity.UDStep);
//                             }
//                             else
//                             {
//                                 m_sensitivity.UDStep = Mathf.Abs(m_sensitivity.UDStep);
//                             }
//                             m_sensitivity.bUDBack = true;
//                         }
//                     }
//                 }
                if (m_sensitivity.bUDBack == true)
                {
                    //滚轮向前拉近：+=      滚轮向前拉远：-=
                    m_sensitivity.UDSensitivity += m_sensitivity.UDStep;
                    //print("m_wheel:" + m_sensitivity.UDSensitivity);
                    //m_sensitivity.UDSensitivity = (int)Mathf.Clamp((float)m_sensitivity.UDSensitivity, m_sensitivity.udAngle.x, m_sensitivity.udAngle.y);
                    float dwCurPoint = m_sensitivity.UDSensitivity / (m_sensitivity.udAngle.y - m_sensitivity.udAngle.x);
                    m_normalInfo.distance = Mathf.Lerp(m_sensitivity.disRange.y, m_sensitivity.disRange.x, dwCurPoint);
                    if (dwCurPoint >= 1.0001)
                    {
                        m_sensitivity.bUDBack = false;
                        m_sensitivity.UDSensitivity = m_sensitivity.udAngle.y - m_sensitivity.udAngle.x;
                    }
                    else if (dwCurPoint <= -0.001)
                    {
                        m_sensitivity.bUDBack = false;
                        m_sensitivity.UDSensitivity = 0;
                    }
                    calculatePos(false); 
                }
            } 
            shakeUpdate();
        }
        void LateUpdate()
        {
            if (ml_Target != null)
            {
                if (m_Target != null)
                {
                    ml_Target.position = m_Target.position + m_normalInfo.offSet;
                    
                    //ml_Target.localEulerAngles = Vector3.zero;
                    //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(m_Target.position - transform.position), 4*Time.deltaTime);
                    switch (type)
                    {
                        case CameraCastType.Normal:
                            break;
                        case CameraCastType.Fixed:
                            break;
                        case CameraCastType.Tracking:
                            {
                                ml_Target.eulerAngles = m_Target.eulerAngles;
                            }
                            break;
                        case CameraCastType.Interactive:
                            break;
                    }
                }
                transform.LookAt(ml_Target.position);
            }

            //DetecteTargetBeKeepedOut();
        }

        public void shakeUpdate()
        {
            if (m_shakeParam.bShake == false) return;
            if (m_shakeParam.fStart > Time.time) return;

            //if (m_shakeParam.intensity > 0)
            if (m_shakeParam.durTime > 0)
            {
                //print("shakeupdate:" + m_shakeParam.intensity+":"+Time.deltaTime);
                m_normalInfo.offSet = m_shakeParam.originalPos + UnityEngine.Random.insideUnitSphere * m_shakeParam.intensity * shake_rate;//*0.005f;
//                 transform.rotation = new Quaternion(m_shakeParam.originalRotation.x + UnityEngine.Random.Range(-m_shakeParam.intensity, m_shakeParam.intensity) * .003f,
//                                                     m_shakeParam.originalRotation.y + UnityEngine.Random.Range(-m_shakeParam.intensity, m_shakeParam.intensity) * .003f,
//                                                     m_shakeParam.originalRotation.z + UnityEngine.Random.Range(-m_shakeParam.intensity, m_shakeParam.intensity) * .003f,
//                                                     m_shakeParam.originalRotation.w + UnityEngine.Random.Range(-m_shakeParam.intensity, m_shakeParam.intensity) * .003f);

                //m_shakeParam.intensity -= m_shakeParam.decay * Time.deltaTime;
                m_shakeParam.durTime -=  Time.deltaTime;
            }
            else if (m_shakeParam.bShake)
            {
                m_shakeParam.bShake = false;
                //transform.position = m_shakeParam.originalPos;
                //transform.rotation = m_shakeParam.originalRotation;
                m_normalInfo.offSet = m_shakeParam.originalPos;
            }
        }
        /// //////////////////////////////////////////////////////
        public CameraCastType type
        {
            get { return m_Type; }
            set
            {
                if (m_Type != value)
                {
                    m_Type = value;
                    if (ml_Target!=null)
                        ml_Target.localEulerAngles = Vector3.zero;
                }
            }
        }
        public float distance
        {
            get { return m_normalInfo.distance; }
            set
            {
                m_normalInfo.distance = value;
            }
        }
        public Vector3 offset
        {
            get { return m_normalInfo.offSet; }
            set
            {
                m_normalInfo.offSet = value;
            }
        }
        public float LRAnge
        {
            get { return m_normalInfo.rotationLR; }
            set
            {
                m_normalInfo.rotationLR = value;
            }
        }
        public float UDAngle
        {
            get { return m_normalInfo.rotationUD; }
            set
            {
                m_normalInfo.rotationUD = value;
            }
        }
        public Transform target
        {
            get { return m_Target; }
            set { m_Target = value; }
        }
        /// //////////////////////////////////////////////////////
        public void ClearParam()
        {
            ml_paramTab.Clear();
        }
        public void AddParam(object key, object value)
        {
            ml_paramTab.Add(key, value);
        }
        public void CameraMove(Vector3 target)
        {
            TweenPosition _ptween = TweenPosition.Begin(gameObject, 3, target);
            _ptween.PlayForward(); 
            //iTween.MoveTo(gameObject, iTween.Hash("position", target, "islocal", true, "time", tm));
        }
        public void calculatePos(bool bSmooth)
        {
            if (ml_Target == null) return;
            double upRidus = Math.PI * (m_normalInfo.rotationUD-m_sensitivity.UDSensitivity)/180;
            double flatRidus = Math.PI * (m_normalInfo.rotationLR - 180 + m_sensitivity.LRSensitivity) / 180;
            float z = (float)(m_normalInfo.distance * Math.Cos(upRidus) * Math.Cos(flatRidus));
            float x = (float)(m_normalInfo.distance * Math.Cos(upRidus) * Math.Sin(flatRidus));
            float y = (float)(m_normalInfo.distance * Math.Sin(upRidus));
            ml_Vtmp.Set(x, y, z);

            if (bSmooth)
            {
                
                if (ml_paramTab.Count != 0)
                {
                    //print("calculatePos:"+ ml_Vtmp.ToString());
                    if (ml_paramTab.ContainsKey("islocal")) ml_paramTab["islocal"] = true;
                    else ml_paramTab.Add("islocal", true);
                    if (ml_paramTab.ContainsKey("position")) ml_paramTab["position"] = ml_Vtmp;
                    else ml_paramTab.Add("position", ml_Vtmp);
  
                    iTween.MoveTo(gameObject, ml_paramTab);
                }
                else
                {
                    CameraMove(ml_Vtmp);
                }
            }
            else
            {
                transform.localPosition = ml_Vtmp;
            }
        }
        /// <summary>
        /// virtual calculate the position which distance=dis, eulerAngles.y=lr, eulerAngles.x = ud;
        /// </summary>
        /// <param name="dis"></param>the distance of this.gameobject
        /// <param name="lr"></param> the angle of left-right
        /// <param name="ud"></param> the angle of up-down
        /// <returns></returns>
        public Vector3 virtualPos(float dis, float lr, float ud)
        {
            float upRidus = Mathf.Deg2Rad * ud;
            float flatRidus = Mathf.Deg2Rad * lr;
            float z = -dis * Mathf.Cos(upRidus) * Mathf.Cos(flatRidus);
            float x = -dis * Mathf.Cos(upRidus) * Mathf.Sin(flatRidus);
            float y = dis * Mathf.Sin(upRidus);
            return new Vector3(x, y, z);
        }
        /// <summary>                                              
        /// make a savePoint
        /// </summary>
        public void SavePoint()
        {
            m_SavePoint.bUseful = true;
            m_SavePoint.type = type;
            m_SavePoint.rotationUD = m_normalInfo.rotationUD;
            m_SavePoint.rotationLR = m_normalInfo.rotationLR;
            m_SavePoint.distance = m_normalInfo.distance;
            m_SavePoint.target = m_Target;
        }
        /// <summary>
        /// back to the pre savePoint
        /// </summary>
        /// <param name="bSmooth"></param>where smooth move
        public void BackPoint(bool bSmooth)
        {
            if (m_SavePoint.bUseful)
            {
                type = m_SavePoint.type;
                m_normalInfo.distance = m_SavePoint.distance;
                m_normalInfo.rotationUD = m_SavePoint.rotationUD;
                m_normalInfo.rotationLR = m_SavePoint.rotationLR;
                m_Target = m_SavePoint.target;
                m_SavePoint.bUseful = false;
                calculatePos(bSmooth);
            }
        }
        public void SetLRAngle(float fAngle)
        {
            m_normalInfo.rotationLR = fAngle;
        }
        public void SetUDAngle(float uAngle)
        {
            m_normalInfo.rotationUD = uAngle;
        }
        IEnumerator WaitShake(int dwType, float fDelay)
        {
            yield return new WaitForSeconds(fDelay);
            DoShake(dwType, 0, 0);
        }
        public void DoShake(int dwtype, float fDelay, float durtime = 0)
        {
           
            switch (dwtype)
            {
                case 1:
                    m_shakeParam.intensity = 0.6f;
                    m_shakeParam.durTime = 0.08f;
                    break;
                case 2:
                    m_shakeParam.intensity = 1;
                    m_shakeParam.durTime = 0.2f;
                    break;
                case 3:
                    m_shakeParam.intensity = 1;
                    m_shakeParam.durTime = 0.3f;
                    break;
                case 4:
                    m_shakeParam.intensity = 2;
                    m_shakeParam.durTime = 0.1f;
                    break;
                case 5:
                    m_shakeParam.intensity = 2;
                    m_shakeParam.durTime = 0.2f;
                    break;
                case 6:
                    m_shakeParam.intensity = 2;
                    m_shakeParam.durTime = 0.3f;
                    break;
                case 7:
                    m_shakeParam.intensity = 3;
                    m_shakeParam.durTime = 0.1f;
                    break;
                case 8:
                    m_shakeParam.intensity = 3;
                    m_shakeParam.durTime = 0.2f;
                    break;
                case 9:
                    m_shakeParam.intensity = 3;
                    m_shakeParam.durTime = 0.3f;
                    break;
                case 10:
                    m_shakeParam.intensity = 0.1f;
                    m_shakeParam.durTime = 0.1f;
                    break;
                case 11:
                    m_shakeParam.intensity = 0.2f;
                    m_shakeParam.durTime = 0.1f;
                    break;
                case 12:
                    m_shakeParam.intensity = 0.5f;
                    m_shakeParam.durTime = 0.1f;
                    break;
                case 13:
                    m_shakeParam.intensity = 0.7f;
                    m_shakeParam.durTime = 0.1f;
                    break;
                case 14:
                    m_shakeParam.intensity = 0.3f;
                    m_shakeParam.durTime = 1.8f;
                    break;
                default:
                    break;
            }
            if (durtime > 0 )
            {
                m_shakeParam.intensity = dwtype;
                m_shakeParam.durTime = durtime;
            } 
            if (m_shakeParam.bShake == false)
            {
                m_shakeParam.originalPos = m_normalInfo.offSet;
            }
            //print("========LuaGame:camera:DoShake:" + dwtype + ":" + fDelay + ":" + durtime + ":" + ":" + m_shakeParam.intensity + ":" + m_shakeParam.durTime + ":" + shake_rate +":"+ Time.time);
            if (fDelay > 0f)
                StartCoroutine(WaitShake(dwtype, fDelay));
            m_shakeParam.fStart = Time.time + fDelay;
            m_shakeParam.bShake = true;
        }
        IEnumerator WaitShake(float fDelay)
        {
            yield return new WaitForSeconds(fDelay);
            ml_paramTab.Clear();
            ml_paramTab.Add("time", mo_time);
            ml_paramTab.Add("easeType", iTween.EaseType.linear);
            BackPoint(true);
        }
        public void DoMove(float height, float runtime, float allTime, float delay = 0, float a = 0)
        {
            SavePoint();
            ml_paramTab.Clear();
            ml_paramTab.Add("time", runtime);
            ml_paramTab.Add("delay", delay);
            ml_paramTab.Add("easeType", iTween.EaseType.linear);
            m_normalInfo.distance = m_normalInfo.distance - height;
            calculatePos(true);
            StartCoroutine(WaitShake(allTime));
        }
        public void UseParam(normalInfo ninfo)
        {
            m_normalInfo = ninfo;
        }
        /// //////////////////////////////////////////////////////
        private void onstart(LuaObjWithCallFun callBackFun)
        {
            //print("luaGameCamera:onstart.................");
            if (m_callBackFun.dwStartFun == 0) return;
            ILuaState lua = Game.LuaApi;
            lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, m_callBackFun.dwStartFun);

            if (lua.PCall(0, 0, 0) != 0)
            {
                Debug.LogWarning(lua.ToString(-1));
                lua.Pop(1);
            }

            lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, callBackFun.dwStartFun);
        }
        private void onupdate(LuaObjWithCallFun callBackFun)
        {
            
        }
        public List<EventDelegate> onTweenStop = new List<EventDelegate>();
        private void oncomplete(LuaObjWithCallFun callBackFun)
        {
            //print("luaGameCamera:oncomplete:................."+callBackFun);
            EventDelegate.Execute(onTweenStop);
			ml_paramTab.Clear();
            if (m_callBackFun.dwConpleteFun == 0) return;
            ILuaState lua = Game.LuaApi;
            lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, m_callBackFun.dwConpleteFun);

            if (lua.PCall(0, 0, 0) != 0)
            {
                Debug.LogWarning(lua.ToString(-1));
                lua.Pop(1);
            }

            //lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, callBackFun.dwStartFun);
            //m_callBackFun.dwConpleteFun = 0;
        }

        public void LookTarget(Transform pLuaTarget, bool bSmooth=false)
        {
            if (pLuaTarget == null)
            {
                if (m_Target != null)
                {
                    transform.parent.position = m_Target.transform.position + m_normalInfo.offSet;
                    m_Target = null;
                }
                calculatePos(false);
                return;
            }
            Transform pTarget = pLuaTarget.transform;
            m_Target = pTarget;
            calculatePos(bSmooth);
        }
        public void SeparateParent()
        {
            transform.parent = null;
        }
        public void CombineParent()
        {
            transform.parent = ml_Target;
            if (target != null)
                calculatePos(false);
        }
        public void StopTween()
        {
            iTween.Stop(gameObject);
        }
        //-----------------------------
        int Lua_LookTarget(ILuaState lua)
        {
            LuaObject pLuaTarget = LuaObject.GetLuaObject(lua, 2);
            bool bSmooth = false;
            if (lua.Type(3) == LuaType.LUA_TBOOLEAN)
            {
                bSmooth = lua.ToBoolean(3);
            }
            if (pLuaTarget == null)
                LookTarget(null, bSmooth);
            else
                LookTarget(pLuaTarget.transform, bSmooth);
            return 0;
        }
        public void ResetParam(Transform pTarget)
        {
            m_normalInfo.offSet = ml_Target.position - pTarget.position;
            m_normalInfo.distance = Vector3.Distance(transform.position, pTarget.position);
            this.UDAngle = transform.eulerAngles.x;
            this.LRAnge = transform.eulerAngles.y;
            target = pTarget;
            calculatePos(false);
        }
        int Lua_ResetParam(ILuaState lua)
        {
            LuaObject pLuaTarget = LuaObject.GetLuaObject(lua, 2);
            if (pLuaTarget == null)
            {
                Debug.LogWarning("LuaGameCamera ResetParam param1 is not luaObject....");
                return 0;
            }

            bool bChangeTarget = false;
            if (lua.Type(3) == LuaType.LUA_TBOOLEAN)
            {
                bChangeTarget = lua.ToBoolean(3);
            }
            ResetParam(pLuaTarget.transform);

            return 0;
        }
        int Lua_SetLRAngle(ILuaState lua)
        {
            float fAngle = (float)lua.L_CheckNumber(2);
            SetLRAngle(fAngle);
            return 0;
        }
        int Lua_SetUDAngle(ILuaState lua)
        {
            float uAngle = (float)lua.L_CheckNumber(2);
            SetUDAngle(uAngle);
            return 0;
        }
        int Lua_SavePoint(ILuaState lua)
        {
            SavePoint();
            return 0;
        }
        int Lua_BackPoint(ILuaState lua)
        {
            bool bSmooth = false;
            if (lua.Type(2) == LuaType.LUA_TBOOLEAN)
            {
                bSmooth = lua.ToBoolean(2);
            }
            BackPoint(bSmooth);
            return 0;
        }
        int Lua_Flush(ILuaState lua)
        {
            bool bSmooth = false;
            if (lua.Type(2) == LuaType.LUA_TBOOLEAN)
            {
                bSmooth = lua.ToBoolean(2);
            }
            if (lua.Type(3) == LuaType.LUA_TTABLE)
            {
                lua.PushValue(3);
                ml_paramTab = LuaItween.ItweenParamHasHTable(lua, m_callBackFun);
                if (m_callBackFun.dwConpleteFun != 0)
                {
                    ml_paramTab.Add("oncompleteparams", m_callBackFun);
                }
                if (m_callBackFun.dwStartFun != 0)
                {
                    ml_paramTab.Add("onstartparams", m_callBackFun);
                }
                if (m_callBackFun.dwUpdateFun != 0)
                {
                    ml_paramTab.Add("onupdateparams", m_callBackFun);
                }

                ml_paramTab.Add("onstarttarget", gameObject);
                ml_paramTab.Add("onupdatetarget", gameObject);
                ml_paramTab.Add("oncompletetarget", gameObject); 
            }
            calculatePos(bSmooth);
            return 0;
        }
        int Lua_SetOffSet(ILuaState lua)
        {
            float x = (float)lua.L_CheckNumber(2);
            float y = (float)lua.L_CheckNumber(3);
            float z = (float)lua.L_CheckNumber(4);
            m_normalInfo.offSet.Set(x, y, z);
            return 0;
        }
        int Lua_virtualPos(ILuaState lua)
        {
            float dis = (float)lua.L_CheckInteger(2);
            float lr = (float)lua.L_CheckInteger(3);
            float ud = (float)lua.L_CheckInteger(4);
            Vector3 vpos = virtualPos(dis, lr, ud);
            lua.NewTable();
            lua.PushNumber(vpos.x);
            lua.SetField(-2, "x");
            lua.PushNumber(vpos.y);
            lua.SetField(-2, "y");
            lua.PushNumber(vpos.z);
            lua.SetField(-2, "z");
            return 1;
        }
        int Lua_UseParam(ILuaState lua)
        {
            if (lua.Type(2) != LuaType.LUA_TTABLE)
            {
                Debug.LogWarning("Lua Game Camera UseParam parm table excepted.");
                return 0;
            }
            lua.PushNil();
            while (lua.Next(-2))
            {
                string key = lua.L_CheckString(-2);
                switch (key)
                {
                    case "distance":
                        m_normalInfo.distance = (float)lua.L_CheckNumber(-1);
                        break;
                    case "offset":
                        m_normalInfo.offSet = LuaItween.GetVector3(lua, -1);
                        break;
                    case "LRAngle":
                        m_normalInfo.rotationLR = (float)lua.L_CheckNumber(-1);
                        break;
                    case "UDAngle":
                        m_normalInfo.rotationUD = (float)lua.L_CheckNumber(-1);
                        break;
                    default:
                        break;
                }
                lua.Pop(1);
            }
            lua.Pop(1);
            return 0;
        }
        int Lua_StopTween(ILuaState lua)
        {
            StopTween();
            return 0;
        }
        int Lua_SeparateParent(ILuaState lua)
        {
            SeparateParent();
            return 0;
        }
        int Lua_CombineParent(ILuaState lua)
        {
            CombineParent();
            return 0;
        }
        int Lua_DoShake(ILuaState lua)
        {
            float intensity = (float)lua.ToNumber(2);
            float durtime = (float)lua.ToNumber(3);

            m_shakeParam.intensity = intensity;
            m_shakeParam.durTime = durtime;

            if (m_shakeParam.bShake == false)
            {
                m_shakeParam.originalPos = m_normalInfo.offSet;
            }
            m_shakeParam.fStart = Time.time;
            m_shakeParam.bShake = true;
            return 0;
        }
        int Lua_GetMeshImage(ILuaState lua)
        {
            LuaMeshImage obj = GetComponentInChildren<LuaMeshImage>();
            if (obj != null)
            {
                obj.PushThis(lua);
                return 1;
            }
            return 0;
        }

        int Lua_StoryGrayscaleExcute(ILuaState lua)
        {
            GrayscaleEffect obj = this.GetComponent<GrayscaleEffect>();

            if (obj == null)
            {
                obj = this.gameObject.AddComponent<GrayscaleEffect>();
            }
            obj.shader = Shader.Find("Hidden/Grayscale Effect");
            obj.enabled = true;

            return 0;   
        }

        int Lua_StoryGrayscaleFinish(ILuaState lua) { 
             
            GrayscaleEffect obj = this.GetComponent<GrayscaleEffect>();
            if (obj)
                obj.enabled = false;
            return 0;
        }
	}
}