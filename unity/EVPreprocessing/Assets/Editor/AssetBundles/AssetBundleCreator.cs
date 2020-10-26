using System.IO;
using UnityEditor;
using UnityEngine;


public class AssetBundleCreator
{
    //Creates AssetBundle 
    private string outputPath;

    public AssetBundleCreator(string outputPath)
    {
        this.outputPath = outputPath;
    }

    //Creates AssetBundle
    public void Create(ModelImport.ModelImporter importedModel)
    {
        AssetBundleBuild[] buildMapArray = BuildMapABs(importedModel);
        CreateAssetBundle(buildMapArray);
    }

    // Create the array of bundle build details.
    private AssetBundleBuild[] BuildMapABs(ModelImport.ModelImporter importedModel)
    {

        AssetBundleBuild buildMap = new AssetBundleBuild();
        buildMap.assetBundleName = importedModel.Info.Caption + "_bundle";
        buildMap.assetNames = importedModel.AssetPaths.ToArray();

        return new AssetBundleBuild[1] {buildMap};
    }
    //Creates appropriate AssetBundle for the model.
    private void CreateAssetBundle(AssetBundleBuild[] buildMapArray)
    {
	    Directory.CreateDirectory(outputPath);
        BuildPipeline.BuildAssetBundles(outputPath, buildMapArray, BuildAssetBundleOptions.None, BuildTarget.WSAPlayer);

        // this is necessary to clear references to this asset
        
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDirs.TempAssetsDir + "/icon.asset");
        UnityEngine.Object.DestroyImmediate(texture, true);

        // this is still necessary even after above DestroyImmediate.
        AssetDatabase.DeleteAsset(AssetDirs.TempAssetsDir + "/icon.asset");

        //Cleaning up an unnecessesary bundle
        string folderBundle = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(outputPath));
        RecursiveDeleter.DeleteRecursivelyWithSleep(folderBundle);
        RecursiveDeleter.DeleteRecursivelyWithSleep(folderBundle + ".manifest");
    }
}
