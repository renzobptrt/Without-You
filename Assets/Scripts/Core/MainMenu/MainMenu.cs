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
    public Animator TransitionAnimator;

    public RectTransform[] MainContainers = null;
    public Button[] ButtonsOptionsMainMenu = null;
    public Button[] ButtonsOptionsMenu = null;

    public RectTransform[] PanelsOption = null;

    public TextMeshProUGUI titleOption = null;

    private void Start()
    {
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
            ButtonsOptionsMenu[temp].onClick.RemoveAllListeners();
            ButtonsOptionsMenu[temp].onClick.AddListener(() =>
            {
                ChoiceOptionMenuOptions(temp);
            });
        }

        ScreenCapture.CaptureScreenshot("SomeLevel");
    }

    void ChoiceOption(int indexButton)
    {
        switch (indexButton)
        {
            case 0:
                LoadNextScene("Chapter0");
                break;
            case 1:
                ShowOptionPanel(0);
                TransitionMenuOptions(true);
                titleOption.text = "Load Game";
                break;
            case 2:
                ShowOptionPanel(1);
                TransitionMenuOptions(true);
                titleOption.text = "Setting";
                break;
            case 3:
                ShowOptionPanel(2);
                TransitionMenuOptions(true);
                titleOption.text = "Help";
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
                break;
            case 1:
                ShowOptionPanel(0);
                titleOption.text = "Load Game";
                break;
            case 2:
                ShowOptionPanel(1);
                titleOption.text = "Setting";
                break;
            case 3:
                ShowOptionPanel(2);
                titleOption.text = "Help";
                break;
            case 4:
                GoToExitGame();
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
