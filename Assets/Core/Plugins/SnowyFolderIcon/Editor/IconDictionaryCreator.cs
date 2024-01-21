using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Snowy.Editor
{
    public class IconDictionaryCreator : AssetPostprocessor
    {
        private static string assetsPath = "";

        private static string AssetsPath
        {
            get
            {
                if (assetsPath.Length > 1) return assetsPath;
                string[] res = Directory.GetFiles(Application.dataPath, "IconDictionaryCreator.cs", SearchOption.AllDirectories);
                if (res.Length == 0)
                {
                    Debug.LogError("error message ....");
                } 

                res[0] = res[0].Split("Assets\\", 2)[1];
                assetsPath = res[0].Replace("\\Editor\\IconDictionaryCreator.cs", "").Replace("\\", "/");
                return assetsPath;
            }
        }
        
        internal static Dictionary<string, Texture> IconDictionary;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!ContainsIconAsset(importedAssets) &&
                !ContainsIconAsset(deletedAssets) &&
                !ContainsIconAsset(movedAssets) &&
                !ContainsIconAsset(movedFromAssetPaths))
            {
                return;
            }

            BuildDictionary();
        }

        private static bool ContainsIconAsset(string[] assets)
        {
            foreach (string str in assets)
            {

                if (ReplaceSeparatorChar(Path.GetDirectoryName(str)) == "Assets/" + AssetsPath + "/Icons")
                {
                    return true;
                }
            }
            return false;
        }

        private static string ReplaceSeparatorChar(string path)
        {
            return path.Replace("\\", "/");
        }

        internal static void BuildDictionary() 
        {
            var dictionary = new Dictionary<string, Texture>();
            
            var dir = new DirectoryInfo(Application.dataPath + "/" + AssetsPath + "/Icons");
            FileInfo[] info = dir.GetFiles("*.png");

            foreach(FileInfo f in info)
            {
                var texture = (Texture)AssetDatabase.LoadAssetAtPath($"Assets/{AssetsPath}/Icons/{f.Name}", typeof(Texture2D));
                dictionary.Add(Path.GetFileNameWithoutExtension(f.Name),texture);
            }
            dir = new DirectoryInfo(Application.dataPath + "/" + AssetsPath + "/Icons/Presets");
            FileInfo[] infoSO = dir.GetFiles("*.asset");

            foreach (FileInfo f in infoSO) 
            {
                var folderIconSO = (FolderIconSO)AssetDatabase.LoadAssetAtPath($"{AssetsPath}/Icons/Presets/{f.Name}", typeof(FolderIconSO));

                if (folderIconSO != null) 
                {
                    var texture = (Texture)folderIconSO.icon;

                    foreach (string folderName in folderIconSO.folderNames) 
                    {
                        if (folderName != null) 
                        {
                            dictionary.TryAdd(folderName, texture);
                        }
                    }
                }
            }
            
            IconDictionary = dictionary;
        }
    }
}
