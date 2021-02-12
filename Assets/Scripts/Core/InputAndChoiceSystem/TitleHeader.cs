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
        typeWriter,
        floatingSlowFade
    }

    public DISPLAY_METHOD displayMethod = DISPLAY_METHOD.instant;
    public float fadeSpped = 1f;

    Coroutine revealing = null;
    public bool isRevealing { get { return revealing != null; } }

    private bool cachedBannerPos = false;
    private Vector3 cachedBannedOriginalPosition = Vector3.zero;

    public void Show(string displayText)
    {
        title = displayText;

        if (isRevealing)
            StopCoroutine(revealing);

        if (!cachedBannerPos)
            cachedBannedOriginalPosition = banner.transform.position;

        revealing = StartCoroutine(Revealing());
    }

    public void Hide()
    {
        if (isRevealing)
            StopCoroutine(revealing);
        revealing = null;

        banner.enabled = false;
        titleText.enabled = false;

        if (cachedBannerPos)
            banner.transform.position = cachedBannedOriginalPosition;
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
                yield return SlowFade();
                break;
            case DISPLAY_METHOD.typeWriter:
                yield return TypeWriter();
                break;

            case DISPLAY_METHOD.floatingSlowFade:
                yield return FloatingSlowFade();
                break;
        }

        revealing = null;
    }

    IEnumerator SlowFade()
    {
        banner.color = GlobalFunction.SetAlpha(banner.color, 0);
        titleText.color = GlobalFunction.SetAlpha(titleText.color, 0);

        while (banner.color.a < 1)
        {
            banner.color = GlobalFunction.SetAlpha(banner.color, Mathf.MoveTowards(
                banner.color.a, 1, fadeSpped * Time.unscaledDeltaTime));
            titleText.color = GlobalFunction.SetAlpha(titleText.color, banner.color.a);
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator TypeWriter()
    {
        banner.color = GlobalFunction.SetAlpha(banner.color, 1);
        titleText.color = GlobalFunction.SetAlpha(titleText.color, 1);
        TextArchitect architect = new TextArchitect(titleText, title);
        while (architect.isConstructing)
        {
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FloatingSlowFade()
    {
        banner.color = GlobalFunction.SetAlpha(banner.color, 0);
        titleText.color = GlobalFunction.SetAlpha(titleText.color, 0);

        float amount = 5f * ((float)Screen.height / 1080f);
        Vector3 downPos = new Vector3(0, amount, 0);
        banner.transform.position = cachedBannedOriginalPosition - downPos ;

        while(banner.color.a<1 || banner.transform.position != cachedBannedOriginalPosition)
        {
            banner.color = GlobalFunction.SetAlpha(banner.color, Mathf.MoveTowards(
                banner.color.a, 1, fadeSpped * Time.unscaledDeltaTime));
            titleText.color = GlobalFunction.SetAlpha(titleText.color, banner.color.a);

            banner.transform.position = Vector3.MoveTowards(banner.transform.position,
                cachedBannedOriginalPosition, 11 * fadeSpped * Time.unscaledDeltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

}
