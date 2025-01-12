// ReSharper disable InconsistentNaming
namespace ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;

public class stat_h
{
    //-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //
    // Flags
    //
    //-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    public const int _S_IFMT = 0xF000; // File type mask
    public const int _S_IFDIR = 0x4000; // Directory
    public const int _S_IFCHR = 0x2000; // Character special
    public const int _S_IFIFO = 0x1000; // Pipe
    public const int _S_IFREG = 0x8000; // Regular
    public const int _S_IREAD = 0x0100; // Read permission, owner
    public const int _S_IWRITE = 0x0080; // Write permission, owner
}