namespace ivrToolkit.Core.Interfaces
{
    /// <summary>
    /// The purpose of this interface is to define functionality that will be called from another thread like the PluginManager
    /// </summary>
    public interface ILineManagement
    {
        /// <summary>
        /// Triggers a Dispose event in the line.
        /// </summary>
        void Dispose();
    }
}