/*
 * Modified from PixelStandard
 * https://gitlab.com/s-ilent/pixelstandard
 *
 * Copyright (c) 2021 s-ilent
 * Released under the MIT license. see https://opensource.org/licenses/MIT
 */
Shader "ku6dra/Custom/Silent/PixelUnlitTransparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
       Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        LOD 100

        Blend One OneMinusSrcAlpha
        //ZWrite Off
        //Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            //#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(int, _EnableSharp)
            UNITY_INSTANCING_BUFFER_END(Props)

            // Returns pixel sharpened to nearest pixel boundary. 
            // texelSize is Unity _Texture_TexelSize; zw is w/h, xy is 1/wh
            float2 sharpSample( float4 texelSize , float2 p )
            {
                p = p*texelSize.zw;
                float2 c = max(0.0, abs(fwidth(p)));
                p = p + abs(c);
                p = floor(p) + saturate(frac(p) / c);
                p = (p - 0.5)*texelSize.xy;
                return p;
            }


            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, _EnableSharp ? sharpSample(_MainTex_TexelSize, i.uv) : i.uv);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

    }
}
