using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Probably not needed in own script.
/// 
/// Holds function to place a cell.
/// </summary>
public class PlaceCell : MonoBehaviour
{
    [Tooltip("The main tilemap from the grid object.")]
    [SerializeField]
    private Tilemap _tilemap;

    [SerializeField]
    private AllCells _allCells;

    static int _zPos = 0;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetMouseButtonDown(0)) 
        {
            Cell cell = new Cell(Instantiate(Resources.Load<Tile>("Tiles/1x1/1x1_1")), 0);
            //cell.SetRotation(90f);
            Vector3Int asdad = _tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            asdad.z = 0;
            PlaceCellOnGrid(asdad, cell);
        }
        if (Input.GetMouseButtonDown(1)) {
            Cell cell = new Cell(Instantiate(Resources.Load<Tile>("Tiles/1x1/1x1_1")), 90);
            Vector3Int asdad = _tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            asdad.z = 1;
            PlaceCellOnGrid(asdad, cell);
        }*/
    }

    /// <summary>
    /// Not sure if this is correct.
    /// Places a cell on the Tile Map based on a position.
    /// </summary>
    /// <param name="position">The tile position to place the call at</param>
    /// <param name="cell">The cell to place</param>
    public void PlaceCellOnGrid(Vector3Int position, Cell cell)
    {
        if(cell._tile == null) 
        { 
            Debug.Log("Cell is Null");
            return; 
        }
        position.z = _zPos++;
        _tilemap.SetTile(position, cell._tile);
    }

}
