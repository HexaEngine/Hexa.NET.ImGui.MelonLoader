namespace Hexa.NET.ImGui.MelonLoader.Plugins
{
    public abstract class Plugin
    {
        public abstract string Name { get; }

        public virtual void OnInitialized()
        {
        }

        public virtual void OnUnload()
        {
        }

        /// <summary>
        /// Runs when a new Scene is loaded.
        /// </summary>
        public virtual void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
        }

        /// <summary>
        /// Runs once a Scene is initialized.
        /// </summary>
        public virtual void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
        }

        /// <summary>
        /// Runs once a Scene unloads.
        /// </summary>
        public virtual void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
        }

        /// <summary>
        /// Runs once per frame.
        /// </summary>
        public virtual void OnUpdate()
        {
        }

        /// <summary>
        /// Can run multiple times per frame. Mostly used for Physics.
        /// </summary>
        public virtual void OnFixedUpdate()
        {
        }

        /// <summary>
        /// Runs once per frame, after <see cref="OnUpdate"/>.
        /// </summary>
        public virtual void OnLateUpdate()
        {
        }

        /// <summary>
        /// Can run multiple times per frame. Mostly used for Unity's IMGUI.
        /// </summary>
        public virtual void OnGUI()
        {
        }

        /// <summary>
        /// Called when the controller switches input states.
        /// </summary>
        /// <param name="active"></param>
        public virtual void OnInputSwitch(bool active)
        {
        }
    }
}