using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private bool isMainMenu;
    [SerializeField] private GameObject menuCollection;

    private PlayerInput playerInput;

    private void Awake()
    {
        if (!isMainMenu) 
        {
            playerInput = GetComponentInParent<PlayerInput>();
        }
    }

    private void ResetInput() 
    {
        playerInput.SwitchCurrentActionMap("Gameplay");
        Time.timeScale = 1f;
    }

    public void OnOpenMenu(InputValue value) 
    {
        if (value.isPressed) 
        {
            if(playerInput != null && !isMainMenu) 
            {
                playerInput.SwitchCurrentActionMap("UI");
                menuCollection.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    public void OnCloseMenu(InputValue value) 
    {
        if (value.isPressed) 
        {
            if (playerInput != null && !isMainMenu)
            {
                menuCollection.SetActive(false);
                ResetInput();
            }
        }
    }

    public void PlayButton() 
    {
        SceneLoader.Load("PlayScene");
    }

    public void ExitButton() 
    {
        Application.Quit();
    }

    public void ResumeButton() 
    {
        if (playerInput != null && !isMainMenu)
        {
           menuCollection.SetActive(false);
           ResetInput() ;
        }
    }

    public void RestartButton() 
    {
        SceneLoader.Load("PlayScene", ResetInput);
    }

    public void MainMenuButton() 
    {
        SceneLoader.Load("MainMenu");
    }
}
