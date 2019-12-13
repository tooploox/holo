using UnityEditor;
using UnityEngine;
using System.IO;

public static class AssetDirs
{
    /* Place temporary assets here.
	 * Never ends with path delimiter (/ or \).
     *
     * Internal note: when changing this constant, adjust also directory creation
     * code in LoadModel. */
    public const string TempAssetsDir = "Assets/Temporary";

    /* Create the directory within assets if necessary.
	 * The directory to create is the last component of given path.
	 */
	public static void CreateDirectory(string path)
	{
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(Path.GetDirectoryName(path), Path.GetFileName(path));
        }        
	}
}
