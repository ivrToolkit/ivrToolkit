namespace ivrToolkit.Core.Interfaces
{
    /// <summary>
    /// The purpose of this interface is to define functionality that will be called from another thread like the PluginManager
    /// </summary>
    public interface IIvrLineManagement
    {
        /// <summary>
        /// Triggers a TriggerDispose event in the line.
        /// </summary>
        void TriggerDispose();
    }
}