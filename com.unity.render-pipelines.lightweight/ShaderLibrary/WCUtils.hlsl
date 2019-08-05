#ifndef WC_Utils_INCLUDED
#define WC_Utils_INCLUDED

half DesaturateColor(half3 c)
{
    return dot(clamp(c, 0, 1), half3(0.3, 0.6, 0.1));
}

struct CustomLightInput
{
    half diluteArea;
    half cangiante;
    half cangianteAera;
    half dilutePaint;
    half highlightRollOff;
    half highlightTransparency;
    half diffuseFactor;
    half shadeWrap;
    half3 diluteColor;
    half3 shadeColor;
};

float4 unpack(float2 xy)
{
    //but theny will never be negetive
    //and also it will
    //xy = xy*4096.0*4096.0;
    return fmod(float4(xy.x / 4096.0, xy.x, xy.y / 4096.0, xy.y)*4096.0*4096.0, 4096.0)/4096;
    //return float4(floor(xy.x/4096.0),fmod(xy.x,4096.0),floor(xy.y/4096.0),fmod(xy.y,4096.0))/4095.0;
}

half Linstep(half minV, half maxV, half value)
{
    return saturate((value - minV) / (maxV - minV));
}
// Original by: Ian McEwan, Ashima Arts.
//     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
//               Distributed under the MIT License. See LICENSE file: http://opensource.org/licenses/MIT
//               https://github.com/ashima/webgl-noise
// 
float3 SimplexNoise_mod289(float3 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float4 SimplexNoise_mod289(float4 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float2 SimplexNoise_mod289(float2 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

struct SimplexNoiseOutput
{
    float noise;
};

float3 SimplexNoise2D_permute(float3 x)
{
    return SimplexNoise_mod289(((x*34.0) + 1.0)*x);
}

float SimplexNoise2D_snoise(float2 v, float brightness)
{
    const float4 C = float4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
        0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
        -0.577350269189626,  // -1.0 + 2.0 * C.x
        0.024390243902439); // 1.0 / 41.0
    // First corner
    float2 i = floor(v + dot(v, C.yy));
    float2 x0 = v - i + dot(i, C.xx);

    // Other corners
    float2 i1;
    //i1.x = step( x0.y, x0.x ); // x0.x > x0.y ? 1.0 : 0.0
    //i1.y = 1.0 - i1.x;
    i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
    // x0 = x0 - 0.0 + 0.0 * C.xx ;
    // x1 = x0 - i1 + 1.0 * C.xx ;
    // x2 = x0 - 1.0 + 2.0 * C.xx ;
    float4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;

    // Permutations
    i = SimplexNoise_mod289(i); // Avoid truncation effects in permutation
    float3 p = SimplexNoise2D_permute(SimplexNoise2D_permute(i.y + float3(0.0, i1.y, 1.0))
        + i.x + float3(0.0, i1.x, 1.0));

    float3 m = max(0.5 - float3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), 0.0);
    m = m * m;
    m = m * m;

    // Gradients: 41 points uniformly over a line, mapped onto a diamond.
    // The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

    float3 x = 2.0 * frac(p * C.www) - 1.0;
    float3 h = abs(x) - 0.5;
    float3 ox = floor(x + 0.5);
    float3 a0 = x - ox;

    // Normalise gradients implicitly by scaling m
    // Approximation of: m *= inversesqrt( a0*a0 + h*h );
    m *= 1.79284291400159 - 0.85373472095314 * (a0*a0 + h * h);

    // Compute final noise value at P
    float3 g;
    g.x = a0.x  * x0.x + h.x  * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return brightness * dot(m, g);
}



float4 SimplexNoise3D_permute(float4 x)
{
    return SimplexNoise_mod289(((x*34.0) + 1.0)*x);
}

float4 SimplexNoise3D_taylorInvSqrt(float4 r)
{
    return 1.79284291400159 - 0.85373472095314 * r;
}

float SimplexNoise3D_snoise(float3 v, float brightness)
{
    const float2  C = float2(1.0 / 6.0, 1.0 / 3.0);
    const float4  D = float4(0.0, 0.5, 1.0, 2.0);

    // First corner
    float3 i = floor(v + dot(v, C.yyy));
    float3 x0 = v - i + dot(i, C.xxx);

    // Other corners
    float3 g = step(x0.yzx, x0.xyz);
    float3 l = 1.0 - g;
    float3 i1 = min(g.xyz, l.zxy);
    float3 i2 = max(g.xyz, l.zxy);

    //   x0 = x0 - 0.0 + 0.0 * C.xxx;
    //   x1 = x0 - i1  + 1.0 * C.xxx;
    //   x2 = x0 - i2  + 2.0 * C.xxx;
    //   x3 = x0 - 1.0 + 3.0 * C.xxx;
    float3 x1 = x0 - i1 + C.xxx;
    float3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
    float3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

    // Permutations
    i = SimplexNoise_mod289(i);
    float4 p = SimplexNoise3D_permute(SimplexNoise3D_permute(SimplexNoise3D_permute(
        i.z + float4(0.0, i1.z, i2.z, 1.0))
        + i.y + float4(0.0, i1.y, i2.y, 1.0))
        + i.x + float4(0.0, i1.x, i2.x, 1.0));

    // Gradients: 7x7 points over a square, mapped onto an octahedron.
    // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
    float n_ = 0.142857142857; // 1.0/7.0
    float3  ns = n_ * D.wyz - D.xzx;

    float4 j = p - 49.0 * floor(p * ns.z * ns.z);  //  mod(p,7*7)

    float4 x_ = floor(j * ns.z);
    float4 y_ = floor(j - 7.0 * x_);    // mod(j,N)

    float4 x = x_ * ns.x + ns.yyyy;
    float4 y = y_ * ns.x + ns.yyyy;
    float4 h = 1.0 - abs(x) - abs(y);

    float4 b0 = float4(x.xy, y.xy);
    float4 b1 = float4(x.zw, y.zw);

    //float4 s0 = float4(lessThan(b0,0.0))*2.0 - 1.0;
    //float4 s1 = float4(lessThan(b1,0.0))*2.0 - 1.0;
    float4 s0 = floor(b0)*2.0 + 1.0;
    float4 s1 = floor(b1)*2.0 + 1.0;
    float4 sh = -step(h, float4(0, 0, 0, 0));

    float4 a0 = b0.xzyw + s0.xzyw*sh.xxyy;
    float4 a1 = b1.xzyw + s1.xzyw*sh.zzww;

    float3 p0 = float3(a0.xy, h.x);
    float3 p1 = float3(a0.zw, h.y);
    float3 p2 = float3(a1.xy, h.z);
    float3 p3 = float3(a1.zw, h.w);

    //Normalise gradients
    float4 norm = SimplexNoise3D_taylorInvSqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
    p0 *= norm.x;
    p1 *= norm.y;
    p2 *= norm.z;
    p3 *= norm.w;

    // Mix final noise value
    float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
    m = m * m;
    return brightness * dot(m*m, float4(dot(p0, x0), dot(p1, x1),
        dot(p2, x2), dot(p3, x3)));
}
//W is default to _time
SimplexNoiseOutput SimplexNoise3DFunc(float2 UV, float W, float brightness)
{
    SimplexNoiseOutput OUT;
    OUT.noise = SimplexNoise3D_snoise(float3(UV, W), brightness);
    return OUT;
}

SimplexNoiseOutput SimplexNoise2DFunc(float2 UV, float brightness)
{
    SimplexNoiseOutput OUT;
    OUT.noise = SimplexNoise2D_snoise(UV, brightness);
    return OUT;
}
#endif
