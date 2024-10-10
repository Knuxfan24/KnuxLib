namespace KnuxLib.Helpers
{
    public class RotationHelpers
    {
        /// <summary>
        /// Converts a Quaternion to a Vector3 storing the equivalent Euler angles.
        /// Taken from HedgeLib#, with a credit link for http://quat.zachbennett.com/ (which is now dead).
        /// </summary>
        /// <param name="quaternion">The quaternion to convert.</param>
        /// <param name="radians">Whether or not to return the results as Radians instead of Euler.</param>
        public static Vector3 ConvertQuaternionToEuler(Quaternion quaternion, bool radians = false)
        {
            float qx2 = quaternion.X * quaternion.X;
            float qy2 = quaternion.Y * quaternion.Y;
            float qz2 = quaternion.Z * quaternion.Z;
            float negativeChecker = quaternion.X * quaternion.Y + quaternion.Z * quaternion.W;

            if (negativeChecker > 0.499)
                return GetVect(0, 360 / Math.PI * Math.Atan2(quaternion.X, quaternion.W), 90);

            if (negativeChecker < -0.499)
                return GetVect(0, -360 / Math.PI * Math.Atan2(quaternion.X, quaternion.W), -90);

            double h = Math.Atan2(2 * quaternion.Y * quaternion.W - 2 * quaternion.X * quaternion.Z, 1 - 2 * qy2 - 2 * qz2);
            double a = Math.Asin(2 * quaternion.X * quaternion.Y + 2 * quaternion.Z * quaternion.W);
            double b = Math.Atan2(2 * quaternion.X * quaternion.W - 2 * quaternion.Y * quaternion.Z, 1 - 2 * qx2 - 2 * qz2);

            return GetVect(Math.Round(b * 180 / Math.PI), Math.Round(h * 180 / Math.PI), Math.Round(a * 180 / Math.PI));

            // Sub-Methods
            Vector3 GetVect(double x, double y, double z)
            {
                float multi = (radians) ? 0.0174533f : 1;
                return new Vector3((float)x * multi,
                    (float)y * multi, (float)z * multi);
            }
        }

        /// <summary>
        /// Converts a signed integer stored in the Binary Angle Measurement System to a floating point value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static float CalculateBAMsValue(int value) => value * 360f / 65536f;

        /// <summary>
        /// Converts a floating point value to a signed integer stored in the Binary Angle Measurement System.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static int CalculateBAMsValue(float value) => (int)(value * 65536f / 360f);
    }
}
