#define MAX_LIGHTS 3

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 CameraPosition;

float3 LightDirections[MAX_LIGHTS];
float3 LightPositions[MAX_LIGHTS];
float3 LightColors[MAX_LIGHTS];
float LightCount;

texture Texture;
texture Normalmap;
texture Heightmap;

texture Lightmap0;
texture Lightmap1;
texture Lightmap2;

bool NormalmapEnabled;
bool SpotLightEnabled;
bool ShadowEnabled;
bool HeightmapEnabled;


sampler2D NormalmapSampler = sampler_state { 
	texture = <Normalmap>; 
	MagFilter = anisotropic;
	MinFilter = anisotropic;
	MipFilter = linear;
	AddressU = wrap; 
	AddressV = wrap;
};


sampler2D TextureSampler = sampler_state { 
	texture = <Texture>; 
	MagFilter = anisotropic;
	MinFilter = anisotropic;
	MipFilter = linear;
	AddressU = wrap; 
	AddressV = wrap;
};


sampler2D HeightmapSampler = sampler_state { 
	texture = <Heightmap>; 
	MagFilter = anisotropic;
	MinFilter = anisotropic;
	MipFilter = linear;
	AddressU = wrap; 
	AddressV = wrap;
};


sampler2D LightmapSamplers[MAX_LIGHTS] = {
	sampler_state { 
		texture = <Lightmap0>;
		MagFilter = point;
		MinFilter = point;
		MipFilter = point;
	},
	sampler_state { 
		texture = <Lightmap1>; 
		MagFilter = point;
		MinFilter = point;
		MipFilter = point;
	},
	sampler_state { 
		texture = <Lightmap2>; 
		MagFilter = point;
		MinFilter = point;
		MipFilter = point;
	}
};



// RENDER SCENE

struct VertexShaderInput
{
	float4 Position	: POSITION0;
	float2 TexCoord	: TEXCOORD0;
	float3 Normal		: NORMAL0;
	float3 Tangent		: TANGENT0;
};


struct VertexShaderOutput
{
	float4 Position			: POSITION0;
	float2 TexCoord			: TEXCOORD0;
	float3 Normal				: TEXCOORD1;
	float3 Binormal			: TEXCOORD2;
	float3 Tangent				: TEXCOORD3;
	float3 View					: TEXCOORD4;
	float4 WorldPosition		: TEXCOORD5;
	float4 ModelPosition		: TEXCOORD6;
	float4 ScreenPosition	: TEXCOORD7;
};


// VertexShader zum Rendern der Szene
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);

	output.Position = mul(mul(worldPosition, View), Projection);
	output.ScreenPosition = output.Position;
	output.WorldPosition = worldPosition;
	output.ModelPosition = input.Position;
	output.TexCoord = input.TexCoord * 10.0f;
	output.View = CameraPosition - worldPosition;
	output.Tangent = mul(input.Tangent, World);
	output.Normal = mul(input.Normal, World);
	output.Binormal = cross(output.Tangent, output.Normal);

	return output;
}


// Funktion für Pixel Shader Displacement Mapping
float2 DisplacementMapping(float2 texCoord, float3 view, float3x3 worldToTangent)
{
	// 3D Textur Koordinaten (z mit 1 initialisieren)
	float3 sampleCoord = float3(texCoord, 1.0f);

	// View Vektor in den Tangent Space transformieren
	float3 tanView = mul(view, worldToTangent);

	// Offset-Vektor für die Textur-Koordinaten (z = -1)
	float3 offset = -tanView / tanView.z;

	// Offset-Vektor skalieren
	offset.xy *= 0.01f;

	int i, steps = 10;

	// Offset-Vektor durch Anzahl an Stufen divieren
	offset /= steps;

	// linear search (offset addieren wenn height-value kleiner als aktuelle height)
	for (i = 0; i < steps; ++i)
		sampleCoord += offset * (tex2D(HeightmapSampler, sampleCoord.xy).r < sampleCoord.z);
			
	// binary search (offset in jedem Schritt verkleinern, Richtung der Suche abhängig von letzem Vergleich)
	for (i = 0; i < steps; ++i)
		sampleCoord += offset * pow(0.5f, i+1) * ((tex2D(HeightmapSampler, sampleCoord.xy).r < sampleCoord.z) * 2 - 1);

	// neue Texturkoordinaten zurückliefern
	return sampleCoord.xy;
}


// PixelShader zum Rendern der Szene
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 tangent = normalize(input.Tangent);
	float3 binormal = normalize(input.Binormal);
	float3 normal = normalize(input.Normal);
	float3 view = normalize(input.View);
	float2 texCoord = input.TexCoord;

	float3x3 worldToTangent = transpose(float3x3(tangent, -binormal, normal));
	float3x3 tangentToWorld = float3x3(tangent, binormal, normal);

	// wenn Displacement Mapping aktiviert -> neue Texturkoordinaten ermitteln
	if (HeightmapEnabled)
		texCoord = DisplacementMapping(texCoord, view, worldToTangent);

	// Farbwert aus Textur auslesen
	float3 color = tex2D(TextureSampler, texCoord);

	// wenn Normal Mapping aktiviert -> Normalvektor aus Normalmap ermitteln und in World Space transformieren
	if (NormalmapEnabled) 
		normal = normalize(mul(tex2D(NormalmapSampler, texCoord) * 2.0f - 1.0f, tangentToWorld));

	// Cutoff bestimmen
	float cutoffTreshold = (SpotLightEnabled) ? 0.9f : 0.0f;

	float3 diffuse = 0.0f;
	float3 specular = 0.0f;

	// Screen Koordinaten ermitteln (0 bis 1)
	float2 screenCoord = (input.ScreenPosition.xy / input.ScreenPosition.w) / 2.0f + 0.5f;
	screenCoord.y = 1.0f - screenCoord.y;

	// für jedes Licht
	[unroll]
	for (int i = 0; i < LightCount; ++i)
	{
		// Light und Reflection Vector ermitteln
		float3 light = normalize(LightPositions[i] - input.WorldPosition);
		float3 reflection = reflect(-light, normal);

		// wenn Schatten aktiviert -> Lightmap auslesen
		float shadow = (ShadowEnabled) ? tex2D(LightmapSamplers[i], screenCoord).r : 1.0f;

		// Cutoff überprüfen (für Spotlight)
		float cutoff = saturate(dot(normalize(light), normalize(-LightDirections[i]))) >= cutoffTreshold;

		// Light Attenuation berechnen
		float attenuation = saturate(1.0f - length(LightPositions[i] - input.WorldPosition) / 10000.0f);

		// diffuse und specular light berechnen
		diffuse += LightColors[i] * saturate(dot(normal, light)) * attenuation * cutoff * shadow;
		specular += LightColors[i] * pow(saturate(dot(view, reflection)), 25.0f) * attenuation * cutoff * shadow;
	}

	float3 ambient = 0.05f;

	// Lichtwerte addieren
	color *= saturate(ambient + diffuse) + specular;

	return float4(color, 1.0f);
}



// TECHNIQUES

technique RenderScene
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}