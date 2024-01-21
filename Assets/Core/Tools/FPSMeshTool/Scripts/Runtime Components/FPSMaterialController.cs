using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CBG {

    public enum FPSBodyPart {
        Body,
        Head,
        Arms,
        Legs
    }

    [System.Serializable]
    public struct FPSMaterialEntry {
        public Material material;
        public FPSBodyPart bodyPart;
        public FPSMaterialEntry(Material mat) {
            material = mat;
            bodyPart = GuessBodyPart(mat.name);
        }

        static FPSBodyPart GuessBodyPart(string name) {
            if (name.Contains(" arm")) {
                return FPSBodyPart.Arms;
            }
            if (name.Contains(" head")) {
                return FPSBodyPart.Head;
            }
            if (name.Contains(" leg")) {
                return FPSBodyPart.Legs;
            }
            return FPSBodyPart.Body;
        }
    }

    [System.Serializable]
    public struct RendererEntry {
        public SkinnedMeshRenderer renderer;
        public List<FPSMaterialEntry> materialEntries;

        public RendererEntry(SkinnedMeshRenderer renderer) {
            this.renderer = renderer;
            materialEntries = new List<FPSMaterialEntry>();
            var mats = renderer.sharedMaterials;
            foreach (var mat in mats) {
                materialEntries.Add(new FPSMaterialEntry(mat));
            }
        }
    }

    public class RendererMaterialData {
        public SkinnedMeshRenderer renderer;
        public Material[] originalMaterials;
        public Dictionary<FPSBodyPart, int[]> matLookup;

        public RendererMaterialData(SkinnedMeshRenderer rend, Material[] mats, List<int> body, List<int> head, List<int> arms, List<int> legs) {
            renderer = rend;
            originalMaterials = mats;
            matLookup = new Dictionary<FPSBodyPart, int[]>(4);
            matLookup[FPSBodyPart.Body] = body.ToArray();
            matLookup[FPSBodyPart.Head] = head.ToArray();
            matLookup[FPSBodyPart.Arms] = arms.ToArray();
            matLookup[FPSBodyPart.Legs] = legs.ToArray();
        }
    }

    public class FPSMaterialController : MonoBehaviour {
        [Tooltip("The renderers found on this GameObject, and the body part each material pertains to.")]
        public List<GameObject> ignoreRenderersOn;
        public List<RendererEntry> renderers;
        public List<FPSBodyPart> hideInFirstPerson = new List<FPSBodyPart> { FPSBodyPart.Head, FPSBodyPart.Arms };
        public Material invisibleMaterial;
        List<RendererMaterialData> materialData;

#if UNITY_EDITOR
        void Reset() {
            BuildRendererList();
            FindInvisibleMaterial();
        }

        public void BuildRendererList() {
            // Clear our renderer list
            renderers = new List<RendererEntry>();
            // Find renderers
            var foundRenderers = new List<SkinnedMeshRenderer>(GetComponentsInChildren<SkinnedMeshRenderer>());
            RemoveUndesiredRenderers(foundRenderers);
            // Add all found renderers to the list
            foreach (var rend in foundRenderers) {
                renderers.Add(new RendererEntry(rend));
            }
        }

        protected virtual void FindInvisibleMaterial() {
            string[] guids = AssetDatabase.FindAssets("InvisibleMaterial t:material");
            if (guids.Length > 0) {
                invisibleMaterial = (Material)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(Material));
            }
        }

        void RemoveUndesiredRenderers(List<SkinnedMeshRenderer> foundRenderers) {
            if (ignoreRenderersOn != null) {
                List<SkinnedMeshRenderer> ignoreRenderers = new List<SkinnedMeshRenderer>();
                foreach (var ignoreObject in ignoreRenderersOn) {
                    ignoreRenderers.AddRange(ignoreObject.GetComponentsInChildren<SkinnedMeshRenderer>());
                }
                for (int i = foundRenderers.Count - 1; i >= 0; i--) {
                    if (ignoreRenderers.Contains(foundRenderers[i])) {
                        foundRenderers.RemoveAt(i);
                    }
                }
            }
        }

        public bool RendererDataIsStale() {
            var foundRenderers = new List<SkinnedMeshRenderer>(GetComponentsInChildren<SkinnedMeshRenderer>());
            RemoveUndesiredRenderers(foundRenderers);
            if (foundRenderers.Count != renderers.Count) {
                return true;
            }
            for (int i = 0; i < foundRenderers.Count; i++) {
                if (renderers[i].renderer != foundRenderers[i]) {
                    return true;
                }
                var mats = foundRenderers[i].sharedMaterials;
                if (mats.Length != renderers[i].materialEntries.Count) {
                    return true;
                }
                for (int j = 0; j < mats.Length; j++) {
                    if (renderers[i].materialEntries[j].material != mats[j]) {
                        return true;
                    }
                }
            }
            return false;
        }

#endif

        void Awake() {
            BuildMaterialData();
        }

        void BuildMaterialData() {
            materialData = new List<RendererMaterialData>();
            foreach (var rendEntry in renderers) {
                var rend = rendEntry.renderer;
                var mats = rend.sharedMaterials;
                var body = new List<int>();
                var head = new List<int>();
                var arms = new List<int>();
                var legs = new List<int>();
                for (int i = 0; i < mats.Length; i++) {
                    switch (rendEntry.materialEntries[i].bodyPart) {
                    case FPSBodyPart.Body:
                        body.Add(i);
                        break;
                    case FPSBodyPart.Head:
                        head.Add(i);
                        break;
                    case FPSBodyPart.Arms:
                        arms.Add(i);
                        break;
                    case FPSBodyPart.Legs:
                        legs.Add(i);
                        break;
                    }
                }
                materialData.Add(new RendererMaterialData(rend, mats, body, head, arms, legs));
            }
        }

        public void OnPerspectiveChanged(bool firstPerson) {
            foreach (var bodyPart in hideInFirstPerson) {
                SetBodyPartVisible(bodyPart, !firstPerson);
            }
        }

        void SetBodyPartVisible(FPSBodyPart part, bool visible) {
            foreach (var rendEntry in materialData) {
                var mats = rendEntry.renderer.sharedMaterials;
                var indices = rendEntry.matLookup[part];
                foreach (var index in indices) {
                    mats[index] = visible ? rendEntry.originalMaterials[index] : invisibleMaterial;
                }
                rendEntry.renderer.sharedMaterials = mats;
            }
        }

        public void HideBodyPart(FPSBodyPart part) {
            SetBodyPartVisible(part, false);
        }

        public void ShowBodyPart(FPSBodyPart part) {
            SetBodyPartVisible(part, true);
        }

    }
}