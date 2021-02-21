using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using static Callbacks;

public class DialogueSystem : MonoBehaviour
{   
    //Singleton
    public static DialogueSystem instance;
    //Serialize
    public ELEMENTS elements;
    //Feature
    public bool _isSpeaking { get { return speaking != null; } }
    private Coroutine speaking = null;
    public TextArchitect textArchitect = null;
    [HideInInspector]
    public bool _isWaitingForUserInput = false;
    public string targetSpeech = "";


    public float speedText = 1f;
    private void Awake()
    {
        instance = this;
    }

    //Say something
    public void Say(string speech, string speaker="",bool additive = false)
    {
        StopSpeaking();
        if(additive)
            speechText.text = targetSpeech;
        speaking = StartCoroutine(Speaking(speech,additive,speaker));
    }

    public void StopSpeaking()
    {
        if (_isSpeaking)
        {
            StopCoroutine(speaking);
        }
        if(textArchitect!=null && textArchitect.isConstructing)
        {
            textArchitect.Stop();
        }
        speaking = null;
    }

    IEnumerator Speaking(string speech, bool additive, string targetSpeaker = "")
    {
        speechPanel.SetActive(true);
        targetSpeech = speech;

        string additiveSpeech = additive ? speechText.text : "";
        
        targetSpeech = additiveSpeech + speech;

        if (textArchitect == null)
            textArchitect = new TextArchitect(speechText, speech, additiveSpeech);
        else
            textArchitect.Renew(speech, additiveSpeech);

        speakerNameText.text = DeterminateSpeaker(targetSpeaker);
        speakerNamePanel.SetActive(speakerNameText.text != "");
        speechBox.SetActive(true);

        _isWaitingForUserInput = false;

        while(textArchitect.isConstructing)
        {
            if (Input.GetKey(KeyCode.Space))
                textArchitect.skip = true;

            yield return new WaitForEndOfFrame();
        }

        _isWaitingForUserInput = true;
        while (_isWaitingForUserInput)
            yield return new WaitForEndOfFrame();

        StopSpeaking();
    }

    string DeterminateSpeaker(string s)
    {
        string retVal = speakerNameText.text; //default is the current name
        if (s != speakerNameText.text && s != "")
        {
            if (s.ToLower().Contains("narrator"))
            {
                retVal = "";
            }
            else if(s.ToLower().Contains("mc"))
            {
                retVal = NovelManager.instance.mainCharacterName;
            }
            else
            {

                retVal = s;
            }
        }
        return retVal;
    }   

    public void Close()
    {
        StopSpeaking();
        for(int i=0; i < SpeechPanelRequeriments.Length; i++)
        {
            SpeechPanelRequeriments[i].SetActive(false);
        }
    }

    public void OpenAllRequerimentsForDialogueSystemVisibility(bool v)
    {
        for (int i = 0; i < SpeechPanelRequeriments.Length; i++)
        {
            SpeechPanelRequeriments[i].SetActive(v);
        }
    }

    public void Open(string speakerName="", string speech = "")
    {
        if(speakerName == "" && speech == "")
        {
            OpenAllRequerimentsForDialogueSystemVisibility(false);
            return;
        }

        OpenAllRequerimentsForDialogueSystemVisibility(true);
        speakerNameText.text = speakerName;
        speakerNamePanel.SetActive(speakerName != "");
        speechText.text = speech;
    }

    public void Buzz()
    {
        StopBuzz();
        currentBuzzing = StartCoroutine(Buzzing());
    }

    void StopBuzz()
    {
        if (isBuzzing)
            StopCoroutine(currentBuzzing);
        currentBuzzing = null;
    }

    Coroutine currentBuzzing = null;
    bool isBuzzing { get { return currentBuzzing != null; } }
    IEnumerator Buzzing()
    {
        buzzPanel.GetComponent<Animator>().SetTrigger("isEffect");
        yield return new WaitForEndOfFrame();
        BuzzPanels(0,3,()=> {
            buzzPanel.GetComponent<Animator>().SetTrigger("isEffect");
        });
        yield return new WaitForEndOfFrame();
    }

    void BuzzPanels(int ini, int max,OnComplete onComplete)
    {
        if (ini < max)
        {
            speechPanel.GetComponent<RectTransform>().DOAnchorPosX(10, 0.05f).OnComplete(() =>
            {
                speechPanel.GetComponent<RectTransform>().DOAnchorPosX(-10, 0.05f).OnComplete(()=>BuzzPanels(ini+1,max, onComplete)
                );
            });
        }
        else
        {
            onComplete();
        }
    }

    public bool isClosed
    {
        get { return !speechBox.activeInHierarchy; }
    }

    [System.Serializable]
    public class ELEMENTS
    {
        public GameObject speechPanel;
        public GameObject speakerNamePanel;
        public TextMeshProUGUI speakerNameText;
        public TextMeshProUGUI speechText;
    }

    public GameObject speechPanel { get { return elements.speechPanel; } }
    public TextMeshProUGUI speakerNameText { get { return elements.speakerNameText; } }
    public TextMeshProUGUI speechText { get { return elements.speechText; } }

    public GameObject speakerNamePanel { get { return elements.speakerNamePanel; } }
    public GameObject buzzPanel;
    public GameObject[] SpeechPanelRequeriments;
    public GameObject speechBox;
}
