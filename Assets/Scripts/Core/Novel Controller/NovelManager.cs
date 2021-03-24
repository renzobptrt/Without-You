using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using DG.Tweening;

public class NovelManager : MonoBehaviour
{
    public static NovelManager instance;

    List<string> data = new List<string>();

    Coroutine handlingLine = null;
    Coroutine handlingChapterFile = null; 
    Coroutine FinishMessageCoroutine = null;
    [HideInInspector]public bool isHandlingLine { get { return handlingLine != null; } }

    [HideInInspector] public bool isHandlingChapterFile { get { return handlingChapterFile != null; } }
    
    public GameFile activeGameFile
    {
        get { return GameFile.activeFile; }
        set { GameFile.activeFile = value; }
    }

    [Header("Game File")]
    [SerializeField] private int chapterProgress = 0;
    public string activeGameFileName = "";
    string activeChapterName = string.Empty;
    public bool encrypGameFile = true;
    public string cachedLastSpeaker = "";
    public string mainCharacterName = "";
    public string[] listIgnoreInTxt;
    bool _next = false;

    [Header("Heroines Affinity")]
    public int tachibanaAffinity = 0;
    public int chitoseAffinity = 0;
    public int akikoAffinity = 0;

    [Header("Current Accumulated Resources")]
    public List<string> accumulatedResources = new List<string>();

    [Header("Audio Componentes")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Message Layer")]
    public GameObject MessageLayer;
    public GameObject MessageLayerBackground;
    public TextMeshProUGUI MessageLayerText;

    [Header("Save Layer")]
    public GameSavePanel saveLoadPanel;
    public RectTransform SavePanel;
    public RectTransform SettingsPanel;
    public TextMeshProUGUI musicText;
    public TextMeshProUGUI sfxText;

    [Header("Input Layer")]
    public Button CheckInput;
    public bool isCheck;
    public bool isAuto;

    [Header("Lenguage Components")]
    public INTERFACE_NOVELSCENE interfaceLenguage;
    public TextMeshProUGUI[] TextButtonsOptions = null;
    public TextMeshProUGUI[] TextSettings = null;
    public TextMeshProUGUI GoMainMenu = null;

    [Header("Current Conditionals")]
    List<string> actionsConditional = new List<string>();

    //temporary
    byte[] keys
    {
        get { return FileManager.keys; }
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //LoadGameFile(1);
        saveLoadPanel.gameObject.SetActive(false);
        //LoadChapterFile("chapter_start");
        LoadGameFile(FileManager.LoadFile(FileManager.savPath + "savData/file.txt")[0]);

        saveLoadPanel.LoadFilesOntoScreen(saveLoadPanel.currentSaveLoadPage);
        AudioManager.instance.SetSlidersAndText(musicSlider, sfxSlider, musicText, sfxText);
        Command_PlayMusic("Time");
        isCheck = false;
        CheckInput.onClick.RemoveAllListeners();

        CheckInput.onClick.AddListener(() =>
        {
            if (InputScreen.currentInput != "")
            {
                isCheck = true;
                CheckInput.transform.localScale = Vector2.zero;
                CheckInput.interactable = false;
                if (PlayerPrefs.GetInt("IsAuto") == 0)
                    isAuto = true;
                else isAuto = false;
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Next();
        }
    }

    void SetLenguageText(int position)
    {
        for (int i = 0; i < TextButtonsOptions.Length; i++)
            TextButtonsOptions[i].text = interfaceLenguage.lenguagesText[position].optionsText[i];
        for (int i = 0; i < TextSettings.Length; i++)
            TextSettings[i].text = interfaceLenguage.lenguagesText[position].settingsText[i];
        GoMainMenu.text = interfaceLenguage.lenguagesText[position].goMenu;
    }

    public bool IsSpeakerIgnore(string speaker)
    {
        for(int i = 0; i < listIgnoreInTxt.Length; i++)
        {
            if(speaker == listIgnoreInTxt[i])
            {
                return true;
            }
        }

        return false;
    }

    public void LoadGameFile(string gameFileName)
    {
        activeGameFileName = gameFileName;

        string filePath = FileManager.savPath + "savData/gameFiles/" + gameFileName + ".txt";
        print(filePath);

        if (!System.IO.File.Exists(filePath))
        {
            activeGameFile = new GameFile();
        }
        else
        {
            if (encrypGameFile)
                activeGameFile = FileManager.LoadEncryptedJSON<GameFile>(filePath, keys);
            else
                activeGameFile = FileManager.LoadJSON<GameFile>(filePath);
        }

        //Load the file
        data = FileManager.ReadTextAsset(Resources.Load<TextAsset>($"Story/{PlayerPrefs.GetString("CurrentLenguage")}/{activeGameFile.chapterName}"));
        activeChapterName = activeGameFile.chapterName;
        cachedLastSpeaker = activeGameFile.cachedLastSpeaker;
        mainCharacterName = activeGameFile.playerName;

        tachibanaAffinity = activeGameFile.tachibanaAffinity;
        chitoseAffinity = activeGameFile.chitoseAffinity;
        akikoAffinity = activeGameFile.akikoAffinity;

        accumulatedResources = activeGameFile.listAccumulatedResources;

        if (System.IO.File.Exists(filePath))
            DialogueSystem.instance.Open(activeGameFile.currentTextSystemSpeakerDisplayText, activeGameFile.currentTextSystemDisplayText);


        //Load all characters in the scene
        for (int i = 0; i < activeGameFile.charactersInScene.Count; i++)
        {
            GameFile.CHARACTERDATA data = activeGameFile.charactersInScene[i];

            if(data.bodyExpression != "AlphaOnly" && data.bodyExpression != string.Empty)
            {
                Character character = CharacterManager.instance.CreateCharacter(data.characterName, data.enabled);
                print(data.bodyExpression);
                string rightExpression = data.bodyExpression.Split('_')[1];
                character.SetNewEmotion(rightExpression);
                character.SetPosition(data.position);
            }
        }

        //Load the layer
        if (activeGameFile.background != null)
            BackgroundManager.instance.background.SetTexture(activeGameFile.background);
        if (activeGameFile.foreground != null)
            BackgroundManager.instance.foreground.SetTexture(activeGameFile.foreground);

        //Start the music
        if (activeGameFile.music != null)
            AudioManager.instance.PlayMusic(activeGameFile.music);

        if (handlingChapterFile != null)
        {
            StopCoroutine(handlingChapterFile);
        }
        handlingChapterFile = StartCoroutine(HandlingChapterFile());
        chapterProgress = activeGameFile.chapterProgress;

        if (!System.IO.File.Exists(filePath))
            Next();
        else
        {
            chapterProgress--;
            Next();
        }
    }

    public void SaveGameFile()
    {
        string filePath = FileManager.savPath + "savData/gameFiles/" + activeGameFileName + ".txt";
        activeGameFile.chapterName = activeChapterName;
        activeGameFile.chapterProgress = chapterProgress;
        activeGameFile.cachedLastSpeaker = cachedLastSpeaker;
        activeGameFile.playerName = mainCharacterName;
        activeGameFile.currentTextSystemSpeakerDisplayText = DialogueSystem.instance.speakerNameText.text;
        activeGameFile.currentTextSystemDisplayText = DialogueSystem.instance.speechText.text;
        AudioManager.instance.SaveVolume();

        //Affinity and Resources
        activeGameFile.tachibanaAffinity = tachibanaAffinity;
        activeGameFile.chitoseAffinity = chitoseAffinity;
        activeGameFile.akikoAffinity = akikoAffinity;
        activeGameFile.listAccumulatedResources = accumulatedResources;

        //Get all characters and save their stats
        activeGameFile.charactersInScene.Clear();
        for(int i = 0; i < CharacterManager.instance.characters.Count; i++)
        {
            Character character = CharacterManager.instance.characters[i];
            GameFile.CHARACTERDATA data = new GameFile.CHARACTERDATA(character);
            activeGameFile.charactersInScene.Add(data);
        }

        //Save layers
        BackgroundManager b = BackgroundManager.instance;
        activeGameFile.background = b.background.activeImage != null ? b.background.activeImage.texture : null;
        activeGameFile.foreground = b.foreground.activeImage != null ? b.foreground.activeImage.texture : null;

        //Save the music
        activeGameFile.music = AudioManager.activeSong != null ? AudioManager.activeSong.clip : null;

        //Save a preview image (screenshot)
        //save the ambiance to disk if there is any playing.
        //activeGameFile.ambiance = AudioManager.activeAmbianceClips;

        //save the tempvals to disk. for easy variable storage.
        //activeGameFile.tempVals = CACHE.tempVals;

        //save a preview image (screenshot) to be viewed from the save load screen
        string screenShotPath = FileManager.savPath + "savData/gameFiles/" + activeGameFileName + ".png";
        if (FileManager.TryCreateDirectoryFromPath(screenShotPath + ".png"))
        {
            GameFile.activeFile.previewImage = ScreenCapture.CaptureScreenshotAsTexture();
            byte[] textureData = activeGameFile.previewImage.EncodeToPNG();
            FileManager.SaveComposingBytes(screenShotPath, textureData);
        }

        //save the data and time this file was created or modified.
        activeGameFile.modificationDate = System.DateTime.Now.ToString();

        if (encrypGameFile)
            FileManager.SaveEncryptedJSON(filePath, activeGameFile, keys);
        else
            FileManager.SaveJSON(filePath, activeGameFile);
    }



    public void LoadChapterFile(string fileName)
    {
        activeChapterName = fileName;

        data = FileManager.ReadTextAsset(Resources.Load<TextAsset>($"Story/{PlayerPrefs.GetString("CurrentLenguage")}/{fileName}"));

        cachedLastSpeaker = "";

        if (handlingChapterFile != null)
        {
            StopCoroutine(handlingChapterFile);
        }
        handlingChapterFile = StartCoroutine(HandlingChapterFile());

        Next();//Auto start the chapter
    }

    public void Next()
    {
        _next = true;
    }

    public void Auto()
    {
        isAuto = !isAuto;

        if (isAuto)
        {
            Next();
            PlayerPrefs.SetInt("IsAuto", 0);
        }else
            PlayerPrefs.SetInt("IsAuto", 1);
    }

    IEnumerator HandlingChapterFile()
    {
        chapterProgress = 0;

        while(chapterProgress < data.Count)
        {
            if (_next)
            {
                string line = data[chapterProgress];

                if (line.StartsWith("choice"))
                {
                    yield return HandlingChoiceLine(line);
                    chapterProgress++;
                }
                else if (line.StartsWith("input"))
                {
                    yield return HandlingInputLine(line);
                    chapterProgress++;
                }
                else
                {
                    if (line.StartsWith("if"))
                    {
                        AND(line);
                    }else if (line.StartsWith("or"))
                    {
                        OR(line);
                    }
                    else if(line.StartsWith("{") || line.StartsWith("}"))
                    {
                        chapterProgress++;
                        Next();
                    }
                    else
                    {
                        HandleLine(line);
                        chapterProgress++;
                    }

                    while (isHandlingLine)
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    if (isAuto)
                    {
                        yield return new WaitForSeconds(0.5f);
                        Next();
                    }
                }

            }
            //We need a way of knowing when the player wants to advance. We need a "next" trigger.
            yield return new WaitForEndOfFrame();
        }
        handlingChapterFile = null;
    }

    IEnumerator HandlingInputLine(string line)
    {
        string title = line.Split('"')[1];

        //get the one or more commands to execute when this input is done and accepted.
        string[] parts = line.Split(' ');
        List<string> endingCommands = new List<string>();
        if (parts.Length >= 3)
        {
            for (int i = 2; i < parts.Length; i++)
            {
                endingCommands.Add(parts[i]);
            }
        }

        //we have the title and the ending commands to execute. Now we need to bring up the input screen.
        InputScreen.Show(title);
        CheckInput.transform.localScale = Vector2.one;
        CheckInput.interactable = true;
        isCheck = false;
        while (InputScreen.isShowingInputField || InputScreen.isRevaling)
        {
            //wait for the input screen to finish revealing before being able to accept input.
            if (isCheck && !InputScreen.isRevaling)
            {
                //if the input is not empty, accept it.
                if (InputScreen.currentInput != "")
                {
                    InputScreen.instance.Accept();
                    isCheck = false;
                }
            }
            yield return new WaitForEndOfFrame();
        }

        //the input has been accepted, now it is time to execute the commands that follow.
        for (int i = 0; i < endingCommands.Count; i++)
        {
            string command = endingCommands[i];
            HandleAction(command);
        }
    }

    bool IsANDorOR(string condition, bool isAND = true)
    {
        string[] listConditions = new string[0];

        List<string> currentConditionsDoing = new List<string>();
        if (condition.Contains(";"))
        {
            listConditions = condition.Split(";"[0]);
        }
        else
        {
            listConditions = new string[1] { condition };
        }

        int affinityConditionalTachibana = -1;
        int affinityConditionalAkiko = -1;
        int affinityConditionalChitose = -1;

        for (int i = 0; i < listConditions.Length; i++)
        {
            if (listConditions[i].Contains(","))
            {
                switch (listConditions[i].Split(',')[0])
                {
                    case "Tachibana":
                        affinityConditionalTachibana = int.Parse(listConditions[i].Split(',')[1]);
                        break;
                    case "Chitose":
                        affinityConditionalChitose = int.Parse(listConditions[i].Split(',')[1]);
                        break;
                    case "Akiko":
                        affinityConditionalAkiko = int.Parse(listConditions[i].Split(',')[1]);
                        break;
                }
            }
            else
            {
                currentConditionsDoing.Add(listConditions[i]);
            }
        }

        if (isAND)
        {
            if ((affinityConditionalTachibana != -1 && affinityConditionalTachibana != tachibanaAffinity) ||
                (affinityConditionalChitose != -1 && affinityConditionalChitose != chitoseAffinity) ||
                (affinityConditionalAkiko != -1 && affinityConditionalAkiko != akikoAffinity)
                )
                return false;
        }
        else
        {
            if ((affinityConditionalTachibana != -1 && affinityConditionalTachibana == tachibanaAffinity) ||
                (affinityConditionalChitose != -1 && affinityConditionalChitose == chitoseAffinity) ||
                (affinityConditionalAkiko != -1 && affinityConditionalAkiko == akikoAffinity))
                return true;
        }

        if (accumulatedResources.Count > 0)
        {
            int count = 0;
            for (int i = 0; i < currentConditionsDoing.Count; i++)
            {
                if (currentConditionsDoing[i].Contains("!"))
                {
                    currentConditionsDoing[i] = currentConditionsDoing[i].Replace("!", "");
                    bool isFalse = true;
                    for (int j = 0; j < accumulatedResources.Count; j++)
                    {
                        if (accumulatedResources[j].Equals(currentConditionsDoing[i]))
                        {
                            isFalse = false;
                            break;
                        }
                    }

                    if (isFalse)
                        count++;
                }
                else
                {
                    for (int j = 0; j < accumulatedResources.Count; j++)
                    { 
                        if (accumulatedResources[j].Equals(currentConditionsDoing[i]))
                        {
                            count++;
                            break;
                        }
                    }
                }
            }

            if (isAND)
            {
                if (count == currentConditionsDoing.Count)
                    return true;
            }
            else
            {
                if (count > 0)
                    return true;
            }
        }
        else
        {
            if (currentConditionsDoing.Count == 0)
                return true;
        }
        return false;
    }
     
    void OR(string line)
    {
        string condition = line.Split('"')[1];
        bool rpta = IsANDorOR(condition, false); 
        
        bool gatheringActions = true;

        int currentChapterProgress = chapterProgress;

        while (gatheringActions)
        {
            currentChapterProgress++;

            line = data[currentChapterProgress];

            if (line == "{")
                continue;

            line = line.Replace("  ", "");

            if (line != "}")
            {
                actionsConditional.Add(line.Split('"')[1]);
            }
            else
            {
                gatheringActions = false;
            }
        }

        if (rpta)
            chapterProgress++;
        else
            chapterProgress += actionsConditional.Count + 3;

        actionsConditional.Clear();
        Next();
    }

    void AND(string line)
    {
        string condition = line.Split('"')[1];

        bool rpta = IsANDorOR(condition);

        bool gatheringActions = true;

        int currentChapterProgress = chapterProgress;

        while (gatheringActions)
        {
            currentChapterProgress++;

            line = data[currentChapterProgress];

            if (line == "{")
                continue;

            line = line.Replace("  ", "");

            if(line != "}")
            {
                actionsConditional.Add(line.Split('"')[1]); 
            }
            else
            {
                gatheringActions = false;
            }
        }

        if(rpta)
            chapterProgress++;
        else
            chapterProgress += actionsConditional.Count + 3;

        actionsConditional.Clear();
        Next();
    }
    
    IEnumerator HandlingChoiceLine(string line)
    {
        string title = line.Split('"')[1];
        List<string> choices = new List<string>();
        List<string> actions = new List<string>();

        bool gatheringChoices = true;

        while (gatheringChoices)
        {
            chapterProgress++;
            line = data[chapterProgress];

            if (line == "{")
                continue;

            line = line.Replace("    ","");

            if (line != "}")
            {
                string currentChoice = line.Split('"')[1];
                if (currentChoice.Contains("|"))
                {
                    string conditionLine = currentChoice.Split('|')[1];
                    if (IsANDorOR(conditionLine))
                    {
                        choices.Add(currentChoice.Split('|')[0]);
                        actions.Add(data[chapterProgress + 1].Replace("    ", ""));
                    }
                    //actionsConditional.Clear();
                }
                else
                {
                    choices.Add(line.Split('"')[1]);
                    actions.Add(data[chapterProgress + 1].Replace("    ", ""));
                }
                chapterProgress++;
            }
            else
            {
                gatheringChoices = false;
            }
        }

        if (choices.Count > 0)
        {
            ChoiceScreen.Show(title, choices.ToArray());
            yield return new WaitForEndOfFrame();

            while (ChoiceScreen.isWaitingForChoiceToBeMade)
                yield return new WaitForEndOfFrame();

            string action = actions[ChoiceScreen.lastChoiceMade.index];
            HandleLine(action);

            while (isHandlingLine)
                yield return new WaitForEndOfFrame();
        }
        else
        {
            Debug.LogError("No se encontraron elecciones");
        }
    }

    void HandleLine(string rawLine)
    {
        ControllerLineManager.LINE line = ControllerLineManager.Interpret(rawLine);

        StopHandlingLine();
        handlingLine = StartCoroutine(HandlingLine(line));
    }

    void StopHandlingLine()
    {
        if (isHandlingLine)
            StopCoroutine(handlingLine);

        handlingLine = null;
    }

    IEnumerator HandlingLine(ControllerLineManager.LINE line)
    {
        _next = false;
        int lineProgress = 0;
        while (lineProgress < line.segments.Count)
        {
            if (isAuto)
            {
                yield return new WaitForSeconds(0.5f);
                Next();
            }

            else
                _next = false;
            ControllerLineManager.LINE.SEGMENT segment = line.segments[lineProgress];
            if (lineProgress > 0)
            {
                if(segment.trigger == ControllerLineManager.LINE.SEGMENT.TRIGGER.autoDelay)
                {
                    for(float timer = segment.autoDelay; timer>=0; timer -= Time.deltaTime)
                    {
                        yield return new WaitForEndOfFrame();
                        if (_next)
                            break;
                    }
                }
                else
                {
                    while(!_next)
                        yield return new WaitForEndOfFrame();
                }
            }
            _next = false;

            segment.Run();

            while (segment.isRunning)
            {
                yield return new WaitForEndOfFrame();
                if (_next)
                {
                    if (!segment.architect.skip)
                        segment.architect.skip = true;
                    else
                        segment.ForceFinish();
                    _next = false;
                }
            }

            lineProgress++;
            yield return new WaitForEndOfFrame();
        }
        for(int i=0;i< line.actions.Count; i++)
        {
            HandleAction(line.actions[i]);
        }
        handlingLine = null;
    }

    public void HandleAction(string action)
    {
        print("Accion: " + action);
        string[] data = action.Split('(', ')');
        switch (data[0])
        {
            case "setBackground":
                {
                    Command_SetLayerImage(data[1], BackgroundManager.instance.background);
                    break;
                }
            case "setForeground":
                {
                    Command_SetLayerImage(data[1], BackgroundManager.instance.foreground);
                    break;
                }
            case "playSfx":
                {
                    Command_PlaySfx(data[1]);
                    break;
                }
            case "playMusic":
                {
                    Command_PlayMusic(data[1]);
                    break;
                }
            case "moveCharacter":
                {
                    Command_MoveCharacter(data[1]);
                    break;
                }
            case "setPosition":
                {
                    Command_SetPosition(data[1]);
                    break;
                }
            case "setExpression":
                {
                    Command_ChangeExpression(data[1]);
                    break;
                }
            case "enter":
                {
                    Command_Enter(data[1]);
                    break;
                }
            case "exit":
                {
                    Command_Exit(data[1]);
                    break;
                }
            case "Load":
                {
                    Command_Load(data[1]);
                    break;
                }
            case "Affinity":
                {
                    Command_Affinity(data[1]);
                    break;
                }
            case "SavePlayerName":
                {
                    Command_SavePlayerName();
                    break;
                }
            case "Next":
                {
                    _next = true;
                    break;
                }
            case "Buzz":
                {
                    Command_Buzz();
                    break; 
                }
            case "FinishGame":
                {
                    Command_Finish();
                    break;
                }
            case "AddResource":
                {
                    Command_AddResource(data[1]);
                    break;
                }
            case "Message":
                {
                    Command_Message(data[1]);
                    break;
                }
        }
    }

    void Command_Message(string data)
    {
        MessageLayerBackground.SetActive(true);
        string message = data;
        message = message.Replace("_", " ");
        MessageLayerText.text = message;
        FinishMessageCoroutine = StartCoroutine(FinishSeeMessageLayer());
    }

    IEnumerator FinishSeeMessageLayer()
    {
        MessageLayer.SetActive(true);
        MessageLayer.GetComponent<Animator>().SetTrigger("Finish");
        yield return new WaitForSeconds(6.0f);
        MessageLayer.GetComponent<Animator>().SetTrigger("Finish");
        yield return new WaitForSeconds(1.0f);
        MessageLayerBackground.SetActive(false);
        MessageLayer.SetActive(false);
        NextFinishMessageLayer();
    }

    void NextFinishMessageLayer()
    {
        Next();
        StopCoroutine(FinishSeeMessageLayer());
        FinishMessageCoroutine = null;
    }

    void Command_Finish()
    {
        MessageLayer.SetActive(true);
        MessageLayer.GetComponent<Animator>().SetTrigger("Finish");
    }

    void Command_Buzz()
    {
        DialogueSystem.instance.Buzz();
    }

    void Command_SavePlayerName()
    {
        mainCharacterName = InputScreen.instance.inputField.text; 
    }

    void Command_Affinity(string data)
    {
        string[] parameters = data.Split(',');
        string character = parameters[0];
        int affinity = int.Parse(parameters[1]);

        switch (character)
        {
            case "Tachibana":
                tachibanaAffinity += affinity;
                break;
            case "Chitose":
                chitoseAffinity += affinity;
                break;
            case "Akiko":
                akikoAffinity += affinity;
                break;
        }
    }
    void Command_AddResource(string data)
    {
        accumulatedResources.Add(data);
    }

    void Command_Load(string chapterName)
    {
        NovelManager.instance.LoadChapterFile(chapterName); 
    }

    void Command_SetLayerImage(string data,BackgroundManager.LAYER layer)
    {
        string textureName = data.Contains(",") ? data.Split(',')[0] : data;
        Texture2D tex = textureName == "null" ? null : Resources.Load("images/UI/Backdrops/" + textureName) as Texture2D;
        float spd = 2f;
        bool smooth = false;

        if (data.Contains(","))
        {
            string[] parameters = data.Split(',');
            foreach(string p in parameters)
            {
                float fVal = 0;
                bool bVal = false;
                
                if(float.TryParse(p,out fVal))
                {
                    spd = fVal; continue;
                }
                if(bool.TryParse(p, out bVal))
                {
                    smooth = bVal; continue;
                }
            }
        }

        layer.TransitionToTexture(tex, spd, smooth);
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

    void Command_MoveCharacter(string data)
    {
        string[] parameters = data.Split(',');
        string character = parameters[0];
        float locationX = float.Parse(parameters[1]);
        float locationY = float.Parse(parameters[2]);
        float speed = parameters.Length >= 4 ? float.Parse(parameters[3]) : 1f;
        bool smooth = parameters.Length == 5 ? bool.Parse(parameters[4]) : true;
        Character c = CharacterManager.instance.GetCharacter(character);
        c.MoveTo(new Vector2(locationX, locationY), speed,smooth);
    }

    void Command_SetPosition(string data)
    {
        string[] parameters = data.Split(',');
        string character = parameters[0];
        float locationX = float.Parse(parameters[1]);
        float locationY = float.Parse(parameters[2]);
        Character c = CharacterManager.instance.GetCharacter(character);
        c.SetPosition(new Vector2(locationX, locationY));
    }

    void Command_ChangeExpression(string data)
    {
        string[] parameters = data.Split(',');
        string character = parameters[0];
        string region = parameters[1];
        string expression = parameters[2];
        float speed = parameters.Length == 4 ? float.Parse(parameters[3]) : 1f;

        Character c = CharacterManager.instance.GetCharacter(character);
        Sprite sprite = c.GetSprite(expression);

        if(region.ToLower() == "body")
        {
            //c.SetNewEmotion(expression);
            c.TransitionBody(sprite, speed,true);
        }
    }

    void Command_Exit(string data)
    {
        string[] parameters = data.Split(',');
        string[] characters = parameters[0].Split(';');
        float speed = 3;
        bool smooth = false;
        for (int i = 1; i < parameters.Length; i++)
        {
            float fVal = 0; bool bVal = false;
            if (float.TryParse(parameters[i], out fVal)) { speed = fVal; continue; }
            if (bool.TryParse(parameters[i], out bVal)) { smooth = bVal; continue; }
        }
        
        foreach(string s in characters)
        {
            Character c = CharacterManager.instance.GetCharacter(s);
            c.FadeOut(speed, smooth);
        }
    }

    void Command_Enter(string data)
    {
        string[] parameters = data.Split(',');
        string[] characters = parameters[0].Split(';');
        float speed = 3;
        bool smooth = false;
        for (int i = 1; i < parameters.Length; i++)
        {
            float fVal = 0; bool bVal = false;
            if (float.TryParse(parameters[i], out fVal)) { speed = fVal; continue; }
            if (bool.TryParse(parameters[i], out bVal)) { smooth = bVal; continue; }
        }

        foreach (string s in characters)
        {
            Character c = CharacterManager.instance.GetCharacter(s,true,false);
            if (!c.enabled)
            {
                c.renderers.bodyRenderer.color = new Color(1, 1, 1, 0);
                c.enabled = true;

                c.TransitionBody(c.renderers.bodyRenderer.sprite, speed, smooth);
            }
            else
                c.FadeIn(speed, smooth);
        }
    }

    public void GoToMenuScene()
    {
        AudioManager.instance.SaveVolume();
        SceneManager.LoadScene("Menu");
    }

    public void OpenSaveLayer()
    {
        if (!saveLoadPanel.gameObject.activeInHierarchy)
        {
            saveLoadPanel.gameObject.SetActive(true);
            isAuto = false;
            SavePanel.anchoredPosition = Vector2.zero;
            SettingsPanel.anchoredPosition = new Vector2(2000f, 0);
        }
    }

    public void OpenSettingsLayer()
    {
        if (!saveLoadPanel.gameObject.activeInHierarchy)
        {
            isAuto = false;
            saveLoadPanel.gameObject.SetActive(true);
            SavePanel.anchoredPosition = new Vector2(2000f, 0);
            SettingsPanel.anchoredPosition = Vector2.zero;
        }
    }

    [System.Serializable]
    public class INTERFACE_NOVELSCENE
    {
        public LENGUAGETEXT_NOVELSCENE[] lenguagesText;
    }

    [System.Serializable]
    public class LENGUAGETEXT_NOVELSCENE
    {
        public enum NAMELENGUAGETEXT
        {
            spanish,
            english
        }
        public NAMELENGUAGETEXT nameLenguage = NAMELENGUAGETEXT.spanish;
        public string[] optionsText;
        public string goMenu = string.Empty;
        public string[] settingsText = null;
        public string inputText = null;
    }
}
