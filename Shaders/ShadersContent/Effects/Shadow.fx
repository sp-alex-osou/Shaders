#define MAX_LIGHTS 3

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightPosition;

float ShadowStart;
float ShadowEnd;

texture Shadowmap;
texture Lightmap;

bool SoftShadowEnabled;
bool BlurVertical;

samplerCUBE ShadowmapSampler = sampler_state {
	texture = <Shadowmap>; 
	MagFilter = point;
	MinFilter = point;
	MipFilter = point;
};

sampler2D LightmapSampler = sampler_state {
	texture = <Lightmap>; 
	MagFilter = point;
	MinFilter = point;
	MipFilter = point;
};


// RENDER SHADOWMAP

struct SMVertexShaderOutput
{
    float4 Position			: POSITION0;
    float Depth				: TEXCOORD0;
};


// VertexShader zum Rendern der Shadowmap
SMVertexShaderOutput SMVertexShaderFunction(float4 position : POSITION0)
{
    SMVertexShaderOutput output;

	 float4 worldPosition = mul(position, World);

    output.Position = mul(mul(worldPosition, View), Projection);

	 // Distanz zwischen Vertex und Licht ermitteln
    output.Depth = distance(worldPosition, LightPosition);

    return output;
}


// PixelShader zum Rendern der Shadowmap
float4 SMPixelShaderFunction(SMVertexShaderOutput input) : COLOR0
{       
	// Distanz zwischen Pixel und Licht in der ShadowMap speichern
	return saturate((input.Depth - ShadowStart) / (ShadowEnd - ShadowStart));
}



// RENDER LIGHTMAP

struct LMVertexShaderInput
{
	float4 Position	: POSITION0;
	float3 Normal		: NORMAL0;
	float3 Tangent		: TANGENT0;
};

struct LMVertexShaderOutput
{
   float4 Position			: POSITION0;
   float4 WorldPosition		: TEXCOORD0;
	float3 Normal				: TEXCOORD1;
	float3 Binormal			: TEXCOORD2;
	float3 Tangent				: TEXCOORD3;
};


// VertexShader zum Rendern der Lightmap
LMVertexShaderOutput LMVertexShaderFunction(LMVertexShaderInput input)
{
   LMVertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);

   output.Position = mul(mul(worldPosition, View), Projection);
	output.WorldPosition = worldPosition;
	output.Tangent = mul(input.Tangent, World);
	output.Normal = mul(input.Normal, World);
	output.Binormal = cross(output.Tangent, output.Normal);

	return output;
}


// PixelShader zum Rendern der Lightmap
float4 LMPixelShaderFunction(LMVertexShaderOutput input) : COLOR0
{       
	float3 tangent = normalize(input.Tangent);
	float3 binormal = normalize(input.Binormal);
	float3 normal = normalize(input.Normal);

	// Lookup-Vektor für den Shadow-Cube
	float3 lookup = normalize(input.WorldPosition - LightPosition);

	// z Komponenten invertieren (Left-Handed vs. Right-Handed)
	lookup.z *= -1.0f;

	float light = 0.0f;

	// Distanz zwischen Pixel und Light bestimmen
	float depthInScene = saturate((distance(input.WorldPosition, LightPosition) - ShadowStart) / (ShadowEnd - ShadowStart));

	// PCF Kernel initialisieren
	float3 filters[9] = { 
		float3(0, 0, 0), 
		tangent,
		-tangent,
		binormal,
		-binormal,
		tangent + binormal,
		tangent - binormal,
		-tangent + binormal,
		-tangent - binormal
	};

	float softness = 0.01f;
	
	int numFilters = (SoftShadowEnabled) ? 9 : 1;

	// ein Lookup im Shadow Cube pro Filter
	for (int j = 0; j < numFilters; ++j)
	{
		float depthInTexture = texCUBE(ShadowmapSampler, lookup + filters[j] * softness).r;

		// Wert in Shadow Cube mit zuvor berechnetem Wert vergleichen
		light += (depthInScene < depthInTexture + 0.001f) ? 1.0f : 0.0f;
	}

	// Lichtwert durch Anzahl an Filtern dividieren
	return light / numFilters;
}



// BLUR LIGHTMAP


struct BlurVertexShaderInput
{
    float4 Position	: POSITION0;
    float2 TexCoord	: TEXCOORD0;
};


struct BlurVertexShaderOutput
{
    float4 Position	: POSITION0;
    float2 TexCoord	: TEXCOORD0;
};


float BlurWeights[13] = 
{
    0.002216,
    0.008764,
    0.026995,
    0.064759,
    0.120985,
    0.176033,
    0.199471,
    0.176033,
    0.120985,
    0.064759,
    0.026995,
    0.008764,
    0.002216,
};


// VertexShader für Lightmap Blur
BlurVertexShaderOutput BlurVertexShaderFunction(BlurVertexShaderInput input)
{
    BlurVertexShaderOutput output;

    output.Position = input.Position;
    output.TexCoord = input.TexCoord;

    return output;
}


// PixelShader für Lightmap Blur
float4 BlurPixelShaderFunction(BlurVertexShaderOutput input) : COLOR0
{      
   float light = 0.0f;
	float blur = 256.0f;
	float2 direction = (BlurVertical) ? float2(0.0f, 1.0f) : float2(1.0f, 0.0f);

	for (int i = 0; i < 13; i++) {
		light += tex2D(LightmapSampler, input.TexCoord + direction * (i - 6.0f) / blur) * BlurWeights[i];
	}

   return light;
}


// TECHNIQUES

technique RenderShadowmap
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 SMVertexShaderFunction();
		PixelShader = compile ps_3_0 SMPixelShaderFunction();
	}
}


technique RenderLightmap
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 LMVertexShaderFunction();
		PixelShader = compile ps_3_0 LMPixelShaderFunction();
	}
}


technique BlurLightmap
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 BlurVertexShaderFunction();
		PixelShader = compile ps_3_0 BlurPixelShaderFunction();
	}
}