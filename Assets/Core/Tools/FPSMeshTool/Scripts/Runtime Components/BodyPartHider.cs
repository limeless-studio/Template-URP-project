using UnityEngine;
using System.Collections;

namespace CBG {
    public class BodyPartHider : MonoBehaviour {
        public FPSBodyPart bodyPart;

        FPSMaterialController controller;
        bool initialized;
        bool hidden;

        void Awake() {
            controller = GetComponentInParent<FPSMaterialController>();
        }

        void Start() {
            Init();
            ShowBodyPart();
        }

        void Init() {
            initialized = true;
        }

        void OnEnable() {
            if (!initialized)
                return;

            ShowBodyPart();
        }

        void OnDisable() {
            if (!initialized)
                return;

            HideBodyPart();
        }

        void ShowBodyPart() {
            if (controller && hidden) {
                controller.ShowBodyPart(bodyPart);
                hidden = false;
            }
        }

        void HideBodyPart() {
            if (controller && !hidden) {
                controller.HideBodyPart(bodyPart);
                hidden = true;
            }
        }
    }
}