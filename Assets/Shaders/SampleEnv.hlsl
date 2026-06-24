void SampleEnv_float(float3 ReflectDir, float Roughness, out float3 Color)
{
    Color = GlossyEnvironmentReflection(ReflectDir, Roughness, 1.0);
}