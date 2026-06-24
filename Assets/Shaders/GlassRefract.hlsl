#ifndef GLASS_REFRACT_INCLUDED
#define GLASS_REFRACT_INCLUDED

void GlassRefract_float(
    float3 WorldPos, float3 WorldNormal, float3 ViewDirWS,
    float3 SphereCenter, float SphereRadius, float IOR, float RefractDist,
    bool IsFront,
    out float2 RefractUV, out float Chord)
{
    float3 V = normalize(ViewDirWS); // surface -> camera
    float3 I = -V; // camera -> surface (incident)
    float3 N = normalize(WorldNormal); // authored outward normal
    float3 exitPos;

    if (IsFront)
    {
        // Outside view: full ball lens. Air -> medium at front surface.
        float eta = 1.0 / IOR;
        float3 d1 = refract(I, N, eta);
        if (all(d1 == 0))
            d1 = I; // TIR guard

        // Far intersection = back surface of the sphere (closed form).
        float3 L = WorldPos - SphereCenter;
        float b = dot(d1, L);
        float c = dot(L, L) - SphereRadius * SphereRadius;
        float t = -b + sqrt(max(b * b - c, 0.0));
        float3 Pb = WorldPos + d1 * t;
        Chord = t; // path length through medium

        // Medium -> air at back surface; flip outward normal to face the ray.
        float3 Nb = normalize(Pb - SphereCenter);
        float3 d2 = refract(d1, -Nb, IOR);
        if (all(d2 == 0))
            d2 = d1;
        exitPos = Pb + d2 * RefractDist;
    }
    else
    {
        // Inside view: camera is in the medium. Single boundary, medium -> air.
        // We see the inner side of the shell, so flip the authored normal.
        float3 Ni = -N;
        float3 d = refract(I, Ni, IOR);
        if (all(d == 0))
            d = I;
        Chord = 0.0;
        exitPos = WorldPos + d * RefractDist;
    }

    float4 clip = TransformWorldToHClip(exitPos);
    float2 uv = clip.xy / clip.w * 0.5 + 0.5;
#if UNITY_UV_STARTS_AT_TOP
        uv.y = 1.0 - uv.y;
#endif
    RefractUV = uv;
}
#endif