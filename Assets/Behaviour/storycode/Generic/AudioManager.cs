using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UniLua;

namespace Hstj
{
    public enum AudioPlayType
    {
        Once=0,
        Keep=1,
    }

    public class AudioObject : LuaObject
    {
        private AudioSource m_source;
        private float m_fStartTime;
        private AudioPlayType m_ePlayType = AudioPlayType.Once;

        protected override void ExtraRefLua()
        {
            base.ExtraRefLua();
            m_source = GetComponent<AudioSource>();

            Game.Lua.SetTableFunction(-1, "Play", new CSharpFunctionDelegate(Lua_Play));
            Game.Lua.SetTableFunction(-1, "Stop", new CSharpFunctionDelegate(Lua_Stop));
            Game.Lua.SetTableFunction(-1, "Pause", new CSharpFunctionDelegate(Lua_Pause));
            Game.Lua.SetTableFunction(-1, "SetMute", new CSharpFunctionDelegate(Lua_SetMute));
            Game.Lua.SetTableFunction(-1, "SetVolume", new CSharpFunctionDelegate(Lua_SetVolume));
        }

        public void Update()
        {
            if (m_source.loop)
                return;

            if (m_source.clip == null)
                return;

            float fInterval = Time.time - m_fStartTime;
            if (fInterval > m_source.clip.length)
            {
                gameObject.SetActive(false);
                m_fStartTime = 0;
                m_source.clip = null;
                AudioManager.ReturnFx(this);
            }
        }

        public void SetClip(AudioClip clip)
        {
            m_source.clip = clip;
        }

        public void SetVolume(float fValue)
        {
            m_source.volume = fValue;
        }

        public float GetVolume()
        {
            return m_source.volume;
        }

        public void SetMute(bool bValue)
        {
            m_source.mute = bValue;
        }

        public bool GetMute()
        {
            return m_source.mute;
        }

        public void SetLoop(bool bValue)
        {
            m_source.loop = bValue;
        }

        public bool GetLoop()
        {
            return m_source.loop;
        }

        public void Play(bool bLoop, float fDelay = 0)
        {
            if (m_source.isPlaying)
            {
                m_source.Stop();
            }
            m_source.loop = bLoop;
            m_fStartTime = Time.time + fDelay;
            if (Mathf.Abs(fDelay) <= 0.0001)
                m_source.Play();
            else
                m_source.PlayDelayed(fDelay);
        }

        public void Stop()
        {
            m_source.Stop();
            m_fStartTime = 0;
        }

        public AudioPlayType Type
        {
            get { return m_ePlayType; }
            set { m_ePlayType = value; }
        }

        public bool IsPlaying
        {
            get
            {
                return m_fStartTime != 0;
            }
        }

        public void Pause()
        {
            m_source.Pause();
        }

        private int Lua_Pause(ILuaState lua)
        {
            Pause();
            return 0;
        }

        private int Lua_Play(ILuaState lua)
        {
            bool bLoop = false;
            if (lua.Type(2) == LuaType.LUA_TBOOLEAN)
            {
                bLoop = lua.ToBoolean(2);
            }
            Play(bLoop);
            return 0;
        }

        private int Lua_Stop(ILuaState lua)
        {
            Stop();
            return 0;
        }

        private int Lua_SetVolume(ILuaState lua)
        {
            SetVolume((float)lua.L_CheckNumber(2));
            return 0;
        }

        private int Lua_SetMute(ILuaState lua)
        {
            SetMute(lua.ToBoolean(2));
            return 0;
        }
        
    }

    public class AudioManager
    {
        private static int m_luaIndex;
        private static List<GCHandle> _lstLuaFunction = new List<GCHandle>();
        private static List<AudioObject> m_lstAudios = new List<AudioObject>();
        private static float m_fFxVolume = 1.0f;
        private static bool m_bEnableFx = true;
        private static Transform m_root; 

        public static void Init()
        {
            RefLua();
        }

        public static void RefLua()
        {
            if (m_luaIndex != 0)
                return;

            ILuaState lua = Game.LuaApi;
            lua.NewTable();
            m_luaIndex = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);

            PushThis(lua);
            Game.Lua.BegTableFunction(_lstLuaFunction);

            Game.Lua.SetTableFunction(-1, "CreateAudio", new CSharpFunctionDelegate(Lua_CreateAudio));
            Game.Lua.SetTableFunction(-1, "DestroyAudio", new CSharpFunctionDelegate(Lua_DestroyAudio));
            Game.Lua.SetTableFunction(-1, "SetFxVolume", new CSharpFunctionDelegate(Lua_SetFxVolume));
            Game.Lua.SetTableFunction(-1, "EnableFx", new CSharpFunctionDelegate(Lua_EnableFx));
            Game.Lua.SetTableFunction(-1, "Play2DSound", new CSharpFunctionDelegate(Lua_Play2DSound));
            Game.Lua.SetTableFunction(-1, "Play3DSound", new CSharpFunctionDelegate(Lua_Play3DSound));

            lua.SetGlobal("AudioManager");

            Game.Lua.EndTableFunction(_lstLuaFunction);
        }

        public static void UnrefLua()
        {
            if (m_luaIndex != 0)
            {
                Clear();

                Game.LuaApi.L_Unref(LuaDef.LUA_REGISTRYINDEX, m_luaIndex);
                m_luaIndex = 0;
            }
        }

        public static void PushThis(ILuaState lua)
        {
            if (m_luaIndex != 0)
            {
                lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, m_luaIndex);
            }
        }

        public static void SetFxVolume(float fValue)
        {
            m_fFxVolume = fValue;

            for (int i = 0; i < m_lstAudios.Count; ++i)
            {
                m_lstAudios[i].SetVolume(fValue);
            }
        }

        //载入背景音乐循环播放
        public static AudioObject CreateAudio(string fileName,bool bLoop)
        {
            AudioObject objSound = CreateAudioObject();
//             objSound.Type = AudioPlayType.Keep;
//             AudioProxy pResProxy = new AudioProxy();
//             pResProxy.Create(objSound, fileName);
//             objSound.Play(bLoop            
            return objSound;
        }

        public static void DestroyAudio(AudioObject audio)
        {
            if (audio == null)
                return;

            GameObject.Destroy(audio.gameObject);
        }

        public static AudioObject PlaySound(string name, Transform refPos, float fDelay = 0)
        {
            return PlaySound(name, refPos.localPosition.x, refPos.localPosition.y, refPos.localPosition.z, fDelay);
        }

        public static AudioObject PlaySound(string name,float fPosX, float fPosY, float fPosZ,float fDelay = 0)
        {
            if (m_bEnableFx == false)
                return null;

            AudioObject pObject = GetPooledAudio();
//             AudioProxy pResProxy = new AudioProxy();
// 
//             pResProxy.Create(pObject, name);
//             pObject.transform.localPosition = new Vector3(fPosX, fPosY, fPosZ);
//             pObject.gameObject.SetActive(true);
//             pObject.Play(false, fDelay);
//             pObject.SetVolume(m_fFxVolume);
            return pObject;
        }

        public static void Clear()
        {
            for (int i = 0; i < m_lstAudios.Count; ++i)
            {
                //GameObject.Destroy(m_lstAudios[i].gameObject);
            }
            m_lstAudios.Clear();

            if(m_root != null)
            {
                //GameObject.DestroyImmediate(m_root.gameObject);
                m_root = null;
            }
        }

        public static AudioObject CreateAudioObject()
        {
            if(m_root == null)
            {
                GameObject pRoot = new GameObject();
                pRoot.name = "AudioObjects";
                m_root = pRoot.transform;
            }

            GameObject obj = new GameObject();
            AudioSource pSource = obj.AddComponent<AudioSource>();
            AudioObject pAudioObject = obj.AddComponent<AudioObject>();
            pAudioObject.RefLua();
            obj.name = "audio";
            obj.transform.parent = m_root;

            return pAudioObject;
        }

        private static AudioObject GetPooledAudio()
        {
            AudioObject pObj = null;
            if (m_lstAudios.Count > 0)
            {
                pObj = m_lstAudios[m_lstAudios.Count - 1];
                m_lstAudios.RemoveAt(m_lstAudios.Count - 1);
            }
            else
            {
                pObj = CreateAudioObject();
            }
            return pObj;
        }

        public static void ReturnFx(AudioObject audio)
        {
            if (audio != null)
            {
                m_lstAudios.Add(audio);
            }
        }

        public static void EnableFx(bool bValue)
        {
            m_bEnableFx = bValue;
        }

        private static int Lua_CreateAudio(ILuaState lua)
        {
            string strName = lua.L_CheckString(2);
            bool bLoop = false;
            if (lua.Type(3) == LuaType.LUA_TBOOLEAN)
            {
                bLoop = lua.ToBoolean(3);
            }
            AudioObject obj = CreateAudio(strName, bLoop);
            if (obj)
            {
                obj.PushThis(lua);
                return 1;
            }
            return 0;
        }

        private static int Lua_DestroyAudio(ILuaState lua)
        {
            var obj =  LuaObject.GetLuaObject(lua, 2);
            AudioObject audio = obj as AudioObject;
            DestroyAudio(audio);
            return 0;
        }

        private static int Lua_SetFxVolume(ILuaState lua)
        {
            float fValue = (float)lua.L_CheckNumber(2);
            SetFxVolume(fValue);
            return 0;
        }

        private static int Lua_EnableFx(ILuaState lua)
        {
            EnableFx(lua.ToBoolean(2));
            return 0;
        }

        private static int Lua_Play2DSound(ILuaState lua)
        {
            string name = lua.L_CheckString(2);
            float fDelay = 0;
            if(lua.Type(3) == LuaType.LUA_TNUMBER)
            {
                fDelay = (float)lua.ToNumber(3);
            }
            PlaySound(name, 0,0,0, fDelay);
            return 0;
        }

        private static int Lua_Play3DSound(ILuaState lua)
        {
            string name = lua.L_CheckString(2);

            float fDelay = 0;
            if (lua.Type(3) == LuaType.LUA_TNUMBER)
            {
                fDelay = (float)lua.ToNumber(3);
            }
            PlaySound(name, 0, 0, 0, fDelay);
            return 0;
        }

    }


}