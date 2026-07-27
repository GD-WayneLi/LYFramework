using System;
using UnityEngine;

namespace QFramework.Demo
{
    public class testBase
    {
        
    }

    public class a : testBase
    {
        
    }

    public class b
    {
        
    }
    
    public class Launcher : MonoBehaviour
    {
        void Start()
        {
            //LYGameManager.Instance.Init();

            var a = Check<a>(new a());
            var b = Check<b>(new b());
            var c = Check<a>(new b());
            
            Debug.Log(a);
            Debug.Log(b);
            Debug.Log(c);
        }

        private void OnDestroy()
        {
            //LYGameManager.Instance.Dispose();
        }
        
        bool Check<T>(object t)
        {
            if (t is T)
            {
                return true;
            }

            return false;
        }
    }
}