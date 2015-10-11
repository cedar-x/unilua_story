using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using UniLua;
using Hstj;

//author: 代军礼
//Desc: Lua方便封装


public class LuaContex
{

    class mtype
    {
        public LuaType t;

        public int index;
        public string str;
        public double num;
        public bool b;
    }

    public delegate void PfnRemoteCallback(byte[] buffer, int nOffset, int len, System.Object pUserData);
    public delegate void PfnLog(int dwLevel, string info);

	private ILuaState _lua;

    private string _rootPath = "";
    private string _configPath = "";
    private string _scriptPath = "";
    private mtype[] _callparms = new mtype[24];
    private int _callparmCount = 0;
    private bool m_bUseUtf8 = false;
    private int m_nFloatAccuracy = 3;
	//用于同步 C 里面的 time 函数返回的值
	public static DateTime _utcOrigin = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
    PfnLog _pfnLog = null;

    private List<GCHandle> _lstSetTableFunctions = null;
    private List<GCHandle> m_pfnHandles = new List<GCHandle>();

    private UnityEngine.AssetBundle _scriptBundle = null;
    private bool m_bEnableUnloadScriptBundle = false;
    public static string ScriptBundleName = "Scripts";

    public LuaContex(PfnLog pfnLog,ILuaState pLua)
    {
        _pfnLog = pfnLog;

        _lua = pLua;

        if (_lua == null)
        {
            //_lua = LuaAPI.NewState(IntPtr.Zero);
            _lua.L_OpenLibs();
        }

        AddGlobalFunction("pcall", Lua_pcall);

        AddGlobalFunction("print", Lua_Print);
    
        AddGlobalFunction("warn", Lua_Warn);

        AddGlobalFunction("dofile", Lua_DoFile);

        AddGlobalFunction("_useutf8", Lua_UseUtf8);

        AddGlobalFunction("_ParseInteger", Lua_ParseInteger);

        AddGlobalFunction("_floatAccuracy", Lua_FloatAccuracy);

        AddGlobalFunction("GetTime", Lua_GetTime);

        AddGlobalFunction("GetTimeInfo", Lua_GetTimeInfo);

        AddGlobalFunction("loadstring", Lua_loadstring);

        for (int i = 0; i < _callparms.Length; ++i)
        {
            _callparms[i] = new mtype();
        }

        //UniLua.Tools.ULDebug.Log = WriteInfo;
        //UniLua.Tools.ULDebug.LogError = WriteError;
	}

    public bool EnableUnloadScriptBundle
    {
        get { return m_bEnableUnloadScriptBundle; }
        set { m_bEnableUnloadScriptBundle = value; }
    }

    public void ClearScriptBundle()
    {
        if (_scriptBundle == null)
            return;
        _scriptBundle.Unload(true);
        _scriptBundle = null;
    }

    private void AddGlobalFunction(string funName, CSharpFunctionDelegate pfn,int nParm=0)
    {
        if (nParm == 0)
        {
            //m_pfnHandles.Add(_lua.PushCSharpFunction(pfn));
            _lua.SetGlobal(funName);
        }
        else
        {
            //m_pfnHandles.Add(_lua.PushCSharpClosure(pfn,nParm));
            _lua.SetGlobal(funName);
        }
    }

    private string ConstructString(ILuaState lua)
    {
        int n = lua.GetTop();
        if (n == 1)
        {
            return lua.ToString(1);
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 1; i <= n; ++i)
        {
            LuaType t = lua.Type(i);
            switch (t)
            {
                case LuaType.LUA_TSTRING:
                    sb.Append(lua.ToString(i));
                    break;
                case LuaType.LUA_TNIL:
                    sb.Append("nil");
                    break;
                case LuaType.LUA_TNUMBER:
                    sb.Append(lua.ToString(i));
                    break;
                case LuaType.LUA_TTABLE:
                    sb.Append("table");
                    break;
                case LuaType.LUA_TFUNCTION:
                    sb.Append(string.Format("function"));
                    break;
                case LuaType.LUA_TBOOLEAN:
                    sb.Append(lua.ToBoolean(i));
                    break;
                default:
                    sb.Append("unknow");
                    break;
            }

            if (i + 1 <= n)
            {
                sb.Append(",");
            }

        }
        return sb.ToString();
    }

    public static List<string> GetStringArray(ILuaState lua, int tbIndex)
    {
        if (lua.Type(tbIndex) != LuaType.LUA_TTABLE)
        {
            return null;
        }

        List<string> arrays = null;
        lua.PushValue(tbIndex);
        lua.PushNil();
        while (lua.Next(-2))
        {
            if (lua.Type(-1) == LuaType.LUA_TSTRING)
            {
                if (arrays == null)
                    arrays = new List<string>();

                arrays.Add(lua.ToString(-1));
            }
            lua.Pop(1);
        }
        lua.Pop(1);

        return arrays;
    }

    public static float GetTableNumber(ILuaState lua,int index,string field)
    {
        float fNumber = 0;
        lua.GetField(index, field);
        if (lua.Type(-1) == LuaType.LUA_TNUMBER)
        {
            fNumber = (float)lua.ToNumber(-1);
        }
        lua.Pop(1);
        return fNumber;
    }

    public static string GetTableString(ILuaState lua, int index, string field)
    {
        string str = null;
        lua.GetField(index, field);
        if (lua.Type(-1) == LuaType.LUA_TSTRING)
        {
            str = lua.ToString(-1);
        }
        lua.Pop(1);
        return str;
    }

    /// <summary>
    /// 为啥加这个这是个神奇的问题
    /// </summary>
    /// <param name="lua"></param>
    /// <returns></returns>
    private int Lua_pcall(ILuaState lua)
    {
        if (lua.Type(1) != LuaType.LUA_TFUNCTION)
        {
            lua.PushBoolean(false);
            //lua.PushString(lua.PrintLuaStack("pcall failed parm 1 function. excepted."));
            return 2;
        }

        int nCount = lua.GetTop();
        lua.PushValue(1);
        for (int i = 2; i <= nCount; ++i)
        {
            lua.PushValue(i);
        }
        if (lua.PCall(nCount - 1,0,0) != 0)
        {
            lua.PushBoolean(false);
            lua.Insert(-2);
            return 2;
        }
        lua.PushBoolean(true);
        return 1;
    }

    private int Lua_Print(ILuaState lua)
    {
        WriteInfo(ConstructString(lua));
        return 0;
    }

    private int Lua_Warn(ILuaState lua)
    {
        WriteError(ConstructString(lua));
        return 0;
    }

    private int Lua_loadstring(ILuaState lua)
    {
        if (lua.Type(1) != LuaType.LUA_TSTRING)
            return 0;

        if (DoString(lua.ToString(1)) == ThreadStatus.LUA_OK)
            return 1;
        return 0;
    }

	public ILuaState Lua
    {
		get { return _lua; }
	}

    public string ConfigPath
    {
        get { return _configPath; }
        set { _configPath = value;  }
    }

    public string ScriptPath
    {
        get { return _scriptPath; }
        set { _scriptPath = value;  }
    }

    public string RootPath
    {
        get { return _rootPath; }
        set { _rootPath = value; }
    }

    public void GarbagCollect()
    {
        //LuaAPI.lua_gc(_lua.GetLuaPtr(),LuaAPI.LUA_GCCOLLECT,0);
    }

    /// <summary>
    /// 返回Lua用了多少K内存
    /// </summary>
    /// <returns></returns>
    public int GetUsedMemory()
    {
        //return LuaAPI.lua_gc(_lua.GetLuaPtr(), LuaAPI.LUA_GCCOUNT, 0);
        return 0;
    }

    public void Close()
    {
        //         if (_lua.GetLuaPtr() != IntPtr.Zero)
        //         {
        //             _lua.Close();
        //             _lua = null;
        //            }
    }

	public void DoFile(string path)
    {
#if  UNITY_EDITOR 
        DoLocalScript(path);
        //DoBundleScript(path);
#else
        DoBundleScript(path);
        //DoLocalScript(path);
#endif
    }

    void DoLocalScript(string path)
    {
        string szFilePath = null;
        if (path.StartsWith("Config/"))
        {
            szFilePath = _rootPath + ConfigPath + path.Substring(6);
        }
        else
        {
            szFilePath = _rootPath + ScriptPath + path;
        }

        try
        {
            if (_lua.L_DoFile(szFilePath) != ThreadStatus.LUA_OK)
            {
                this.WriteError(_lua.ToString(-1));
                _lua.Pop(1);
            }
        }
        catch (IOException e)
        {
            UnityEngine.Debug.LogWarning("lua dofile failed." + e.ToString());
        }
    }

    void DoBundleScript(string path)
    {
        if (_scriptBundle == null)
        {
//             _scriptBundle = ResLoader.LoadBundleFromFile(LuaContex.ScriptBundleName);
//             if (_scriptBundle == null)
//             {
//                 UnityEngine.Debug.Log("do lua script failed load script bundle failed.");
//                 return;
//             }
        }

        var pScript = _scriptBundle.Load(path) as UnityEngine.TextAsset;
        //判断是否带有 UTF-8 BOM 信息
        if (pScript.bytes.Length <= 3)
            return;

        if (pScript.bytes[0] == 0xEF && pScript.bytes[1] == 0xBB && pScript.bytes[2] == 0xBF)
        {
            byte[] bytes = new byte[pScript.bytes.Length - 3];
            Array.Copy(pScript.bytes, 3, bytes, 0, bytes.Length);
            if (_lua.L_LoadBytes(bytes, path) != ThreadStatus.LUA_OK)
            {
                this.WriteError(_lua.ToString(-1));
                _lua.Pop(1);
            }
        }
        else
        {
            if (_lua.L_LoadBytes(pScript.bytes, path) != ThreadStatus.LUA_OK)
            {
                this.WriteError(_lua.ToString(-1));
                _lua.Pop(1);
            }
        }

        if (m_bEnableUnloadScriptBundle)
        {
            ClearScriptBundle();
        }
    }

    /// <summary>
    /// 调用该方法让对象保持delegate的引用，避免GC收集掉回调方法导致系统崩溃！！
    /// 一个Beg对应要调用End来结束
    /// </summary>
    /// <param name="lstFunctions"></param>
    public bool BegTableFunction(List<GCHandle> lstFunctions)
    {
        if (_lstSetTableFunctions != null)
        {
            return false;
        }
        _lstSetTableFunctions = lstFunctions;
        return true;
    }

    public void SetTableFunction(int index, string funName, CSharpFunctionDelegate pfn)
    {
        if (_lstSetTableFunctions == null)
        {
            throw new Exception("Before You Call LuaContex.SetTableFunction You Must Call BegTableFunction");
        }

        if (index >= 0)
        {
            this.WriteError("LuaContex.SetTableFunction Can Only Have Negative Index.");
            return;
        }

        if (_lua.Type(index) != LuaType.LUA_TTABLE)
        {
            this.WriteError(String.Format("index:{0} is not a table, can't add function.", index));
            return;
        }
        _lua.PushString(funName);
//         GCHandle hPfn = _lua.PushCSharpFunction(pfn);
//         _lua.SetTable(index - 2);
//         _lstSetTableFunctions.Add(hPfn);
    }

    /// <summary>
    /// 
    /// </summary>
    public void EndTableFunction(List<GCHandle> lstFunction)
    {
        if (_lstSetTableFunctions != lstFunction)
        {
            throw new Exception("Not End The Same  !!!");
        }
        _lstSetTableFunctions= null;
    }

    public void SetMetatable(int index, CSharpFunctionDelegate pfnReader, CSharpFunctionDelegate pfnWriter)
    {
        if (_lstSetTableFunctions == null)
        {
            throw new Exception("Before You Call LuaContex.SetMetatable You Must Call BegTableFunction");
        }

        if (index >= 0)
        {
            this.WriteError("LuaContex.SetTableFunction Can Only Have Negative Index.");
            return;
        }

        if (_lua.Type(index) != LuaType.LUA_TTABLE)
        {
            this.WriteInfo(String.Format("index:{0} is not a table, can't add meta function.", index));
            return;
        }
        _lua.NewTable();

        if (pfnReader != null)
        {
            _lua.PushString("__index");
//             GCHandle hPfn = _lua.PushCSharpFunction(pfnReader);
//             _lua.SetTable(index -2);
//             _lstSetTableFunctions.Add(hPfn);
        }

        if (pfnWriter != null)
        {
            _lua.PushString("__newindex");
//             GCHandle hPfn = _lua.PushCSharpFunction(pfnWriter);
//             _lua.SetTable(index - 2);
//             _lstSetTableFunctions.Add(hPfn);
        }
        _lua.SetMetaTable(index - 1);
    }

    public ThreadStatus DoString(string code)
    {
        ThreadStatus eStatus = _lua.L_DoString(code);
        if (eStatus != ThreadStatus.LUA_OK)
        {
            this.WriteInfo(_lua.L_CheckString(-1));
            _lua.Pop(1);
        }
        return eStatus;
	}

//private:
	private int Lua_DoFile(ILuaState lua){
		string file = lua.L_CheckString (1);
		this.DoFile (file);
		return 0;
	}

	private int Lua_LoadString(ILuaState lua){
		string code = lua.L_CheckString(1);
		this.DoString(code);
		return 1;
	}

	private int Lua_GetTime(ILuaState lua)
	{
        long now = Hstj.Game.Instance.GetTime();
		lua.PushNumber((double)now);
		return 1;
	}

	private int Lua_GetTimeInfo(ILuaState lua)
	{
		double v = lua.L_CheckNumber(1);
		DateTime date = _utcOrigin.AddMilliseconds(v + (3600 * 8 * 1000) );
        
		lua.NewTable();
		lua.PushString("year");
		lua.PushInteger(date.Year);
		lua.SetTable(-3);

		lua.PushString("month");
		lua.PushInteger(date.Month);
		lua.SetTable(-3);

		lua.PushString("day");
		lua.PushInteger(date.Day);
		lua.SetTable(-3);

		lua.PushString("hour");
		lua.PushInteger(date.Hour);
		lua.SetTable(-3);

		lua.PushString("minute");
		lua.PushInteger(date.Minute);
		lua.SetTable(-3);

		lua.PushString("second");
		lua.PushInteger(date.Second);
		lua.SetTable(-3);

        lua.PushString("wday");
        lua.PushInteger((int)date.DayOfWeek);
        lua.SetTable(-3);

		return 1;
	}


    private void WriteInfo(System.Object info)
    {
        if (_pfnLog != null)
        {
            _pfnLog(0, info.ToString());
        }
    }

    private void WriteError(System.Object info)
    {
        if (_pfnLog != null)
        {
            _pfnLog(1, info.ToString());
        }
    }

    private int Lua_UseUtf8(ILuaState lua)
    {
        m_bUseUtf8 = lua.ToBoolean(1);
        return 0;
    }

	private int Lua_ParseInteger(ILuaState lua)
	{
		long num = (long)lua.L_CheckNumber(1);
		int dwStart = lua.L_CheckInteger(2);
		int dwLen = lua.L_CheckInteger(3);
		string strNum = num.ToString();
		if(dwStart > strNum.Length)
			return 0;

		if(dwStart<=0)
			return 0;
		
		string outStr = strNum.Substring(dwStart-1,dwLen);
		lua.PushInteger(Convert.ToInt32(outStr));
		return 1;
	}

    public int Lua_FloatAccuracy(ILuaState lua)
    {
        m_nFloatAccuracy = lua.L_CheckInteger(1);
        return 0;
    }

    public int Lua_GetFloatAccuracy(ILuaState lua)
    {
        lua.PushInteger(m_nFloatAccuracy);
        return 1;
    }
}


