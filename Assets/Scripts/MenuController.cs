using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private bool isMainMenu;
    [SerializeField] private GameObject menuCollection;

    private void Start()
    {
        Services.Input.OnOpenMenuEvent += OnOpenMenu;
        Services.Input.OnCloseMenuEvent += OnCloseMenu;
    }

    private void OnDisable()
    {
        //Services.Input.OnOpenMenuEvent -= OnOpenMenu;
        //Services.Input.OnCloseMenuEvent -= OnCloseMenu;
    }

    private void ResetInput() 
    {
        Services.Input.EnableGameplay();
        Time.timeScale = 1f;
    }

    public void OnOpenMenu() 
    {
        if(!isMainMenu) 
        {
            Services.Input.EnableUI();
            menuCollection.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void OnCloseMenu() 
    {
        if (!isMainMenu)
        {
            menuCollection.SetActive(false);
            ResetInput();
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
        if (!isMainMenu)
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
