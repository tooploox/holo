using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VolumetricLoader : MonoBehaviour
{
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
    private Renderer TargetRenderer;

    private byte[] RawData;
    private bool dataInitialized = false;

    private int xysize, size;

    private void Start()
    {
        SetSizes();
    }

    void SetSizes()
    {
        if (Width > 0 && Height > 0 && Depth > 0 && size == 0)
        {
            xysize = Width * Height;
            size = xysize * Depth;
        }
    }

    public void SetRawBytes(byte[] bytes)
    {
        SetSizes();
        if (bytes.Length == 0)
        {
            Debug.Log("Empty raw bytes array!");
            return;
        }
        if (bytes.Length != size * 2)
        {
            Debug.LogError("Invalid size of raw bytes: " + bytes.Length + ", expecting: " + size * 2);
        }

        RawData = bytes;
        InitializeWithData();
    }

    public void LoadRawDataFromFile(string filePath)
    {
        SetSizes();
        if (File.Exists(filePath) && RawData == null)
        {
            LocalConfig localConfig = Resources.Load<LocalConfig>("LocalConfig");
            string dir = localConfig.GetBundlesDirectory();
            Debug.Log("Going to load micro data [size: " + size * 2 + "] from: " + filePath);
            var s = new FileStream(filePath, FileMode.Open);
            BinaryReader br = new BinaryReader(s);
            var bytes = br.ReadBytes(size * 2);
            Debug.Log("Bytes read: " + bytes.Length);
            SetRawBytes(bytes);
            br.Dispose();
        }
    }

    private void InitializeWithData() {
        if (dataInitialized) return;

        if (RawData == null || RawData.Length == 0)
        {
            Debug.Log("Empty data for initialization!");
            return;
        }

        TargetRenderer = this.gameObject.GetComponent<MeshRenderer>();
            
        Color32[] colorArray = new Color32[size];
        texture = new Texture3D(Width, Height, Depth, TextureFormat.RGBA32, true);     

        Debug.Log("Raw data size: " + RawData.Length);       

        for (int z = 0; z < Depth; ++z)
            for (int it = 0; it < xysize; ++it)
            {
                //todo: this if chain sucks in inner loop
                byte r = 0; byte g = 0; byte b = 0; byte a = 0;
                r = RawData[z * xysize * Channels + it];
                if (Channels > 1)
                    g = RawData[z * xysize * Channels + xysize + it];
                if (Channels > 2)
                    b = RawData[z * xysize * Channels + xysize * 2 + it];
                if (Channels > 3)
                    a = RawData[z * xysize * Channels + xysize * 3 + it];

                colorArray[z * xysize + it] = new Color32(r, g, b, a);
            }

        texture.SetPixels32(colorArray);
        //texture.SetPixelData(bytes, 0);
        texture.wrapModeU = TextureWrapMode.Clamp;
        texture.wrapModeV = TextureWrapMode.Clamp;
        texture.wrapModeW = TextureWrapMode.Clamp;

        texture.Apply();

        TargetRenderer.sharedMaterial.SetTexture("_MainTex", texture);
        dataInitialized = true;
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
        var mat = GetComponent<Renderer>().sharedMaterial;
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
