using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniLua;

namespace Hstj
{
    public class HUIShowPicture : LuaObject
    {
        public class showPictureData
        {
            public GameObject gameObject;
            public Transform transform;
            public UISprite sprite;
            public UITexture texture;
            public UIBasicSprite.Type type
            {
                get
                {
                    if (sprite != null)
                        return sprite.type;
                    return texture.type;
                }
                set
                {
                    if (sprite != null)
                        sprite.type = value;
                    else
                        texture.type = value;
                }
            }
            public UIBasicSprite.Flip flip
            {
                get
                {
                    if (sprite != null)
                        return sprite.flip;
                    return texture.flip;
                }
                set
                {
                    if (sprite != null)
                        sprite.flip = value;
                    else
                        texture.flip = value;
                }
            }
            public UIBasicSprite.FillDirection fillDirection
            {
                get
                {
                    if (sprite != null)
                        return sprite.fillDirection;
                    return texture.fillDirection;
                }
                set
                {
                    if (sprite != null)
                        sprite.fillDirection = value;
                    else
                        texture.fillDirection = value;
                }
            }
            public float fillAmount
            {
                get
                {
                    if (sprite != null)
                        return sprite.fillAmount;
                    return texture.fillAmount;
                }
                set
                {
                    if (sprite != null)
                        sprite.fillAmount= value;
                    else
                        texture.fillAmount = value;
                }
            }
            public bool invert
            {
                get
                {
                    if (sprite != null)
                        return sprite.invert;
                    return texture.invert;
                }
                set
                {
                    if (sprite != null)
                        sprite.invert = value;
                    else
                        texture.invert = value;
                }
            }

            public bool init(Transform trans)
            {
                if (trans == null)
                {
                    Debug.LogWarning("showPictureData init failed Transform is null");
                    return false;
                }
                gameObject = trans.gameObject;
                transform = trans;
                if (trans.GetComponent<UISprite>() != null)
                {
                    sprite = trans.GetComponent<UISprite>();
                    return true;
                }else if (trans.GetComponent<UITexture>() != null)
                {
                    texture = trans.GetComponent<UITexture>();
                    return true;
                }
                Debug.LogWarning("exception no sprite and no texture...");
                return false;
            }
            public bool init(GameObject obj)
            {
                if (obj == null)
                {
                    Debug.LogWarning("showPictureData init failed GameObject is null");
                    return false;
                }
                return init(obj.transform);
            }
            public void SetFilled(UIBasicSprite.FillDirection fillDirection, bool invert)
            {
                if (sprite != null)
                {
                    sprite.type = UIBasicSprite.Type.Filled;
                    sprite.flip = UIBasicSprite.Flip.Nothing;
                    sprite.fillDirection = fillDirection;
                    sprite.fillAmount = 1;
                    sprite.invert = invert;
                }else if (texture != null)
                {
                    texture.type = UIBasicSprite.Type.Filled;
                    texture.flip = UIBasicSprite.Flip.Nothing;
                    texture.fillDirection = fillDirection;
                    texture.fillAmount = 1;
                    texture.invert = invert;
                }
            }
        }
        public enum Arrangement
        {
            Horizontal,
            Vertical,
        }

        public BetterList<showPictureData> _pictures = new BetterList<showPictureData>();
        public Arrangement arrangement = Arrangement.Horizontal;
        public float cellWidth = 200f;
        public float cellHeight = 200f;
        public int maxPerLine = 0;
        public bool hideInactive = true;
        [HideInInspector]
        public UIBasicSprite.FillDirection mFillDirection = UIBasicSprite.FillDirection.Vertical;
        //[HideInInspector]
        public bool _instantTween;
        public bool invert = true;
        public float speedAmount = 0.1f;
        public float speedTime = 0.1f;
        public bool bAwake = true;

        private bool _bStart;
        private float _start;
        private int _index;
        private float _fillAmount;

        public bool instantTween
        {
            get
            {
                return _instantTween;
            }
            set
            {
                _instantTween = value;
            }
        }
        public UIBasicSprite.FillDirection fillDirection
        {
            get
            {
                return mFillDirection;
            }
            set
            {
                mFillDirection = value;
                for (int i = 0, imax = _pictures.size; i < imax; ++i)
                {
                    _pictures[i].SetFilled(mFillDirection, invert);
                }
            }
        }
        void Awake()
        {
            base.Awake();
            InitMember();
            if (bAwake == true)
                showPicture();
        }
        protected override void ExtraRefLua()
        {
            base.ExtraRefLua();

            Game.Lua.SetTableFunction(-1, "Reposition", new CSharpFunctionDelegate(Lua_Reposition));
            Game.Lua.SetTableFunction(-1, "showPicture", new CSharpFunctionDelegate(lua_showPicture));
        }
        void InitMember()
        {
            Debug.Log("HUIShowPicture InitMember.....");
            _pictures.Clear();
            showPictureData pic = new showPictureData();
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform t = transform.GetChild(i);
                if (!hideInactive || (t && NGUITools.GetActive(t.gameObject)))
                {
                    if (pic.init(t))
                    {
                        pic.SetFilled(mFillDirection, invert);
                        _pictures.Add(pic);
                        pic = new showPictureData();
                    }
                }
            }
        }
        public void startShow()
        {
            _bStart = true;
            _start = Time.time;
            _index = 0;
            _fillAmount = 0;
        }
        void Update()
        {
            if (_bStart == false)
                return;
            if (Time.time - _start < speedTime)
                return;
            _start = Time.time;
            _fillAmount += speedAmount;
            
            showPictureData pic = GetChild(_index);
            pic.fillAmount = _fillAmount;

            
            if (_fillAmount >= 1)
            {
                _index += 1;
                if (_index >= GetChildListCount())
                    _bStart = false;
                _fillAmount = 0;
            }

        }
        public BetterList<showPictureData> GetChildList()
        {
            return _pictures;
        }

        public int GetChildListCount()
        {
            return GetChildList().size;
        }

        public showPictureData GetChild(int index)
        {
            BetterList<showPictureData> list = GetChildList();
            return (index < list.size) ? list[index] : null;
        }

        public int GetIndex(showPictureData trans) { return GetChildList().IndexOf(trans); }

        protected void ResetPosition(BetterList<showPictureData> list)
        {
            //mReposition = false;
            int x = 0;
            int y = 0;
            int maxX = 0;
            int maxY = 0;
            Transform myTrans = transform;

            // Re-add the children in the same order we have them in and position them accordingly
            for (int i = 0, imax = list.size; i < imax; ++i)
            {
                Transform t = list[i].transform;

                float depth = t.localPosition.z;
                Vector3 pos = (arrangement == Arrangement.Horizontal) ?
                    new Vector3(cellWidth * x, -cellHeight * y, depth) :
                    new Vector3(cellWidth * y, -cellHeight * x, depth);

                t.localPosition = pos;

                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);

                if (++x >= maxPerLine && maxPerLine > 0)
                {
                    x = 0;
                    ++y;
                }
            }

            // Apply the origin offset
//             if (pivot != UIWidget.Pivot.TopLeft)
//             {
//                 Vector2 po = NGUIMath.GetPivotOffset(pivot);
// 
//                 float fx, fy;
// 
//                 if (arrangement == Arrangement.Horizontal)
//                 {
//                     fx = Mathf.Lerp(0f, maxX * cellWidth, po.x);
//                     fy = Mathf.Lerp(-maxY * cellHeight, 0f, po.y);
//                 }
//                 else
//                 {
//                     fx = Mathf.Lerp(0f, maxY * cellWidth, po.x);
//                     fy = Mathf.Lerp(-maxX * cellHeight, 0f, po.y);
//                 }
// 
//                 for (int i = 0; i < myTrans.childCount; ++i)
//                 {
//                     Transform t = myTrans.GetChild(i);
//                     SpringPosition sp = t.GetComponent<SpringPosition>();
// 
//                     if (sp != null)
//                     {
//                         sp.target.x -= fx;
//                         sp.target.y -= fy;
//                     }
//                     else
//                     {
//                         Vector3 pos = t.localPosition;
//                         pos.x -= fx;
//                         pos.y -= fy;
//                         t.localPosition = pos;
//                     }
//                 }
//             }
        }
        [ContextMenu("Execute")]
        public void Reposition()
        {
            InitMember();
            BetterList<showPictureData> list = GetChildList();
            ResetPosition(list);
        }
        [ContextMenu("ShowPicture")]
        public void showPicture()
        {    
            BetterList<showPictureData> list = GetChildList();
            ResetPosition(list);
            for (int i=0, imax = list.size; i < imax; ++i)
            {
                list[i].fillAmount = 0;
            }
            startShow();
        }
        private int Lua_Reposition(ILuaState lua)
        {
            Reposition();
            return 0;
        }
        private int lua_showPicture(ILuaState lua)
        {
            showPicture();
            return 0;
        }

    }
}

