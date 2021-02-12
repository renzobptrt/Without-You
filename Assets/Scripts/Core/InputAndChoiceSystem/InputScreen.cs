using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputScreen : MonoBehaviour
{
    public static InputScreen instance;

    public TMP_InputField inputField;
    public static string currentInput { get { return instance.inputField.text; } }

    public TitleHeader header;

    public GameObject root;

    static Coroutine revealing = null;
    public static bool isWaitingForUserInput { get { return instance.root.activeInHierarchy; } }
    public static bool isRevaling { get { return revealing != null; } }

    void Awake()
    {
        instance = this;
        Hide();
    }

    public static void Show(string title, bool clearCurrentInput = true)
    {
        instance.root.SetActive(true);

        if (clearCurrentInput)
            instance.inputField.text = "";

        if (title != "")
            instance.header.Show(title);
        else
            instance.header.Hide();

        if (isRevaling)
            instance.StopCoroutine(revealing);
        revealing = instance.StartCoroutine(Revealing());
    }

    public static void Hide()
    {
        instance.root.SetActive(false);
        instance.header.Hide(); 
    }

    static IEnumerator Revealing()
    {
        instance.inputField.gameObject.SetActive(false);

        while (instance.header.isRevealing)
            yield return new WaitForEndOfFrame();

        instance.inputField.gameObject.SetActive(true);

        revealing = null;
    }

    public void Accept()
    {
        Hide();
    }
}
