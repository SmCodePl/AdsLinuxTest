using System.Runtime.InteropServices;
using PlcDataModel.PlcStructure.Model;

namespace PlcDataModel.PlcStructure;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct AdsDataType
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public StringStruct[] TestString = new StringStruct[100];

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4000)]
    public float[] TestFloat = new float[4000];

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4000)]
    public short[] TestShort = new short[4000];

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2000)]
    public BoolStruct[] TestBool = new BoolStruct[2000];

    public AdsDataType()
    {
    }
}