using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static Callbacks;
using DG.Tweening;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public Animator TransitionAnimator;

    public RectTransform[] MainContainers = null;
    public Button[] ButtonsOptionsMainMenu = null;
    public Button[] ButtonsOptionsMenu = null;
    public Sprite[] SpriteNormalOptionsMenu = new Sprite[4];

    public RectTransform[] PanelsOption = null;

    public GameSavePanel saveLoadPanel;

    [Header("Audio Componentes")]
    public Slider musicSlider;
    public Slider sfxSlider;

    public TextMeshProUGUI musicText;
    public TextMeshProUGUI sfxText;

    int currentSaveLoadPage
    {
        get { return saveLoadPanel.currentSaveLoadPage; }
    }
    public string selectedGameFile = "";

    private void Start()
    {
        SpriteNormalOptionsMenu = new Sprite[4];
        for(int i=0; i < ButtonsOptionsMainMenu.Length;i++)
        {
            int temp = i;
            ButtonsOptionsMainMenu[temp].onClick.RemoveAllListeners();
            ButtonsOptionsMainMenu[temp].onClick.AddListener(() =>
            {
                ChoiceOption(temp);
            });
        }

        for(int i = 0; i < ButtonsOptionsMenu.Length; i++)
        {
            int temp = i;
            SpriteNormalOptionsMenu[temp] = ButtonsOptionsMenu[temp].GetComponent<Image>().sprite;
            ButtonsOptionsMenu[temp].onClick.RemoveAllListeners();
            ButtonsOptionsMenu[temp].onClick.AddListener(() =>
            {
                ChoiceOptionMenuOptions(temp);
            });
        }

        AudioManager.instance.SetSlidersAndText(musicSlider, sfxSlider, musicText, sfxText);
        Command_PlayMusic("StrangeMelody");
    }

    void Command_PlaySfx(string data)
    {
        AudioClip clip = Resources.Load("Audio/SFX/" + data) as AudioClip;
        if (clip != null)
            AudioManager.instance.PlaySfx(clip);
        else
            Debug.LogError("Clip does not exits: " + data);
    }

    void Command_PlayMusic(string data)
    {
        AudioClip clip = Resources.Load("Audio/Music/" + data) as AudioClip;
        if (clip != null)
            AudioManager.instance.PlayMusic(clip);
        else
            Debug.LogError("Clip does not exits: " + data);
    }

    void ChoiceOption(int indexButton)
    {
        switch (indexButton)
        {
            case 0:
                LoadNextScene("Novel");
                AudioManager.instance.SaveVolume();
                break;
            case 1:
                ShowOptionPanel(0);
                TransitionMenuOptions(true);
                ButtonsOptionsMenu[1].GetComponent<Image>().sprite = ButtonsOptionsMenu[1].spriteState.pressedSprite;
                saveLoadPanel.LoadFilesOntoScreen(currentSaveLoadPage);
                break;
            case 2:
                ShowOptionPanel(1);
                ButtonsOptionsMenu[2].GetComponent<Image>().sprite = ButtonsOptionsMenu[2].spriteState.pressedSprite;
                TransitionMenuOptions(true);
                break;
            case 3:
                ShowOptionPanel(2);
                ButtonsOptionsMenu[3].GetComponent<Image>().sprite = ButtonsOptionsMenu[3].spriteState.pressedSprite;
                TransitionMenuOptions(true);
                break;
            case 4:
                GoToExitGame();
                break;
        }
    }

    void ChoiceOptionMenuOptions(int indexButton)
    {
        switch (indexButton)
        {
            case 0:
                TransitionMenuOptions(false);
                ButtonsOptionsMenu[1].GetComponent<Image>().sprite = SpriteNormalOptionsMenu[1];
                ButtonsOptionsMenu[2].GetComponent<Image>().sprite = SpriteNormalOptionsMenu[2];
                ButtonsOptionsMenu[3].GetComponent<Image>().sprite = SpriteNormalOptionsMenu[3];
                break;
            case 1:
                ShowOptionPanel(0);
                ButtonsOptionsMenu[2].GetComponent<Image>().sprite = SpriteNormalOptionsMenu[2];
                ButtonsOptionsMenu[3].GetComponent<Image>().sprite = SpriteNormalOptionsMenu[3];
                break;
            case 2:
                ShowOptionPanel(1);
                ButtonsOptionsMenu[1].GetComponent<Image>().sprite = SpriteNormalOptionsMenu[1];
                ButtonsOptionsMenu[3].GetComponent<Image>().sprite = SpriteNormalOptionsMenu[3];
                break;
            case 3:
                ShowOptionPanel(2);
                ButtonsOptionsMenu[1].GetComponent<Image>().sprite = SpriteNormalOptionsMenu[1];
                ButtonsOptionsMenu[2].GetComponent<Image>().sprite = SpriteNormalOptionsMenu[2];
                break;
        }
    }

    public void GoToExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void LoadNextScene(string nameNextScene)
    {
        selectedGameFile = "chapter_1";
        FileManager.SaveFile(FileManager.savPath + "savData/file", selectedGameFile);
        StartCoroutine(TransitionScene(() =>
        {
            SceneManager.LoadScene(nameNextScene);
        }));
    }

    IEnumerator TransitionScene(OnComplete onComplete)
    {
        //TransitionAnimator.SetTrigger("IsOut");
        yield return new WaitForSeconds(0.35f);
        onComplete();
    }

    void TransitionMenuOptions(bool isInOptions)
    {
        DisableButton(ButtonsOptionsMainMenu);
        DisableButton(ButtonsOptionsMenu);
        if (isInOptions)
        {   
            MainContainers[0].DOAnchorPosX(-1954f, 1f);
            MainContainers[1].DOAnchorPosX(0, 1f).OnComplete(()=>
            {
                DisableButton(ButtonsOptionsMenu,true);
            });
        }
        else
        {
            MainContainers[1].DOAnchorPosX(1954, 1f);
            MainContainers[0].DOAnchorPosX(0, 1f).OnComplete(() =>
            {
                DisableButton(ButtonsOptionsMainMenu, true);
            });
        }
    }

    void DisableButton(Button[] ArrayButton, bool isFalse = false)
    {
        for (int i = 0; i < ArrayButton.Length; i++)
            ArrayButton[i].interactable = isFalse;
    } 

    void ShowOptionPanel(int index)
    {
        for(int i = 0; i < PanelsOption.Length; i++)
        {
            if (i == index)
                PanelsOption[i].anchoredPosition = Vector2.zero;
            else
                PanelsOption[i].anchoredPosition = new Vector2(2000f, 0f);
        }
    }
}
