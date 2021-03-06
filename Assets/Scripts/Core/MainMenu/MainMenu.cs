﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static Callbacks;
using DG.Tweening;
using TMPro;

public class MainMenu : MonoBehaviour
{   
    [Header("General Components")]
    public RectTransform[] MainContainers = null;
    public Button[] ButtonsOptionsMainMenu = null;
    public Button[] ButtonsOptionsMenu = null;
    [HideInInspector] public Sprite[] SpriteNormalOptionsMenu = new Sprite[4];

    public RectTransform[] PanelsOption = null;

    public GameSavePanel saveLoadPanel;

    public TextMeshProUGUI[] LoadText;
    public TextMeshProUGUI[] SettingsText;
    public TextMeshProUGUI[] HelpText;

    [Header("Audio Componentes")]
    public Slider musicSlider;
    public Slider sfxSlider;

    public TextMeshProUGUI musicText;
    public TextMeshProUGUI sfxText;

    public Animator TransitionPanel;

    int currentSaveLoadPage
    {
        get { return saveLoadPanel.currentSaveLoadPage; }
    }
    public string selectedGameFile = "";

    [Header("Auto and Lenguage")]
    public Button[] AutoButtons = null;
    public Button[] LenguageButtons = null;

    public Sprite alphaOnly;
    public Sprite Selected;

    public Color DefaultText;
    public Color SelectText;

    [Header("Interface Text")]
    public INTERFACE_MENUSCENE interfaceText;

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

        for(int i = 0; i < AutoButtons.Length; i++)
        {
            int temp = i;
            AutoButtons[temp].onClick.RemoveAllListeners();
            AutoButtons[temp].onClick.AddListener(() =>
            {
                SetAuto(AutoButtons,temp);
            });
        }

        for(int i = 0; i < LenguageButtons.Length; i++)
        {
            int temp = i;
            LenguageButtons[temp].onClick.RemoveAllListeners();
            LenguageButtons[temp].onClick.AddListener(() =>
            {
                SetLenguage(LenguageButtons,temp);
            });
        }

        saveLoadPanel.LoadFilesOntoScreen(currentSaveLoadPage);
        AudioManager.instance.SetSlidersAndText(musicSlider, sfxSlider, musicText, sfxText);
        Command_PlayMusic("StrangeMelody");

        if (!PlayerPrefs.HasKey("IsAuto"))
        {
            PlayerPrefs.SetInt("IsAuto", 1);
            SetAuto(AutoButtons,1);
        }
        else
        {
            SetAuto(AutoButtons, PlayerPrefs.GetInt("IsAuto"));
        }

        if (!PlayerPrefs.HasKey("CurrentLenguage"))
        {
            PlayerPrefs.SetString("CurrentLenguage", "English");
            SetLenguage(LenguageButtons, 1);
        }
        else
        {
            if(PlayerPrefs.GetString("CurrentLenguage").Equals("Spanish"))
                SetLenguage(LenguageButtons, 0);
            else SetLenguage(LenguageButtons, 1);
        }
    }

    void SetSelect(Button[] CurrentButtons,int position)
    {
        switch (position)
        {
            case 0:
                CurrentButtons[0].GetComponent<Image>().sprite = Selected;
                CurrentButtons[1].GetComponent<Image>().sprite = alphaOnly;
                CurrentButtons[0].GetComponentInChildren<TextMeshProUGUI>().color = SelectText;
                CurrentButtons[1].GetComponentInChildren<TextMeshProUGUI>().color = DefaultText;
                break;
            case 1:
                CurrentButtons[1].GetComponent<Image>().sprite = Selected;
                CurrentButtons[0].GetComponent<Image>().sprite = alphaOnly;
                CurrentButtons[1].GetComponentInChildren<TextMeshProUGUI>().color = SelectText;
                CurrentButtons[0].GetComponentInChildren<TextMeshProUGUI>().color = DefaultText;
                break;
        }
    }

    void SetAuto(Button[] CurrentButtons,int position)
    {
        SetSelect(CurrentButtons,position);
        if(position == 0) PlayerPrefs.SetInt("IsAuto", 0);
        else PlayerPrefs.SetInt("IsAuto", 1);
    }

    void SetLenguage(Button[] CurrentButtons,int position)
    {
        SetSelect(CurrentButtons,position);
        if (position == 0)
        {
            PlayerPrefs.SetString("CurrentLenguage", "Spanish");
            SetLenguageText(0);
        }
        else { 
            PlayerPrefs.SetString("CurrentLenguage", "English");
            SetLenguageText(1);
        }

        print("Idioma actual: " + PlayerPrefs.GetString("CurrentLenguage"));
    }

    void SetLenguageText(int lenguage)
    {
        LENGUAGETEXT_MENUSCENE currentLenguageSelect = interfaceText.lenguagesText[lenguage];

        for (int i = 0; i < ButtonsOptionsMainMenu.Length; i++)
            ButtonsOptionsMainMenu[i].GetComponentInChildren<TextMeshProUGUI>().text = currentLenguageSelect.mainMenuText[i];

        for(int i=0; i < ButtonsOptionsMenu.Length; i++)
            ButtonsOptionsMenu[i].GetComponentInChildren<TextMeshProUGUI>().text = currentLenguageSelect.optionsButtonSettingText[i];

        for(int i = 0; i < LoadText.Length; i++)
            LoadText[i].text = currentLenguageSelect.loadText;

        for(int i = 0; i < SettingsText.Length; i++)
            SettingsText[i].text = currentLenguageSelect.settingsText[i];

        for (int i = 0; i < HelpText.Length; i++)
            HelpText[i].text = currentLenguageSelect.helpText[i];
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
                //saveLoadPanel.LoadFilesOntoScreen(currentSaveLoadPage);
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
        AudioManager.instance.SaveVolume();
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
        TransitionPanel.SetTrigger("TransitionT");
        yield return new WaitForSeconds(1f);
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

    [System.Serializable]
    public class INTERFACE_MENUSCENE
    {
        public LENGUAGETEXT_MENUSCENE[] lenguagesText;
    }

    [System.Serializable]
    public class LENGUAGETEXT_MENUSCENE
    {
        public enum NAMELENGUAGETEXT
        {
            spanish,
            english
        }
        public NAMELENGUAGETEXT nameLenguage = NAMELENGUAGETEXT.spanish;
        public string[] mainMenuText = null;
        public string[] optionsButtonSettingText = null;
        public string loadText = null;
        public string[] settingsText = null;
        public string[] helpText = null;
    }
}
