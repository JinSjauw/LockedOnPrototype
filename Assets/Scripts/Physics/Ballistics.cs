using UnityEngine;

public class Ballistics : MonoBehaviour
{
    public static bool CalculateAngleToHitTarget(out float theta1, out float theta2, float projectileSpeed, Vector3 originPosition, Vector3 targetPosition)
    {
        //Initial speed
        float v = projectileSpeed;

        Vector3 targetVec = targetPosition - originPosition;

        //Vertical distance
        float y = targetVec.y;

        //Reset y so we can get the horizontal distance x
        targetVec.y = 0f;

        //Horizontal distance
        float x = targetVec.magnitude;

        //Gravity
        float g = 9.81f;

        //Calculate the angles

        float vSqr = v * v;

        float underTheRoot = (vSqr * vSqr) - g * (g * x * x + 2 * y * vSqr);

        //Check if we are within range
        if (underTheRoot >= 0f)
        {
            float rightSide = Mathf.Sqrt(underTheRoot);

            float top1 = vSqr + rightSide;
            float top2 = vSqr - rightSide;

            float bottom = g * x;

            theta1 = Mathf.Atan2(top1, bottom) * Mathf.Rad2Deg;
            theta2 = Mathf.Atan2(top2, bottom) * Mathf.Rad2Deg;

            return true;
        }
        else
        {
            theta1 = 0;
            theta2 = 0;

            return false;
        }
    }

    public static float CalculateTimeToHitTarget(float projectileSpeed, Transform barrel, Vector3 targetPosition, float h)
    {
        //Init values
        Vector3 currentVelocity = barrel.forward * projectileSpeed;
        Vector3 currentPosition = barrel.position;

        Vector3 newPosition = Vector3.zero;
        Vector3 newVelocity = Vector3.zero;

        //The total time it will take before we hit the target
        float time = 0f;

        //Limit to 30 seconds to avoid infinite loop if we never reach the target
        for (time = 0f; time < 30f; time += h)
        {
            CurrentIntegrationMethod(h, currentPosition, currentVelocity, out newPosition, out newVelocity);

            //If we are moving downwards and are below the target, then we have hit
            if (newPosition.y < currentPosition.y && newPosition.y < targetPosition.y)
            {
                //Add 2 times to make sure we end up below the target when we display the path
                time += h * 2f;

                break;
            }

            currentPosition = newPosition;
            currentVelocity = newVelocity;
        }

        return time;
    }

    //Integration Methods
    public static void CurrentIntegrationMethod(
    float h,
    Vector3 currentPosition,
    Vector3 currentVelocity,
    out Vector3 newPosition,
    out Vector3 newVelocity)
    {
        //IntegrationMethods.EulerForward(h, currentPosition, currentVelocity, out newPosition, out newVelocity);
        IntegrationMethods.Heuns(h, currentPosition, currentVelocity, out newPosition, out newVelocity);
        //IntegrationMethods.RungeKutta(h, currentPosition, currentVelocity, out newPosition, out newVelocity);
        //IntegrationMethods.BackwardEuler(h, currentPosition, currentVelocity, out newPosition, out newVelocity);
    }
}

