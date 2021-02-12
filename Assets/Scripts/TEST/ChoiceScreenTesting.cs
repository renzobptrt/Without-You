using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceScreenTesting : MonoBehaviour
{
    public string title = "Con quien hago grupo...";
    public string[] choices;


    // Update is called once per frame
    void Start()
    {
        StartCoroutine(DynamicStoryExample());
    }

    IEnumerator DynamicStoryExample()
    {
        NovelManager.instance.LoadChapterFile("chapter0_start");
        yield return new WaitForEndOfFrame();

        while (NovelManager.instance.isHandlingChapterFile)
            yield return new WaitForEndOfFrame();

        ChoiceScreen.Show("A quien escoges...", "Tachibana", "Chitose");
        while (ChoiceScreen.isWaitingForChoiceToBeMade)
            yield return new WaitForEndOfFrame();

        if (ChoiceScreen.lastChoiceMade.index == 0)
            NovelManager.instance.LoadChapterFile("chapter_a1");
        else
            NovelManager.instance.LoadChapterFile("chapter_a2");

        yield return new WaitForEndOfFrame();
        NovelManager.instance.Next();

        while (NovelManager.instance.isHandlingChapterFile)
            yield return new WaitForEndOfFrame();
    }
}
