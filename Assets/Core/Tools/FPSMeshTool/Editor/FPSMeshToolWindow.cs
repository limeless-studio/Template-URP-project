using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CBG.FPSMeshTool {

    public class FPSMeshToolWindow : EditorWindow {

        [SerializeField]
        GameObject sourceObject;
        GameObject newSourceObject;

        [SerializeField]
        GameObject previewInstance;


        // source renderers and whether they are enabled or not
        [SerializeField]
        SkinnedMeshRenderer[] sourceRenderers;
        [SerializeField]
        bool[] useRenderer;
        bool newUseRenderer;
        [SerializeField]
        SkinnedMeshRenderer[] targetRenderers;
        [SerializeField]
        List<Material> materials;
        [SerializeField]
        List<string> materialNames;

        // Animator info - used to improve root bone detection
        Animator anim;

        // patterns to match bone names
        string[] headPatterns = { "head" };
        string[] leftArmPatterns = { "left.*arm", "left.*upper.*arm", "l.*upper.*arm", "upper_arm.l", "lshldr", "l.*arm", "l.*shoulder" };
        string[] rightArmPatterns = { "right.*arm", "right.*upper.*arm", "r.*upper.*arm", "upper_arm.r", "rshldr", "r.*arm", "r.*shoulder" };
        string[] leftLegPatterns = { "left.*thigh", "left.*upper.*leg", "left.*up.*leg", "l.*thigh", "l.*upper.*leg", "left.*leg", "l.*leg" };
        string[] rightLegPatterns = { "right.*thigh", "right.*upper.*leg", "right.*up.*leg", "r.*thigh", "r.*upper.*leg", "right.*leg", "r.*leg" };

        // bones for root, head and arms
        [SerializeField]
        Transform rootBone;
        [SerializeField]
        Transform headBone;
        Transform newHeadBone;
        [SerializeField]
        Transform leftArmBone;
        Transform newLeftArmBone;
        [SerializeField]
        Transform rightArmBone;
        Transform newRightArmBone;
        [SerializeField]
        Transform leftLegBone;
        Transform newLeftLegBone;
        [SerializeField]
        Transform rightLegBone;
        Transform newRightLegBone;

        // bools for arm and leg separation
        [SerializeField]
        bool separateArms = false;
        bool newSeparateArms;
        [SerializeField]
        bool processLegs = false;
        bool newProcessLegs;
        [SerializeField]
        bool separateLegs = false;
        bool newSeparateLegs;

        // bools for what to include when building FPS mesh
        bool _includeAll = true;
        [SerializeField]
        bool includeAll {
            get { return _includeAll; }
            set {
                if (value) {
                    includeHead = true;
                    includeBody = true;
                    includeLeftArm = true;
                    includeRightArm = true;
                    includeLeftLeg = true;
                    includeRightLeg = true;
                }
                _includeAll = value;
            }
        }
        bool newIncludeAll;
        [SerializeField]
        bool includeHead = true;
        bool newIncludeHead;
        [SerializeField]
        bool includeBody = true;
        bool newIncludeBody;
        [SerializeField]
        bool includeLeftArm = true;
        bool newIncludeLeftArm;
        [SerializeField]
        bool includeRightArm = true;
        bool newIncludeRightArm;
        [SerializeField]
        bool includeLeftLeg = true;
        bool newIncludeLeftLeg;
        [SerializeField]
        bool includeRightLeg = true;
        bool newIncludeRightLeg;

        float newWeightThreshold = 1;
        int newVertexThreshold = 1;
        [SerializeField]
        float weightThreshold = 1;
        [SerializeField]
        int vertexThreshold = 1;

        string assetPath = "/FPSMeshTool/";
        string meshSubPath = "Meshes/";
        string matSubPath = "Materials/";

        bool isPrefab { get { return sourceObject != null && EditorUtility.IsPersistent(sourceObject); } }
        bool validObject { get { return sourceObject != null && !EditorUtility.IsPersistent(sourceObject); } }
        bool validRenderers { get { return sourceRenderers != null && sourceRenderers.Length > 0 && sourceRenderers[0] != null; } }
        bool validRootBone { get { return IsValidBone(sourceObject.transform, rootBone); } }
        bool validHeadBone { get { return IsValidBone(rootBone, headBone); } }
        bool validLeftArmBone { get { return IsValidBone(rootBone, leftArmBone); } }
        bool validRightArmBone { get { return IsValidBone(rootBone, rightArmBone); } }
        bool validLeftLegBone { get { return IsValidBone(rootBone, leftLegBone); } }
        bool validRightLegBone { get { return IsValidBone(rootBone, rightLegBone); } }
        bool validBones { get { return validRootBone && validHeadBone && validLeftArmBone && validRightArmBone && ((!processLegs) || (validLeftLegBone && validRightLegBone)); } }

        bool validAnimator { get { return anim != null && anim.isHuman; } }

        bool showPreview { get { return previewInstance != null; } }

        bool canShowPreview { get { return validObject && validRenderers && validBones; } }
        bool canHidePreview { get { return previewInstance != null; } }

        // GUI strings and whatnot
        GUIContent wtContent = new GUIContent("Weight Threshold", "Minimum bone weight to count a vertex as being attached to a bone.  Increase this if arms or head are including triangles they shouldn't.");
        GUIContent vtContent = new GUIContent("Vertex Threshold", "Number of attached vertices required to count a triangle as being part of arms or head.  Increase this if arms or head are including triangles they shouldn't.");
        GUIContent saContent = new GUIContent("Separate Arms", "Create separate materials for each arm.  This allows toggling visibility on a single arm at a time if desired.");
        GUIContent plContent = new GUIContent("Process Legs", "In addition to separating out the head and arms, also separate out the legs.");
        GUIContent slContent = new GUIContent("Separate Legs", "Create separate materials for each leg.  This allows toggling visibility on a single leg at a time if desired.");

        // GUI layout variables
        Vector2 scrollPos;

        [MenuItem("Custom/FPS Mesh Tool")]
        public static void Init() {
            EditorWindow.GetWindow(typeof(FPSMeshToolWindow), false, "FPS Mesh Tool");
        }

        void OnInspectorUpdate() {
            // This will only get called 10 times per second.
            Repaint();
        }

        void OnGUI() {
            // sanity checks //
            // if preview is showing, but one of the source objects is invalid
            if (showPreview && (!validObject || !validRenderers || !validBones)) {
                RemovePreview();
            }

            // create window as a large scrolling area
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUI.BeginChangeCheck();
            newSourceObject = (GameObject)EditorGUILayout.ObjectField("Object or Prefab", sourceObject, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(this, "sourceObject changed");
                sourceObject = newSourceObject;
                OnSourceObjectUpdated();
            }
            if (!validObject) {
                if (isPrefab) {
                    EditorGUILayout.HelpBox("Source Object is a prefab.  Press Instantiate to continue.", MessageType.Warning);
                    if (GUILayout.Button("Instantiate")) {
                        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(sourceObject);
                        Undo.RegisterCreatedObjectUndo(go, "Instantiated " + go.name);
                        go.name = sourceObject.name;
                        Undo.RecordObject(this, "switched sourceObject to instance");
                        sourceObject = go;
                        OnSourceObjectUpdated();
                    }
                } else {
                    EditorGUILayout.HelpBox("Select a GameObject or prefab to begin.", MessageType.Warning);
                }
            }

            EditorGUI.BeginChangeCheck();
            newWeightThreshold = EditorGUILayout.Slider(wtContent, weightThreshold, 0f, 1f);
            newVertexThreshold = EditorGUILayout.IntSlider(vtContent, vertexThreshold, 1, 3);

            if (EditorGUI.EndChangeCheck()) {
                if (newWeightThreshold != weightThreshold) {
                    Undo.RecordObject(this, "Adjusted weight threshold");
                    weightThreshold = newWeightThreshold;
                }
                if (newVertexThreshold != vertexThreshold) {
                    Undo.RecordObject(this, "Adjusted vertex threshold");
                    vertexThreshold = newVertexThreshold;
                }
                OnThresholdUpdated();
            }

            EditorGUILayout.Space();

            if (validObject) {
                if (validRenderers) {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Label("Choose Renderers", EditorStyles.boldLabel);
                    for (int i = 0; i < sourceRenderers.Length; i++) {
                        newUseRenderer = EditorGUILayout.Toggle(sourceRenderers[i].name, useRenderer[i]);
                        if (newUseRenderer != useRenderer[i]) {
                            Undo.RecordObject(this, "Updated useRenderer");
                            useRenderer[i] = newUseRenderer;
                        }
                    }
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("None")) {
                        for (int i = 0; i < useRenderer.Length; i++) {
                            useRenderer[i] = false;
                        }
                        Undo.RecordObject(this, "Updated useRenderer");
                    }
                    if (GUILayout.Button("All")) {
                        for (int i = 0; i < useRenderer.Length; i++) {
                            useRenderer[i] = true;
                        }
                        Undo.RecordObject(this, "Updated useRenderer");
                    }
                    GUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck()) {
                        OnRenderersUpdated();
                    }
                } else {
                    EditorGUILayout.HelpBox("Object doesn't contain any skinned mesh renderers.", MessageType.Warning);
                    if (showPreview) {
                        RemovePreview();
                    }
                }
                EditorGUILayout.Space();
            }

            if (validObject && validRenderers) {
                if (!validRootBone) {
                    // Reguess bones in case root bone has been set since last failure
                    GuessBones();
                }
                string boneMessage;
                if (validRootBone) {
                    boneMessage = "Select a bone that is a child of " + rootBone.name;
                } else {
                    boneMessage = "Please set the root bone in your skinned mesh renderer.";
                }
                GUILayout.Label("Bones", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                newHeadBone = (Transform)EditorGUILayout.ObjectField("Head", headBone, typeof(Transform), true);
                if (!validHeadBone) EditorGUILayout.HelpBox(boneMessage, MessageType.None);
                newLeftArmBone = (Transform)EditorGUILayout.ObjectField("Left Arm", leftArmBone, typeof(Transform), true);
                if (!validLeftArmBone) EditorGUILayout.HelpBox(boneMessage, MessageType.None);
                newRightArmBone = (Transform)EditorGUILayout.ObjectField("Right Arm", rightArmBone, typeof(Transform), true);
                if (!validRightArmBone) EditorGUILayout.HelpBox(boneMessage, MessageType.None);
                if (EditorGUI.EndChangeCheck()) {
                    if (newHeadBone != headBone) {
                        Undo.RecordObject(this, "Changed head bone");
                        headBone = newHeadBone;
                    }
                    if (newLeftArmBone != leftArmBone) {
                        Undo.RecordObject(this, "Changed left arm bone");
                        leftArmBone = newLeftArmBone;
                    }
                    if (newRightArmBone != rightArmBone) {
                        Undo.RecordObject(this, "Changed right arm bone");
                        rightArmBone = newRightArmBone;
                    }
                    OnBonesUpdated();
                }
                // separate arms
                EditorGUI.BeginChangeCheck();
                newSeparateArms = EditorGUILayout.Toggle(saContent, separateArms);
                if (EditorGUI.EndChangeCheck()) {
                    if (newSeparateArms != separateArms) {
                        Undo.RecordObject(this, "Toggled Separate Arms");
                        separateArms = newSeparateArms;
                    }
                    OnBonesUpdated();
                }
                // show leg section
                EditorGUI.BeginChangeCheck();
                newProcessLegs = EditorGUILayout.Toggle(plContent, processLegs);
                if (processLegs) {
                    newSeparateLegs = EditorGUILayout.Toggle(slContent, separateLegs);
                    newLeftLegBone = (Transform)EditorGUILayout.ObjectField("Left Leg", leftLegBone, typeof(Transform), true);
                    if (!validLeftLegBone) EditorGUILayout.HelpBox(boneMessage, MessageType.None);
                    newRightLegBone = (Transform)EditorGUILayout.ObjectField("Right Leg", rightLegBone, typeof(Transform), true);
                    if (!validRightLegBone) EditorGUILayout.HelpBox(boneMessage, MessageType.None);
                }
                if (EditorGUI.EndChangeCheck()) {
                    if (newProcessLegs != processLegs) {
                        Undo.RecordObject(this, "Toggled Process Legs");
                        processLegs = newProcessLegs;
                    } else {
                        if (newSeparateLegs != separateLegs) {
                            Undo.RecordObject(this, "Toggled Separate Legs");
                            separateLegs = newSeparateLegs;
                        }
                        if (newLeftLegBone != leftLegBone) {
                            Undo.RecordObject(this, "Changed left leg bone");
                            leftLegBone = newLeftLegBone;
                        }
                        if (newRightLegBone != rightLegBone) {
                            Undo.RecordObject(this, "Changed right leg bone");
                            rightLegBone = newRightLegBone;
                        }
                    }
                    OnBonesUpdated();
                }
                // show "Include xxxxx" section
                GUILayout.Label("Final Mesh Contents", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                newIncludeAll = EditorGUILayout.Toggle("Include All", includeAll);
                if (!includeAll) {
                    newIncludeHead = EditorGUILayout.Toggle("Include Head", includeHead);
                    newIncludeBody = EditorGUILayout.Toggle("Include Body", includeBody);
                    newIncludeLeftArm = EditorGUILayout.Toggle("Include LeftArm", includeLeftArm);
                    newIncludeRightArm = EditorGUILayout.Toggle("Include RightArm", includeRightArm);
                    if (processLegs) {
                        newIncludeLeftLeg = EditorGUILayout.Toggle("Include LeftLeg", includeLeftLeg);
                        newIncludeRightLeg = EditorGUILayout.Toggle("Include RightLeg", includeRightLeg);
                    }
                }
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(this, "Changed include toggles");
                    if (newIncludeAll != includeAll) {
                        includeAll = newIncludeAll;
                    } else {
                        if (newIncludeHead != includeHead) includeHead = newIncludeHead;
                        if (newIncludeBody != includeBody) includeBody = newIncludeBody;
                        if (newIncludeLeftArm != includeLeftArm) includeLeftArm = newIncludeLeftArm;
                        if (newIncludeRightArm != includeRightArm) includeRightArm = newIncludeRightArm;
                        if (processLegs) {
                            if (newIncludeLeftLeg != includeLeftLeg) includeLeftLeg = newIncludeLeftLeg;
                            if (newIncludeRightLeg != includeRightLeg) includeRightLeg = newIncludeRightLeg;
                        }
                    }
                    OnIncludeUpdated();
                }

            }


            if (canHidePreview) {
                if (GUILayout.Button("Remove Preview")) {
                    RemovePreview();
                }
            } else if (canShowPreview) {
                if (sourceObject != null) {
                    if (GUILayout.Button("Create Preview")) {
                        CreatePreview();
                    }
                }
            }

            if (showPreview) {
                if (GUILayout.Button("Build FPS mesh")) {
                    BuildFPSMesh();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        void OnSourceObjectUpdated() {
            if (sourceObject != null) {
                if (EditorUtility.IsPersistent(sourceObject)) {
                    // Remove preview if sourceObject is a prefab
                    RemovePreview();
                }
                Undo.RecordObject(this, "changed source renderers");
                // collect renderers and reset useRenderer array
                sourceRenderers = GetRenderers(sourceObject);
                useRenderer = new bool[sourceRenderers.Length];
                for (int i = 0; i < useRenderer.Length; i++) {
                    useRenderer[i] = true;
                }
                // get animator and avatar if present
                GetAnimatorInfo(sourceObject);
            } else {
                // source object removed - clear preview and renderers
                RemovePreview();
                Undo.RecordObject(this, "changed source renderers");
                sourceRenderers = null;
                useRenderer = null;
                // clear bone selections
            }
            GuessBones();
            OnRenderersUpdated();
        }

        void OnThresholdUpdated() {
            if (showPreview) {
                UpdatePreview();
            }
        }

        void OnIncludeUpdated() {
            if (showPreview) {
                UpdatePreview();
            }
        }

        void OnRenderersUpdated() {
            if (showPreview) {
                UpdatePreview();
            }
        }

        void OnBonesUpdated() {
            if (showPreview) {
                UpdatePreview();
            }
        }

        bool IsValidBone(Transform rootBone, Transform trans) {
            return validRenderers && (trans != null) && (trans.IsChildOf(rootBone));
        }

        void GetAnimatorInfo(GameObject sourceObject) {
            anim = sourceObject.GetComponentInChildren<Animator>();
        }

        void GuessBones() {
            Undo.RecordObject(this, "Changed bones");
            if (validObject && validRenderers) {
                rootBone = GetRootBone();
                if (validRootBone) {
                    headBone = GetBoneMatch(rootBone, headPatterns);
                    leftArmBone = GetBoneMatch(rootBone, leftArmPatterns);
                    rightArmBone = GetBoneMatch(rootBone, rightArmPatterns);
                    leftLegBone = GetBoneMatch(rootBone, leftLegPatterns);
                    rightLegBone = GetBoneMatch(rootBone, rightLegPatterns);
                } else {
                    headBone = null;
                    leftArmBone = null;
                    rightArmBone = null;
                    leftLegBone = null;
                    rightLegBone = null;
                }
            } else {
                rootBone = null;
                headBone = null;
                leftArmBone = null;
                rightArmBone = null;
                leftLegBone = null;
                rightLegBone = null;
            }
        }

        Transform GetRootBone() {
            Transform rootCandidate = null;
            if (validAnimator) {
                rootCandidate = anim.GetBoneTransform(HumanBodyBones.Hips);
            }
            return GetRoot(rootCandidate, GetRootBone(sourceRenderers));
        }

        Transform GetRootBone(SkinnedMeshRenderer[] renderers) {
            Transform rootBone = null;
            foreach (SkinnedMeshRenderer renderer in renderers) {
                rootBone = GetRoot(rootBone, renderer.rootBone);
            }
            return rootBone;
        }

        Transform GetRoot(Transform t1, Transform t2) {
            if (t1 == null) return t2;
            if (t2 == null) return t1;
            if (t1.IsChildOf(t2)) {
                return t2;
            }
            return t1;
        }

        Transform GetBoneMatch(Transform bone, string pattern) {
            Transform boneMatch = null;
            if (System.Text.RegularExpressions.Regex.IsMatch(bone.name, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) {
                return bone;
            }
            foreach (Transform child in bone) {
                boneMatch = GetBoneMatch(child, pattern);
                if (boneMatch != null) {
                    return boneMatch;
                }
            }
            return boneMatch;
        }

        Transform GetBoneMatch(Transform bone, string[] patterns) {
            Transform boneMatch = null;
            foreach (string pattern in patterns) {
                boneMatch = GetBoneMatch(bone, pattern);
                if (boneMatch != null) {
                    return boneMatch;
                }
            }
            return boneMatch;
        }

#if (UNITY_5_3) || (UNITY_5_3_OR_NEWER)
        // Copy all blend shapes from one mesh to another.
        // Only works in Unity 5.3 and newer.
        void CopyBlendShapes(Mesh sourceMesh, Mesh destMesh) {
            // Process blend shapes
            int blendShapeCount = sourceMesh.blendShapeCount;
            int frameCount = 0;
            Vector3[] deltaVertices = new Vector3[sourceMesh.vertexCount];
            Vector3[] deltaNormals = new Vector3[sourceMesh.vertexCount];
            Vector3[] deltaTangents = new Vector3[sourceMesh.vertexCount];

            // Go through blend shapes
            for (int shapeIndex = 0; shapeIndex < blendShapeCount; shapeIndex++) {
                // This can take a while - update the progress bar
                EditorUtility.DisplayProgressBar("Building FPS Mesh Prefab", "Processing blend shapes for " + sourceMesh.name, (float)shapeIndex / blendShapeCount);
                string shapeName = sourceMesh.GetBlendShapeName(shapeIndex);
                frameCount = sourceMesh.GetBlendShapeFrameCount(shapeIndex);
                // For each frame in the shape, get vertices and add the frame to the new mesh
                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
                    float frameWeight = sourceMesh.GetBlendShapeFrameWeight(shapeIndex, frameIndex);
                    sourceMesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);
                    destMesh.AddBlendShapeFrame(shapeName, frameWeight, deltaVertices, deltaNormals, deltaTangents);
                }
            }
        }

        // Check for blend shapes, and process if found.
        // Only works in Unity 5.3 and newer.
        void ProcessBlendShapes(SkinnedMeshRenderer sourceRenderer, SkinnedMeshRenderer targetRenderer) {
            // Check for blend shapes
            if (sourceRenderer.sharedMesh.blendShapeCount > 0) {
                // If found, save undo info on target renderer and copy blend shapes
                Undo.RecordObject(targetRenderer, "Updated renderer blend shapes");
                CopyBlendShapes(sourceRenderer.sharedMesh, targetRenderer.sharedMesh);
            }
        }
#endif

        // Convert the preview object into a prefab with appropriate materials
        // Copy blend shapes if there are any (Unity 5.3+)
        void BuildFPSMesh() {
            EditorUtility.DisplayProgressBar("Building FPS Mesh Prefab", "Creating directories.", 0f);
            // Create directories if not already present
            if (!System.IO.Directory.Exists(Application.dataPath + assetPath)) {
                AssetDatabase.CreateFolder("Assets", "FPSMeshTool");
            }
            if (!System.IO.Directory.Exists(Application.dataPath + assetPath + meshSubPath)) {
                AssetDatabase.CreateFolder("Assets/FPSMeshTool", "Meshes");
            }
            if (!System.IO.Directory.Exists(Application.dataPath + assetPath + matSubPath)) {
                AssetDatabase.CreateFolder("Assets/FPSMeshTool", "Materials");
            }

            int matIndex = 0;
            for (int i = 0; i < targetRenderers.Length; i++) {
                // Show progress bar
                EditorUtility.DisplayProgressBar("Building FPS Mesh Prefab", "Processing skinned mesh renderer " + sourceRenderers[i].name, (float)i / targetRenderers.Length);
                if (useRenderer[i]) {

#if (UNITY_5_3) || (UNITY_5_3_OR_NEWER)
                    // Process blend shapes (requires Unity 5.3+)
                    // Do this before saving mesh prefab - blendshapes not properly saved otherwise
                    ProcessBlendShapes(sourceRenderers[i], targetRenderers[i]);
#endif
                    // Create mesh asset if not already present
                    if (!AssetDatabase.Contains(targetRenderers[i].sharedMesh)) {
                        EditorUtility.DisplayProgressBar("Building FPS Mesh Prefab", "Creating mesh asset for " + sourceRenderers[i].name, (float)i / targetRenderers.Length);
                        AssetDatabase.CreateAsset(targetRenderers[i].sharedMesh, GetValidAssetName(GenerateMeshFileName(previewInstance, sourceRenderers[i])));
                    }
                    // Add materials to nesh
                    EditorUtility.DisplayProgressBar("Building FPS Mesh Prefab", "Creating materials for " + sourceRenderers[i].name, (float)i / targetRenderers.Length);
                    Material[] mats = new Material[targetRenderers[i].sharedMaterials.Length];
                    for (int rendererMatIndex = 0; rendererMatIndex < targetRenderers[i].sharedMaterials.Length; rendererMatIndex++) {
                        mats[rendererMatIndex] = new Material(materials[matIndex]);
                        AssetDatabase.CreateAsset(mats[rendererMatIndex], GetValidAssetName(GenerateMaterialFileName(previewInstance, materialNames[matIndex])));
                        matIndex++;
                    }
                    Undo.RecordObject(targetRenderers[i], "Updated renderer materials");
                    targetRenderers[i].sharedMaterials = mats;

                }
            }
            // Create prefab
            PrefabUtility.CreatePrefab(GetValidAssetName(GeneratePrefabFileName(previewInstance)), previewInstance, ReplacePrefabOptions.ConnectToPrefab);
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        string GenerateMaterialFileName(GameObject go, string matName) {
            return assetPath + matSubPath + go.name + " - " + matName + ".mat";
        }

        string GenerateMeshFileName(GameObject go, SkinnedMeshRenderer renderer) {
            return assetPath + meshSubPath + go.name + " - " + renderer.sharedMesh.name + ".asset";
        }

        string GeneratePrefabFileName(GameObject go) {
            return assetPath + go.name + ".prefab";
        }

        string GetValidAssetName(string preferredName) {
            string fileName = Application.dataPath + preferredName;
            if (System.IO.File.Exists(fileName)) {
                int splitPos = preferredName.LastIndexOf('.');
                return GetValidAssetName(preferredName.Substring(0, splitPos) + " 1" + preferredName.Substring(splitPos));
            }
            return "Assets" + preferredName;
        }

        // Build preview meshes
        void BuildPreviewMeshes() {
            if (canShowPreview) {
                //float startTime = Time.realtimeSinceStartup;

                // reset material and material name lists
                Undo.RecordObject(this, "Changed materials");
                materials = new List<Material>();
                materialNames = new List<string>();
                for (int i = 0; i < targetRenderers.Length; i++) {
                    if (useRenderer[i]) {
                        ConvertToFPSMesh(sourceRenderers[i], targetRenderers[i], headBone, leftArmBone, rightArmBone, processLegs, leftLegBone, rightLegBone);
                    }
                }
                //float endTime = Time.realtimeSinceStartup;
                //Debug.Log("Elapsed Time: " + (endTime - startTime));
            }
        }

        // add to materials and material names, substituting missing mats for a default diffuse mat
        void AddMaterial(List<Material> materials, List<string> materialNames, Material[] mats, int matIndex, string suffix) {
            Material mat = (mats != null && mats.Length > matIndex) ? mats[matIndex] : null;
            if (mat == null) {
                mat = new Material(Shader.Find("Diffuse"));
                mat.name = "Placeholder";
            }
            materials.Add(mat);
            materialNames.Add(mat.name + " " + suffix);
        }

        void ConvertToFPSMesh(SkinnedMeshRenderer sourceRenderer, SkinnedMeshRenderer targetRenderer, Transform head, Transform leftArm, Transform rightArm, bool processLegs, Transform leftLeg, Transform rightLeg) {

            // save undo information
            Undo.RecordObject(this, "Updated materials");


            // collect preview materials
            // alternately, could build new materials instead - would leave a cleaner file system
            Material headMat = (Material)Resources.Load("FPSMeshMaterials/HeadMat");
            Material bodyMat = (Material)Resources.Load("FPSMeshMaterials/BodyMat");
            Material armsMat = (Material)Resources.Load("FPSMeshMaterials/ArmsMat");
            Material legsMat = (Material)Resources.Load("FPSMeshMaterials/LegsMat");

            Mesh baseMesh = sourceRenderer.sharedMesh;
            BoneWeight[] boneWeights = baseMesh.boneWeights;
            // get all bones
            Transform[] bones = sourceRenderer.bones;
            // collect head bones
            Transform[] headBones = GetBones(head, bones);
            int[] headBoneIndices = GetBoneIndices(headBones, bones);
            // collect left arm bones
            Transform[] leftArmBones = GetBones(leftArm, bones);
            int[] leftArmBoneIndices = GetBoneIndices(leftArmBones, bones);
            // collect right arm bones
            Transform[] rightArmBones = GetBones(rightArm, bones);
            int[] rightArmBoneIndices = GetBoneIndices(rightArmBones, bones);
            // collect left leg bones
            Transform[] leftLegBones = GetBones(processLegs ? leftLeg : null, bones);
            int[] leftLegBoneIndices = GetBoneIndices(leftLegBones, bones);
            // collect right Leg bones
            Transform[] rightLegBones = GetBones(processLegs ? rightLeg : null, bones);
            int[] rightLegBoneIndices = GetBoneIndices(rightLegBones, bones);

            Dictionary<int, int[]> boneDict = GetBoneDict(headBoneIndices, leftArmBoneIndices, rightArmBoneIndices, leftLegBoneIndices, rightLegBoneIndices);

            List<int[]> subMeshes = new List<int[]>();
            List<Material> mats = new List<Material>();

            // create new mesh
            Mesh newMesh = new Mesh();
            // copy vertices, uv, boneweights
            newMesh.vertices = baseMesh.vertices;
            newMesh.uv = baseMesh.uv;
            newMesh.normals = baseMesh.normals;
            newMesh.colors = baseMesh.colors;
            newMesh.tangents = baseMesh.tangents;
            newMesh.boneWeights = baseMesh.boneWeights;
            newMesh.bindposes = baseMesh.bindposes;

            // process each submesh (material)
            for (int subMeshID = 0; subMeshID < baseMesh.subMeshCount; subMeshID++) {
                List<Triangle> headTriangles = new List<Triangle>();
                List<Triangle> leftArmTriangles = new List<Triangle>();
                List<Triangle> rightArmTriangles = new List<Triangle>();
                List<Triangle> leftLegTriangles = new List<Triangle>();
                List<Triangle> rightLegTriangles = new List<Triangle>();
                List<Triangle> bodyTriangles = new List<Triangle>();
                // collect triangles for submesh
                int[] triangles = baseMesh.GetTriangles(subMeshID);
                Triangle tri = new Triangle();
                int index = 0;
                // go through each triangle in submesh
                while (index < triangles.Length) {
                    tri = new Triangle(triangles, index);
                    index += 3;
                    // check triangle for connection to head or arms
                    // add to appropriate triangle list
                    int[] belongsTo = Belongs(tri, baseMesh, boneDict, boneWeights);
                    if (belongsTo == headBoneIndices) {
                        headTriangles.Add(tri);
                    } else if (belongsTo == leftArmBoneIndices) {
                        leftArmTriangles.Add(tri);
                    } else if (belongsTo == rightArmBoneIndices) {
                        rightArmTriangles.Add(tri);
                    } else if (belongsTo == leftLegBoneIndices) {
                        leftLegTriangles.Add(tri);
                    } else if (belongsTo == rightLegBoneIndices) {
                        rightLegTriangles.Add(tri);
                    } else {
                        bodyTriangles.Add(tri);
                    }
                }
                if (bodyTriangles.Count > 0 && includeBody) {
                    AddSubMesh(subMeshes, mats, bodyTriangles.ToArray(), bodyMat);
                    AddMaterial(materials, materialNames, sourceRenderer.sharedMaterials, subMeshID, "body");
                }
                if (headTriangles.Count > 0 && includeHead) {
                    AddSubMesh(subMeshes, mats, headTriangles.ToArray(), headMat);
                    AddMaterial(materials, materialNames, sourceRenderer.sharedMaterials, subMeshID, "head");
                }
                if (separateArms) {
                    if (includeLeftArm) {
                        if (leftArmTriangles.Count > 0) {
                            AddSubMesh(subMeshes, mats, leftArmTriangles.ToArray(), armsMat);
                            AddMaterial(materials, materialNames, sourceRenderer.sharedMaterials, subMeshID, "left arm");
                        }
                    }
                    if (includeRightArm) {
                        if (rightArmTriangles.Count > 0) {
                            AddSubMesh(subMeshes, mats, rightArmTriangles.ToArray(), armsMat);
                            AddMaterial(materials, materialNames, sourceRenderer.sharedMaterials, subMeshID, "right arm");
                        }
                    }
                } else {
                    List<Triangle> armTris = new List<Triangle>();
                    if (includeLeftArm) {
                        armTris.AddRange(leftArmTriangles);
                    }
                    if (includeRightArm) {
                        armTris.AddRange(rightArmTriangles);
                    }
                    if (armTris.Count > 0) {
                        AddSubMesh(subMeshes, mats, armTris.ToArray(), armsMat);
                        AddMaterial(materials, materialNames, sourceRenderer.sharedMaterials, subMeshID, "arms");
                    }
                }
                if (separateLegs) {
                    if (includeLeftLeg) {
                        if (leftLegTriangles.Count > 0) {
                            AddSubMesh(subMeshes, mats, leftLegTriangles.ToArray(), legsMat);
                            AddMaterial(materials, materialNames, sourceRenderer.sharedMaterials, subMeshID, "left leg");
                        }
                    }
                    if (includeRightLeg) {
                        if (rightLegTriangles.Count > 0) {
                            AddSubMesh(subMeshes, mats, rightLegTriangles.ToArray(), legsMat);
                            AddMaterial(materials, materialNames, sourceRenderer.sharedMaterials, subMeshID, "right leg");
                        }
                    }
                } else {
                    List<Triangle> legTris = new List<Triangle>();
                    if (includeLeftLeg) {
                        legTris.AddRange(leftLegTriangles);
                    }
                    if (includeRightLeg) {
                        legTris.AddRange(rightLegTriangles);
                    }
                    if (legTris.Count > 0) {
                        AddSubMesh(subMeshes, mats, legTris.ToArray(), legsMat);
                        AddMaterial(materials, materialNames, sourceRenderer.sharedMaterials, subMeshID, "legs");
                    }
                }

            }
            AssignSubMeshes(targetRenderer, newMesh, subMeshes, mats);
            targetRenderer.sharedMesh = newMesh;
            targetRenderer.sharedMaterials = mats.ToArray();
        }

        void AssignSubMeshes(SkinnedMeshRenderer meshRenderer, Mesh mesh, List<int[]> subMeshes, List<Material> mats) {
            Undo.RecordObject(meshRenderer, "Updated mesh");
            mesh.subMeshCount = subMeshes.Count;
            for (int i = 0; i < mesh.subMeshCount; i++) {
                mesh.SetTriangles(subMeshes[i], i);
            }
            meshRenderer.sharedMaterials = mats.ToArray();
        }

        void AddSubMesh(List<int[]> subMeshes, List<Material> mats, Triangle[] tris, Material mat) {
            subMeshes.Add(Flatten(tris));
            mats.Add(mat);
        }

        T[] Add<T>(T item, T[] array) {
            T[] newArray = new T[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[array.Length] = item;
            return newArray;
        }

        int[] Belongs(Triangle tri, Mesh mesh, Dictionary<int, int[]> boneDict, BoneWeight[] boneWeights) {
            Dictionary<int[], float> limbWeights = new Dictionary<int[], float>();
            Dictionary<int[], int> limbVerts = new Dictionary<int[], int>();
            float minWeight = Mathf.Max(.00001f, weightThreshold);
            int minVerts = vertexThreshold;
            BoneWeight weight;
            int vertCount;
            foreach (int vert in tri.verts) {
                weight = boneWeights[vert];
                // check all four bone weights for a nonzero value matching anything in the boneDict
                ProcessBoneWeight(weight.boneIndex0, weight.weight0, limbWeights, boneDict);
                ProcessBoneWeight(weight.boneIndex1, weight.weight1, limbWeights, boneDict);
                ProcessBoneWeight(weight.boneIndex2, weight.weight2, limbWeights, boneDict);
                ProcessBoneWeight(weight.boneIndex3, weight.weight3, limbWeights, boneDict);
                foreach (int[] limb in limbWeights.Keys) {
                    if (limbWeights[limb] >= minWeight) {
                        limbVerts.TryGetValue(limb, out vertCount);
                        limbVerts[limb] = vertCount + 1;
                    }
                }
            }
            foreach (int[] limb in limbVerts.Keys) {
                if (limbVerts[limb] >= minVerts) {
                    return limb;
                }
            }
            return null;
        }

        void ProcessBoneWeight(int bone, float weight, Dictionary<int[], float> limbWeights, Dictionary<int, int[]> boneDict) {
            if (weight > 0 && boneDict.ContainsKey(bone)) {
                float prevWeight;
                int[] limb = boneDict[bone];
                limbWeights.TryGetValue(limb, out prevWeight);
                limbWeights[limb] = weight + prevWeight;
            }
        }

        int[] Flatten(Triangle[] sourceTris) {
            List<int> tris = new List<int>();
            foreach (Triangle t in sourceTris) {
                tris.Add(t[0]);
                tris.Add(t[1]);
                tris.Add(t[2]);
            }
            return tris.ToArray();
        }

        bool IsIn<T>(T lhs, T[] rhs) {
            return (System.Array.IndexOf(rhs, lhs) != -1);
        }

        Dictionary<int, int[]> GetBoneDict(int[] headBoneIndices, int[] leftArmBoneIndices, int[] rightArmBoneIndices, int[] leftLegBoneIndices, int[] rightLegBoneIndices) {
            Dictionary<int, int[]> boneDict = new Dictionary<int, int[]>();
            foreach (int bone in headBoneIndices) {
                boneDict[bone] = headBoneIndices;
            }
            foreach (int bone in leftArmBoneIndices) {
                boneDict[bone] = leftArmBoneIndices;
            }
            foreach (int bone in rightArmBoneIndices) {
                boneDict[bone] = rightArmBoneIndices;
            }
            foreach (int bone in leftLegBoneIndices) {
                boneDict[bone] = leftLegBoneIndices;
            }
            foreach (int bone in rightLegBoneIndices) {
                boneDict[bone] = rightLegBoneIndices;
            }
            return boneDict;
        }

        int[] GetBoneIndices(Transform[] bones, Transform[] bonesMaster) {
            List<int> boneIndices = new List<int>();
            foreach (Transform bone in bones) {
                boneIndices.Add(System.Array.IndexOf(bonesMaster, bone));
            }
            return boneIndices.ToArray();
        }

        Transform[] GetBones(Transform rootBone, Transform[] bones) {
            List<Transform> subBoneList = new List<Transform>();
            if (rootBone != null) {
                foreach (Transform child in rootBone.GetComponentsInChildren<Transform>()) {
                    if (IsIn<Transform>(child, bones)) {
                        subBoneList.Add(child);
                    }
                }
            }
            return subBoneList.ToArray();
        }


        SkinnedMeshRenderer[] GetRenderers(GameObject go) {
            List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
            SkinnedMeshRenderer renderer;
            GameObject[] objects = GetObjects(go);
            foreach (GameObject currentObject in objects) {
                renderer = currentObject.GetComponent<SkinnedMeshRenderer>();
                if (renderer != null) {
                    renderers.Add(renderer);
                }
            }

            return renderers.ToArray();
        }

        GameObject[] GetObjects(GameObject go) {
            List<GameObject> objects = new List<GameObject>();
            if (go != null) {
                objects.Add(go);
            }
            foreach (Transform child in go.transform) {
                objects.AddRange(GetObjects(child.gameObject));
            }
            return objects.ToArray();
        }

        void UpdatePreview() {
            Vector3 pos = sourceObject.transform.position + sourceObject.transform.forward;
            Quaternion rot = sourceObject.transform.rotation;
            if (previewInstance != null) {
                pos = previewInstance.transform.position;
                rot = previewInstance.transform.rotation;
                RemovePreview();
            }
            Undo.RecordObject(this, "switched previewInstance");
            previewInstance = (GameObject)Instantiate(sourceObject, pos, rot);
            Undo.RegisterCreatedObjectUndo(previewInstance, "Created previewInstance");
            previewInstance.name = "FPSMesh - " + sourceObject.name;
            Undo.RecordObject(this, "changed targetRenderers");
            targetRenderers = GetRenderers(previewInstance);
            BuildPreviewMeshes();
        }

        void CreatePreview() {
            UpdatePreview();
        }

        void RemovePreview() {
            if (showPreview) {
                Undo.RecordObject(this, "Removed previewInstance");
                Undo.DestroyObjectImmediate(previewInstance);
            }
        }

    }
}