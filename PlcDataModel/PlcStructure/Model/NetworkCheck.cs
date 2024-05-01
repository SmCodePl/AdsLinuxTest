using System.Runtime.InteropServices;

namespace PlcDataModel.PlcStructure.Model;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct NetworkCheck
{
    public short CheckNetwork;
    public short EchoCheckNetwork;
}