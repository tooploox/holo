/*
using System.IO;
using UnityEngine;


//Static class containing methods for extracting and adapting vertices from a STL file. 
public static class STLImportUtils
{
    public static Vector3 GetVector3(this BinaryReader binaryReader)
    {
        Vector3 vector3 = new Vector3();
        for (int i = 0; i < 3; i++)
        {
            vector3[i] = binaryReader.ReadSingle();
        }
        //maintaining Unity counter-clockwise orientation
        vector3.z = -vector3.z;

        return vector3;
    }
}
*/