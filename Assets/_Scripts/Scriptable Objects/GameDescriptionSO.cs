using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName ="Description", menuName ="Game Description")]
public class GameDescriptionSO : ScriptableObject
{   
    public string game_description;
    public Sprite game_image;
}
