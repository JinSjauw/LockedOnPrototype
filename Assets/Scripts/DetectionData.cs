using UnityEngine;

[System.Serializable]
//Change to struct later and handle hologram spawning on sonarController side
public class DetectionData
{
    public SonarDetectable sonarDetectable;
    public Vector3 position;
    public string ID;
    public bool isUnderFOW;

    public DetectionData(SonarDetectable sonarDetectable, Vector3 position, string ID, bool isVisible) 
    {
        this.sonarDetectable = sonarDetectable;
        this.position = position;   
        this.ID = ID;
        this.isUnderFOW = isVisible;
    }
}
