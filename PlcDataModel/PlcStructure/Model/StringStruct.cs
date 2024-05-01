using System.Runtime.InteropServices;

namespace PlcDataModel.PlcStructure.Model;
[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct StringStruct
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 89)]
    public string PlcCodeString;
}