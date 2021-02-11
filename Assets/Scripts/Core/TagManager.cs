using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagManager : MonoBehaviour
{   
    //Función para reemplazar los tags correspondientes
    public static void Inject(ref string s)
    {
        if (!s.Contains("["))
            return;

        //Reemplaza mainCharName con el nombre actual del jugador
        s = s.Replace("[mainCharName]", "Favio");

    }

    public static string[] SplitByTags(string targetText)
    {
        return targetText.Split(new char[2] { '<', '>' });
    }
}
