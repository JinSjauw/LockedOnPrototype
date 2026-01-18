using UnityEngine;

public class SonarSignalBlock : MonoBehaviour
{
    public int Index { get; private set; }
    
    public void SetIndex(int index) 
    {
        Index = index;
    }
}
