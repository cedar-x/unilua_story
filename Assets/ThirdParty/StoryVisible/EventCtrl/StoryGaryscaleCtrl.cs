using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--画面去色--事件的调度
/// 设计时间：2015-08-18
/// </summary>

namespace xxstory {

    public class StoryGrayscaleCtrl : StoryBaseCtrl {

        public float durTime;
        public float dwStart = 0f;
 
        /// ////////////////////////////////////////////////////////
      
        public override string luaName
        {
            get { return "StoryGrayscaleCtrl"; }
        }

        public override string ctrlName
        {  
            get { return "画面去色"; }
        }

        public override void initInfo()
        {
            bWait = true;
            base.initInfo();
            expList.Add("durTime");
        }

        public override StoryBaseCtrl CopySelf()
        {
            StoryGrayscaleCtrl obj = new StoryGrayscaleCtrl();
            obj.bWait = bWait;
            obj.bClick = bClick;
            obj._baseCtrl = _baseCtrl;
            obj.durTime = durTime;
            return obj;
        }

        public override void Execute()
        {
            GrayscaleEffect obj = objMainCamera.gameObject.GetComponent<GrayscaleEffect>();   
            if (obj == null)
            {
               objMainCamera.gameObject.AddComponent<GrayscaleEffect>();
               obj = objMainCamera.gameObject.GetComponent<GrayscaleEffect>();   
            }
            dwStart = Time.time;
            obj.shader = Shader.Find("Hidden/Grayscale Effect");
            obj.enabled = true;
        }

        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key){
                case "durTime":
                    lua.PushNumber(durTime);
                    break;
                default:
                    return base.WidgetReadOper(lua, key);
            }
            return true;
        }
        ////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR

        //Inspector param
        public override void OnParamGUI(){
            
            durTime = EditorGUILayout.FloatField("durTime", durTime);
            base.OnParamGUI();
        }
#endif
        public override void Update()
        {
            if (dwStart == 0) 
                return;
            if (Time.time - dwStart > durTime) {
                dwStart = 0f;
                OnFinish();
            }     
        }

        public override void OnFinish()
        {
            objMainCamera.gameObject.GetComponent<GrayscaleEffect>().enabled = false;
            base.OnFinish();

        }

        
    }
}