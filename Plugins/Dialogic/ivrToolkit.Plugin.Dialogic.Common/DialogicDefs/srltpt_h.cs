using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable CommentTypo

namespace ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;

public class srltpt_h
{
    /*
     * Termination Parameter Types
     */
    public const int IO_CONT = 0x01;        /* Next TPT is contiguous in memory  */
    public const int IO_LINK = 0x02;        /* Next TPT found thru tp_nextp ptr */
    public const int IO_EOT = 0x04; /* End of the Termination Parameters */



}

[StructLayout(LayoutKind.Sequential)]
public struct DV_TPT
{
    public ushort tp_type; /* Flags Describing this Entry */
    public ushort tp_termno; /* Termination Parameter Number */
    public ushort tp_length; /* Length of Terminator */
    public ushort tp_flags; /* Termination Parameter Attributes Flag */
    public ushort tp_data; /* Optional Additional Data */
    public ushort rfu; /* Reserved */


    /// DV_TPT*
    public IntPtr tp_nextp; /* Ptr to next DV_TPT if IO_LINK set */
    //public DV_TPT* tp_nextp;
}