using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UniLua;

namespace Hstj
{
	/// <summary>
    /// obj.typeinfo
	/// obj.name
    /// obj.active
    /// obj.onMouseDown = function(this)
    /// obj.onMouseHover = function(this,bHover)
    /// obj.parent               --get parent
    /// obj.position            --ready only  get world position return pos.x pos.y, pos.z        
    /// obj.scale                  --get set scale
	/// </summary>
	public class LuaObject : MonoBehaviour 
    {
		protected int _luaIndex;
		private int _pfnMouseHover;
		private int _pfnMouseDown;
        protected static LuaObject _pHoverObject = null;
        private List<GCHandle> _lstPfnDelegate = null;
	    public Vector3 LookDir = Vector3.zero;
        private GCHandle _pUserDataHandle;

        public static int dwCreatedObjectCount;
        public static int dwCurObjectCount;

		public virtual void Awake()
		{
			RefLua();
		}

        public virtual void RefLua(int dwParentLuaIndex=0)
		{
		}

        public List<GCHandle> GetLuaFunctions()
        {
            return _lstPfnDelegate;
        }

		protected virtual void ExtraRefLua()
		{
			
		}

        protected virtual void OnEndRefLua()
        {

        }
		
		public virtual void UnrefLua()
		{
		}

        public void PushThis(ILuaState lua)
		{
		}

        public static LuaObject HoverObject
        {
            get { return _pHoverObject; }
        }

		public static LuaObject GetLuaObject(ILuaState lua, int index)
		{
// 			if(lua.Type(index) != LuaType.LUA_TTABLE)
// 			{
// 				return null;
// 			}
// 
//             lua.PushValue(index);
//             lua.PushString("_ref");
//             lua.GetTable(-2);
//             LuaObject obj = lua.ToUserData(-1).Target as LuaObject;
            LuaObject obj = null;
            lua.Pop(2);
            return obj;
        }

		protected virtual void OnDestroy()
		{
            if (LuaObject.HoverObject == this)
            {
                this.MouseExit();
            }

            if (_pUserDataHandle.IsAllocated)
            {
                _pUserDataHandle.Free();
            }

            if (_lstPfnDelegate != null)
            {
                for (int i = 0; i < _lstPfnDelegate.Count; ++i)
                {
                    _lstPfnDelegate[i].Free();
                }
                _lstPfnDelegate.Clear();
            }
            
            try
            {
                UnrefLua();
            }
            catch(System.Exception)
            {

            }
		}

	    public void MouseEnter()
	    {
            if (_pfnMouseHover != 0)
            {
                CallOnMouseHover(true);
            }
            _pHoverObject = this;
            OnMouseHover(true);
	    }

        public void MouseExit()
        {
            if (_pfnMouseHover != 0)
            {
                CallOnMouseHover(false);
            }
            _pHoverObject = null;
            OnMouseHover(false);
        }

		public virtual void MouseDown()
		{
			if(_pfnMouseDown !=0){
				CallOnMouseDown();
			}

		}

		protected virtual void OnMouseHover(bool bHover)
		{
            if (_pfnMouseHover != 0)
            {
                CallOnMouseHover(bHover);
            }
		}

		private void CallOnMouseHover(bool bHover)
		{
		}

		private void CallOnMouseDown()
		{

		}

        protected virtual bool WidgetReadOper(ILuaState lua, string key)
        {
            return false;
        }

        protected virtual bool WidgetWriteOper(ILuaState lua, string key)
        {
            return false;
        }

    }


}