using UnityEngine;
using System.Collections;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UniLua;
using Hstj;

/// <summary>
/// 设计目的：用于--选项类--事件的调度
/// 设计时间：2015-09-21
/// </summary>

namespace xxstory
{
    public class StoryOptionCtrl : StoryBaseCtrl
    {
        //事件相关属性

        /// ////////////////功能重写部分-必须实现部分/////////////////////////////////////////////
        public override string luaName
        {
            get { return "StoryOptionCtrl"; }
        }
        public override string ctrlName
        {
            get { return "选项"; }
        }
        public override void initInfo()
        {
            base.initInfo();

        }
        public override StoryBaseCtrl CopySelf()
        {
            StoryOptionCtrl obj = new StoryOptionCtrl();
            obj.bClick = bClick;
            obj.bWait = bWait;
            obj._baseCtrl = _baseCtrl;
            //////本类事件属性赋值
            return obj;
        }
        public override void Execute()
        {
            ILuaState lua = Game.LuaApi;
            lua.GetGlobal("UIStoryShowOpt");
            if (lua.PCall(0, 0, 0) != 0)
            {
                Debug.LogWarning(lua.ToString(-1));
                lua.Pop(-1);
            }
        }
 
        /// ////////////////功能重写部分-需要保存状态时可以重写/////////////////////////////////////////////

        /// 每帧执行一次调用-与MonoBehaviour调用频率相同
//         public override void Update()
//         {
//         }
//         //事件等待结束，结束有特殊需求可以重写-用于bWait=true的事件的结束调用
//         public override void OnFinish()
//         {
//         }
//         //修改事件内容可以重写-点击页签AddEvent-修改时调用
//         public override void ModInfo()
//         {
//         }
//         //保存存储点时可以重写-点击页签AddEvent-存储点时调用
//         public override void SavePoint()
//         {
//         }
//         //重设存储点时可以重写-点击页签AddEvent-重设时调用
//         public override void ResetPoint()
//         {
//         }
        /// //////////////////属性导出导入部分-有导入导出需求时重写///////////////////////////////////////////////
        protected override bool WidgetReadOper(ILuaState lua, string key)
        {
            switch (key)
            {
                default:
                    return base.WidgetReadOper(lua, key);
            }
            return true;
        }
#if UNITY_EDITOR 
        /// ////////////////UI显示部分-AddEvent页签中创建相应事件UI显示/////////////////////////////////////////////
        public override void OnParamGUI()
        {
            GUILayout.Label("StoryOptionCtrl");
            base.OnParamGUI();
        }
#endif
        /// /////////////////////////////// 功能函数 ///////////////////////////////////////////////////////////////

    }
}