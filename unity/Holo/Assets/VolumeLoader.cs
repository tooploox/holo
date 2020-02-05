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

    [Range(1, 4)]
    public int Channels;

    public Color channel1;
    public Color channel2;
    public Color channel3;
    public Color channel4;

    private Texture3D texture;

    void Start() {

        int xysize = Width * Height;
        int size = xysize * Depth;

        Color32[] colorArray = new Color32[size];
        texture = new Texture3D(Width, Height, Depth, TextureFormat.RGBA32, true);

        LocalConfig localConfig = Resources.Load<LocalConfig>("LocalConfig");
        string dir = localConfig.GetBundlesDirectory();
        var localPath = System.IO.Path.Combine(dir, Path);
        var s = new FileStream(localPath, FileMode.Open);
        BinaryReader br = new BinaryReader(s);
        var bytes = br.ReadBytes(size * 2);
        br.Dispose();

        for (int z = 0; z < Depth; ++z)
            for (int it = 0; it < xysize; ++it)
            {
                //todo: this if chain sucks in inner loop
                byte r = 0; byte g = 0; byte b = 0; byte a = 0;
                r = bytes[z * xysize * Channels + it];
                if (Channels > 1)
                    g = bytes[z * xysize * Channels + xysize + it];
                if (Channels > 2)
                    b = bytes[z * xysize * Channels + xysize * 2 + it];
                if (Channels > 3)
                    a = bytes[z * xysize * Channels + xysize * 3 + it];

                colorArray[z * xysize + it] = new Color32(r, g, b, a);
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

        mat.SetColor("_Channel1", channel1);
        mat.SetColor("_Channel2", channel2);
        mat.SetColor("_Channel3", channel3);
        mat.SetColor("_Channel4", channel4);
    }
}
