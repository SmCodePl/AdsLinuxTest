
using System.Text;

namespace PlcDataModel
{
    public static class TestDataHelper
    {
        public static void SetBitInShort(ref byte data, int position, bool value)
        {

            if (value)
            {
                data = (byte)(data | (1 << position));
            }
            else
            {
                data = (byte)(data & ~(1 << position));
            }
        }
        public static string GetTimeStamp()
        {
            DateTime dt = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.Append(dt.Date.ToString("yyyy-MM-dd"));
            sb.Append("T");
            sb.Append(dt.ToString("HH:mm:ss.fff"));
            sb.Append("+02:00");

            return sb.ToString();
        }
    }
}
