using UnityEngine;
using System.Collections;
using System;
using UniLua;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

//author: 代军礼
//Desc: 游戏管理器

namespace Hstj
{
    enum StartMode
    {
        DirectStart=1,
        LaunchStart=2,
    }

    public class Game : MonoBehaviour
    {
        public string _szStartScript = "Project.lua";
        public int _nStoryScript = 1001; 
        private int _luaIndex = 0;
        private int _pfnOnLevelLoaded = 0;
        private static LuaContex _lua = null;
        private static Game _instance = null;
		private long _nUpdateInterval = 100;
        private long _nLastUpdateTime = 0; 
		private LuaItween _movementMgr = null;
        private Camera _pMainCamera = null;
        private LuaGameCamera _pExtraCamera = null;
        private LuaObject _pCameraParent = null;
        public static DateTime _utcOrigin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private long _sysTime = 0;
        private StartMode m_eStartMode;

        private List<GCHandle> _lstLuaFunctions = new List<GCHandle>();

        void Awake()
        {
            DontDestroyOnLoad(transform.gameObject);
        }

        void Start()
        {

        }

        private void SetStartType()
        {
        }

        //regist lua function 
        private void RefLua()
        {
            LuaApi.NewTable();
            _luaIndex = LuaApi.L_Ref(LuaDef.LUA_REGISTRYINDEX);

            this.PushThis();
            _lua.BegTableFunction(_lstLuaFunctions);
            //_lua.SetTableFunction(-1, "SyncServerTime", new CSharpFunctionDelegate(Lua_SyncServerTime));
           

            LuaApi.Pop(1);
            _lua.EndTableFunction(_lstLuaFunctions);
        }

        private void UnrefLua()
        {
            if (_luaIndex != 0)
            {
                LuaApi.L_Unref(LuaDef.LUA_REGISTRYINDEX, _luaIndex);
                _luaIndex = 0;
            }
        }

        public void PushThis()
        {
            LuaApi.RawGetI(LuaDef.LUA_REGISTRYINDEX, _luaIndex);
        }

        public static Game Instance
        {
            get { return _instance; }
        }
		 
        public static LuaContex Lua
        {
            get { return _lua; }
        }

        public static ILuaState LuaApi
        {
            get 
            {
                if (_lua != null)
                {
                    return _lua.Lua; 
                }
                else
                {
                    return null;
                }
                
            }
        }

		public Camera GetMainCamera()
		{
			return _pMainCamera;
		}

        // Update is called once per frame
        void Update()
        {

            UpdateScript();

        }

        void LateUpdate()
        {

            CollectMemory();
        }

        private void OnLoadStartResources(int code, string name, System.Object objParm)
        {
            if (code != 0)
            {
                Debug.Log("$Load Start Resources Failed. Code:" + code);
                return;
            }

            long fStart = GetTime();
#if !UNITY_EDITOR
            string resname = "Scripts";
            AssetBundle pScriptBundle = ResLoader.GetBundle(ref resname);
            if (pScriptBundle == null)
            {
                Debug.Log("$Unknow Error Get Scripts Bundle Failed. ");
                return;
            }
            ResLoader.RemoveBundle("Scripts");

            resname = "SceneLoading";
            AssetBundle pSceneLoading = ResLoader.GetBundle(ref resname);
            if(pSceneLoading == null)
            {
                Debug.Log("$Unknow Error Get SceneLoading Bundle Failed. ");
            }
            ResLoader.RemoveBundle("SceneLoading");

            SceneLoad.SceneLoadBundle = pSceneLoading;
            _lua.SetScriptBundle(pScriptBundle);
#endif
            
            _lua.DoFile(_szStartScript);
            long fElapse = GetTime() - fStart;
            Debug.Log(string.Format("$Load Script Comsume {0} Second", fElapse * 0.001));

            StartCoroutine(Call_Main());
        }

        public long GetTime()
        {
            return  (long)(DateTime.UtcNow - _utcOrigin).TotalMilliseconds;
        }

		private static float _fLastCollectTime = 0;
        private static float _fLastCollectLuaTime = 0;
		public void CollectMemory(bool bImmdiately=false)
		{
			if(bImmdiately || Time.time - _fLastCollectTime > 30)
            {
				_fLastCollectTime = Time.time;
				Resources.UnloadUnusedAssets();
				//GC.Collect();
			}

            if(bImmdiately || Time.time -  _fLastCollectLuaTime > 35)
            {
                _fLastCollectLuaTime = Time.time;
                LuaMemoryCollect();
            }
		}

        public void LuaMemoryCollect()
        {
        }

        public int GetLuaIndex()
        {
            return _luaIndex;
        }
		
        void OnDestroy()
        {
            
        }

        void LoadStartRes()
        {
#if !UNITY_EDITOR && !VER_WIN_TINY
            List<string> lstReses = new List<string>();
            //lstReses.Add("UI/Base");
            lstReses.Add("Scripts");
            ResManager.AsyncLoadBundle(lstReses, OnLoadStartResources, null);
            Debug.Log("######LoadStartRes###Complete##");
#elif VER_WIN_TINY
            List<string> lstReses = new List<string>();
            lstReses.Add("SceneLoading");
            lstReses.Add("Scripts");
            ResManager.AsyncLoad(lstReses, AsyncLoadMode.Background,OnLoadStartResources, null);
#else
            OnLoadStartResources(0,null,null);
#endif
        }

		void OnApplicationQuit()
		{
			Debug.Log("Game destroyed");
		}

        private bool InitLua()
        {
            return true;
        }

        private void DelaySceneWasLoaded()
        {

        }
        private void LevelResLoaded(int code, string name, System.Object objParm)
        {
        }

        public void LoadScene(string name, List<string> lstPreloads)
        {
        }

        void OnLevelWasLoaded()
        {
        }

        //public IEnumerator AsyncLoadLevel(string name)
        //{
        //    //HUIManager.GetUILoading().ShowUI();

        //    AsyncOperation asyncOper = Application.LoadLevelAsync(name);
        //    yield return asyncOper;

        //    Debug.Log("%%load scene async complete.");
        //    HUIManager.Clear();
        //    Scene.Instance.Clear();
        //    ResManager.Clear();
        //    AudioManager.Clear();

        //    HUIManager.Init();

        //    Resources.UnloadUnusedAssets();
        //    GC.Collect();

        //    LuaMemoryCollect();

        //    if (_pfnOnLevelLoaded != 0)
        //    {
        //        ILuaState lua = LuaApi;
        //        lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, _pfnOnLevelLoaded);
        //        if (lua.PCall(0, 0, 0) != 0)
        //        {
        //            Debug.LogWarning(LuaApi.ToString(-1));
        //            LuaApi.Pop(1);
        //        }
        //    }
        //    //HUIManager.GetUILoading().ShowUI();
        //    ResLoader.SetSplashVisible(false);
        //}

        private void OnServerClose()
        {
            if (_luaIndex == 0)
                return;

            ILuaState lua = LuaApi;

            PushThis();
            lua.PushString("OnServerClose");
            lua.GetTable(-2);
            if (lua.Type(-1) != LuaType.LUA_TFUNCTION)
            {
                lua.Pop(2);
                return;
            }
            lua.PushValue(-2);
            if (lua.PCall(1, 0, 1) != 0)
            {
                Debug.LogWarning(lua.ToString(-1));
                lua.Pop(1);
            }

            lua.Pop(1);
        }

        private void OnLuaWarn(int level,string info)
        {
            if(level == 0)
            {
                Debug.Log(info);
            }
            else
            {
                Debug.LogWarning(info);
            }
            
        }

        private IEnumerator Call_Main()
        {
            if (_luaIndex == 0)
                yield break;

            LuaApi.GetGlobal("main");
            if (LuaApi.Type(-1) != LuaType.LUA_TFUNCTION)
            {
                Debug.LogWarning("not found main function in lua script. can't start game.");
                LuaApi.Pop(1);
                yield break;
            }

            this.PushThis();
            if (LuaApi.PCall(3, 0, 1) != 0)
            {
                Debug.LogWarning(LuaApi.ToString(-1));
                LuaApi.Pop(1);
            }
        }

        private void UpdateScript()
        {
            if (_luaIndex == 0)
                return;
            
            long nNow = GetTime();
            long interval = nNow - _nLastUpdateTime;
            if(interval < _nUpdateInterval)
                return;
            _nLastUpdateTime = nNow;
            //int luatop = LuaApi.GetTop();

            PushThis();
            LuaApi.PushString("Update");
            LuaApi.GetTable(-2);

            if (LuaApi.Type(-1) != LuaType.LUA_TFUNCTION)
            {
                LuaApi.Pop(2);
                return;
            }

            LuaApi.PushValue(-2);
            if (LuaApi.PCall(1, 0, 0) != 0)
            {
                Debug.LogWarning(LuaApi.ToString(-1));
                LuaApi.Pop(1);
            }

            LuaApi.Pop(1);
            //if (luatop != LuaApi.GetTop()) throw new Exception("lua stack not correct, have bug in code!");
        }

		private void RefLuaRecurisve(LuaObject pParent)
		{
			for(int i=0; i<pParent.transform.childCount; ++i)
			{
				Transform pChild = pParent.transform.GetChild(i);
				LuaObject pLuaObject = pChild.GetComponent<LuaObject>();
				if(pLuaObject == null){
					pLuaObject = pChild.gameObject.AddComponent<LuaObject>();
					pLuaObject.RefLua();
				}
				RefLuaRecurisve(pLuaObject);
			}
		}

    }

}
