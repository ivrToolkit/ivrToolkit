namespace ivrToolkit.Core.Enums
{
    /// <summary>
    /// Outgoing call analysis.
    /// </summary>
    public enum CallAnalysis
    {
        /// <summary>
        /// The line is currently busy.
        /// </summary>
        Busy,
        /// <summary>
        /// no answer.
        /// </summary>
        NoAnswer,
        /// <summary>
        /// No ringback (not sure what this is).
        /// </summary>
        NoRingback,
        /// <summary>
        /// Call is connected.
        /// </summary>
        Connected,
        /// <summary>
        /// Operator Intercept(not used. This would come across as an answering machine instead).
        /// </summary>
        OperatorIntercept,
        /// <summary>
        /// line has been stopped.
        /// </summary>
        Stopped,
        /// <summary>
        /// No dial tone on the line.
        /// </summary>
        NoDialTone,
        /// <summary>
        /// Fax Tone.
        /// </summary>
        FaxTone,
        /// <summary>
        /// An error happened on the voice card.
        /// </summary>
        Error,
        /// <summary>
        /// An answering machine has been detected. based on salutation length. See voice.properties
        /// </summary>
        AnsweringMachine,
        /// <summary>
        /// No free line available. This is characterized by a fast busy signal. It happens if all the switchboard lines are used up. For example you dial 9 to get an out line
        /// and there are none available.
        /// </summary>
        NoFreeLine
    }
}