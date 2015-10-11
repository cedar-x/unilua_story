 using System;
using System.Collections.Generic;
using System.Text;
using UniLua;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


namespace Hstj
{
	public class LuaObjWithCallFun{

		public	int dwConpleteFun = 0 ;
		public	int dwStartFun = 0 ;
		public	int dwUpdateFun = 0;
		
	}
	
	public class LuaItween : MonoBehaviour
	{
		private List<LuaObjWithCallFun> CallBackList = new List<LuaObjWithCallFun> ();
		private int _luaIndex;
        private List<GCHandle> _lstLuaFunction = new List<GCHandle>();

        void Start()
        {
        }
		void Update()
		{

		}

		public void RefLua()
		{
// 			if(_luaIndex != 0)
// 				return;
// 			ILuaState lua = Game.LuaApi;
// 			lua.NewTable();
// 			_luaIndex = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
// 
//             Game.lua.BegTableFunction(_lstLuaFunction);
// 			PushThis(Game.lua);
// 			Game.lua.SetTableFunction(-1,"ScaleTo",new CSharpFunctionDelegate(Lua_ScaleTo));
// 			Game.lua.SetTableFunction(-1,"MoveTo",new CSharpFunctionDelegate(Lua_MoveTo));
//             Game.lua.SetTableFunction(-1, "MoveBy", new CSharpFunctionDelegate(Lua_MoveBy));
//             Game.lua.SetTableFunction(-1, "RotateTo", new CSharpFunctionDelegate(Lua_RotateTo));
//             Game.lua.SetTableFunction(-1, "RotateBy", new CSharpFunctionDelegate(Lua_RotateBy));
//             Game.lua.SetTableFunction(-1, "ShakePosition", new CSharpFunctionDelegate(Lua_ShakePosition));
//             Game.lua.SetTableFunction(-1, "ShakeRotation", new CSharpFunctionDelegate(Lua_ShakeRotation));
//             Game.lua.SetTableFunction(-1, "ShakeScale", new CSharpFunctionDelegate(Lua_ShakeScale));
//             Game.lua.SetTableFunction(-1, "FadeTo", new CSharpFunctionDelegate(Lua_FadeTo));
//             Game.lua.SetTableFunction(-1, "CameraFadeTo", new CSharpFunctionDelegate(lua_CameraFadeTo));
//             Game.lua.SetTableFunction(-1, "CameraFadeAdd", new CSharpFunctionDelegate(lua_CameraFadeAdd));
//             Game.lua.SetTableFunction(-1, "CameraFadeDestroy", new CSharpFunctionDelegate(lua_CameraFadeDestroy));
//             Game.lua.SetTableFunction(-1, "ChangeCamera", new CSharpFunctionDelegate(lua_ChangeCamera));
//             Game.lua.SetTableFunction(-1, "Stop", new CSharpFunctionDelegate(lua_Stop));
//             Game.lua.SetTableFunction(-1, "ValueTo", new CSharpFunctionDelegate(Lua_ValueTo));
//        
//             lua.Pop(1);
//             Game.lua.EndTableFunction(_lstLuaFunction);
		}
	
		private int lua_ChangeCamera(ILuaState lua)
		{

// 			int cameraIndex = lua.L_CheckInteger (2);
// 			CameraMgr cameraMgr = Camera.main.GetComponent<CameraMgr>();
// 				if(cameraMgr != null)
// 				{
// 				cameraMgr.ActiveCamera(cameraIndex);
// 				}
			return 0;
		}
		private int lua_Stop(ILuaState lua)
		{
			if (lua.IsNone (2)) {
				iTween.Stop();			
			}
			LuaObject luaObj = LuaObject.GetLuaObject (lua, 2);
			if (luaObj == null) {
								Debug.Log ("this luaobj is nil ");
								return 0;			
						} else if (lua.IsNoneOrNil (3) && lua.IsNoneOrNil (4)) {
								iTween.Stop (luaObj.gameObject);
						} else if (!lua.IsNoneOrNil (3) && lua.IsNoneOrNil (4)) {
								string szType = lua.L_CheckString (3);
								iTween.Stop (luaObj.gameObject,szType);
						} else if (lua.IsNoneOrNil (3) && !lua.IsNoneOrNil (4)) {
								bool isIncludeChild = lua.ToBoolean(4);
							iTween.Stop(luaObj.gameObject,isIncludeChild);
						}
			return 0;
		}

		private int lua_CameraFadeAdd(ILuaState lua)
		{
			if (lua.IsNoneOrNil (2)) {
				iTween.CameraFadeAdd();	
				return 0;
			}

			string szImage = lua.L_CheckString (2);
			Texture2D texture = (Texture2D)Resources.Load("Images/"+ szImage);
			if (texture != null) {
								iTween.CameraFadeAdd (texture);	
						} else {
				iTween.CameraFadeAdd();
			}

			return 0;
		}
		private int lua_CameraFadeDestroy(ILuaState lua)
		{
			iTween.CameraFadeDestroy ();
			return 0;
		}

		private int lua_CameraFadeTo(ILuaState lua)
		{
			LuaObject luaObj = LuaObject.GetLuaObject (lua, 2);
			if (luaObj == null)
				return 0;
			
			if (lua.Type (3) != LuaType.LUA_TTABLE) {
				return 0;
			}
			Hashtable hashTable = ItweenAdaptHasHTable(lua);
			iTween.CameraFadeTo (hashTable);
			
			return 0;
		}
		
		private int Lua_FadeTo(ILuaState lua)
		{
			LuaObject luaObj = LuaObject.GetLuaObject (lua, 2);
			if (luaObj == null)
				return 0;
			
			if (lua.Type (3) != LuaType.LUA_TTABLE) {
				return 0;
			}
			Hashtable hashTable = ItweenAdaptHasHTable(lua);
			iTween.FadeTo (luaObj.gameObject,hashTable);
			
			return 0;
		}
		private int Lua_RotateBy(ILuaState lua)
		{
			LuaObject luaObj = LuaObject.GetLuaObject (lua, 2);
			if (luaObj == null)
				return 0;
			
			if (lua.Type (3) != LuaType.LUA_TTABLE) {
				return 0;
			}
			Hashtable hashTable = ItweenAdaptHasHTable(lua);
			iTween.RotateBy (luaObj.gameObject, hashTable);
			
			return 0;
			
		}
		private int Lua_RotateTo(ILuaState lua)
		{
			LuaObject luaObj = LuaObject.GetLuaObject (lua, 2);
			if (luaObj == null)
				return 0;
			
			if (lua.Type (3) != LuaType.LUA_TTABLE) {
				return 0;
			}
			Hashtable hashTable = ItweenAdaptHasHTable(lua);
			iTween.RotateTo (luaObj.gameObject, hashTable);

			return 0;

		}
		private int Lua_MoveBy(ILuaState lua)
		{
			
			LuaObject luaObj = LuaObject.GetLuaObject (lua, 2);
			if (luaObj == null)
				return 0;
			
			if (lua.Type (3) != LuaType.LUA_TTABLE) {
				return 0;
			}
			Hashtable hashTable = ItweenAdaptHasHTable(lua);
			
			iTween.MoveBy (luaObj.gameObject, hashTable);
			
			return 0;
		}
		private int Lua_ScaleTo(ILuaState lua)
		{
			
			LuaObject luaObj = LuaObject.GetLuaObject (lua, 2);
			if (luaObj == null)
				return 0;
			
			if (lua.Type (3) != LuaType.LUA_TTABLE) {
				return 0;
			}
			Hashtable hashTable = ItweenAdaptHasHTable(lua);
			
			iTween.ScaleTo (luaObj.gameObject, hashTable);
			
			return 0;
		}

		private int Lua_MoveTo(ILuaState lua)
		{

			LuaObject luaObj = LuaObject.GetLuaObject (lua, 2);
			if (luaObj == null)
				return 0;
			
			if (lua.Type (3) != LuaType.LUA_TTABLE) {
				return 0;
			}
			Hashtable hashTable = ItweenAdaptHasHTable(lua);
	
			iTween.MoveTo (luaObj.gameObject, hashTable);
			
			return 0;
		}
		private int Lua_ValueTo(ILuaState lua)
		{
			
			LuaObject luaObj = LuaObject.GetLuaObject (lua, 2);
			if (luaObj == null)
				return 0;
			
			if (lua.Type (3) != LuaType.LUA_TTABLE) {
				return 0;
			}
			Hashtable hashTable = ItweenAdaptHasHTable(lua);
			
			iTween.ValueTo (luaObj.gameObject, hashTable);
			
			return 0;
		}
		private int Lua_ShakePosition(ILuaState lua)
		{
			
			LuaObject luaObj = LuaObject.GetLuaObject (lua, 2);
			if (luaObj == null)
				return 0;
			
			if (lua.Type (3) != LuaType.LUA_TTABLE) {
				return 0;
			}
			Hashtable hashTable = ItweenAdaptHasHTable(lua);
			
			iTween.ShakePosition (luaObj.gameObject, hashTable);
			
			return 0;
		}
		private int Lua_ShakeRotation(ILuaState lua)
		{
			
			LuaObject luaObj = LuaObject.GetLuaObject (lua, 2);
			if (luaObj == null)
				return 0;
			
			if (lua.Type (3) != LuaType.LUA_TTABLE) {
				return 0;
			}
			Hashtable hashTable = ItweenAdaptHasHTable(lua);
			
			iTween.ShakeRotation (luaObj.gameObject, hashTable);
			
			return 0;
		}
		private int Lua_ShakeScale(ILuaState lua)
		{
			
			LuaObject luaObj = LuaObject.GetLuaObject (lua, 2);
			if (luaObj == null)
				return 0;
			
			if (lua.Type (3) != LuaType.LUA_TTABLE) {
				return 0;
			}
			Hashtable hashTable = ItweenAdaptHasHTable(lua);
			
			iTween.ShakeScale (luaObj.gameObject, hashTable);
			
			return 0;
		}
		public void UnrefLua(LuaContex luaCtx)
		{
			if(_luaIndex != 0)
			{
				ILuaState lua = luaCtx.Lua;
				lua.L_Unref(LuaDef.LUA_REGISTRYINDEX,_luaIndex);
				_luaIndex = 0;
			}
		}
		public void PushThis(ILuaState lua)
		{
			if(_luaIndex != 0)
			{
				lua.RawGetI(LuaDef.LUA_REGISTRYINDEX,+_luaIndex);
			}
		}
        static public Hashtable ItweenParamHasHTable(ILuaState lua, LuaObjWithCallFun callBackFun)
        {
            Hashtable hashTable = new Hashtable();
			lua.PushNil ();
			while (lua.Next(-2)) {
				string key = lua.L_CheckString(-2);
				switch(key)
				{
				case "path":
				{
					Vector3[] path = GetPath(lua,-1);
					hashTable.Add(key,path);
					break;
				}
				case "rotation":
				case "position":
				case "looktarget":
				//case "amount":
				case "scale":
				{
					Vector3  transfrom = GetVector3(lua,-1);
					hashTable.Add(key,transfrom);
					break;
				}
				case "movetopath":
				case "islocal":
				{		
					bool bIsLocal = false;
					if( lua.Type(-1) == LuaType.LUA_TBOOLEAN){
						 bIsLocal = lua.ToBoolean(-1);
					}
					hashTable.Add(key,bIsLocal);
					break;
				}
				case "easetype":
				{
					iTween.EaseType value = (iTween.EaseType)lua.L_CheckInteger(-1);
					hashTable.Add(key,value);
					break;
				}
				case "LoopType":
				{
					iTween.LoopType value = (iTween.LoopType)lua.L_CheckInteger(-1);
					hashTable.Add(key,value);
					break;
				}
				case "onstart":
				{
					if(lua.Type(-1) == LuaType.LUA_TFUNCTION)
					{
						lua.PushValue(-1);
						callBackFun.dwStartFun = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
						string value = key;
						hashTable.Add(key,value);
					}
					break;
				}
				case "onupdate":
				{
					if(lua.Type(-1) == LuaType.LUA_TFUNCTION)
					{
						lua.PushValue(-1);
						callBackFun.dwUpdateFun = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
						string value = key;
						hashTable.Add(key,value);
					}
					break;
				}
				case "oncomplete":
				{
					if(lua.Type(-1) == LuaType.LUA_TFUNCTION)
					{
						lua.PushValue(-1);
						callBackFun.dwConpleteFun = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
						string value = key;
						hashTable.Add(key,value);
					}
					break;
				}
				case "onstarttarget":
				case "onupdatetarget":
				case "oncompletetarget":
                {
					break;
				}

				
				default:
				{
					float value = (float)lua.L_CheckNumber(-1);
					hashTable.Add(key,value);
					break;
				}
					
				}
				lua.Pop(1);
			}
			lua.Pop(1);
            return hashTable;
        }
		private Hashtable ItweenAdaptHasHTable(ILuaState lua)
		{
			LuaObjWithCallFun callBackFun = new LuaObjWithCallFun ();

            lua.PushValue(3);
		    Hashtable hashTable = ItweenParamHasHTable(lua, callBackFun);

			if (callBackFun.dwConpleteFun != 0) {
				hashTable.Add("oncompleteparams",callBackFun);
			}
			if (callBackFun.dwStartFun != 0) {
				hashTable.Add("onstartparams",callBackFun);
			}
			if (callBackFun.dwUpdateFun != 0) {
				hashTable.Add("onupdateparams",callBackFun);
			}	
			
			hashTable.Add ("onstarttarget", gameObject);
			hashTable.Add ("onupdatetarget", gameObject);
			hashTable.Add ("oncompletetarget", gameObject); 

			return hashTable;
		}

		public static Vector3[] GetPath(ILuaState lua , int dwIndex){
			int count =	lua.L_Len (-1);
			Vector3[] path = new Vector3[count];
			for (int i = 1; i<= count; i++) {
				lua.PushNumber (i);
				lua.GetTable (-2);	
				Vector3 v = GetVector3(lua,-1);
				lua.Pop(1);
				path[i-1] = v;

			}
			//lua.Pop (1);
			return path;
		}

		public static Vector3 GetVector3(ILuaState lua, int dwIndex)
		{
			Vector3 v = Vector3.zero;
			lua.PushValue (dwIndex);
			lua.PushString ("x");
			lua.GetTable (-2);
			v.x = (float)lua.ToNumber (-1);
			lua.Pop (1);

			lua.PushString ("y");
			lua.GetTable (-2);
			v.y = (float)lua.ToNumber (-1);
			lua.Pop ( 1);

			lua.PushString ("z");
			lua.GetTable (-2);
			v.z = (float)lua.ToNumber (-1);
			lua.Pop (1);

			lua.Pop (1);

			return v;
		}


		private object GetCSharpParam(ILuaState lua,object arg,int index)
		{
			iTween.Hash ();
			/*
			string paramType = (string)arg;
			if (paramType == "position" || paramType == "looktarget") {
								LuaObject luaTarget = LuaObject.GetLuaObject (lua, index + 1);
								return luaTarget.transform;

						} else if (paramType == "easetype") {
								return (int)lua.ToObject (index + 1);
						} else if (paramType == "speed") {
						return (float)lua.L_CheckString(index + 1);
						}

			return lua.ToObject(index + 1);
			*/
			return null;
		}

		private void onstart(LuaObjWithCallFun callBackFun)
		{
			ILuaState lua = Game.LuaApi;
			lua.RawGetI(LuaDef.LUA_REGISTRYINDEX,callBackFun.dwStartFun);
			
			if(lua.PCall(0,0,0) != 0)
			{
				Debug.LogWarning(lua.ToString(-1));
				lua.Pop(1);
			}
			
			lua.L_Unref(LuaDef.LUA_REGISTRYINDEX,callBackFun.dwStartFun);
		}
		private void onupdate(LuaObjWithCallFun callBackFun)
		{
			ILuaState lua = Game.LuaApi;
			lua.RawGetI(LuaDef.LUA_REGISTRYINDEX,callBackFun.dwUpdateFun);
			
			if(lua.PCall(0,0,0) != 0)
			{
				Debug.LogWarning(lua.ToString(-1));
				lua.Pop(1);
			}

		}
		private void oncomplete(LuaObjWithCallFun callBackFun)
		{
			ILuaState lua = Game.LuaApi;
			lua.RawGetI(LuaDef.LUA_REGISTRYINDEX,callBackFun.dwConpleteFun);

			if(lua.PCall(0,0,0) != 0)
			{
				Debug.LogWarning(lua.ToString(-1));
				lua.Pop(1);
			}
			lua.L_Unref(LuaDef.LUA_REGISTRYINDEX,callBackFun.dwConpleteFun);
			if (callBackFun.dwUpdateFun != 0) {
				lua.L_Unref(LuaDef.LUA_REGISTRYINDEX,callBackFun.dwUpdateFun);
			}
			callBackFun = null;
		}


        // zsy add
        private int Lua_FlyAction(ILuaState lua)
        {
            LuaObject luaObj = LuaObject.GetLuaObject(lua, 2);
            if (luaObj == null)
                return 0;

            if (lua.Type(3) != LuaType.LUA_TTABLE)
            {
                return 0;
            }
            Hashtable hashTable = MoveControllerAdaptHasHTable(lua);

            //moveController.DoFlyEffect(luaObj.transform, hashTable);
			
            return 0;
        }
        // zsy add
        private Hashtable MoveControllerAdaptHasHTable(ILuaState lua)
        {
            LuaObjWithCallFun callBackFun = new LuaObjWithCallFun();
            //print("*********** state len" + lua.GetTop());
            Hashtable hashTable = new Hashtable();
            lua.PushValue(3);
            lua.PushNil();
            while (lua.Next(-2))
            {
                string key = lua.L_CheckString(-2);
                switch (key)
                {
                    case "animation":
                        {
                            Hashtable animation = new Hashtable();
                            lua.PushValue(-1);
                            lua.PushNil();

                            float value = 0;
                            string strtype = "ease";
                            bool blean = true;

                            while (lua.Next(-2))
                            {
                                string childkey = lua.L_CheckString(-2);
                                switch (childkey)
                                {
                                    case "onstart":
                                        {
                                            if (lua.Type(-1) == LuaType.LUA_TFUNCTION)
                                            {
                                                lua.PushValue(-1);
                                                callBackFun.dwStartFun = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
                                                animation.Add(childkey, childkey);
                                            }
                                            break;
                                        }
                                    case "onupdate":
                                        {
                                            if (lua.Type(-1) == LuaType.LUA_TFUNCTION)
                                            {
                                                lua.PushValue(-1);
                                                callBackFun.dwUpdateFun = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);

                                                animation.Add(childkey, childkey);
                                            }
                                            break;
                                        }
                                    case "oncomplete":
                                        {
                                            if (lua.Type(-1) == LuaType.LUA_TFUNCTION)
                                            {
                                                lua.PushValue(-1);
                                                callBackFun.dwConpleteFun = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);

                                                animation.Add(childkey, childkey);
                                            }
                                            break;
                                        }

                                    default:
                                        switch (lua.Type(-1))
                                        {
                                            case LuaType.LUA_TNUMBER:
                                                value = (float)lua.L_CheckNumber(-1);
                                                animation.Add(childkey, value);
                                                break;
                                            case LuaType.LUA_TBOOLEAN:
                                                blean = lua.ToBoolean(-1);
                                                animation.Add(childkey, blean);
                                                break;
                                            case LuaType.LUA_TSTRING:
                                                strtype = lua.L_CheckString(-1);
                                                animation.Add(childkey, strtype);
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                }
                                
                                lua.Pop(1);
                            }
                            lua.Pop(1);

                            if (callBackFun.dwConpleteFun != 0)
                            {
                                animation.Add("oncompleteparams", callBackFun);
                            }
                            if (callBackFun.dwStartFun != 0)
                            {
                                animation.Add("onstartparams", callBackFun);
                            }
                            if (callBackFun.dwUpdateFun != 0)
                            {
                                animation.Add("onupdateparams", callBackFun);
                            }

                            animation.Add("onstarttarget", gameObject);
                            animation.Add("onupdatetarget", gameObject);
                            animation.Add("oncompletetarget", gameObject); 

                            hashTable.Add(key, animation);
                            break;
                        }
                    case "keyframes":
                        {
                            Hashtable keyframes = new Hashtable();
                            lua.PushValue(-1);
                            lua.PushNil();

                            int numkey = -1;
                            string strkey = "from";

                            while (lua.Next(-2))
                            {
                                Hashtable tempHash = new Hashtable();
                                if (lua.Type(-1) == LuaType.LUA_TTABLE)
                                {
                                    lua.PushValue(-1);
                                    lua.PushNil();
                                    while (lua.Next(-2))
                                    {
                                        string child2key = lua.L_CheckString(-2);
                                        switch (child2key)
                                        {
                                            case "pos":
                                            case "scale":
                                            case "rotate":
                                            case "color":
                                                Vector3 vecValue = GetVector3(lua, -1);
                                                tempHash.Add(child2key, vecValue);
                                                break;                                                
                                            case "alpha":
                                                float alpha = (float)lua.L_CheckNumber(-1);
                                                tempHash.Add(child2key, alpha);
                                                break;
                                            default:
                                                break;
                                        }
                                        lua.Pop(1);
                                    }
                                    lua.Pop(1);
                                }
                                if (lua.Type(-2) == LuaType.LUA_TSTRING)
                                {
                                    strkey = lua.L_CheckString(-2);
                                    keyframes.Add(strkey, tempHash);
                                }
                                else if (lua.Type(-2) == LuaType.LUA_TNUMBER)
                                {
                                    numkey = (int)lua.L_CheckInteger(-2);
                                    keyframes.Add(numkey, tempHash);
                                }
                                lua.Pop(1);
                            }
                            lua.Pop(1);

                            hashTable.Add(key, keyframes);
                            break;
                        }
                    default:
                        break;

                }
                lua.Pop(1);
            }
            lua.Pop(1);
            //print("***********AAAAAAA state len" + lua.GetTop());
            return hashTable;
        }
	}
}