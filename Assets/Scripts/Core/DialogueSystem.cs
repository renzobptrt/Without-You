using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueSystem : MonoBehaviour
{   
    //Singleton
    public static DialogueSystem instance;
    //Serialize
    public ELEMENTS elements;
    //Feature
    public bool _isSpeaking { get { return speaking != null; } }
    private Coroutine speaking = null;
    public bool _isWaitingForUserInput = false;
    string targetSpeech = "";
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        
    }

    //Say something
    public void Say(string speech, string speaker="")
    {
        StopSpeaking();
        speaking = StartCoroutine(Speaking(speech,false,speaker));
    }

    public void SayAdd(string speech, string speaker = "")
    {
        StopSpeaking();
        speechText.text = targetSpeech;
        speaking = StartCoroutine(Speaking(speech, true, speaker));
    }

    public void StopSpeaking()
    {
        if (_isSpeaking)
        {
            StopCoroutine(speaking);
        }
        speaking = null;
    }

    IEnumerator Speaking(string speech, bool additive,string targetSpeaker = "")
    {
        speechPanel.SetActive(true);
        targetSpeech = speech;
        if (!additive)
            speechText.text = "";
        else
            targetSpeech = speechText.text + targetSpeech;
        speakerNameText.text = DeterminateSpeaker(targetSpeaker);
        _isWaitingForUserInput = false;
        while(speechText.text != targetSpeech)
        {
            speechText.text += targetSpeech[speechText.text.Length];
            yield return new WaitForSeconds(0.05f);
        }

        _isWaitingForUserInput = true;
        while (_isWaitingForUserInput)
            yield return new WaitForEndOfFrame();

        StopSpeaking();
    }

    string DeterminateSpeaker(string s)
    {
        string retVal = speakerNameText.text; //default is the current name
        if (s != speakerNameText.text && s!="")
            retVal = (s.ToLower().Contains("narrator")) ? "" : s;
        return retVal;
    }   

    [System.Serializable]
    public class ELEMENTS
    {
        public GameObject speechPanel;
        public TextMeshProUGUI speakerNameText;
        public TextMeshProUGUI speechText;
    }

    public GameObject speechPanel { get { return elements.speechPanel; } }
    public TextMeshProUGUI speakerNameText { get { return elements.speakerNameText; } }
    public TextMeshProUGUI speechText { get { return elements.speechText; } }


}
