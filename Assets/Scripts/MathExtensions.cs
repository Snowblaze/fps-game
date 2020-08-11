using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

public static class MathExtensions
{
    public static quaternion Concatenate(quaternion lhs, quaternion rhs)
    {
        float3 lhsXYZ = lhs.value.xyz;
        float3 rhsXYZ = rhs.value.xyz;
        float lhsW = lhs.value.w;
        float rhsW = rhs.value.w;

        float3 axis = lhsW * rhsXYZ + rhsW * lhsXYZ + math.cross(lhsXYZ, rhsXYZ);
        float scalar = lhsW * rhsW - math.dot(lhsXYZ, rhsXYZ);

        return new quaternion(axis.x, axis.y, axis.z, scalar);
    }
}
