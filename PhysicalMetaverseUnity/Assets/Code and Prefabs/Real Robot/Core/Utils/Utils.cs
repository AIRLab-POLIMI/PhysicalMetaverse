using System.Text;
using UnityEngine;



public class UtilsPrint
{
        public static void PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            Debug.Log(sb.ToString());
        }
    }