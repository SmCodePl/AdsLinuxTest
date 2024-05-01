using PlcDataModel.PlcStructure.Model;
using System.Runtime.InteropServices;

namespace PlcDataModel.PlcStructure;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HeaderStruct
{
    public short ItemId;
    public byte ItemStatus;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
    public LogTimeStamp[] TimeStampLog;

    public HeaderStruct()
    {
        TimeStampLog = new LogTimeStamp[1];
    }
}