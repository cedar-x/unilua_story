using UnityEngine;
using System.Collections.Generic;
using UniLua;

namespace Hstj
{
    public enum EffectType
    {
        Once,
        Keep,
    }

    public class LuaEffect  : LuaObject
    {
        private Animator[] m_animators = null;
        private ParticleSystem[] m_syses = null;
        private EffectDelay[] m_modelEffects = null;
        private bool m_bRun = false;
        private float m_fStartTime = 0;
        private float m_fDelay = 0;
        private bool m_bStarted = false;
        private int m_pfnFinish = 0;
        private bool m_bDestroyOnFinish;
        private float m_fEffectTime = 0;
        public bool DelayModelEffect;
        private EffectType m_eEffectType = EffectType.Once;

        protected override void ExtraRefLua()
        {
            Game.Lua.SetTableFunction(-1, "Play", new CSharpFunctionDelegate(Lua_Play));
            Game.Lua.SetTableFunction(-1, "Stop", new CSharpFunctionDelegate(Lua_Stop));
        }

        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "onFinish":
                    if (m_pfnFinish != 0)
                    {
                        //lua.RawGetRegistry(m_pfnFinish);
                    }
                    return true;
                case "bDestroyOnFinish":
                    lua.PushBoolean(m_bDestroyOnFinish);
                    return true;
                case "finishTime":
                    lua.PushNumber(m_fEffectTime);
                    return true;
            }
            return base.WidgetReadOper(lua, key);
        }

        protected override bool WidgetWriteOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "onFinish":
                    {
                        //lua.L_UnrefRegistry(ref m_pfnFinish);

                        if (lua.Type(3) != LuaType.LUA_TFUNCTION)
                        {
                            return true;
                        }

                        lua.PushValue(3);
                        m_pfnFinish = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
                    }
                    return true;
                case "bDestroyOnFinish":
                    m_bDestroyOnFinish = lua.ToBoolean(3);
                    return true;
                case "finishTime":
                    m_fEffectTime = (float)lua.ToNumber(3);
                    return true;
            }
            return base.WidgetWriteOper(lua, key);
        }

        public EffectType Type
        {
            get { return m_eEffectType; }
            set { m_eEffectType = value; }
        }

        public virtual void Update()
        {
            if (!m_bRun)
                return;

            float fNow = Time.time;
            float fInterval = fNow - m_fStartTime;

            if (fInterval >= m_fDelay && m_bStarted == false)
            {
                m_bStarted = true;
                m_fStartTime = fNow;
                DoPlayEffect();
            }

            if (m_fEffectTime != 0 && fInterval > m_fEffectTime)
            {
                OnFinish();
                m_bRun = false;
                if (m_bDestroyOnFinish)
                {
                    GameObject.DestroyImmediate(gameObject);
                }
            }

        }

        public bool Running
        {
            get { return m_bRun; }
        }

        public float StartDelay
        {
            get { return m_fDelay; }
        }

        public float StartTime
        {
            get { return m_fStartTime; }
        }

        public bool DestroyOnFinish
        {
            get { return m_bDestroyOnFinish; }
            set { m_bDestroyOnFinish = true; }
        }
        
        public virtual  void Play(float fDelay,float fPalyTime=0,bool bDestroyOnFinish=false)
        {
            m_bStarted = false;
            m_bRun = true;
            m_fStartTime = Time.time;
            m_fDelay = fDelay;
            if (fPalyTime != 0)
            {
                m_fEffectTime = fPalyTime;
            }
            m_bDestroyOnFinish = bDestroyOnFinish;

            if (gameObject.activeSelf == false)
                gameObject.SetActive(true);

            InitMember();
        }

        public virtual void Stop()
        {
            InitMember();

            if(DelayModelEffect == false)
            {
                for (int i = 0; i < m_syses.Length; ++i)
                {
                    m_syses[i].Stop();
                }

                //for (int i = 0; i < m_animators.Length; ++i)
                //{
                //    m_animators[i].Rebind();
                //    m_animators[i].Play(m_animators[i].GetCurrentAnimatorStateInfo(0).nameHash);
                //}
            }
            
            gameObject.SetActive(false);
        }

        public float ExistTime
        {
            get { return m_fEffectTime; }
            set { m_fEffectTime = value; }
        }

        public void Reinit()
        {
            if (DelayModelEffect == false)
            {
                m_animators = GetComponentsInChildren<Animator>();

                m_syses = GetComponentsInChildren<ParticleSystem>();
            }
            else
            {
                m_modelEffects = GetComponentsInChildren<EffectDelay>();

                for (int i = 0; i < m_modelEffects.Length; ++i)
                {
                    m_modelEffects[i].ReInit(this);
                }
            }
        }

        private void InitMember()
        {
            if(DelayModelEffect == false)
            {
                //如果不带，延迟的模型特效，那么直接初始化
                if (m_animators == null)
                {
                    m_animators = GetComponentsInChildren<Animator>();
                }

                if (m_syses == null)
                {
                    m_syses = GetComponentsInChildren<ParticleSystem>();
                }
                return;
            }

            if (m_modelEffects == null)
            {
                m_modelEffects = GetComponentsInChildren<EffectDelay>();

                for (int i = 0; i < m_modelEffects.Length; ++i)
                {
                    m_modelEffects[i].InitMember(this);
                }

            }

        }


        private void DoPlayEffect()
        {
            InitMember();

            if(DelayModelEffect == false)
            {
                for (int i = 0; i < m_syses.Length; ++i)
                {
                    m_syses[i].Play();
                }

                for (int i = 0; i < m_animators.Length; ++i)
                {
                    m_animators[i].Rebind();
                    m_animators[i].Play(m_animators[i].GetCurrentAnimatorStateInfo(0).nameHash);
                }
                return;
            }

            for (int i = 0; i < m_modelEffects.Length; ++i)
            {
                m_modelEffects[i].Play();
            }

        }

        protected override void OnDestroy()
        {
            //Debug.LogWarning("####OnDestroy####" + Util.ParentObjectNames(gameObject));
            UnrefOnFinish();
            base.OnDestroy();
        }

        private void SetOnFinishCallback(int pfn)
        {
            UnrefOnFinish();
            m_pfnFinish = pfn;
        }

        protected void OnFinish()
        {
            gameObject.SetActive(false);

            if (m_pfnFinish == 0)
                return;

            ILuaState lua = Game.LuaApi;
            lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, m_pfnFinish);
            PushThis(lua);
            if (lua.PCall(1, 0, 1) != 0)
            {
                Debug.LogWarning(lua.ToString(-1));
                lua.Pop(1);
            }
        }

        private void UnrefOnFinish()
        {
            if (m_pfnFinish != 0)
            {
                ILuaState lua = Game.LuaApi;
                lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, m_pfnFinish);
                m_pfnFinish = 0;
            }
        }

        private int Lua_Play(ILuaState lua)
        {
            float fDelay = (float)lua.ToNumber(2);
            float fPlayTime = (float)lua.ToNumber(3);
            bool bDestroyOnFinish = lua.ToBoolean(4);
            Play(fDelay,fPlayTime,bDestroyOnFinish);
            return 0;
        }

        private int Lua_Stop(ILuaState lua)
        {
            Stop();
            return 0;
        }
    }
}