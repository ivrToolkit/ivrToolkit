// ReSharper disable InconsistentNaming
namespace ivrToolkit.Plugin.Dialogic.Common.DialogicDefs
{
    public class fcntl_h
    {
        public const int _O_RDONLY = 0x0000; // open for reading only
        public const int _O_WRONLY = 0x0001; // open for writing only
        public const int _O_RDWR = 0x0002; // open for reading and writing
        public const int _O_APPEND = 0x0008; // writes done at eof

        public const int _O_CREAT = 0x0100; // create and open file
        public const int _O_TRUNC = 0x0200; // open and truncate
        public const int _O_EXCL = 0x0400; // open only if file doesn't already exist

        // O_TEXT files have <cr><lf> sequences translated to <lf> on read()'s and <lf>
        // sequences translated to <cr><lf> on write()'s

        public const int _O_TEXT = 0x4000; // file mode is text (translated)
        public const int _O_BINARY = 0x8000; // file mode is binary (untranslated)
        public const int _O_WTEXT = 0x10000; // file mode is UTF16 (translated)
        public const int _O_U16TEXT = 0x20000; // file mode is UTF16 no BOM (translated)
        public const int _O_U8TEXT = 0x40000; // file mode is UTF8  no BOM (translated)

        // macro to translate the C 2.0 name used to force binary mode for files
        public const int _O_RAW = _O_BINARY;

        public const int _O_NOINHERIT = 0x0080; // child process doesn't inherit file
        public const int _O_TEMPORARY = 0x0040; // temporary file bit (file is deleted when last handle is closed)
        public const int _O_SHORT_LIVED = 0x1000; // temporary storage file, try not to flush
        public const int _O_OBTAIN_DIR = 0x2000; // get information about a directory
        public const int _O_SEQUENTIAL = 0x0020; // file access is primarily sequential
        public const int _O_RANDOM = 0x0010; // file access is primarily random

    }
}
