using System;
using System.Collections.Generic;
using System.Text;
using UniLua;
using UnityEngine;

namespace Hstj
{
    public enum EntityLookState
    {
        None=0,
		Red=1,
		Green=2,
    }

	public enum EntityType
	{
		Unknow=0,
		Entity=2,
		Npc=4,
		Player=8,
		Monster=16,
	}

    /// <summary>
    /// entity.visible
    /// entity.direction
    /// entity.type
    /// </summary>
    public class Entity : LuaObject
    {
        protected int _dwID;
        protected bool _bVisible = true;
        protected float _fDirection = 0;
        protected float m_height = 0;
        protected float m_Radius = 0.3f;
        protected EntityLookState _eHoverState = EntityLookState.None;
        protected EntityLookState _ePickState = EntityLookState.None;
		protected EntityType _entityType = EntityType.Entity;
        protected Dictionary<string, ActorEffect> _effects = new Dictionary<string, ActorEffect>();
        protected bool m_bCacheable = false;
        public int _pfnDrawMoveEnd = 0;
        public int _pfnTraceEnd = 0;
        public int _pfnTraceStart = 0;

        protected Color _highlightColor = Color.black;
        protected bool _bIsHighLighting = false;

		public virtual void Start()
		{
			RecomputeHeight();
		}

        protected override void ExtraRefLua()
        {
            LuaContex luaCtx = Game.Lua;
            luaCtx.SetTableFunction(-1, "GetID", new CSharpFunctionDelegate(Lua_GetID));
            luaCtx.SetTableFunction(-1, "SetPosition", new CSharpFunctionDelegate(Lua_SetPosition));
            luaCtx.SetTableFunction(-1, "GetPosition", new CSharpFunctionDelegate(Lua_GetPosition));
            luaCtx.SetTableFunction(-1, "CreatePanel", new CSharpFunctionDelegate(Lua_CreatePanel));
            luaCtx.SetTableFunction(-1, "PlayEffect", new CSharpFunctionDelegate(Lua_PlayEffect));
            luaCtx.SetTableFunction(-1, "StopEffect", new CSharpFunctionDelegate(Lua_StopEffect));
            luaCtx.SetTableFunction(-1, "PlaySound", new CSharpFunctionDelegate(Lua_PlaySound));
        }

		public void RecomputeHeight()
        {
			BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                m_height = box.center.y * 2;
                m_Radius = box.size.z/2;
            }
		}

        public virtual void LateUpdate()
		{

		}

        public virtual void FixedUpdate()
        {

        }

        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "visible":
                    lua.PushBoolean(Visible);
                    return true;
                case "direction":
                    lua.PushNumber(GetDirection());
                    return true;
                case "type":
                    lua.PushInteger((int)GetEntityType());
                    return true;
                case "scale":
                    lua.PushNumber(transform.localScale.x);
                     return true;
                case "height":
                    lua.PushNumber(m_height);
                    return true;
                case "radius":
                    lua.PushNumber(m_Radius);
                    return true;
                case "highlightcolor":
                    lua.NewTable();
                     
                    lua.PushNumber(_highlightColor.r);
                    lua.SetField(-2,"r");

                    lua.PushNumber(_highlightColor.g);
                    lua.SetField(-2,"g");

                    lua.PushNumber(_highlightColor.b);
                    lua.SetField(-2,"b");

                    lua.PushNumber(_highlightColor.a);
                    lua.SetField(-2,"a");
                    return true;
            }

            return base.WidgetReadOper(lua, key);
        }

        protected override bool WidgetWriteOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "visible":
                    Visible = lua.ToBoolean(3);
                    return true;
                case "direction":
                    SetDirection((float)lua.L_CheckNumber(3));
                    return true;
                case "type":
                    SetEntityType((EntityType)lua.L_CheckInteger(3));
                    return true;
                case "scale":
                    SetLocalScale((float)lua.L_CheckNumber(3));
                    return true;
                case "highlightcolor":
                    if (lua.Type(3) != LuaType.LUA_TTABLE)
                    {
                        Debug.LogWarning("Entity hightlightcolor attribute parm table excepted.");
                        return true;
                    }

                    lua.GetField(3, "r");
                    _highlightColor.r = (float)lua.L_CheckNumber(-1);
                    lua.Pop(1);

                    lua.GetField(3, "g");
                    _highlightColor.g = (float)lua.L_CheckNumber(-1);
                    lua.Pop(1);

                    lua.GetField(3, "b");
                    _highlightColor.b = (float)lua.L_CheckNumber(-1);
                    lua.Pop(1);

                    lua.GetField(3, "a");
                    //_highlightColor.a = (float)lua.L_CheckNumber(-1);
                    float a = (float)lua.L_CheckNumber(-1);
                    lua.Pop(1);
                    //print("***** Entity highlight color " + _highlightColor);
                    return true;
            }

            return base.WidgetWriteOper(lua, key);
        }

        public AudioObject PlaySound(string name, float fDelay = 0)
        {
            return AudioManager.PlaySound(name, transform, fDelay);
        }

        public int GetID()
        {
            return _dwID;
        }

        public void SetID(int dwID)
        {
            _dwID = dwID;
        }

        public void SetPosition(float x, float y, float z)
        {
        }

        public void GetPosition(ref float x, ref float y, ref float z)
        {
            x = transform.position.x;
            y = transform.position.y;
			z = transform.position.z;
        }

		public Vector3 GetPosition()
		{
			return transform.position;
		}

		
        public void SetDirection(float fDir)
        {
            Vector3 vTmp = Vector3.zero;
            vTmp.y = fDir;
            transform.eulerAngles = vTmp;
        }

        public float GetDirection()
        {
            return transform.eulerAngles.y;
        }

        public void SetHoverState(EntityLookState e)
        {
            _eHoverState = e;
        }

        public void SetPickState(EntityLookState e)
        {
            _ePickState = e;
        }

        public int GetLuaIndex()
        {
            return _luaIndex;
        }

        public virtual bool Visible
        {
            get 
            {
                return _bVisible; 
            }
            set 
            {
                if (value != _bVisible)
                {
                    Renderer[]  _renderers = GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < _renderers.Length; ++i)
                    {
                        _renderers[i].enabled = value;
                    }
                }
                _bVisible = value;
                BoxCollider box = GetComponent<Collider>() as BoxCollider;
                if (box != null)
                    box.enabled = _bVisible;

            }
        }

		public void CameraLook()
		{

		}

		public Collider GetCollider()
		{
            return GetComponent<Collider>();
		}

		public override void MouseDown()
		{
		}

        public void StopAllEffects(string typeName = null)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                foreach(var itr in _effects)
                {

                    itr.Value.Stop();
                }
            }
            else
            {
                foreach (var itr in _effects)
                {
                    if (itr.Value.name.Contains(typeName))
                        itr.Value.Stop();
                }
            }
        }

        protected override void OnMouseHover(bool bHover)
        {
        }

		public virtual EntityType GetEntityType()
		{
			return _entityType;
		}

		public void SetEntityType(EntityType e)
		{
			_entityType = e;
		}

		public virtual void SetLayer(int dwLayer)
		{
			var renders = GetComponentsInChildren<MeshRenderer>();
			for(int i=0; i<renders.Length; ++i)
			{
				MeshRenderer render = renders[i];
				render.gameObject.layer = dwLayer;
			}
		}

        public void SetLocalScale(float fScale)
        {
            Vector3 vScale = Vector3.zero;
            vScale.Set(fScale, fScale, fScale);
            transform.localScale = vScale;
            RecomputeHeight();
        }

		protected override void OnDestroy()
		{
            _effects.Clear();

            try
            {
                if (_pfnDrawMoveEnd != 0)
                {
                    Game.LuaApi.L_Unref(LuaDef.LUA_REGISTRYINDEX, _pfnDrawMoveEnd);
                    _pfnDrawMoveEnd = 0;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(ex.ToString());
            }            


            base.OnDestroy();
		}

        public virtual void PlayEffect(
                string name,
                float fDelay,
                bool bWorldPos,
                float fExistTime,
                EffectType type)
        {
        }

        public virtual void StopEffect(string name)
        {
            ActorEffect effect = GetEffect(name);
            if (effect)
            {
                effect.Stop();
            }
        }

        public ActorEffect GetEffect(string name)
        {
            try
            {
                return _effects[name];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool Cacheable
        {
            get { return m_bCacheable; }
            set { m_bCacheable = value; }
        }

        public Dictionary<string, ActorEffect> GetEffects()
        {
            return _effects;
        }

        protected virtual void PlayWorldEffect(ref string name, float fDelay,float fExistTime)
        {

        }


        int Lua_GetPosition(ILuaState lua)
        {
			float x=0,y=0,z=0;
            GetPosition(ref x, ref y,ref z);
            lua.PushNumber(x);
            lua.PushNumber(y);
			lua.PushNumber(z);
            return 3;
        }

        int Lua_SetPosition(ILuaState lua)
        {
            float x = (float)lua.L_CheckNumber(2);
            float y = (float)lua.L_CheckNumber(3);
			float z = (float)lua.L_CheckNumber(4);
            SetPosition(x, y,z);
            return 0;
        }

        int Lua_GetID(ILuaState lua)
        {
            lua.PushInteger(_dwID);
            return 1;
        }

        int Lua_PlaySound(ILuaState lua)
        {
            string name = lua.L_CheckString(2);
            float fDelay = 0;
            if (lua.Type(3) == LuaType.LUA_TNUMBER)
            {
                fDelay = (float)lua.L_CheckNumber(3);
            }
            var audio = PlaySound(name, fDelay);
            if (audio != null)
            {
                audio.PushThis(lua);
                return 1;
            }
            return 0;
        }

        int Lua_PlayEffect(ILuaState lua)
        {
            string effect = lua.ToString(2);
            float fDelay = (float)lua.ToNumber(3);
            bool bWorldPos = lua.ToBoolean(4);
            float fExistTime = (float)lua.ToNumber(5);
            int dwEffectType = (int)EffectType.Once;

            if(lua.Type(6) == LuaType.LUA_TNUMBER)
            {
                dwEffectType = lua.ToInteger(6);
            }
            this.PlayEffect(effect, fDelay,bWorldPos,fExistTime,(EffectType)dwEffectType);
            return 0;
        }

        int Lua_StopEffect(ILuaState lua)
        {
            string name = lua.L_CheckString(2);
            StopEffect(name);
            return 0;
        }

        int Lua_CreatePanel(ILuaState lua)
        {
            return 0;
        }

        public void PlayDrawFlyActionEnd()
        {

        }


        public bool AppendHighLightingFunc()
        {
            return false;
        }

        public void DelHighLightingFunc()
        {
  
        }

        public void CloseHighLightingFunc()
        {
  
        }
        public void DoHighLightEffect(Color color)
        {

        }

        public void DoFlashWhiteEffect(Color color, float time, float delay, string shader,float power)
        {

        }

        public void PlayTraceActionEnd()
        {

        }
        public void PlayTraceActionStart( Vector3 endPos )
        {

        }

        public void SetUIPanelOnDefaultState()
        {

        }
        // add shadows
        public bool AttachShadow()
        {

            return true;
        }
        public bool DetachShadow()
        {
            return true;
        }
        public void SwitchShadow(bool bOpen)
        {

        }
        public void AddOccludeViewer()
        {

        }
    }
}
