sampler textureSampler;
int radius;
float gameTime;
int playerX;
int playerY;

float4 scream(float2 coords: TEXCOORD) : COLOR
{
	float2 position = float2(coords.x * 1920.0, coords.y * 1080.0);
	float4 color = tex2D(textureSampler, coords);
	float dist = distance(position,float2(playerX, playerY));
	float space = 200.0 + sin(dist * 0.5); //100.0;
	float thickness = 5.0;
	float speed = 0.5;
	float time = (gameTime + 30000.0);
	float intensity = fmod(dist - (radius * 0.1)  - fmod(time * speed, space) + space, space) < thickness * 10.;
	float3 newColor = clamp(float3(color.rgb + 0.3), color.rgb, intensity);

	float fallOff = (dist * 2.)/(radius * 2.5 + 550.0);
	float3 bwColor = clamp(color.rgb - 0.2, newColor.rgb, 1.-fallOff);
	bwColor = clamp(tex2D(textureSampler, coords).rgb - 0.2, bwColor.rgb, 1.-bwColor.rgb);
	
	color = float4(bwColor,tex2D(textureSampler, coords).a);
	
	
	return color;
}
  
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 scream();
    }  
}  