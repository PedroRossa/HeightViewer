// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Cg  shader for billboards" {
	Properties{
	   _MainTex("Texture Image", 2D) = "white" {}
	   _Scale("Scale", Float) = 1.0
		[Toggle(Disable_Look_At_Cam)]
	   _DisableLookAtCam("DisableLookAtCam",Float) = 0
	   _GrayEffectAmount("Gray Effect Amount",Float) = 0.0

	}
		SubShader{
			Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "DisableBatching" = "True"}
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		   Pass {
			  CGPROGRAM

			  #include "UnityCG.cginc"
			  #include "TerrainEngine.cginc"

			  #pragma vertex vert  
			  #pragma fragment frag

			  // User-specified uniforms            
			  uniform sampler2D _MainTex;
			  uniform float _Scale;
			  uniform bool _DisableLookAtCam;
			  uniform float _GrayEffectAmount;

			  struct vertexInput {
				  float4 vertex : POSITION;
				  float4 tangent : TANGENT;
				  float4 tex : TEXCOORD0;
				  float4 color    : COLOR;
			  };
			  struct vertexOutput {
				  float4 pos : SV_POSITION;
				  float4 tangent : TANGENT;
				  float4 tex : TEXCOORD0;
			  };


			  vertexOutput vert(vertexInput input)
			  {
				 vertexOutput output;

				 if (_DisableLookAtCam) {
					 output.pos = UnityObjectToClipPos(input.vertex * float4(_Scale, _Scale, _Scale, _Scale));

					 output.tex = input.tex;

					 return output;
				  }


					 float3 localSpaceCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1));
					 float3 camVect = normalize(input.vertex - localSpaceCameraPos);
					 float3 up = float3(0, 1, 0);


					 float3 zaxis = camVect;
					 float3 xaxis = normalize(cross(up, zaxis));
					 float3 yaxis = cross(zaxis, xaxis);

					 float4x4 lookatMatrix = {
						 xaxis.x,            yaxis.x,            zaxis.x,       0,
						 xaxis.y,            yaxis.y,            zaxis.y,       0,
						 xaxis.z,            yaxis.z,            zaxis.z,       0,
							   0,                  0,                  0,       1
					 };

					 // Apply LookAt matrix
					  output.pos = mul(lookatMatrix, input.vertex.xyz);

					  // Apply MVP matrix to model
					  output.pos = UnityObjectToClipPos(output.pos * float4(_Scale, _Scale, _Scale, _Scale));

					  output.tex = input.tex;

					  return output;

				   }

				   float4 frag(vertexOutput input) : COLOR
				   {
							  half4 texcol = tex2D(_MainTex, input.tex);
							  texcol.rgb = lerp(texcol.rgb, dot(texcol.rgb, float3(0.3, 0.59, 0.11)), _GrayEffectAmount);
							 
							  return texcol;
							// return tex2D(_MainTex, float2(input.tex.xy));
				   }

				   ENDCG
				}
	   }
}