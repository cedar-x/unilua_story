using UnityEngine;
using System.Collections;
using UniLua;


namespace Hstj
{
    //[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class LuaMeshImage : LuaObject {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRender;
        private Mesh _mesh;
//         public int width = 2560;
//         public int height = 1440;
//         public float rate = 500f;
        public int width = 4200;
        public int height = 1800;
        public float rate = 400f;
        public int tt = 1;
        float whRate = 0;
        public void Awake()
        {
            base.RefLua();
        }
        public static LuaMeshImage CreateInstance(string meshName="MeshImage")
        {
            GameObject obj = new GameObject(meshName);
            return obj.AddComponent<LuaMeshImage>();
        }
        protected override void ExtraRefLua()
        {
            Game.Lua.SetTableFunction(-1, "Init", new CSharpFunctionDelegate(Lua_Init));
            Game.Lua.SetTableFunction(-1, "Clear", new CSharpFunctionDelegate(Lua_Clear));
        }
	    // Use this for initialization
	    public void init(string spName, float fieldOfView = -1) {

            if (_meshFilter == null || _meshRender == null)
            {
                _meshFilter = GetComponent<MeshFilter>();
                _meshRender = GetComponent<MeshRenderer>();
                if (_meshFilter == null || _meshRender == null) {
                    Debug.LogWarning("LuaMeshImage RequireComponent is not complete.." + _meshFilter + ":" + _meshRender);
                    return;
                }
            }
            _meshFilter.mesh =_mesh = new Mesh();
            _mesh.name = "chgImage";
            _mesh.vertices = new Vector3[] { new Vector3(-width, -width, 0), new Vector3(width, -width, 0), new Vector3(width, width, 0), new Vector3(-width, width, 0) };
            _mesh.triangles = new int[6]{0, 1, 2, 2, 3, 0};
            _mesh.normals = new Vector3[4] { new Vector3(0, 0, 5), new Vector3(0, 0, 5), new Vector3(0, 0, 5), new Vector3(0, 0, 5) };
            _mesh.uv = new Vector2[4] { new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
            transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            screenSize(fieldOfView);
            //Material mat = ResManager.CreateMaterialFromPrefab("Model/storyBackground/materials/"+spName);
            //SetMaterial(mat);
        }
        public void Clear()
        {
            _mesh = null;
            _meshFilter.mesh = null;
        }
        public void screenSize(float fieldOfView)
        {
            if (fieldOfView == -1)
            {
                GameObject obj = GameObject.FindGameObjectWithTag("JQCamera");
                if (obj != null)
                {
                    Camera cam = obj.GetComponent<Camera>();
                    fieldOfView = cam.fieldOfView;
                }
                else
                {
                    Debug.LogWarning("can't find Camera with tag JQCamera...");
                    return;
                }
            }
            float distance = _meshFilter.transform.localPosition.z;
            float h = distance * Mathf.Tan(fieldOfView / 2 * Mathf.PI / 180) * 2;
            float w = h * Screen.width / Screen.height;
            //Debug.Log("===" + distance + ":" + field + ":" + w + ":" + h);
            chgSize(w, h, 1);
        }
        public void chgSize(float w, float h, float whRate)
        {
            _mesh.vertices = new Vector3[] { new Vector3(-w * whRate / 2, -h * whRate / 2, 0), new Vector3(w * whRate / 2, -h * whRate / 2, 0), new Vector3(w * whRate / 2, h * whRate / 2, 0), new Vector3(-w * whRate / 2, h * whRate / 2, 0) };   
        }
        public void SetMaterial(Material mat)
        {
            _meshRender.material = mat;
        }
//         void OnGUI()
//         {
//             if (GUILayout.Button("chg"))
//             {
//                 if (tt == 1)
//                     whRate = 1 / rate;
//                 else
//                     whRate = rate;
//                 _mesh.vertices = new Vector3[] { new Vector3(-width * whRate / 2, -height * whRate / 2, 0), new Vector3(width * whRate / 2, -height * whRate / 2, 0), new Vector3(width * whRate / 2, height * whRate / 2, 0), new Vector3(-width * whRate / 2, height * whRate / 2, 0) };   
// 
//             }
//             if (GUILayout.Button("resizeMesh"))
//             {
//                 Camera cam = Camera.main;
//                 float distance = _meshFilter.transform.localPosition.z;
//                 float field = cam.fieldOfView;
//                 float h = distance * Mathf.Tan(field / 2 * Mathf.PI / 180)*2;
//                 float w = h*Screen.width/Screen.height;
//                 Debug.Log("===" + distance + ":" + field + ":" + w + ":" + h);
//                 chgSize(w, h, 1);
//             }
//         }

        private int Lua_Init(ILuaState lua)
        {
            string spName = lua.L_CheckString(2);
            float distance = (float)lua.L_CheckNumber(3);
            transform.localPosition = new Vector3(0f, 0f, distance);
            init(spName);
            return 0;
        }
        private int Lua_Clear(ILuaState lua)
        {
            Clear();
            return 0;
        }
	
    }
}

