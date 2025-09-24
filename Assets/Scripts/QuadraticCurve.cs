using UnityEngine;

public class QuadraticCurve : MonoBehaviour
{
    [SerializeField] private Transform a;
    [SerializeField] private Transform b;
    [SerializeField] private Transform control;

    public Vector3 EvaluateCurve(float t) 
    {
        Vector3 ac = Vector3.Lerp(a.position, control.position, t);
        Vector3 cb = Vector3.Lerp(control.position, b.position, t);

        return Vector3.Lerp(ac, cb, t);
    }

    private void OnDrawGizmos()
    {
        if(a == null || b == null || control == null) 
        {
            return;
        }

        for (int i = 0; i < 40; i++) 
        {
            Gizmos.DrawWireSphere(EvaluateCurve(i / 40f), .5f);
        }
    }
}
