using UnityEngine;
using UnityEngine.Rendering;

namespace GI.Universal
{
    public class ToggleEffect : MonoBehaviour
    {

        public VolumeProfile profile;

        GlobalIllumination radiant;

        void Start()
        {
            profile.TryGet(out radiant);
        }


        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                radiant.active = !radiant.active;
            }
        }
    }


}

