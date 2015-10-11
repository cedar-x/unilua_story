using UnityEngine;
using System.Collections;
using UniLua;

namespace Hstj
{

    public interface ICameraRenderListener
    {
        void BeforeRender(LuaCamera pCamera);

        void AfterRender(LuaCamera pCamera);

    }
    public struct RenderSettingDesc
    {
        public Color ambientLight;
        public float ambientIntensity;
        public bool bFog;
        public Color fogColor;
        public float fogDensity;
        public float fogEndDistance;
        public FogMode fogMode;
        public float fogStartDistance;
        public Material skybox;
        public bool bEnableMainLight;
    }

	public class LuaCamera : LuaObject 
    {
		protected Camera _camera;
        protected bool m_bManualRender=false;
        protected RenderSettingDesc m_renderSetting;
        public ICameraRenderListener CameraListener;

        public override void Awake()
        {
            base.Awake();

            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            m_renderSetting = new RenderSettingDesc();
            m_renderSetting.ambientLight = new Color(1.0f,1.0f,1.0f);
            m_renderSetting.ambientIntensity = 0.7f;
            m_renderSetting.bFog = false;
            m_renderSetting.fogColor = Color.white;
            m_renderSetting.fogDensity = 0;
            m_renderSetting.fogEndDistance = 85.0f;
            m_renderSetting.fogMode = FogMode.Linear;
            m_renderSetting.fogStartDistance = 30;
            m_renderSetting.skybox = null;
        }
        public float fieldOfView
        {
            get
            {
                return _camera.fieldOfView;
            }
            set
            {
                _camera.fieldOfView = value;
            }
        }
		public Camera GetCamera() {
			return _camera;
		}

		protected override void ExtraRefLua()
		{
			Game.Lua.SetTableFunction(-1, "SetClearColor", new CSharpFunctionDelegate(Lua_SetClearColor));
			Game.Lua.SetTableFunction(-1, "SetCullMask", new CSharpFunctionDelegate(Lua_SetCullMask));
			Game.Lua.SetTableFunction(-1, "SetDepth", new CSharpFunctionDelegate(Lua_SetDepth));
			Game.Lua.SetTableFunction(-1, "SetTargetTexture", new CSharpFunctionDelegate(Lua_SetTargetTexture));
			Game.Lua.SetTableFunction(-1, "LookAt", new CSharpFunctionDelegate(Lua_LookAt));
			Game.Lua.SetTableFunction(-1, "SetCameraMove", new CSharpFunctionDelegate(Lua_SetCameraMove));
			Game.Lua.SetTableFunction(-1, "SetFieldOfView", new CSharpFunctionDelegate(Lua_SetFieldOfView));
            Game.Lua.SetTableFunction(-1, "SetRenderPath", new CSharpFunctionDelegate(Lua_SetRenderPath));
            Game.Lua.SetTableFunction(-1, "SetProjection", new CSharpFunctionDelegate(Lua_SetProjection));
            Game.Lua.SetTableFunction(-1, "SetIterations", new CSharpFunctionDelegate(Lua_SetIterations));
        }
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "FieldOfView":
                    lua.PushNumber(_camera.fieldOfView);
                    break;
                default:
                    return false;
            }
            return true;
        }
        protected override bool WidgetWriteOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "FieldOfView":
                    _camera.fieldOfView = (float)lua.L_CheckNumber(3);
                    break;
                default:
                    return false;
            }
            return true;
        }

		public void SetClearFlags(CameraClearFlags flag)
		{
			_camera.clearFlags = flag;
		}

		public void SetClearColor(float a, float r, float g, float b)
		{
			Color color = Color.black;
			color.a = a;
			color.r = r;
			color.g = g;
			color.b = b;
			_camera.backgroundColor = color;
		}

		public void SetCullMask(int dwCullMask)
		{
			_camera.cullingMask = dwCullMask;
		}

		public void SetFieldOfView(int fieldOfView)
		{
			_camera.fieldOfView = fieldOfView;
		}

		public void SetDepth(int dwDepth)
		{
			_camera.depth = (float)dwDepth;
		}

		public void SetTargetTexture(RenderTexture target)
		{
			_camera.targetTexture = target;
		}

		public RenderTexture GetTargetTexture()
		{
			return _camera.targetTexture;
		}

        public RenderSettingDesc RenderSetting
        {
            get { return m_renderSetting; }
        }

        public void SetAmbientIntensity(float fIntensity)
        {
            m_renderSetting.ambientIntensity = fIntensity;
        }

        public void SetAmbientColor(Color color)
        {
            m_renderSetting.ambientLight = color;
        }

        public void StartRender()
        {
            if (CameraListener != null)
            {
                CameraListener.BeforeRender(this);
            }

            _camera.Render();

            if (CameraListener != null)
            {
                CameraListener.AfterRender(this);
            }
        }

        public bool ManualRender
        {
            get
            {
                return m_bManualRender;
            }

            set
            {
                if (m_bManualRender == value)
                    return;

                m_bManualRender = value;

                if (value)
                {
                    gameObject.SetActive(false);

                }
                else
                {
                    gameObject.SetActive(true);

                }
            }
        }

        void OnEnable()
        {
            if (m_bManualRender)
            {
                gameObject.SetActive(false);
            }
        }

        protected override void OnDestroy()
        {
            //CameraManualRender.RemoveCamera(this);
            base.OnDestroy();
        }

		int Lua_SetClearFlags(ILuaState lua)
		{
			CameraClearFlags flag = (CameraClearFlags)lua.L_CheckInteger(2);
			SetClearFlags(flag);
			return 0;
		}

		int Lua_SetClearColor(ILuaState lua)
		{
			float a = (float)lua.L_CheckNumber(2);
			float r = (float)lua.L_CheckNumber(3);
			float g = (float)lua.L_CheckNumber(4);
			float b = (float)lua.L_CheckNumber(5);
			SetClearColor(a,r,g,b);
			return 0;
		}

		int Lua_SetCullMask(ILuaState lua)
		{
			int dwMasx = lua.L_CheckInteger(2);
			SetCullMask(dwMasx);
			return 0;
		}

		int Lua_SetFieldOfView(ILuaState lua)
		{
			int dwValue = lua.L_CheckInteger(2);
			SetFieldOfView(dwValue);
			return 0;
		}

		int Lua_SetDepth(ILuaState lua)
		{
			int dwDepth = lua.L_CheckInteger(2);
			SetDepth(dwDepth);
			return 0;
		}

		int Lua_SetTargetTexture(ILuaState lua)
		{
            LuaObject obj = LuaObject.GetLuaObject(lua, 2);
            if (obj == null)
                return 0;
            

			return 0;
		}
		int Lua_LookAtWithPos(ILuaState lua)
		{
			float fPosX = (float)lua.L_CheckNumber(2);
			float fPosY = (float)lua.L_CheckNumber(3);
			float fPosZ = (float)lua.L_CheckNumber(4);
			Vector3 pos = new Vector3 (fPosX, fPosY, fPosZ);
			_camera.transform.LookAt (pos);
			return 0;
		}
		int Lua_LookAt(ILuaState lua)
		{
			if (!lua.IsNoneOrNil (4)) 
			{
				return Lua_LookAtWithPos(lua);	
			}
			LuaObject pLuaTarget = LuaObject.GetLuaObject(lua,2);
			if(pLuaTarget == null)
				return 0;
			Transform pTarget = pLuaTarget.transform;
			_camera.transform.LookAt(pTarget.position);
			return 0;
		}
		int Lua_SetCameraMove(ILuaState lua)
		{
			float fPosX = (float)lua.L_CheckNumber(2);
			float fPosY = (float)lua.L_CheckNumber(3);
			float fPosZ = (float)lua.L_CheckNumber(4);
			TweenPosition _ptween = TweenPosition.Begin(gameObject, 3, new Vector3(fPosX, fPosY, fPosZ));
			_ptween.PlayForward(); 		
			return 0;
		}

        int Lua_SetRenderPath(ILuaState lua)
        {
            RenderingPath path = (RenderingPath)lua.L_CheckInteger(2);
            switch (path)
            {
                case RenderingPath.UsePlayerSettings:
                case RenderingPath.VertexLit:
                case RenderingPath.Forward:
                case RenderingPath.DeferredLighting:
                    _camera.renderingPath = path;
                    break;
            }
            return 0;
        }

        int Lua_SetProjection(ILuaState lua)
        {
            int t = lua.L_CheckInteger(2);
            if (t == 1)
            {
                _camera.orthographic = true;
            }
            else
            {
                _camera.orthographic = false;
            }
            return 0;
        }

        int Lua_SetIterations(ILuaState lua)
        {
            return 0;
        }

	}

}