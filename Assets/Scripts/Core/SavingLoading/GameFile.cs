using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameFile
{
    public static GameFile activeFile = new GameFile();

    public string chapterName = "chapter_start";
    public int chapterProgress = 0;

    public string playerName = "";

    public string cachedLastSpeaker = string.Empty;

    public string currentTextSystemSpeakerDisplayText = string.Empty;
    public string currentTextSystemDisplayText = string.Empty;

    public List<CHARACTERDATA> charactersInScene = new List<CHARACTERDATA>();
    public List<AudioClip> ambiance = new List<AudioClip>();

    public Texture background = null;
    public Texture foreground = null;

    public AudioClip music = null;

    public string modificationDate = "";
    public Texture2D previewImage = null;

    public string[] tempVals = new string[9];

    public GameFile()
    {
        this.chapterName = "chapter_start";
        this.chapterProgress = 0;
        this.cachedLastSpeaker = string.Empty;

        this.playerName = "No Name";

        this.background = null;
        this.foreground = null;

        this.music = null;

        charactersInScene = new List<CHARACTERDATA>();
        ambiance = new List<AudioClip>();
        tempVals = new string[9];
    }

    [System.Serializable]
    public class CHARACTERDATA
    {
        public string characterName="";
        public string displayName = "";
        public bool enabled = true;
        public string bodyExpression = "";
        public Vector2 position = new Vector2(0.5f,0);

        public CHARACTERDATA(Character character)
        {   
            this.characterName = character.characterName;
            //  this.displayName = character.displayName;
            this.enabled = character.isVisibleInScene;
            this.bodyExpression = character.renderers.bodyRenderer.sprite.name;
            this.position = character._targetPosition;
        }
    }
}
