using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArchitectTesting : MonoBehaviour
{
    public TextMeshProUGUI textMPro;
    TextArchitect architect;

    [TextArea(5, 10)]
    public string say;
    public int charactersPerFrame = 1;
    public float speed = 1f;
    public string additive;

    // Start is called before the first frame update
    void Start()
    {
        architect = new TextArchitect(textMPro,say, additive, charactersPerFrame, speed);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            architect = new TextArchitect(textMPro, say, additive, charactersPerFrame, speed);
        }
    }
}
