using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName ="Description", menuName ="Game Description")]
public class GameDescriptionSO : ScriptableObject
{
    public string game_title;
    public string game_description;
    public string game_tutorial;
    public Sprite game_image;
}
