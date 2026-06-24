#ifndef CLOUD_RAYMARCH_INCLUDED
#define CLOUD_RAYMARCH_INCLUDED

float4 _FireflyPositions[8];
float4 _FireflyColors[8];
int    _FireflyCount;
float  _FireflyCoreSoftness;

float _CR_Hash(float3 p)
{
    p = frac(p * 0.3183099 + 0.1);
    p *= 17.0;
    return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
}

float _CR_ValueNoise(float3 p)
{
    float3 i = floor(p);
    float3 f = frac(p);
    float3 u = f * f * (3.0 - 2.0 * f);
    return lerp(
        lerp(lerp(_CR_Hash(i),                _CR_Hash(i + float3(1,0,0)), u.x),
             lerp(_CR_Hash(i + float3(0,1,0)), _CR_Hash(i + float3(1,1,0)), u.x), u.y),
        lerp(lerp(_CR_Hash(i + float3(0,0,1)), _CR_Hash(i + float3(1,0,1)), u.x),
             lerp(_CR_Hash(i + float3(0,1,1)), _CR_Hash(i + float3(1,1,1)), u.x), u.y),
        u.z);
}

float _CR_FBM(float3 p)
{
    float v = 0.0;
    float a = 0.5;
    for (int i = 0; i < 4; i++)
    {
        v += a * _CR_ValueNoise(p);
        p = p * 2.0 + float3(5.2, 1.3, 4.7);
        a *= 0.5;
    }
    return v;
}

float _CR_Density(float3 worldPos, float3 objectPos, float cloudScale, float cosT, float sinT)
{
    float3 localPos = worldPos - objectPos;
    float3 rotatedPos = float3(
        localPos.x * cosT + localPos.z * sinT,
        localPos.y,
        -localPos.x * sinT + localPos.z * cosT);
    return max(0.0, _CR_FBM(rotatedPos * cloudScale) - 0.4);
}

void CloudRaymarch_float(
    float3 worldPos,
    float3 cameraWS,
    float3 objectPos,
    float  sphereRadius,
    float  cloudScale,
    float  cloudDensity,
    float4 cloudColor,
    float  time,
    float  scatterStrength,
    out float3 outColor,
    out float  outAlpha)
{
    outColor = cloudColor.rgb;
    outAlpha = 0.0;

    float3 rayDir = normalize(worldPos - cameraWS);

    // Cast from camera so both near and far cloud layers are visible
    float3 oc   = cameraWS - objectPos;
    float  b    = dot(oc, rayDir);
    float  c    = dot(oc, oc) - sphereRadius * sphereRadius;
    float  disc = b * b - c;
    if (disc < 0.0) return;
    float sqrtDisc = sqrt(disc);
    float tNear = max(0.001, -b - sqrtDisc);
    float tFar  = -b + sqrtDisc;
    if (tFar <= 0.0) return;

    const int steps = 32;
    float stepSize = (tFar - tNear) / float(steps);
    float3 accumColor = float3(0.0, 0.0, 0.0);
    float  accumAlpha = 0.0;

    [loop]
    for (int s = 0; s < steps; s++)
    {
        float t = tNear + (float(s) + 0.5) * stepSize;
        float3 samplePosWS = cameraWS + rayDir * t;

        float cosT = cos(time);
        float sinT = sin(time);

        float density = _CR_Density(samplePosWS, objectPos, cloudScale, cosT, sinT);
        float d = density * cloudDensity * stepSize;

        if (d > 0.0)
        {
            // --- single-tap directional self-shadow ---
            const float kShadowStrength = 4.0;
            const float kAmbient        = 0.15;
            float shadowDist = sphereRadius * 0.25;

            float3 lightDir = _MainLightPosition.xyz;
            float densToLight = _CR_Density(samplePosWS + lightDir * shadowDist,
                                            objectPos, cloudScale, cosT, sinT)
                                * cloudDensity * shadowDist;
            float shadow = exp(-densToLight * kShadowStrength);
            float3 sun   = _MainLightColor.rgb * shadow;

            float3 stepColor = cloudColor.rgb * (kAmbient + sun);

            for (int li = 0; li < _FireflyCount; li++)
            {
                float3 toLight   = _FireflyPositions[li].xyz - samplePosWS;
                float  distSqr   = dot(toLight, toLight);
                float  range     = _FireflyPositions[li].w;
                float  intensity = _FireflyColors[li].w;

                float  factor       = distSqr / max(range * range, 1e-4);
                float  smoothFactor = saturate(1.0 - factor * factor);
                smoothFactor       *= smoothFactor;
                float  atten        = smoothFactor / max(distSqr, _FireflyCoreSoftness);

                stepColor += _FireflyColors[li].rgb * atten * intensity * scatterStrength;
            }

            float weight = d * (1.0 - accumAlpha);
            accumColor  += stepColor * weight;
            accumAlpha  += weight;
        }

        if (accumAlpha >= 0.99) break;
    }

    outAlpha = saturate(accumAlpha);
    outColor = accumColor / max(outAlpha, 0.001);
}

#endif
