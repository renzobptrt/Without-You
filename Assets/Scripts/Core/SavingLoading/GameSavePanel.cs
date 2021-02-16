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

    public Button loadButton;
    public Button saveButton;
    public Button deleteButton;

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
                    b.dateTimeText.text = page.ToString() + "\n" + file.modificationDate;
                }
                else
                {
                    b.button.interactable = allowSavingFromThisScreen;
                    b.previewDisplay.texture = Resources.Load<Texture2D>("Images/UI/EmptyGameFile");
                    b.dateTimeText.text = page.ToString() + "\n" + "empty file...";
                }
            }
        }
        else
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                BUTTON b = buttons[i];
                b.button.interactable = allowSavingFromThisScreen;
                b.previewDisplay.texture = Resources.Load<Texture2D>("Images/UI/EmptyGameFile");
                b.dateTimeText.text = page.ToString() + "\n" + "empty file...";
            }
        }
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

        //Run an error check just to be sure the file has not been removed since load.
        if (System.IO.File.Exists(selectedFilePath))
        {
            if(loadButton!=null)
                loadButton.interactable = allowLoadingFromThisScreen;
            if(saveButton!=null)
                saveButton.interactable = allowSavingFromThisScreen;
            if(deleteButton!=null)
                deleteButton.interactable = allowDeletingFromThisScreen;
        }
        else
        {
            selectedButton.dateTimeText.text = "<color=red>FILE NOT FOUND!";
            if (loadButton != null)
                loadButton.interactable = false;
            if (saveButton != null)
                saveButton.interactable = allowSavingFromThisScreen;
            if (deleteButton != null)
                deleteButton.interactable = true;
        }
    }

    public void LoadFromSelectedSlot()
    {
        //we need to load the data from this slot to know what to do.
        GameFile file = FileManager.LoadEncryptedJSON<GameFile>(selectedFilePath, FileManager.keys);

        //save the name of the file that we will be loading in the visual novel. carries over to next scene.
        FileManager.SaveFile(FileManager.savPath + "savData/file", selectedGameFile);

        UnityEngine.SceneManagement.SceneManager.LoadScene("Novel");

        gameObject.SetActive(false);//deactivate the panel after loading.
    }

    public void LoadFromSelectedSlotMaiMenu()
    {
        //we need to load the data from this slot to know what to do.
        GameFile file = FileManager.LoadEncryptedJSON<GameFile>(selectedFilePath, FileManager.keys);

        //save the name of the file that we will be loading in the visual novel. carries over to next scene.
        FileManager.SaveFile(FileManager.savPath + "savData/file", selectedGameFile);

        UnityEngine.SceneManagement.SceneManager.LoadScene("Novel");
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

        yield return new WaitForEndOfFrame();
        // a screen shot is made during this point.
        NovelManager.instance.SaveGameFile();
        yield return new WaitForEndOfFrame();
        selectedButton.dateTimeText.text = currentSaveLoadPage.ToString() + "\n" + GameFile.activeFile.modificationDate;
        selectedButton.previewDisplay.texture = GameFile.activeFile.previewImage;

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
    }
}
