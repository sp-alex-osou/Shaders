float4x4 World;
float4x4 View;
float4x4 Projection;

texture Texture;
float Time;
float Velocity;
float Acceleration;
float Duration;
bool WireFrame;

sampler2D TextureSampler = sampler_state { 
	texture = <Texture>; 
	MagFilter = anisotropic;
	MinFilter = anisotropic;
	MipFilter = linear;
	AddressU = clamp; 
	AddressV = clamp;
};

struct VertexShaderInput
{
    float3 Position	: POSITION0;
	 float2 TexCoord	: TEXCOORD0;
	 float3 Direction	: TEXCOORD1;
	 float Time			: TEXCOORD2;
};

struct VertexShaderOutput
{
    float4 Position	: POSITION0;
	 float2 TexCoord	: TEXCOORD0;
	 float Age			: TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	// Alter des Partikels
	float age = Time - input.Time;

	// Position mit World Matrix multiplizieren
	float4 worldPosition = mul(float4(input.Position, 1), World);

	// aktuelle Entfernung zum Mittelpunkt bestimmen und mit Partikel-Richtung multiplizieren
	// s = (v * t) + ((a / 2) * t²)
	worldPosition += float4(input.Direction, 0) * (Velocity * age + (Acceleration / 2) * age * age);

	// View und Projection Matrix Multiplikation
	output.Position = mul(mul(worldPosition, View), Projection); 
	output.TexCoord = input.TexCoord;
	output.Age = age;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.TexCoord);

	// Alpha Wert aufgrund des Alters anpassen (Alpha 0 am Ende der Lebensdauer)
	color.a *= 1.0f - (input.Age / Duration);

	return color + float4(1, 1, 1, 1) * WireFrame;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
