using Ami.BroAudio;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundConfig", menuName = "Scriptable Objects/SoundConfig")]
public class SoundConfig : ScriptableObject
{
    public SoundID artillerySound;
    public SoundID autoCannonSound;
    public SoundID railgunSound;

    public SoundID interceptBeepSound;
    public SoundID interceptLockSound;

    public SoundID lockOnSound;
    public SoundID sonarScanStart;
    public SoundID sonarScanEnd;

    public SoundID destructionSound;
    public SoundID burningSound;
    public SoundID impactSound;

    public SoundID defaultAmbience;

    public SoundID signalLockBeep;
    public SoundID signalLockFinish;
}
