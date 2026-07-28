using System;
using System.Collections.Generic;

namespace LYFramework
{
    /// <summary>
    /// Scene 容器。World 只管理 Scene 的所有权、生命周期和按 Id 查询，不参与 Entity 树。
    /// 默认移除或销毁 World 时会销毁其拥有的 Scene。所有操作应在同一线程执行。
    /// </summary>
    public sealed class World : IDisposable
    {
        private readonly Dictionary<long, Scene> m_Scenes = new();
        private bool m_IsDisposed;

        public bool IsDisposed => m_IsDisposed;
        public int SceneCount => m_Scenes.Count;

        /// <summary>
        /// 获取当前 Scene 的快照。修改 World 不会改变已获取的快照。
        /// </summary>
        public IReadOnlyList<Scene> Scenes => new List<Scene>(m_Scenes.Values);

        public T AddScene<T>() where T : Scene, new()
        {
            ThrowIfDisposed();
            return AddScene(new T());
        }

        /// <summary>
        /// 将游离 Scene 加入 World。一个 Scene 同一时间只能属于一个 World。
        /// </summary>
        /// <exception cref="ArgumentNullException">scene 为 null。</exception>
        /// <exception cref="ObjectDisposedException">World 或 scene 已销毁。</exception>
        /// <exception cref="InvalidOperationException">scene 已属于 World，或 Id 已存在。</exception>
        public T AddScene<T>(T scene) where T : Scene
        {
            ThrowIfDisposed();

            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            if (scene.IsDisposed)
            {
                throw new ObjectDisposedException(scene.GetType().Name);
            }

            if (scene.World != null)
            {
                throw new InvalidOperationException("The Scene already belongs to a World.");
            }

            if (m_Scenes.ContainsKey(scene.Id))
            {
                throw new InvalidOperationException($"Scene id '{scene.Id}' already exists in the World.");
            }

            scene.AttachWorld(this);
            m_Scenes.Add(scene.Id, scene);
            return scene;
        }

        public Scene GetScene(long id)
        {
            m_Scenes.TryGetValue(id, out var scene);
            return scene;
        }

        public T GetScene<T>(long id) where T : Scene
        {
            return GetScene(id) as T;
        }

        public bool TryGetScene(long id, out Scene scene)
        {
            return m_Scenes.TryGetValue(id, out scene);
        }

        /// <summary>
        /// 移除 Scene。默认销毁 Scene；dispose 为 false 时，调用方接管 Scene 的生命周期。
        /// </summary>
        public bool RemoveScene(Scene scene)
        {
            ThrowIfDisposed();

            if (scene == null || !ReferenceEquals(scene.World, this))
            {
                return false;
            }

            DetachScene(scene);
            scene.Dispose();

            return true;
        }

        /// <summary>
        /// 按 Id 移除 Scene。默认销毁 Scene；dispose 为 false 时，调用方接管 Scene 的生命周期。
        /// </summary>
        public bool RemoveScene(long id)
        {
            ThrowIfDisposed();

            if (!m_Scenes.TryGetValue(id, out var scene))
            {
                return false;
            }

            return RemoveScene(scene);
        }

        /// <summary>
        /// 销毁 World 拥有的全部 Scene。重复调用安全。
        /// </summary>
        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;

            var scenes = new List<Scene>(m_Scenes.Values);
            foreach (var scene in scenes)
            {
                scene.Dispose();
            }

            m_Scenes.Clear();
        }

        internal void DetachScene(Scene scene)
        {
            if (scene == null)
            {
                return;
            }

            if (m_Scenes.TryGetValue(scene.Id, out var current) && ReferenceEquals(current, scene))
            {
                m_Scenes.Remove(scene.Id);
                scene.DetachWorld(this);
            }
        }

        private void ThrowIfDisposed()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(World));
            }
        }
    }
}
