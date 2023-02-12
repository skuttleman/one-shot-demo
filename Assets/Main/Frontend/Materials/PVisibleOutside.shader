Shader "Custom/PVisibleOutside" {
    Properties {
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.;
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }
 
    SubShader {
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"

            "RenderPipeline" = "UniversalPipeline"
        }
 
        Cull Off
        Lighting Off
        ZWrite Off
        //Blend One OneMinusSrcAlpha
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
 
        Stencil {
            Ref 1
            Comp notequal
            Pass keep
        }
 
        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            half4 _Color;
            sampler2D _MainTex;
 
            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
 
            struct v2f {
                float4 vertex   : SV_POSITION;
                half4 color     : COLOR;
                half2 texcoord  : TEXCOORD0;
            };

            inline float4 UnityPixelSnap (float4 pos) {
                float2 hpc = _ScreenParams.xy * 0.5f;
                #if  SHADER_API_PSSL
                    // An old sdk used to implement round() as floor(x+0.5) current sdks use the round to even method so we manually use the old method here for compatabilty.
                    float2 temp = ((pos.xy / pos.w) * hpc) + float2(0.5f,0.5f);
                    float2 pixelPos = float2(floor(temp.x), floor(temp.y));
                #else
                    float2 pixelPos = round ((pos.xy / pos.w) * hpc);
                #endif
                pos.xy = pixelPos / hpc * pos.w;
                return pos;
            }
 
            v2f vert(appdata_t IN) {
                v2f OUT;
                OUT.vertex = TransformObjectToHClip(IN.vertex.xyz);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
 
                return OUT;
            }
 
            half4 frag(v2f IN) : SV_Target {
                half4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                c.rgb *= c.a;
                return c;
            }
            ENDHLSL
        }
    }
}
