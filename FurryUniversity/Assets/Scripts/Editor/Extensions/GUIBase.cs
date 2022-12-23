using System;
using UnityEngine;

namespace SFramework.EditorExtensions
{
    public abstract class GUIBase : IDisposable
    {
        protected bool Disposed { get; private set; }
        protected Rect Position { get; set; }

        public virtual void OnGUI(Rect position)
        {
            this.Position = position;
        }

        public virtual void Dispose()
        {
            if (this.Disposed)
                return;
            this.OnDispose();
            this.Disposed = true;
        }

        protected abstract void OnDispose();

    }
}