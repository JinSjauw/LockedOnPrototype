using BTreeEditor;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace EnemyAI
{
    [CreateAssetMenu(fileName = "EnemyIdleConfig", menuName = "Scriptable Objects/EnemyIdleConfig")]
    public class EnemyIdleConfig : NodeConfigAsset<EnemyIdleParams> { }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct EnemyIdleParams
    {
        public float waitTime;
    }
}