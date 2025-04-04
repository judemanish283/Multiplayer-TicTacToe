using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] Image tileImage;

    [SerializeField] private Vector2Int tileCoord;

    [SerializeField] public Button button;

    public void SetImage(Sprite spriteToSet) 
    {
        tileImage.sprite = spriteToSet;
    }

    public Vector2Int GetTileCoord() => tileCoord;
}
