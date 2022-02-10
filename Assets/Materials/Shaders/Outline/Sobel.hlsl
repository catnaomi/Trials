
float SampleDepth(float2 uv)
{
#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
    return SAMPLE_TEXTURE2D_ARRAY(_CameraDepthTexture, sampler_CameraDepthTexture, uv, unity_StereoEyeIndex).r;
#else
    return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
#endif
}

void sobel(in float2 uv, in float _Delta, out float value)
{
    float2 delta = float2(_Delta, _Delta);

    float hr = 0;
    float vt = 0;

    hr += SampleDepth(uv + float2(-1.0, -1.0) * delta) * 1.0;
    hr += SampleDepth(uv + float2(1.0, -1.0) * delta) * -1.0;
    hr += SampleDepth(uv + float2(-1.0, 0.0) * delta) * 2.0;
    hr += SampleDepth(uv + float2(1.0, 0.0) * delta) * -2.0;
    hr += SampleDepth(uv + float2(-1.0, 1.0) * delta) * 1.0;
    hr += SampleDepth(uv + float2(1.0, 1.0) * delta) * -1.0;

    vt += SampleDepth(uv + float2(-1.0, -1.0) * delta) * 1.0;
    vt += SampleDepth(uv + float2(0.0, -1.0) * delta) * 2.0;
    vt += SampleDepth(uv + float2(1.0, -1.0) * delta) * 1.0;
    vt += SampleDepth(uv + float2(-1.0, 1.0) * delta) * -1.0;
    vt += SampleDepth(uv + float2(0.0, 1.0) * delta) * -2.0;
    vt += SampleDepth(uv + float2(1.0, 1.0) * delta) * -1.0;

    value = sqrt(hr * hr + vt * vt);
}
