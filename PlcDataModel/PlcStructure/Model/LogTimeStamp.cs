using System.Runtime.InteropServices;

namespace PlcDataModel.PlcStructure.Model;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct LogTimeStamp
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
    public string ReadTime;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
    public string WriteTiem;

}