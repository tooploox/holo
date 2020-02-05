using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VolumeLoader : MonoBehaviour
{
    public string Path;

    public Renderer TargetRenderer;

    public int Width;
    public int Height;
    public int Depth;

    private Texture3D texture;

    void Start() {

        int xysize = Width * Height;
        int size = xysize * Depth;

        Color32[] colorArray = new Color32[size];
        texture = new Texture3D(Width, Height, Depth, TextureFormat.RGBA32, true);

        LocalConfig localConfig = Resources.Load<LocalConfig>("LocalConfig");
        string dir = localConfig.GetBundlesDirectory();
        var localPath = System.IO.Path.Combine(dir, Path);

        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "test.txt"), dir.ToString());
        var s = new FileStream(localPath, FileMode.Open);
        BinaryReader br = new BinaryReader(s);
        var bytes = br.ReadBytes(size * 2);
        br.Dispose();

        for (int z = 0; z < Depth; ++z)
            for (int it = 0; it < xysize; ++it)
            {
                var r = bytes[z * xysize * 2 + it];// * inv;
                byte g = bytes[z * xysize * 2 + xysize + it];// * inv;
                colorArray[z * xysize + it] = new Color32(r, g, 0, 255);
            }

        texture.SetPixels32(colorArray);
        //texture.SetPixelData(bytes, 0);
        texture.wrapModeU = TextureWrapMode.Clamp;
        texture.wrapModeV = TextureWrapMode.Clamp;
        texture.wrapModeW = TextureWrapMode.Clamp;

        texture.Apply();

        TargetRenderer.material.SetTexture("_MainTex", texture);
    }

    Matrix4x4 GetCameraMatrix(Camera.StereoscopicEye eye) {
        var cam = Camera.main;
        if (cam.stereoEnabled)
            return cam.GetStereoViewMatrix(eye).inverse;
        else
            return cam.cameraToWorldMatrix;
    }

    void LateUpdate() {
        var cam = Camera.main;
        var mat = GetComponent<Renderer>().material;
        var left = GetCameraMatrix(Camera.StereoscopicEye.Left).MultiplyPoint3x4(Vector3.zero);
        var right = GetCameraMatrix(Camera.StereoscopicEye.Right).MultiplyPoint3x4(Vector3.zero);
        mat.SetVector("_LeftEye", transform.InverseTransformPoint(left));
        mat.SetVector("_RightEye", transform.InverseTransformPoint(right));
    }
}
