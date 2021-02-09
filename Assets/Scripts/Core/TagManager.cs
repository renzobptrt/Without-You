using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagManager : MonoBehaviour
{
    public static string[] SplitByTags(string targetText)
    {
        return targetText.Split(new char[2] { '<', '>' });
    }
}
