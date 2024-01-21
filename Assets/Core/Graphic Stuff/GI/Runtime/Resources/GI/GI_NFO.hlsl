#ifndef RGI_NFO
#define RGI_NFO

    static float2 randomOffsets[16] = {
         float2(1.0000000, 0.0000000),
         float2(0.9238795, 0.3826835),
         float2(0.7071068, 0.7071068),
         float2(0.3826834, 0.9238795),
         float2(0.0000000, 1.0000000),
         float2(-0.3826835, 0.9238795),
         float2(-0.7071068, 0.7071068),
         float2(-0.9238796, 0.3826833),
         float2(-1.0000000, -0.0000001),
         float2(-0.9238795, -0.3826834),
         float2(-0.7071066, -0.7071069),
         float2(-0.3826831, -0.9238797),
         float2(0.0000000, -1.0000000),
         float2(0.3826836, -0.9238794),
         float2(0.7071070, -0.7071065),
         float2(0.9238796, -0.3826834)
    };

        
	half4 FragRGI (VaryingsRGI i) : SV_Target { 

        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);

        float rawDepth = GetDownscaledRawDepth(uv);
        float eyeDepth = RawToLinearEyeDepth(rawDepth);
        if (eyeDepth > NEAR_FIELD_OBSCURANCE_MAX_CAMERA_DISTANCE + 100) return 0;

        float3 wpos = GetWorldPosition(uv, rawDepth);
        if (IsOutsideBounds(wpos)) return 0;

        float2 pos = uv * SOURCE_SIZE / 2;
        float2 noise = normalize(SAMPLE_TEXTURE2D_LOD(_NoiseTex, sampler_PointRepeat, pos * _NoiseTex_TexelSize.xy, 0).xw);

        float samplingRadius = NEAR_FIELD_OBSCURANCE_SPREAD / eyeDepth;

        float3 normalWS = GetWorldNormal(uv);
        half occlusion = 0.001;

        for(int k=0; k < 16; k++) {

            float2 offset = samplingRadius * reflect(randomOffsets[k], noise);
            float2 occuv = uv + offset;
            float rawDepthN = GetDownscaledRawDepth(occuv);
            float3 occwpos = GetWorldPosition(occuv, rawDepthN);
            half3  occdir = (half3)(occwpos - wpos);
            half   occmag = dot2(occdir * NEAR_FIELD_OBSCURANCE_OCCLUDER_DISTANCE) + 0.1;
            half   occ = saturate(dot(normalWS, occdir / occmag) - 0.15);
            occlusion += occ;
        }
                
        half nfo = NEAR_FIELD_OBSCURANCE_INTENSITY * occlusion / 16.0;
        nfo /= max(1.0, 0.3 * (10 + eyeDepth - NEAR_FIELD_OBSCURANCE_MAX_CAMERA_DISTANCE));

        return nfo;
	}



#endif // NFO