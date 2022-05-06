Shader "Unlit/SolidBorder"
{
    Properties
    {
        _Outline ("Color", Color) = (1,1,1,1)
        _Size ("Cube Size", Range(-0.1, 2)) = 1
        _OutlineSize ("Outline Size", Range(-0.5, 0.5)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Points
            {
                float3 v[8];
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 viewPos : TEXCOORD1;
                Points points: TEXCOORD2;
            };

            float4 _Outline;
            float _OutlineSize;
            float _Size;

            Points GetPointsInView()
            {
                float v = _Size * (1 + ((_OutlineSize < 0) * _OutlineSize))/2;
                Points sPoints =
                {
                    float3(v,v,v), float3(v,v,-v), float3(-v,v,-v), float3(-v,v,v),
                    float3(v,-v,v), float3(v,-v,-v), float3(-v,-v,-v), float3(-v,-v,v)
                };

                for(int i = 0; i < 8 ; i++)
                {
                    sPoints.v[i] = float3(UnityObjectToViewPos(sPoints.v[i]).xy, 0);
                }

                return sPoints;
            }
            
            Interpolators vert (MeshData v)
            {
                Interpolators o;
                v.vertex = v.vertex * _Size * (1 + ((_OutlineSize > 0) * _OutlineSize));
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.viewPos = UnityObjectToViewPos(v.vertex);
                o.points = GetPointsInView();
                o.uv = v.uv;
                return o;
            }

            float h(float3 a, float3 b)
            {
                return (b.y - a.y)/(b.x - a.x);
            }
            
            float f(float3 a, float3 b, float2 iPos)
            {
                return iPos.y - a.y - (h(a, b) * (iPos.x - a.x));
            }

            float g(int a, int b, Interpolators i)
            {
                return f(i.points.v[a], i.points.v[b], i.viewPos);
            }

            float Max(float3 v[8])
            {
                return max(v[0], max(v[1], max(v[2], v[3])));
            }

            float Min(float3 v[8])
            {
                return min(v[0], min(v[1], min(v[2], v[3])));
            }

            fixed4 frag (Interpolators i) : SV_Target
            {            
                float t01 = g(0, 1, i);
                float t23 = g(2, 3, i);
                float b45 = g(4, 5, i);
                float b67 = g(6, 7, i);

                float t03 = g(0, 3, i);
                float t21 = g(2, 1, i);
                float b47 = g(4, 7, i);
                float b65 = g(6, 5, i);

                float lines =
                    (t01 > 0 && t23 > 0 && b45 > 0 && b67 > 0) ||
                    (t03 > 0 && t21 > 0 && b47 > 0 && b65 > 0) ||
                    (t01 < 0 && t23 < 0 && b45 < 0 && b67 < 0) ||
                    (t03 < 0 && t21 < 0 && b47 < 0 && b65 < 0) ||
                    (i.viewPos.x > Max(i.points.v)) ||
                    (i.viewPos.x < Min(i.points.v));
                
                clip(lines - 0.1);

                return _Outline;
            }
            ENDCG
        }
    }
}
