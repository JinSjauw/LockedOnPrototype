using UnityEngine;

[System.Serializable]
//Change to struct later and handle hologram spawning on sonarController side
public class DetectionData
{
    public ActiveSensingDetectable sonarDetectable;
    public Vector3 position;
    public string ID;
    public bool isVisible;

    public DetectionData(ActiveSensingDetectable sonarDetectable, Vector3 position, string ID, bool isVisible) 
    {
        this.sonarDetectable = sonarDetectable;
        this.position = position;   
        this.ID = ID;
        this.isVisible = isVisible;
    }
}
