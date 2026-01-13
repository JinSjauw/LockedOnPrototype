using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SonarUIController : MonoBehaviour
{
    [SerializeField] private UIEvents uiEvents;

    //TrackOne
    [SerializeField] private TextMeshProUGUI trackOneText;
    [SerializeField] private Image trackOneImage;
    private bool trackOneLocked = false;

    //TrackTwo
    [SerializeField] private TextMeshProUGUI trackTwoText;
    [SerializeField] private Image trackTwoImage;
    private bool trackTwoLocked = false;

    #region Unity

    private void Start()
    {
        uiEvents.sonarLockEvent += OnSonarLock;
    }

    #endregion

    #region Private

    private void OnSonarLock(object sender, SonarLockUIData lockData)
    {
        //Only 2 possibilites (0 or 1)
        //Default assumed track is number 1
        TextMeshProUGUI trackText = trackOneText;
        Image trackImage = trackOneImage;
        bool trackLocked = trackOneLocked;

        //If trackIndex is number 2
        if(lockData.trackIndex == 1) 
        {
            trackText = trackTwoText;
            trackImage = trackTwoImage;
            trackLocked = trackTwoLocked;
        }

        if (trackLocked) return;

        trackText.text = "Track #" + lockData.trackName;
        trackImage.color = lockData.trackColor.gamma;
    }

    private void ClearSonarLockUI(int trackIndex)
    {
        //Only 2 possibilites (0 or 1)
        //Default assumed track is number 1
        TextMeshProUGUI trackText = trackOneText;
        Image trackImage = trackOneImage;
        string trackName = "One";

        //If trackIndex is number 2
        if (trackIndex == 1)
        {
            trackText = trackTwoText;
            trackImage = trackTwoImage;
            trackName = "Two";
        }

        trackText.text = "Track #" + trackName;
        trackImage.color = Color.white;
    }

    #endregion

    #region Public

    //Button Logic
    public void OnLockClicked(int trackIndex) 
    {
        uiEvents.OnSonarLockButton(trackIndex);
    }

    public void OnUnLockClicked(int trackIndex) 
    {
        uiEvents.OnSonarLockLoss(trackIndex);
        ClearSonarLockUI(trackIndex);
    }

    #endregion

}
