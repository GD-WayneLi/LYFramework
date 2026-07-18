using System;
using System.Collections.Generic;
using LYFramework.Log;

namespace LYFramework.UI
{
    public class UIManager
    {
        private readonly List<UIBase> m_UIStack = new(50);
        
        public void OpenUI<T>(string path, int layer, Action<UIBase> callback, object data = null) where T : UIBase, new()
        {
            var ui = new T();
            m_UIStack.Add(ui);
            
            ui.Layer = layer;
            callback?.Invoke(ui);

            ui.Load(path, OnUILoadComplete, data);
        }

        public void CloseUI<T>() where T : UIBase, new()
        {
            var index = m_UIStack.FindIndex(ui => ui is T);
            if (index >= 0)
            {
                OnCloseUI(index);
            }
        }

        public void CloseUI(UIBase uiBase)
        {
            var index = m_UIStack.FindIndex(ui => ui == uiBase);
            if (index >= 0)
            {
                OnCloseUI(index);
            }
        }

        public void PopUI()
        {
            if (m_UIStack.Count <= 0)
            {
                LYLogger.Warning("UIStack is empty, buy you still call PopUI");
                return;
            }

            OnCloseUI(m_UIStack.Count - 1);
        }

        public void CloseUIAll()
        {
            foreach (var uiBase in m_UIStack)
            {
                uiBase.Dispose();
            }

            m_UIStack.Clear();
        }

        public void Update()
        {
            for (int i = 0; i < m_UIStack.Count; i++)
            {
                m_UIStack[i].Update();
            }
        }

        public T GetUI<T>() where T : UIBase
        {
            if (m_UIStack.Count > 0)
            {
                foreach (var ui in m_UIStack)
                {
                    if (ui is T uiBase)
                    {
                        return uiBase;
                    }
                }
            }

            return null;
        }

        public bool HasUI<T>() where T : UIBase
        {
            if (m_UIStack.Count > 0)
            {
                foreach (var ui in m_UIStack)
                {
                    if (ui is T)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void OnUILoadComplete(UIBase uiBase)
        {
            SortUIDepth(uiBase.Layer);
            uiBase.OnOpen();

            SetUIVisible();
        }

        void SortUIDepth(int layer)
        {
            var startDepth = layer;
            for (int i = 0; i < m_UIStack.Count; i++)
            {
                if (m_UIStack[i].Layer == layer)
                {
                    m_UIStack[i].Depth = startDepth;
                    startDepth += 100;
                }
            }
        }

        void SetUIVisible()
        {
            
        }
        
        void OnCloseUI(int index)
        {
            m_UIStack[index].Dispose();
            m_UIStack.RemoveAt(index);
        }
    }
}