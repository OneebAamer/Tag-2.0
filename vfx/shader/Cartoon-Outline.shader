Shader "Assets/vfx/shaders/Cartoon-Outline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineThreshold("Outline Threshold", Float) = 10
        _OutlineWidth("Outline Width", Float) = 1
        _DepthMultiplier("Depth Multiplier", Float) = 160
        _EdgeStrengthen("Edge Strengthen", Float) = 100
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "SSAllInOneOutline"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _OutlineColor;
            float _OutlineThreshold;
            float _OutlineWidth;
            float _DepthMultiplier;
            float _EdgeStrengthen;

            float EdgeCompare(float2 uv1, float2 uv2)
            {
                float rawDepth1 = SampleSceneDepth(uv1);
                float rawDepth2 = SampleSceneDepth(uv2);

                float linearDepth1 = Linear01Depth(rawDepth1, _ZBufferParams);
                float linearDepth2 = Linear01Depth(rawDepth2, _ZBufferParams);

                float depthTerm =
                    pow(linearDepth1 - linearDepth2, 2.0) +
                    pow((rawDepth1 - rawDepth2) * _DepthMultiplier, 2.0);

                float3 color1 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv1).rgb;
                float3 color2 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv2).rgb;
                float3 colorTerm = pow((color1 - color2) * _EdgeStrengthen, 2.0);

                // This matches your original shader's behavior:
                // scalar depth terms get added into each RGB channel before thresholding.
                float3 combined = depthTerm.xxx + colorTerm;
                return combined.r + combined.g + combined.b;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                float4 pixel = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                float onePixelW = 1.0 / _ScreenParams.x;
                float onePixelH = 1.0 / _ScreenParams.y;

                float outline = 0.0;
                int width = max(1, (int)round(_OutlineWidth));

                [loop]
                for (int i = 1; i <= width; ++i)
                {
                    float x = i * onePixelW;
                    float y = i * onePixelH;

                    outline += EdgeCompare(float2(uv.x - x, uv.y),     float2(uv.x + x, uv.y));
                    outline += EdgeCompare(float2(uv.x,     uv.y + y), float2(uv.x,     uv.y - y));
                    outline += EdgeCompare(float2(uv.x - x, uv.y - y), float2(uv.x + x, uv.y + y));
                    outline += EdgeCompare(float2(uv.x - x, uv.y + y), float2(uv.x + x, uv.y - y));
                }

                return (outline >= _OutlineThreshold) ? _OutlineColor : pixel;
            }

            ENDHLSL
        }
    }
}