using UnityEngine;

namespace NGIMU.Scripts
{
    internal static class NgimuMathUtils
    {
        public static Quaternion NgimuToUnityQuaternion(NgimuApi.Maths.Quaternion quaternion)
        {
            NgimuApi.Maths.Quaternion ngimuQuaternion = NgimuApi.Maths.Quaternion.Normalise(NgimuApi.Maths.Quaternion.Conjugate(quaternion));

            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, new Quaternion(ngimuQuaternion.X, ngimuQuaternion.Y, ngimuQuaternion.Z, ngimuQuaternion.W), Vector3.one);

            Vector4 yColumn = matrix.GetColumn(1);
            Vector4 zColumn = matrix.GetColumn(2);

            matrix.SetColumn(1, zColumn);
            matrix.SetColumn(2, yColumn);

            Vector4 yRow = matrix.GetRow(1);
            Vector4 zRow = matrix.GetRow(2);

            matrix.SetRow(1, zRow);
            matrix.SetRow(2, yRow);

            return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        }
    }
}