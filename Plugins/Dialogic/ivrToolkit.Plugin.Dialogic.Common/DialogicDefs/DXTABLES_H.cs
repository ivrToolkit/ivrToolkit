using System;
using System.Runtime.InteropServices;

namespace ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;

public class DXTABLES_H
{
    public const int IO_CONT = 0x01; /* Next TPT is contiguous in memory */
    public const int IO_LINK = 0x02; /* Next TPT found thru tp_nextp ptr */
    public const int IO_EOT = 0x04; /* End of the Termination Parameters */
    public const int IO_DEV = 0x00; /* play/record from a file */
    public const int IO_MEM = 0x08; /* play/record from memory */
    public const int IO_UIO = 0x10; /* play/record using user I/O functions */
    public const int IO_STREAM = 0x20; /* End of the Termination for R4 Streaming API */
    public const int IO_CACHED = 0x40; /* play from cache */
    public const int IO_USEOFFSET = 0x80; /* use io_offset and io_length for non-VOX */
    public const int IO_UNIT_TIME = 0x200; /* io_offset and io_length in milliseconds */

    /*
     * Termination mask defines for use with ATDX_TERMMSK( )
     */
    public const int TM_NORMTERM = 0x00000;     /* Normal Termination */
    public const int TM_MAXDTMF = 0x00001;     /* Max Number of Digits Recd */
    public const int TM_MAXSIL = 0x00002;     /* Max Silence */
    public const int TM_MAXNOSIL = 0x00004;     /* Max Non-Silence */
    public const int TM_LCOFF = 0x00008;     /* Loop Current Off */
    public const int TM_IDDTIME = 0x00010;     /* Inter Digit Delay */
    public const int TM_MAXTIME = 0x00020;     /* Max Function Time Exceeded */
    public const int TM_DIGIT = 0x00040;     /* Digit Mask or Digit Type Term. */
    public const int TM_PATTERN = 0x00080;     /* Pattern Match Silence Off */
    public const int TM_USRSTOP = 0x00100;     /* Function Stopped by User */
    public const int TM_EOD = 0x00200;     /* End of Data Reached on Playback */
    public const int TM_TONE = 0x02000;     /* Tone On/Off Termination */
    public const int TM_BARGEIN = 0x08000;     /* Play terminated due to Barge-in */
    public const int TM_ERROR = 0x80000;     /* I/O Device Error */
    public const int TM_MAXDATA = 0x100000;	  /* Max Data reached for FSK */


    /* Error Codes */

    public const int EPERM = 1;
    public const int ENOENT = 2;
    public const int ESRCH = 3;
    public const int EINTR = 4;
    public const int EIO = 5;
    public const int ENXIO = 6;
    public const int E2BIG = 7;
    public const int ENOEXEC = 8;
    public const int EBADF = 9;
    public const int ECHILD = 10;
    public const int EAGAIN = 11;
    public const int ENOMEM = 12;
    public const int EACCES = 13;
    public const int EFAULT = 14;
    public const int EBUSY = 16;
    public const int EEXIST = 17;
    public const int EXDEV = 18;
    public const int ENODEV = 19;
    public const int ENOTDIR = 20;
    public const int EISDIR = 21;
    public const int ENFILE = 23;
    public const int EMFILE = 24;
    public const int ENOTTY = 25;
    public const int EFBIG = 27;
    public const int ENOSPC = 28;
    public const int ESPIPE = 29;
    public const int EROFS = 30;
    public const int EMLINK = 31;
    public const int EPIPE = 32;
    public const int EDOM = 33;
    public const int EDEADLK = 36;
    public const int ENAMETOOLONG = 38;
    public const int ENOLCK = 39;
    public const int ENOSYS = 40;
    public const int ENOTEMPTY = 41;

    public const int EINVAL = 22;
    public const int ERANGE = 34;
    public const int EILSEQ = 42;
    public const int STRUNCATE = 80;

    /*
     * Defines for the TPT
     */
    public const int DX_MAXDTMF = 1;     /* Maximum Number of Digits Received */
    public const int DX_MAXSIL = 2;     /* Maximum Silence */
    public const int DX_MAXNOSIL = 3;     /* Maximum Non-Silence */
    public const int DX_LCOFF = 4;    /* Loop Current Off */
    public const int DX_IDDTIME = 5;     /* Inter-Digit Delay */
    public const int DX_MAXTIME = 6;     /* Function Time */
    public const int DX_DIGMASK = 7;     /* Digit Mask Termination */
    public const int DX_PMOFF = 8;     /* Pattern Match Silence On */
    public const int DX_PMON = 9;     /* Pattern Match Silence Off */
    public const int DX_DIGTYPE = 11;    /* Digit Type Termination */
    public const int DX_TONE = 12;    /* Tone On/Off Termination */
    public const int DX_MAXDATA = 13; /* Maximum bytes for ADSI data*/

    /*
     * Defines for TPT Termination Flags
     */
    public const int TF_EDGE = 0x00;
    public const int TF_LEVEL = 0x01;
    public const int TF_CLREND = 0x02;
    public const int TF_CLRBEG = 0x04;
    public const int TF_USE = 0x08;
    public const int TF_SETINIT = 0x10;
    public const int TF_10MS = 0x20;
    public const int TF_FIRST = TF_CLREND;
    public const int TF_IMMEDIATE = 0x40;

    public const int TF_MAXDTMF = TF_LEVEL | TF_USE;
    public const int TF_MAXSIL = TF_EDGE | TF_USE;
    public const int TF_MAXNOSIL = TF_EDGE | TF_USE;
    public const int TF_LCOFF = TF_LEVEL | TF_USE | TF_CLREND;
    public const int TF_IDDTIME = TF_EDGE;
    public const int TF_MAXTIME = TF_EDGE;
    public const int TF_DIGMASK = TF_LEVEL;
    public const int TF_PMON = TF_EDGE;
    public const int TF_DIGTYPE = TF_LEVEL;
    public const int TF_TONE = TF_LEVEL | TF_USE | TF_CLREND;
    public const int TF_MAXDATA = 0;

}

[StructLayout(LayoutKind.Sequential)]
public struct DX_IOTT
{
    public ushort io_type; /* Transfer type */
    public ushort rfu; /* reserved */
    public int io_fhandle; /* File descriptor */

    /// char*
    [MarshalAs(UnmanagedType.LPStr)]
    public string io_bufp; /* Pointer to base memory */

    public uint io_offset; /* File/Buffer offset */
    public int io_length; /* Length of data */

    /// DX_IOTT*
    public IntPtr io_nextp; /* Pointer to next DX_IOTT if IO_LINK */

    /// DX_IOTT*
    public IntPtr io_prevp; /* (optional) Pointer to previous DX_IOTT */
}