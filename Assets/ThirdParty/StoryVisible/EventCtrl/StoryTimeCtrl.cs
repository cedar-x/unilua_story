using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;


/// <summary>
/// 设计目的：用于--时间等待--事件的调度
/// 设计时间：2015-08-03
/// </summary>

namespace xxstory
{
    public class StoryTimeCtrl : StoryBaseCtrl
    {
        public float durTime;
        private float dwStart = 0f;

        public override void initInfo()
        {
            bWait = true;
            base.initInfo();
            expList.Add("durTime");
        }
        public override string luaName
        {
            get
            {
                return "StoryTimeCtrl";
            }
        }
        public override string ctrlName
        {
            get
            {
                return "等待";
            }
        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryTimeCtrl obj = new StoryTimeCtrl();
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj._baseCtrl = _baseCtrl;
            obj.durTime = durTime;
            return obj;
        }
        public virtual void ModInfo()
        {

        }

        protected override bool WidgetWriteOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "durTime":
                    durTime = (float)lua.L_CheckNumber(-1);
                    break;
                default:
                    return base.WidgetWriteOper(lua, key);
            }
            return true;
        }
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                case "durTime":
                    lua.PushNumber(durTime);
                    break;
                default:
                    return base.WidgetReadOper(lua, key);
            }
            return true;
        }
#if UNITY_EDITOR 
        public override void OnParamGUI()
        {
            durTime = EditorGUILayout.FloatField("durTime", durTime);
            //base.OnParamGUI();
        }
#endif
        public override void Execute()
        {
            dwStart = Time.time;
        }


        public override void Update()
        {
            if (dwStart == 0) return;
            if (Time.time - dwStart > durTime)
            {
                dwStart = 0f;
                OnFinish();
            }
        }

    }
}
