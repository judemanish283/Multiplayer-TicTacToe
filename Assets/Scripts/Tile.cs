using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] Vector2 tilePos;

    public Vector2 GetTilePos() => tilePos;

    public void UpdateImage()
    {
        gameObject.GetComponent<Image>().color = Color.white;
    }
}
