float2 ParallaxMapping(out float2 Out, float _MaxLayerNum, float _MinLayerNum, float heightScale, _DepthMap, float2 uv, float3 viewDir_tangent)
{
    float layerNum = lerp(_MaxLayerNum, _MinLayerNum, abs(dot(float3(0,0,1), viewDir_tangent))); //垂直时用更少的样本
    float layerDepth = 1.0 / layerNum;
    float currentLayerDepth = 0.0;
    float2 deltaTexCoords = viewDir_tangent.xy / viewDir_tangent.z * heightScale / layerNum;

    float2 currentTexCoords = uv;
    float currentDepthMapValue = tex2D(_DepthMap, currentTexCoords).b;

    //unable to unroll loop, loop does not appear to terminate in a timely manner
    //上面这个错误是在循环内使用tex2D导致的，需要加上unroll来限制循环次数或者改用tex2Dlod
    // [unroll(100)]
    while(currentLayerDepth < currentDepthMapValue)
    {
        currentTexCoords -= deltaTexCoords;
        // currentDepthMapValue = tex2D(_DepthMap, currentTexCoords).r;
        currentDepthMapValue = tex2Dlod(_DepthMap, float4(currentTexCoords, 0, 0)).b;
        currentLayerDepth += layerDepth;
    }

    float2 prevTexCoords = currentTexCoords + deltaTexCoords;
    float prevLayerDepth = currentLayerDepth - layerDepth;

    float afterDepth = currentDepthMapValue - currentLayerDepth;
    float beforeDepth = tex2D(_DepthMap, prevTexCoords).b - prevLayerDepth;
    float weight = afterDepth / (afterDepth - beforeDepth);
    float2 finalTexCoords = prevTexCoords * weight + currentTexCoords * (1.0 - weight);

    Out = finalTexCoords;
}
