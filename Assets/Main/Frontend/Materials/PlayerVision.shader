Shader "Custom/PlayerVision"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }
 
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"

            "RenderPipeline" = "UniversalPipeline"
        }
 
        Cull Off
        Lighting Off
        ColorMask 0
        ZWrite Off
        Blend One OneMinusSrcAlpha
 
        Stencil
        {
            Ref 1
            Comp always
            Pass replace
        }
 
        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
 
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                half4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
            };
 
            half4 _Color;
 
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = TransformObjectToHClip(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
 
                return OUT;
            }
 
            sampler2D _MainTex;
 
            half4 frag(v2f IN) : SV_Target
            {
                half4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                if (c.a < 0.1) discard;
                c.rgb *= c.a;
                return c;
            }

            ENDHLSL
        }
    }
}