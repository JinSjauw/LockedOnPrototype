using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Menu
{

    public class MenuController : MonoBehaviour
    {
        [SerializeField] private bool isMainMenu;
        [SerializeField] private GameObject menuCollection;

        private VisualElement menuUI;

        private Button playButton;
        private Button settingsButton;
        private Button quitButton;

        private void Awake()
        {
            menuUI = GetComponent<UIDocument>().rootVisualElement;
        }

        private void Start()
        {
            Services.Input.OnOpenMenuEvent += OnOpenMenu;
            Services.Input.OnCloseMenuEvent += OnCloseMenu;

            //Main Menu Subscribe
            
            playButton = menuUI.Q<Button>("PlayButton");
            playButton.clicked += PlayButton;

            settingsButton = menuUI.Q<Button>("SettingsButton");
            settingsButton.clicked += SettingsButton;

            quitButton = menuUI.Q<Button>("QuitButton");
            quitButton.clicked += ExitButton;
        }

        private void OnDisable()
        {
            //Services.Input.OnOpenMenuEvent -= OnOpenMenu;
            //Services.Input.OnCloseMenuEvent -= OnCloseMenu;
        }

        public void PlayButton()
        {
            SceneLoader.Load("PlayScene");
        }

        private void SettingsButton()
        {
            Debug.Log("Opened Settings!");
        }

        public void ExitButton()
        {
            Application.Quit();
        }


        private void ResetInput()
        {
            Services.Input.EnableGameplay();
            Time.timeScale = 1f;
        }

        public void OnOpenMenu()
        {
            if (!isMainMenu)
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

        public void ResumeButton()
        {
            if (!isMainMenu)
            {
                menuCollection.SetActive(false);
                ResetInput();
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
}
