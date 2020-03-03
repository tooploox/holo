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

    private byte[] RawData;
    private bool dataInitialized = false;

    private int xysize, size;
    private bool recalculationInProgress = false;

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

        if(this.RawData == null || this.RawData.Length == 0)
        {
            this.RawData = new byte[bytes.Length];
        }
        bytes.CopyTo(this.RawData, 0);
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

    public void SetNumberOfChannels(int num)
    {
        Channels = num;
    }

    private Texture3D CalculateTexture()
    {
        Color32[] colorArray = new Color32[size];
        Texture3D resultTexture = new Texture3D(Width, Height, Depth, TextureFormat.RGBA32, true);

        Debug.Log("Calculating 3D Texture. Raw data size: " + (RawData == null ? 0 : RawData.Length));

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

        resultTexture.SetPixels32(colorArray);
        //texture.SetPixelData(bytes, 0);
        resultTexture.wrapModeU = TextureWrapMode.Clamp;
        resultTexture.wrapModeV = TextureWrapMode.Clamp;
        resultTexture.wrapModeW = TextureWrapMode.Clamp;

        resultTexture.Apply();

        return resultTexture;
    }

    public void RecalculateTextures()
    {
        if (RawData == null || RawData.Length == 0)
        {
            Debug.Log("Empty data for texture calculation RawData: " + (RawData == null ? "null" : RawData.Length.ToString()));
            return;
        }

        if (this.gameObject.GetComponent<MeshRenderer>().sharedMaterial && !recalculationInProgress)
        {
            recalculationInProgress = true;
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            Debug.Log("Setting just calculated texture to material [name: " + renderer.sharedMaterial.name + "]");
            renderer.sharedMaterial.mainTexture = CalculateTexture();
            recalculationInProgress = false;
        }
    }

    private void InitializeWithData()
    {
        if (dataInitialized) return;

        if (RawData == null || RawData.Length == 0)
        {
            Debug.Log("Empty data for initialization!");
            return;
        }

        RecalculateTextures();

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
