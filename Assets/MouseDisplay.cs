using UnityEngine;
using UnityEngine.Tilemaps;

public class MouseDisplay : MonoBehaviour
{   
    //The tilemap data to work on
    private Tilemap map;
    public TMPro.TextMeshProUGUI TextMeshGraphic;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        map = FindAnyObjectByType<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pixelPos = Input.mousePosition;

        Vector3 worldSpacePositionOfMouse = Camera.main.ScreenToWorldPoint(pixelPos);
        worldSpacePositionOfMouse.z = 0;

        int tilePosX = (int)worldSpacePositionOfMouse.x;
        int tilePosY = (int)worldSpacePositionOfMouse.y;
        Vector3Int tilePos = new Vector3Int(tilePosX, tilePosY, 0); 
        transform.position = map.GetCellCenterWorld(tilePos);

        TileBase tile = map.GetTile(tilePos);
        if(tile != null )
        {
            TextMeshGraphic.text = tile.name;
        } else
        {
            TextMeshGraphic.text = "empty";
        }

        TextMeshGraphic.text += "\n" + tilePosX + "," + tilePosY; 
    }
}
