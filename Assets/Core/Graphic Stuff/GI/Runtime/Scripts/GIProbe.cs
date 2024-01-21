using UnityEngine;

namespace GI.Universal {

    [ExecuteInEditMode]
    public class GIProbe : MonoBehaviour {

        ReflectionProbe probe;

        void OnEnable() {
            probe = GetComponent<ReflectionProbe>();
            GIRenderFeature.RegisterReflectionProbe(probe);
        }

        void OnDisable() {
            GIRenderFeature.UnregisterReflectionProbe(probe);
        }
    }

}
