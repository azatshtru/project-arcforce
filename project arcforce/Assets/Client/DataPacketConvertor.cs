using System;
using System.Text;
using System.Linq;
using System.IO;
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

    public static string GetString(byte[] value, int length)
    {
        return Encoding.ASCII.GetString(value, 0, length);
    }

    public static int GetInt(byte[] value)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(value);
        }
        return BitConverter.ToInt32(value, 0);
    }

    public static float GetFloat(byte[] value)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(value);
        }
        return BitConverter.ToSingle(value, 0);
    }

    public static Vector3 GetVector(byte[] value)
    {
        Stream stream = new MemoryStream(value);

        float[] vectorfloats = new float[3];

        for (int i = 0; i < 3; i++)
        {
            byte[] dataBytes = new byte[4];
            int bytesRead = stream.Read(dataBytes, 0, dataBytes.Length);
            vectorfloats[i] = GetFloat(dataBytes);
        }

        return new Vector3(vectorfloats[0], vectorfloats[1], vectorfloats[2]);
    }
}
