using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class AllCells : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The JSON file path relative to the project diretory")]
	private string _JSONFile;

	[Tooltip("The list of all cells that can be placed")]
	public List<Cell> _cells;

	// Start is called before the first frame update
	void Start() {
		// Load the JSON file as a string
		string json = File.ReadAllText(_JSONFile);
		// Deserialize the JSON into an object
		TotalCellsData totalCellsData = JsonUtility.FromJson<TotalCellsData>(json);

		// Create a Cell for each TotalCells in the JSON and store them in the list
		_cells = new();
		int maxRots;
		foreach (JSONCells jsonCells in totalCellsData.AllCells) {
			maxRots = 1;
			if (jsonCells.AllRotations) {
				maxRots = 4;
			} else if (jsonCells.HalfRotations) {
				maxRots = 2;
			}

			for (int rotation = 0; rotation < maxRots; ++rotation) {
				Cell cell = new Cell(Instantiate(Resources.Load<Tile>(jsonCells.Tile)), rotation * 90);

				cell._cells = new();
				foreach (JSONBaseCells jsonBaseCells in jsonCells.Cells) {
					BaseCell baseCell = new();
					baseCell._offset = new(jsonBaseCells.Offset[0], jsonBaseCells.Offset[1]);

					// Rotate the cell offset
					for (int i = 0; i < rotation; i++) {
						int oldX = baseCell._offset.x;
						baseCell._offset.x = baseCell._offset.y;
						baseCell._offset.y = -oldX;
					}
					
					// Convert the edges array to a char
					baseCell.SetEdges(jsonBaseCells.Edges[((0 - rotation) + 4) % 4], jsonBaseCells.Edges[((1 - rotation) + 4) % 4], 
						jsonBaseCells.Edges[((2 - rotation) + 4) % 4], jsonBaseCells.Edges[((3 - rotation) + 4) % 4]);
					cell._cells.Add(baseCell);
				}
				_cells.Add(cell);
			}
		}
	}
}

// Classes for deserializing the JSON
[System.Serializable]
public class TotalCellsData {
	public List<JSONCells> AllCells;
}

[System.Serializable]
public class JSONCells {
	public string Tile;
	public bool AllRotations;
	public bool HalfRotations;
	public List<JSONBaseCells> Cells;
}

[System.Serializable]
public class JSONBaseCells {
	public int[] Edges;
	public int[] Offset;
}
