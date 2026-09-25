using System;
using UnityEngine;

namespace DefaultNamespace
{
    
    /*
    public class test1 : UIBase
    {
        public override bool IsVisible { get; set; }
        public override IGameManager GetGameManager()
        {
            return null;
        }

        protected override void Update()
        {
            
        }
        
        protected override void Dispose()
        {
            
        }
    }
    */
    
    
    public class test : MonoBehaviour
    {
        private void Awake()
        {
            Action a = aaa;
            a += bbb;
            a -= bbb;
            Debug.LogError(a == null);
            
            a -= aaa;
            
            Debug.LogError(a == null);

            a -= aaa;
        }

        void aaa()
        {
            
        }

        void bbb()
        {
            
        }
    }
}