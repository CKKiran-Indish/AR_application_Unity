using UnityEditor;

public class BuildBundles
{
    [MenuItem("Build/Build AssetBundles")]
    static void BuildAllBundles()
    {
        BuildPipeline.BuildAssetBundles(
            "Assets/AssetBundles",
            BuildAssetBundleOptions.None,
            BuildTarget.Android   // Change if needed
        );
    }
}