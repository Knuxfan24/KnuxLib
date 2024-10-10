namespace KnuxLib.Helpers
{
    public class SplineHelpers
    {
        /// <summary>
        /// Calculate the distance between two points in 3D space.
        /// </summary>
        /// <param name="pointA">The first point.</param>
        /// <param name="pointB">The second point.</param>
        /// <returns>The calculated distance between the two.</returns>
        public static float CalculateDistance(Vector3 pointA, Vector3 pointB)
        {
            // Subtract the values in the first point from the values in the second.
            float x = pointB.X - pointA.X;
            float y = pointB.Y - pointA.Y;
            float z = pointB.Z - pointA.Z;

            // Square the three calculated values.
            x *= x;
            y *= y;
            z *= z;

            // Calculate the square root of the three values added together.
            return (float)Math.Sqrt(x + y + z);
        }

        /// <summary>
        /// Normalises two points together to calculate the first point's forward vector.
        /// </summary>
        /// <param name="pointA">The first point.</param>
        /// <param name="pointB">The second point.</param>
        /// <returns>The calculated forward vector for the first point.</returns>
        public static Vector3 CalculateForwardVector(Vector3 pointA, Vector3 pointB) => Vector3.Normalize(new(pointB.X - pointA.X, pointB.Y - pointA.Y, pointB.Z - pointA.Z));
        public static Vector3 CalculateForwardVectorZUP(Vector3 pointA, Vector3 pointB) => Vector3.Normalize(new(pointB.X - pointA.X, -pointB.Z - -pointA.Z, pointB.Y - pointA.Y));

        /// <summary>
        /// Calculates a point's up vector for a single spline.
        /// </summary>
        /// <param name="forwardVector">The point's forward vector to calculate the up vector from.</param>
        /// <returns>The calculated up vector.</returns>
        public static Vector3 CalculateSinglePointUpVector(Vector3 forwardVector) => Vector3.Cross(Vector3.Cross(forwardVector, new(0, 1, 0)), forwardVector);

        /// <summary>
        /// Calculates a point's up vector for a double spline.
        /// </summary>
        /// <param name="pointA">The position of the first point.</param>
        /// <param name="pointB">The position of the point opposite the first one.</param>
        /// <param name="pointC">The position of the point connected to the first one.</param>
        public static Vector3 CalculateDoublePointUpVector(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            // Calculate the forward vector between pointA and pointC.
            Vector3 forwardVector = CalculateForwardVector(pointA, pointC);

            // Calculate the forward vector between pointA and pointB to get the right vector.
            var rightVector = CalculateForwardVector(pointA, pointB);

            // Cross the right vector with the forward vector and return the result.
            return Vector3.Normalize(Vector3.Cross(rightVector, forwardVector));
        }
    }
}
