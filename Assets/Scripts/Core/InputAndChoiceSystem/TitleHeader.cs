using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleHeader : MonoBehaviour
{
    public Image banner;
    public TextMeshProUGUI titleText;
    public string title { get { return titleText.text; } set { titleText.text = value; } }

    public enum DISPLAY_METHOD
    {
        instant,
        slowFade,
        typeWriter
    }

    public DISPLAY_METHOD displayMethod = DISPLAY_METHOD.instant;
    public float fadeSpped = 1f;

    Coroutine revealing = null;
    public bool isRevealing { get { return revealing != null; } }

    public void Show(string displayText)
    {
        title = displayText;

        if (isRevealing)
            StopCoroutine(revealing);

        revealing = StartCoroutine(Revealing());
    }

    public void Hide()
    {
        if (isRevealing)
            StopCoroutine(revealing);
        revealing = null;

        banner.enabled = false;
        titleText.enabled = false;
    }

    private IEnumerator Revealing()
    {
        banner.enabled = true;
        titleText.enabled = true;

        switch (displayMethod)
        {
            case DISPLAY_METHOD.instant:
                banner.color = GlobalFunction.SetAlpha(banner.color, 1);
                titleText.color = GlobalFunction.SetAlpha(titleText.color, 1);
                break;
            case DISPLAY_METHOD.slowFade:
                banner.color = GlobalFunction.SetAlpha(banner.color, 0);
                titleText.color = GlobalFunction.SetAlpha(titleText.color, 0);

                while (banner.color.a < 1)
                {
                    banner.color = GlobalFunction.SetAlpha(banner.color, Mathf.MoveTowards(
                        banner.color.a,1, fadeSpped * Time.unscaledDeltaTime));
                    titleText.color = GlobalFunction.SetAlpha(titleText.color, banner.color.a);
                    yield return new WaitForEndOfFrame();
                }
                break;
            case DISPLAY_METHOD.typeWriter:
                banner.color = GlobalFunction.SetAlpha(banner.color, 1);
                titleText.color = GlobalFunction.SetAlpha(titleText.color, 1);
                TextArchitect architect = new TextArchitect(titleText, title);
                while (architect.isConstructing)
                {
                    yield return new WaitForEndOfFrame();
                }
                break;
        }

        revealing = null;
    }

}
