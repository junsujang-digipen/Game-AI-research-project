using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Class for a 1x1 Cell
/// </summary>
public class BaseCell
{
	[Tooltip("The edges of the BaseCell.")]
	private char _edges;

	public void SetEdges(int top, int right, int bottom, int left) {
		_edges = (char)(top + (right << 1) + (bottom << 2) + (left << 3));
	}

	[Tooltip("The offset of the BaseCell.")]
	public Vector2Int _offset;

	/// <summary>
	/// Gets the Top edge of the Cell.
	/// </summary>
	/// <returns></returns>
	public bool Top()
	{
		return (_edges & 0b1) > 0;
	}

	/// <summary>
	/// Gets the Right edge of the Cell.
	/// </summary>
	/// <returns></returns>
	public bool Right()
	{
		return (_edges & 0b10) > 0;
	}

	/// <summary>
	/// Gets the Bottom edge of the Cell.
	/// </summary>
	/// <returns></returns>
	public bool Bottom()
	{
		return (_edges & 0b100) > 0;
	}

	/// <summary>
	/// Gets the Left edge of the Cell.
	/// </summary>
	/// <returns></returns>
	public bool Left() {
		return (_edges & 0b1000) > 0;
	}

	public bool NoWall()
	{
		return _edges == 0;
	}
}

/// <summary>
/// Class for Cells that are larger then 1x1
/// </summary>
public class Cell
{
	
	[Tooltip("The list of tiles in the Cell")]
	public List<BaseCell> _cells;

	[Tooltip("The Tile (image) of the cell")]
	public Tile _tile;

    public Cell(Tile tile = null)
	{
		_tile = tile;
    }
    public Cell(Tile tile, float rotation)
    {
		_tile = tile;
		SetRotation(rotation);
	}
    /// <summary>
    /// Set the rotation of the tile, NEEDS TILE FIRST!!!
    /// </summary>
    /// <param name="rotation"></param>
    public void SetRotation(float rotation)
	{
		if(_tile)
            _tile.transform = (Matrix4x4.Translate(_tile.transform.GetPosition()) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, -rotation)));
    }
}