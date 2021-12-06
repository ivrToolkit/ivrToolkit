using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

// ReSharper disable StringLiteralTypo

namespace ivrToolkit.Plugin.Dialogic.Common.DialogicDefs
{
    public class srllib_h
    {
        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sr_getboardcnt([MarshalAs(UnmanagedType.LPStr)] string brdname, ref int brdcnt);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDV_SUBDEVS(int ddd);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sr_waitevtEx(int[] handlep, int count, int tmout, ref int evthp);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ATDV_LASTERR(int ddd);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sr_getevttype(uint evt_handle);


        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sr_getevtdatap(uint evt_handle);



        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ATDV_ERRMSGP(int ddd);

        [DllImport("libsrlmt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sr_putevt(int dev, uint evt, int len, IntPtr datap, int err);



        /* 
         * EVENT MANAGEMENT
         */
        public const int SR_INTERPOLLID = 0x01; /* Parameter id for inter-poll delay */
        public const int SR_MODEID = 0x02; /* Set SRL to run in SIGNAL/POLL mode */

        public const int SR_DATASZID = 0x03; /* Parameter id for getting/setting SRL's preallocated event data memory size*/

        public const int SR_QSIZEID = 0X04; /* Maximum size of SRL's internal event queue */
        public const int SR_MODELTYPE = 0x05; /* Set SRL model type (for NT only) */
        public const int SR_USERCONTEXT = 0x06; /* Allows user to set per device context */
        public const int SR_WIN32INFO = 0x07; /* Set Win32 Notification mechanism */
        public const int SR_STATISTICS = 0x08; /* Set statistics monitoring */
        public const int SR_PARMHIPRIMODE = 0x09; /* High Priority handler mode */

        public const int SR_POLLMODE = 0; /* Run SRL in polling mode */
        public const int SR_SIGMODE = 1; /* Run SRL in signalling/interrupt mode */
        public const int SRL_DEVICE = 0; /* The SRL device */
        public const int SR_TMOUTEVT = 0; /* Timeout event - occurs on the SRL DEVICE */

        public const int SR_STASYNC = 0; /* Single threaded async model */
        public const int SR_MTASYNC = 1; /* Multithreaded asynchronous model */
        public const int SR_MTSYNC = 2; /* Multithreaded synchronous model */

        public const int SR_NOTIFY_ON = 0; /* Turn on message notification */
        public const int SR_NOTIFY_OFF = 1; /* Turn off message notification */

        public const int SR_HIPRIDEFAULT = 0;
        public const int SR_HIPRISYNC = 1;


        /* SRL errors */
        public const int ESR_NOERR = 0; /* No SRL errors */
        public const int ESR_SCAN = 1; /* SRL scanning function returned an error */
        public const int ESR_PARMID = 2; /* Unknown parameter id */

        public const int ESR_TMOUT = 3; /* Returned by ATDV_LASTERR(SRL_DEVICE) when an
                                 SRL function times out */

        public const int ESR_SYS = 4; /* System error - consult errno */
        public const int ESR_DATASZ = 5; /* Invalid size for default event data memory */
        public const int ESR_QSIZE = 6; /* Illegal event queue size */
        public const int ESR_NOHDLR = 7; /* No such handler */
        public const int ESR_MODE = 8; /* Illegal mode for this operation */
        public const int ESR_NOTIMP = 9; /* Function not implemented */

        public const int ESR_NULL_DATAP = 10; /* Pointer argument is null */
        public const int ESR_BADDEV = 11; /* Invalid or Missing device */
        public const int ESR_NOMEM = 12; /* No or insufficient memory available */
        public const int ESR_BADPARM = 13; /* Invalid parameter or parameter value */
        public const int ESR_NOCOM = 14; /* SRL can not communicate with another sub system */
        public const int ESR_INSUFBUF = 15; /* No or insufficient buffers available */

        public const int ESR_THREAD_DEVICE_GROUP_EXISTS = 16; /* Thread Device Group already exists you for this thread cannot be created again */

        public const int ESR_THREAD_DEVICE_GROUP_NO_GROUP_DEFINED = 17; /* No Thread Device Group created for this thread therefore cannot call sr_WaitThreadDeviceGroup */


        public const int SR_TMOUT = -1; /* Returned by event scanning functions (e.g.sr_waitevt()) when they time out */

    }
}
