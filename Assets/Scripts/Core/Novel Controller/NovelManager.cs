using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovelManager : MonoBehaviour
{
    public static NovelManager instance;

    List<string> data = new List<string>();

    Coroutine handlingLine = null;
    public bool isHandlingLine { get { return handlingLine != null; } }

    public bool isHandlingChapterFile { get { return handlingChapterFile != null; } }

    Coroutine handlingChapterFile = null;

    bool _next = false;

    private int chapterProgress = 0;

    private int activeGameFileNumber = 0;
    GameFile activeGameFile = null;
    string activeChapterName = string.Empty;

   [HideInInspector]
    public string cachedLastSpeaker = "";

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LoadGameFile(1);
    }

    public void LoadGameFile(int gameFileNumber)
    {
        activeGameFileNumber = gameFileNumber;

        string filePath = FileManager.savPath + "Resources/gameFiles/" + gameFileNumber.ToString() + ".txt";

        if (!System.IO.File.Exists(filePath))
        {
            FileManager.SaveEncryptedJSON(filePath, new GameFile(),keys);
        }

        activeGameFile = FileManager.LoadEncryptedJSON<GameFile>(filePath,keys);

        //Load the file
        data = FileManager.LoadFile(FileManager.savPath + "Resources/Story/" + activeGameFile.chapterName);

        activeChapterName = activeGameFile.chapterName;
        cachedLastSpeaker = activeGameFile.cachedLastSpeaker;

        DialogueSystem.instance.Open(activeGameFile.currentTextSystemSpeakerDisplayText,
activeGameFile.currentTextSystemDisplayText);

        //Load all characters in the scene
        for (int i = 0; i < activeGameFile.charactersInScene.Count; i++)
        {
            GameFile.CHARACTERDATA data = activeGameFile.charactersInScene[i];
            Character character = CharacterManager.instance.CreateCharacter(data.characterName,data.enabled);
            string rightExpression = data.bodyExpression.Split('_')[1];
            character.SetNewEmotion(rightExpression);
            character.SetPosition(data.position);
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
    }

    public void SaveGameFile()
    {
        string filePath = FileManager.savPath + "Resources/gameFiles/" + activeGameFileNumber.ToString() + ".txt";
        activeGameFile.chapterName = activeChapterName;
        activeGameFile.chapterProgress = chapterProgress;
        activeGameFile.cachedLastSpeaker = cachedLastSpeaker;

        activeGameFile.currentTextSystemSpeakerDisplayText = DialogueSystem.instance.speakerNameText.text;
        activeGameFile.currentTextSystemDisplayText = DialogueSystem.instance.speechText.text;

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

        FileManager.SaveEncryptedJSON(filePath, activeGameFile,keys);
    }

    //temporary
    public byte[] keys = new byte[3]
    {
        23,70,194
    };

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Next();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveGameFile();
        }
    }

    public void LoadChapterFile(string fileName)
    {
        activeChapterName = fileName;

        data = FileManager.LoadFile(FileManager.savPath + "Resources/Story/"+fileName);
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
                else
                {
                    HandleLine(line);
                    chapterProgress++;
                    while (isHandlingLine)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }

            }
            //We need a way of knowing when the player wants to advance. We need a "next" trigger.
            yield return new WaitForEndOfFrame();
        }

        handlingChapterFile = null;
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
                choices.Add(line.Split('"')[1]);
                actions.Add(data[chapterProgress + 1].Replace("    ", ""));
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
        }
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
            c.TransitionBody(sprite, speed,false);
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
}
