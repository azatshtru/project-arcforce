using System;
using System.Text;
using System.Linq;
using UnityEngine;

public static class DataPacketConvertor
{
    public static byte[] GetBytes(float value)
    {
        string type = "DEC";
        byte[] typeBytes = Encoding.ASCII.GetBytes(type);

        byte[] b_float = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(b_float);
        }

        byte[] decimalBytes = typeBytes.Concat(b_float).ToArray();

        return decimalBytes;
    }

    public static byte[] GetBytes(Vector3 vector)
    {
        string type = "VEC";
        byte[] typeBytes = Encoding.ASCII.GetBytes(type);

        byte[] b_float1 = BitConverter.GetBytes(vector.x);
        byte[] b_float2 = BitConverter.GetBytes(vector.y);
        byte[] b_float3 = BitConverter.GetBytes(vector.z);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(b_float1);
            Array.Reverse(b_float2);
            Array.Reverse(b_float3);
        }

        byte[] vectorBytes = typeBytes.Concat(b_float1).Concat(b_float2).Concat(b_float3).ToArray();

        return vectorBytes;
    }
}
