using UnityEngine;

public class SignalEffectAnimator : MonoBehaviour
{
    [SerializeField] private Material shader;

    [Header("Timing")]
    public float cyclePerSecond = 0.25f; // quarters per second
    public float attackTime = 0.05f;
    public float sustainTime = 0.10f;
    public float decayTau = 6f;    // exp coeff = 1 / tau

    [Header("Noise")]
    public float noiseBase = 0.02f;
    public float noiseVariation = 0.5f;

    [Header("Levels")]
    public Vector4 lowLevels = new Vector4(0.02f, 0.02f, 0.02f, 0.02f);

    readonly float[] envelope = new float[4];
    readonly float[] age = new float[4];

    void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            envelope[i] = lowLevels[i];
            age[i] = 10f; // start "old"
        }
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * cyclePerSecond, 1f);
        int slot = (int)(t * 4f);
        int oldSlot = (int)((t - Time.deltaTime * cyclePerSecond) * 4f);

        // trigger ping on boundary cross
        if (slot != oldSlot)
        {
            envelope[slot] = 1f;
            age[slot] = 0.1f;
        }

        // update each channel
        for (int i = 0; i < 4; i++)
        {
            age[i] += Time.deltaTime;

            if (age[i] < attackTime + sustainTime)
                envelope[i] = 1f;
            else
                envelope[i] = Mathf.Exp(-(age[i] - attackTime - sustainTime) * decayTau);

            // floor never goes below lowLevels
            envelope[i] = Mathf.Max(envelope[i], lowLevels[i]);
        }

        // optional flicker noise
        float noise = noiseBase * (1f + noiseVariation * Mathf.Sin(Time.time * 30f + slot));
        Vector4 noisy = new Vector4(
                Mathf.Max(envelope[0], noise),
                Mathf.Max(envelope[1], noise),
                Mathf.Max(envelope[2], noise),
                Mathf.Max(envelope[3], noise));

        shader.SetVector("_WaveAlphas", noisy);
    }
}
