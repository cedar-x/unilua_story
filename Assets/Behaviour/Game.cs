using System;
using UnityEngine;
using System.Collections;
using UniLua;


public class Game : MonoBehaviour {

    public string _szStartScript = "Script/StoryMain.lua";
    public int _nStoryScript = 1001;
    private int _luaIndex = 0;

    private static ILuaState _lua;
    /// <summary>
    /// /////////////////////////////////////////////
    /// </summary>
    void Awake() {
        DontDestroyOnLoad(gameObject);
    }
	void Start () {
        InitLua();
        Call_Main();
	}
	void Update () {
	
	}
    /////public property/////////////////////////////////////////////////////////////////////
    public static ILuaState LuaApi
    {
        get
        {
            return _lua;
        }
    }
    /////private function ////////////////////////////////////////////////////
// 	void CreateObject<Type>(Type* pObject,string szVarName) where Type:object
// 	{
// 		if(LuaClassName<Type>::Name() == NULL){
// 			WriteError("lua error:before you add member function, regist the type first.\n");
// 			return;
// 		}
// 
// 		PushObject<Type>(pObject);
// 		lua_setglobal(m_lua,szVarName);
//	}
    private void SetMetatable(int index, CSharpFunctionDelegate pfnReader, CSharpFunctionDelegate pfnWriter)
    {
        LuaApi.PushValue(index);
        LuaApi.NewTable();
        LuaApi.PushString("__index");
        LuaApi.PushCSharpFunction(pfnReader);
        LuaApi.SetTable(-3);
        LuaApi.PushString("__newindex");
        LuaApi.PushCSharpFunction(pfnWriter);
        LuaApi.SetTable(-3);
        LuaApi.SetMetaTable(-2);
    }
    private int Lua_ObjectMetaReader(ILuaState lua)
    {
        string key = lua.L_CheckString(2);
        if (WidgetReadOper(lua, key))
            return 1;

        return 0;
    }
    private int Lua_ObjectMetaWriter(ILuaState lua)
    {
        string key = lua.L_CheckString(2);
        if (WidgetWriteOper(lua, key))
            return 0;

        lua.RawSet(1);
        return 0;
    }
    protected virtual bool WidgetWriteOper(ILuaState lua, string key)
    {
        switch (key)
        {
            case "typeinfo":
                Debug.LogWarning("LuaObject set typeinfo failed ready only!");
                return true;
        }
        return false;
    }
    protected virtual bool WidgetReadOper(ILuaState lua, string key)
    {
        switch (key)
        {
            case "typeinfo":
                Debug.LogWarning("LuaObject set typeinfo failed ready only!");
                return true;
            case "storyid":
                lua.PushNumber(_nStoryScript);
                return true;
        }
        return false;
    }

    private void RefLua()
    {
        LuaApi.NewTable();
        _luaIndex = LuaApi.L_Ref(LuaDef.LUA_REGISTRYINDEX);

        this.PushThis();
        SetMetatable(-1, Lua_ObjectMetaReader, Lua_ObjectMetaWriter);
        LuaApi.Pop(1);
    }
    public void PushThis()
    {
        LuaApi.RawGetI(LuaDef.LUA_REGISTRYINDEX, _luaIndex);
    }
    private bool InitLua()
    {
        if (_lua == null)
        {
            _lua = LuaAPI.NewState();
            _lua.L_OpenLibs();
        }
        RefLua();
        return true;
    }
    private void Call_Main()
    {
        var status = LuaApi.L_DoFile(_szStartScript);
        if (status != ThreadStatus.LUA_OK)
        {
            Debug.LogWarning("Game doFile error:" + _szStartScript);
            return;
        }
        LuaApi.GetGlobal("main");
        this.PushThis();
        if (LuaApi.PCall(1, 0, 0) != ThreadStatus.LUA_OK)
        {
            Debug.LogWarning("Game Call Main function error:" + LuaApi.L_ToString(-1));
            return;
        }
    }
    /////public function /////////////////////////////////////////////////////////////////////

}
