using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputScreenTesting : MonoBehaviour
{

    public string displayText = "";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            InputScreen.Show(displayText);

        if (Input.GetKeyDown(KeyCode.Return) && InputScreen.isWaitingForUserInput)
        {
            InputScreen.instance.Accept();
            print("Tu nombre es: " + InputScreen.currentInput);
        }

    }

}
