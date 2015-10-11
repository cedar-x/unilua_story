using System;
using System.Collections.Generic;
using System.Text;
using UniLua;
using UnityEngine;

namespace Hstj
{
	public struct BoneInfo
	{
		public Vector3 pos;
		public Vector3 scale;
		public Quaternion rot;	
	}

    public struct NextAnimInfo
    {
        public float fPlayTime;
        public int pfnOnEnd;
    }

    public class Actor : Entity
    {
        public struct MoveInfo
        {
            public int pfnMoveStop;
            public int pfnMoveStart;
            public int pfnMoveUpdate;
            public Vector3 vDstPos;
        }

        private float _speed = 6.5f;
        private int _dwMMethod = 2;             
        private bool _bMoving = false;
        private float _fUpdateInterval = 0.1f;
        private float _fLastUpdateTime = 0;
        private MoveInfo _moveInfo; 
        private Animator _animator;
        private NextAnimInfo _nextAnimInfo;

        private Dictionary<string, GameObject> _bones = new Dictionary<string, GameObject>();
        private Dictionary<string, SkinnedMeshRenderer[]> _meshparts = null;
        private Dictionary<string, RuntimeAnimatorController> _ctrls = new Dictionary<string, RuntimeAnimatorController>();

        public static float updateInterval = 0.05f;
        public static uint dwCreatedActorCount = 0;
        public static int dwCurActorCount = 0;
        public static int dwMoveHash = -1905755797;


        public void Init()
        {

        }

		protected override void ExtraRefLua()
		{
            
        }

        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "speed":
                    lua.PushNumber(_speed);
                    return true;
                case "dwMMethod":
                    lua.PushInteger(_dwMMethod);
                    return true;
                case "bMoving":
                    lua.PushBoolean(_bMoving);
                    return true;
                case "fUpdateInterval":
                    lua.PushNumber(_fUpdateInterval);
                    return true;
                case "fAnimSpeed":
                    if (_animator)
                    {
                        lua.PushNumber(_animator.speed);
                    }
                    else
                    {
                        lua.PushNumber(0);
                    }
                    return true;
            }

            return base.WidgetReadOper(lua, key);
        }

        protected override bool WidgetWriteOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "speed":
                    SetSpeed((float)lua.L_CheckNumber(3));
                    return true;
                case "bLockDir":
                    if (lua.Type(3)==LuaType.LUA_TBOOLEAN)
                    {
                        SetLockDir(lua.ToBoolean(3));
                    } 
                    return true;

                case "moveTime":
                    SetMoveTime((float)lua.L_CheckNumber(3));
                    return true;
                case "dwMMethod":
                    _dwMMethod = lua.L_CheckInteger(3);
                    return true;
                case "bMoving":
                    Debug.LogWarning("actor.bMoving is readonly.");
                    return true;
                case "fUpdateInterval":
                    _fUpdateInterval = (float)lua.L_CheckNumber(3);
                    return true;
                case "fAnimSpeed":
                    if (_animator)
                    {
                        _animator.speed = (float)lua.L_CheckNumber(3);
                    }
                    return true;
            }

            return base.WidgetWriteOper(lua, key);
        }

        private void SetMoveTime(float time)
        {
        }

        public virtual void Update()
        {
        }

        public void ClearBones()
        {
        }

        private void InitMoveCtrl()
        {
        }

		protected override void OnMouseHover(bool bHover)
		{
        
		}


        public void Talk(string szInfo, float fDisplayTime)
        {

        }

        public void MoveTo(float x, float y, float z, bool bQueryHeight, int pfnMoveStop, int pfnMoveStart, int pfnMoveUpdate)
        {
            Debug.Log("Actor:MoveTo");
        }
        public void OnMoveUpdate()
        {
            Debug.Log("Actor:OnMoveUpdate");
        }
         public void OnMoveEnd()
        {
            Debug.Log("Actor:OnMoveEnd");
        }    
        private void OnPathComplete()
        {
        }

        private void CallOnMoveStart(ILuaState lua,List<Vector3> paths)
        {
        }


        public bool MoveToEntity(int dwEntityID, ref Vector3 vDst, float dis = 2)
		{
            return false;
		}


        public float GetSpeed()
        {
            return _speed;
        }

        public void SetLockDir(bool bLockDir)
        {
        }

        public void SetSpeed(float value)
        {
		}

        private void RotateToDir(Vector3 dir)
        {
 
        }
        public override bool Visible
        {
            get
            {
                return _bVisible;
            }
            set
            {
                if (value != _bVisible)
                {
                    for (int i = 0; i < transform.childCount; ++i)
                    {
                        Transform t = transform.GetChild(i);
                        Renderer pRender = t.GetComponent<Renderer>();
                        if (pRender != null)
                        {
                            pRender.enabled = value;
                        }
                    }
                }
                _bVisible = value;
            }
        }

        public void SetSkinVisible(bool bVisible)
        {

        }

        public void SetAllChildrenVisible(bool bVisible)
        {

        }

		public Transform GetBone(string name)
		{
            return null;
		}

        public void PlayAnim(string name, float fSpeed = 1.0f, int pfnOnEnd = 0, float transitionDuration = 0.2f, float fPer = 0f)
        {
 
        }

        public void SetAnimClip(string clip, string newClipPath)
        {
 
        }

		private void EvtAnim(UnityEngine.Object obj)
		{
  
		}

        public Animator GetAnimator()
        {
            return _animator;
        }

        public override void PlayEffect(
            string name, 
            float fDelay, 
            bool bWorldPos, 
            float fExistTime,
            EffectType type)
		{
			
		}

        protected virtual void PlayWorldEffect(ref string name, float fDelay, float fExistTime)
        {
           
        }

		public void PlayTraceEffect(Transform target,string name,float fSpeed,float fDelay)
		{
			
		}

		public override void SetLayer(int dwLayer)
		{
			var renders = GetComponentsInChildren<SkinnedMeshRenderer>();
			for(int i=0; i<renders.Length; ++i)
			{
				var render = renders[i];
				render.gameObject.layer = dwLayer;
			}
		}

        public bool SetMeshPart(string partName, string resPath, string replaceRes=null)
        {

            return false;
        }

        public bool SetMeshPart(string partName, GameObject pSkinMeshes)
        {
            return true;
        }

        public SkinnedMeshRenderer[] GetMeshPart(string partName)
        {
            try
            {
                return _meshparts[partName];
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public void RemoveMeshPart(string partName)
        {
            var parts = GetMeshPart(partName);
            if (parts != null)
            {
                for (int i = 0; i < parts.Length; ++i)
                {
                    GameObject.DestroyImmediate(parts[i].gameObject);
                }
            }

            if (_meshparts != null)
            {
                _meshparts.Remove(partName);
            }
        }

        private void ReleasePfnAnimEnd(ILuaState lua)
        {
            if (_nextAnimInfo.pfnOnEnd != 0)
            {
                lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, _nextAnimInfo.pfnOnEnd);
                _nextAnimInfo.pfnOnEnd = 0;
            }
        }

        //proxy,表示如果指定的控制器不存在，那么使用该替代的控制器
        //如果proxy为null那么没有
        public void SetAnimCtrl(string name,string proxy=null)
        {
        }

        public void SetAnimCtrl(int nCtrlID, int nAnimID)
        {

        }

        public void ReplaceAnimCtrl(string name, RuntimeAnimatorController ctrl)
        {
            var oldCtrl = GetCtrl(ref name);
            if (oldCtrl != null)
            {
                _ctrls.Remove(name);
            }

            if (ctrl == null)
                return;

            _ctrls.Add(name, ctrl);
            if (oldCtrl == _animator.runtimeAnimatorController)
            {
                _animator.runtimeAnimatorController = ctrl;
            }
        }

        public RuntimeAnimatorController GetCtrl(ref string name)
        {
            RuntimeAnimatorController v = null;
            _ctrls.TryGetValue(name, out v);
            return v;
        }
		
        int Lua_Talk(ILuaState lua)
        {
			string szContent = lua.L_CheckString(2);
			float fTime = (float)lua.L_CheckNumber(3);
			this.Talk(szContent,fTime);
            return 0;
        }

		protected override void OnDestroy()
		{
            if (_luaIndex != 0)
            {
                --dwCurActorCount;
            }

            try
            {
                ILuaState lua = Game.LuaApi;
                ReleasePfnAnimEnd(lua);
                ReleasePfnMoveStart(lua);
                ReleasePfnMoveStop(lua);
                ReleasePfnMoveUpdate(lua);
            }
            catch (System.Exception)
            {

            }
            
			base.OnDestroy();
		}

        public void MoveUpdate()
        {
            if (_bMoving == false)
                return;

            ILuaState lua = Game.LuaApi;
            if (lua == null)
                return;

            if (_moveInfo.pfnMoveUpdate != 0)
            {
                lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, _moveInfo.pfnMoveUpdate);
                PushThis(lua);
                if (lua.PCall(1, 0, 1) != 0)
                {
                    Debug.LogWarning(lua.ToString(-1));
                    lua.Pop(1);
                }
            }
        }
        public List<EventDelegate> onMoveStop = new List<EventDelegate>();
        public void MoveStop(bool bCall = true)
        {   
        }

        private void ReleasePfnMoveStop(ILuaState lua)
        {
            if (_moveInfo.pfnMoveStop != 0)
            {
                lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, _moveInfo.pfnMoveStop);
                _moveInfo.pfnMoveStop = 0;
            }
        }

        private void ReleasePfnMoveUpdate(ILuaState lua)
        {
            if (_moveInfo.pfnMoveUpdate != 0)
            {
                lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, _moveInfo.pfnMoveUpdate);
                _moveInfo.pfnMoveUpdate = 0;
            }
        }
        private void ReleasePfnMoveStart(ILuaState lua)
        {
            if (_moveInfo.pfnMoveStart != 0)
            {
                lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, _moveInfo.pfnMoveStart);
                _moveInfo.pfnMoveStart = 0;
            }
        }

        void Call_Update()
        {
        }








        float GetArrayNumber(ILuaState lua, int tbIndex,int index)
        {
            return 0;
        }




        protected void ActRigidbody(bool bAct,bool bFreeze)
        {
            if (gameObject.GetComponent<Rigidbody>() == null)
            {
                if (bAct)
                {
                    Rigidbody rigid;
                    rigid = gameObject.AddComponent<Rigidbody>();
                    rigid.isKinematic = false;
                    rigid.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    rigid.angularDrag = 3f;

                    if (bFreeze)
                    {
                        rigid.useGravity = false;
                        rigid.constraints = RigidbodyConstraints.FreezeAll;
                    }
                    else
                    { 
                        rigid.useGravity = true;
                    }

                }
            }
            else
            {
                if (!bAct)
                {
                    Destroy(gameObject.GetComponent<Rigidbody>());
                }

            }
        }

        protected void TryForceAtDirection(float fStrength, Vector3 vDir, ForceMode forceType)
        {
            Rigidbody affectedrRigidbody = gameObject.GetComponent<Rigidbody>();
            if (!affectedrRigidbody)
            {
                Debug.Log("Attempting to apply an impulse to an object, but it has no rigid body from USequencerApplyImpulseEvent::FireEvent");
                return;
            }

            affectedrRigidbody.AddForceAtPosition(vDir * fStrength, transform.position,forceType);
        }

 




        public void ShowSkillArea(int shapeType, float fDistance, float fRadius, float fAngle, float fWidth, float fHeight, float fExistTime, int posType)
        {

        }



    }

}