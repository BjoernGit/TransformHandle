using UnityEngine;
using UnityEditor;

public static class ExportTransformHandlePackage
{
    [MenuItem("Tools/Export TransformHandle .unitypackage")]
    public static void Export()
    {
        string[] assetPaths = new []
        {
            "Packages/com.bjoerngit.transformhandle"
        };
        string exportPath = $"TransformHandle-{Application.unityVersion}-v1.1.0.unitypackage";


        AssetDatabase.ExportPackage(
            assetPaths,
            exportPath,
            ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse
        );

        UnityEngine.Debug.Log($"Exported: {exportPath}");
    }
}
