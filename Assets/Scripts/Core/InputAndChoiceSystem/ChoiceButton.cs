using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceButton : MonoBehaviour
{
    public TextMeshProUGUI tmpro;
    public string text { get { return tmpro.text; } set { tmpro.text = value; } }

    [HideInInspector]
    public int choiceIndex = -1;
}
