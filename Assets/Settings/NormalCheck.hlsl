#ifndef CUSTOM_FUNCTIONS_INCLUDED
#define CUSTOM_FUNCTIONS_INCLUDED


void CheckAllAxesAbs_float(float3 Normal, float3 ReplacementColor, float3 BaseColor, float Threshold, out float3 Output)
{
    #ifdef SHADERGRAPH_PREVIEW
    Output = BaseColor;
    #else
    if (abs(Normal.x) > Threshold || abs(Normal.y) > Threshold || abs(Normal.z) > Threshold)
    {
        Output = ReplacementColor;
    }
    else
    {
        Output = BaseColor;
    }
    #endif
}


#endif
