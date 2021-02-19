using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSavePanel : MonoBehaviour
{
    public List<BUTTON> buttons = new List<BUTTON>();

    [HideInInspector]
    public int currentSaveLoadPage = 1;

    public CanvasGroup canvasGroup;

    public enum TASK
    {
        saveToSlot,
        loadFromSlot,
        deleteSlot
    }
    public TASK slotTask = TASK.loadFromSlot;

    public Animator SaveLoadingAnimator = null;

    public void LoadFilesOntoScreen(int page = 1)
    {
        currentSaveLoadPage = page;

        string directory = FileManager.savPath + "savData/gameFiles/" + page.ToString() + "/";

        if (System.IO.Directory.Exists(directory))
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                BUTTON b = buttons[i];
                string expectedFile = directory + (i + 1).ToString() + ".txt";
                if (System.IO.File.Exists(expectedFile))
                {
                    GameFile file = FileManager.LoadEncryptedJSON<GameFile>(expectedFile, FileManager.keys);

                    b.button.interactable = true;
                    byte[] previewImageData = FileManager.LoadComposingBytes(directory + (i + 1).ToString() + ".png");
                    Texture2D previewImage = new Texture2D(2, 2);
                    ImageConversion.LoadImage(previewImage, previewImageData);
                    file.previewImage = previewImage;
                    b.previewDisplay.texture = file.previewImage;

                    //need to read date and time information from file.
                    b.dateTimeText.text = file.modificationDate;
                    b.deleteButton.interactable = true;
                    b.deleteButton.transform.localScale = Vector2.one;
                    b.deleteButton.onClick.RemoveAllListeners();
                    b.deleteButton.onClick.AddListener(() =>
                    {
                        string  temp = expectedFile;
                        System.IO.File.Delete(temp);
                        /*b.button.interactable = allowSavingFromThisScreen;
                        b.previewDisplay.texture = Resources.Load<Texture2D>("images/UI/GameGUI/Settings/SwitchBackground");
                        b.dateTimeText.text = "Empty file...";
                        b.deleteButton.interactable = false;
                        b.deleteButton.transform.localScale = Vector2.zero;*/
                        RefreshBox(b);
                    });
                }
                else
                {
                    /*b.button.interactable = allowSavingFromThisScreen;
                    b.previewDisplay.texture = Resources.Load<Texture2D>("images/UI/GameGUI/Settings/SwitchBackground");
                    b.dateTimeText.text = "Empty file...";
                    b.deleteButton.interactable = false;
                    b.deleteButton.transform.localScale = Vector2.zero;*/
                    RefreshBox(b);
                }
            }
        }
        else
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                BUTTON b = buttons[i];
                /*b.button.interactable = allowSavingFromThisScreen;
                b.previewDisplay.texture = Resources.Load<Texture2D>("images/UI/GameGUI/Settings/SwitchBackground");
                b.dateTimeText.text = "Empty file...";
                b.deleteButton.interactable = false;
                b.deleteButton.transform.localScale = Vector2.zero;*/
                RefreshBox(b);
            }
        }
    }

    void RefreshBox(BUTTON b)
    {
        b.button.interactable = allowSavingFromThisScreen;
        b.previewDisplay.texture = Resources.Load<Texture2D>("images/UI/GameGUI/Settings/SwitchBackground");
        b.dateTimeText.text = "Empty file...";
        b.deleteButton.interactable = false;
        b.deleteButton.transform.localScale = Vector2.zero;
    }

    [HideInInspector]
    public BUTTON selectedButton = null;
    string selectedGameFile = "";
    string selectedFilePath = "";
    public bool allowLoadingFromThisScreen = true;
    public bool allowSavingFromThisScreen = true;
    public bool allowDeletingFromThisScreen = true;

    public void ClickOnSaveSlot(Button button)
    {
        foreach(BUTTON B in buttons)
        {
            if (B.button == button)
                selectedButton = B;
        }

        selectedGameFile = currentSaveLoadPage.ToString() + "/" + (buttons.IndexOf(selectedButton) + 1).ToString();
        selectedFilePath = FileManager.savPath + "savData/gameFiles/" + selectedGameFile + ".txt";
    }

    public void LoadFromSelectedSlot()
    {
        //we need to load the data from this slot to know what to do.
        GameFile file = FileManager.LoadEncryptedJSON<GameFile>(selectedFilePath, FileManager.keys);

        //save the name of the file that we will be loading in the visual novel. carries over to next scene.
        FileManager.SaveFile(FileManager.savPath + "savData/file", selectedGameFile);
        AudioManager.instance.SaveVolume();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Novel");

        gameObject.SetActive(false);//deactivate the panel after loading.
    }

    public void LoadFromSelectedSlotMaiMenu()
    {
        //we need to load the data from this slot to know what to do.

        if (System.IO.File.Exists(selectedFilePath))
        {
            GameFile file = FileManager.LoadEncryptedJSON<GameFile>(selectedFilePath, FileManager.keys);

            //save the name of the file that we will be loading in the visual novel. carries over to next scene.
            FileManager.SaveFile(FileManager.savPath + "savData/file", selectedGameFile);
            AudioManager.instance.SaveVolume();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Novel");
        }
    }

    public void ClosePanel()
    {
        if (gameObject.activeInHierarchy)
            gameObject.SetActive(false);//GetComponent<Animator>().SetTrigger("deactivate");
    }
    Coroutine savingFile = null;
    public void SaveToSelectedSlot()
    {
        //save the open game file to this slot.
        if (NovelManager.instance != null)
        {
            savingFile = StartCoroutine(SavingFile());
        }
    }

    IEnumerator SavingFile()
    {
        NovelManager.instance.activeGameFileName = selectedGameFile;

        //render this part of the screen invisible so we get a clear snapshot of the visual novel.
        SaveLoadingAnimator.SetTrigger("instantVisible");
        yield return new WaitForSeconds(0.8f);
        yield return new WaitForEndOfFrame();
        // a screen shot is made during this point.
        NovelManager.instance.SaveGameFile();

        yield return new WaitForEndOfFrame();
        selectedButton.dateTimeText.text = GameFile.activeFile.modificationDate;
        selectedButton.previewDisplay.texture = GameFile.activeFile.previewImage;
        selectedButton.deleteButton.interactable = true;
        selectedButton.deleteButton.transform.localScale = Vector2.one;
        selectedButton.deleteButton.onClick.RemoveAllListeners();
        selectedButton.deleteButton.onClick.AddListener(() =>
        {
            System.IO.File.Delete(selectedFilePath);
            RefreshBox(selectedButton);
        });
        //render this part of the screen visible again after the screenshot is taken.
        SaveLoadingAnimator.SetTrigger("instantVisible");

        savingFile = null;
    }

    public void DeleteSlot()
    {
        print("We'll do this later.");
    }

    [System.Serializable]
    public class BUTTON
    {
        public Button button;
        public RawImage previewDisplay;
        public TextMeshProUGUI dateTimeText;
        public Button deleteButton;
    }
}
