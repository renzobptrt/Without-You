using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Responsible for adding and mainting characters in the scene
public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;

    public RectTransform characterPanel;

    public List<Character> characters = new List<Character>();
    public Dictionary<string, int> characterDictionary = new Dictionary<string, int>();

    private void Awake()
    {
        instance = this;
    }

    public Character GetCharacter(string characterName, bool createCharacterIfDoesNotExits = true, bool enableCreatedCharacterOnStart = true)
    {
        int index = -1;
        if (characterDictionary.TryGetValue(characterName, out index))
        {
            return characters[index];
        }
        else if(createCharacterIfDoesNotExits)
        {
            return CreateCharacter(characterName, enableCreatedCharacterOnStart);
        }

        return null;
    }

    public Character CreateCharacter(string characterName, bool enableOnStart = true)
    {
        Character newCharacter = new Character(characterName,enableOnStart);
        characterDictionary.Add(characterName, characters.Count);
        characters.Add(newCharacter);
        return newCharacter;
    }

    public class CHARACTERPOSITIONS
    {
        public Vector2 bottonLeft = new Vector2(0, 0);
        public Vector2 topRight = new Vector2(1f, 1f);
        public Vector2 center = new Vector2(0.5f, 0.5f);
        public Vector2 bottonRight = new Vector2(1f, 0);
        public Vector2 topLeft = new Vector2(0, 1f);
    }

    public static CHARACTERPOSITIONS characterPositions = new CHARACTERPOSITIONS();
    public class CHARACTEREXPRESSIONS
    {
        public string normal = "Normal";
        public string shocked = "Shocked";
    }

    public static CHARACTEREXPRESSIONS characterExpressions = new CHARACTEREXPRESSIONS();
}
