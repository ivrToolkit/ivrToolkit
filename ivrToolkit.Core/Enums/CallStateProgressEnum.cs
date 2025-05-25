using System;

namespace ivrToolkit.Core.Enums;

/// <summary>
/// A collection of callout states to help analyze a NoRingBack situation.
/// </summary>
[Flags]
public enum CallStateProgressEnum
{
    None = 0,
    Proceeding = 2,
    Alerting = 4,
    Connected = 8,
    Disconnected = 16,
}