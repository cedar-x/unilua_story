using UnityEngine;
using System.Collections;

namespace Hstj
{

	public class ActorEffect : LuaEffect 
    {

        private string _bindPos;
		private Transform _bone = null;
        private int _dwEffectID=0;

		public void Setup(Transform node)
		{

		}

		public string BoneName {
			get { return _bindPos; }
			set { _bindPos = value; } 
		}

        public int ID
        {
            get { return _dwEffectID; }
            set { _dwEffectID = value; }
        }
	}


}