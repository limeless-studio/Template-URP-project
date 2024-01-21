using System.Collections.Generic;
using UnityEngine;

namespace Snowy.Editor 
{
    //[CreateAssetMenu()]
    public class FolderIconSO : ScriptableObject {

        public Texture2D icon;
        public List<string> folderNames;

        public void OnValidate() {
            IconDictionaryCreator.BuildDictionary();
        }
    }
}

