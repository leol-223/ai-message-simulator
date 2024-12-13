Shader "Custom/RoundedCornersAR"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _AspectRatio ("Aspect Ratio (Width/Height)", Float) = 1.0
        _CornerRadius ("Corner Radius", Range(0,0.5)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _AspectRatio;
            float _CornerRadius;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            bool OutsideCornerCircle(float2 scaledUV, float2 cornerCenter, float radius)
            {
                float dx = scaledUV.x - cornerCenter.x;
                float dy = scaledUV.y - cornerCenter.y;
                return (dx*dx + dy*dy) > (radius*radius);
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Create scaledUV based on AR:
                float2 scaledUV;
                if (_AspectRatio > 1.0)
                {
                    // Wide: scale x by AR
                    scaledUV = float2(uv.x * _AspectRatio, uv.y);
                }
                else if (_AspectRatio < 1.0)
                {
                    // Tall: scale y by 1/AR
                    scaledUV = float2(uv.x, uv.y * (1.0/_AspectRatio));
                }
                else
                {
                    // Square AR=1, no scaling needed
                    scaledUV = uv;
                }

                float r = _CornerRadius;

                // Corners in original UV: (0,0), (1,0), (0,1), (1,1)
                // After scaling:
                // bottom-left corner center: (0,0)
                // bottom-right corner center:
                float2 bottomRight = (_AspectRatio > 1.0) ? float2(_AspectRatio,0) : float2(1,0*(1.0/_AspectRatio));
                if (_AspectRatio == 1.0) bottomRight = float2(1,0);

                float2 topLeft = (_AspectRatio > 1.0) ? float2(0,1) : float2(0,1*(1.0/_AspectRatio));
                if (_AspectRatio == 1.0) topLeft = float2(0,1);

                float2 topRight = (_AspectRatio > 1.0) ? float2(_AspectRatio,1) : float2(1,1*(1.0/_AspectRatio));
                if (_AspectRatio == 1.0) topRight = float2(1,1);

                // Bottom-left corner check
                if (scaledUV.x < r && scaledUV.y < r)
                {
                    if (OutsideCornerCircle(scaledUV, float2(r,r), r)) discard;
                }

                // Bottom-right corner check
                if (scaledUV.x > (bottomRight.x - r) && scaledUV.y < r)
                {
                    if (OutsideCornerCircle(scaledUV, bottomRight - float2(r,-r), r)) discard;
                }

                // Top-left corner check
                if (scaledUV.x < r && scaledUV.y > (topLeft.y - r))
                {
                    if (OutsideCornerCircle(scaledUV, topLeft + float2(r,-r), r)) discard;
                }

                // Top-right corner check
                if (scaledUV.x > (topRight.x - r) && scaledUV.y > (topRight.y - r))
                {
                    if (OutsideCornerCircle(scaledUV, topRight - float2(r,r), r)) discard;
                }

                return tex2D(_MainTex, uv) * _Color;
            }
            ENDCG
        }
    }
    FallBack Off
}
