using Ami.BroAudio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SonarUIController : MonoBehaviour
{
    [SerializeField] private UIEvents uiEvents;

    [Header("Tracker One")]
    //TrackOne
    [SerializeField] private TextMeshProUGUI trackOneText;
    [SerializeField] private Image trackOneImage;
    [SerializeField] private GameObject buttonLockOne;
    [SerializeField] private GameObject buttonSuperLockOne;

    private bool trackOneLocked = false;
    private bool trackOneSuperLocking = false;

    [Header("Tracker Two")]
    //TrackTwo
    [SerializeField] private TextMeshProUGUI trackTwoText;
    [SerializeField] private Image trackTwoImage;
    [SerializeField] private GameObject buttonLockTwo;
    [SerializeField] private GameObject buttonSuperLockTwo;

    private bool trackTwoLocked = false;
    private bool trackTwoSuperLocking = false;

    #region Unity

    private void Start()
    {
        uiEvents.sonarLockEvent += OnSonarLock;
    }

    private void OnDisable()
    {
        uiEvents.sonarLockEvent -= OnSonarLock;
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

        if (lockData.trackIndex == 0)
        {
            trackOneLocked = true;

            buttonLockOne.SetActive(false);
            buttonSuperLockOne.SetActive(true);
            trackOneSuperLocking = true;
        }
        else 
        {
            trackTwoLocked = true;

            buttonLockTwo.SetActive(false);
            buttonSuperLockTwo.SetActive(true);
            trackTwoSuperLocking = true;
        }
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

        if (trackIndex == 0)
        {
            trackOneLocked = false;
        }
        else
        {
            trackTwoLocked = false;
        }
    }

    #endregion

    #region Public

    //Button Logic
    public void OnLockClicked(int trackIndex) 
    {
        uiEvents.OnSonarLockButton(trackIndex);

        if(trackIndex == 0 && trackOneLocked) 
        {
            buttonLockOne.SetActive(false);
            buttonSuperLockOne.SetActive(true);
            trackOneSuperLocking = true;
        }

        if (trackIndex == 1 && trackTwoLocked)
        {
            buttonLockTwo.SetActive(false);
            buttonSuperLockTwo.SetActive(true);
            trackTwoSuperLocking = true;
        }
        //Now open the sonar lock minigame
        //and check if one is already open or not
    }

    public void OnSuperLockClicked(int trackIndex) 
    {
        if(trackIndex == 0 && trackOneLocked) 
        {
            uiEvents.OnSonarSuperLockButton(trackIndex);
        }

        if (trackIndex == 1 && trackTwoLocked)
        {
            uiEvents.OnSonarSuperLockButton(trackIndex);
        }
    }

    public void OnUnLockClicked(int trackIndex) 
    {
        if (trackIndex == 0 && trackOneSuperLocking)
        {
            buttonLockOne.SetActive(true);
            buttonSuperLockOne.SetActive(false);
            trackOneSuperLocking = false;
        }

        if (trackIndex == 1 && trackTwoSuperLocking)
        {
            buttonLockTwo.SetActive(true);
            buttonSuperLockTwo.SetActive(false);
            trackTwoSuperLocking = false;
        }

        uiEvents.OnSonarLockLoss(trackIndex);
        ClearSonarLockUI(trackIndex);
    }

    //public void OnUnSuperLockClicked(int trackIndex)
    //{
    //    uiEvents.OnSonarSuperLockLoss(trackIndex);

    //    if (trackIndex == 0)
    //    {
    //        buttonLockOne.SetActive(true);
    //        buttonSuperLockOne.SetActive(false);
    //    }
    //}

    #endregion

}
