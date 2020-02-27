using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
	public enum CellType
	{
		Neutral,
		Start,
		End,
		Unsettled,
		Settled,
		Path,
		Wall
	}

	public int dist;
	public int g;
	public int h;
	public Cell previous;
	public int i;
	public int j;
	public GameObject referencedCell;
	public CellType cellType;
	public List<Cell> neighbours = new List<Cell>();

	public Cell(int _i, int _j, GameObject cell)
	{
		dist = 0;
		i = _i;
		j = _j;
		referencedCell = cell;

	}

	public void Show()
	{
		if (cellType == CellType.Unsettled)
		{
			referencedCell.GetComponentInChildren<SpriteRenderer>().color = Color.cyan;
		}

		else if (cellType == CellType.Settled)
		{
			referencedCell.GetComponentInChildren<SpriteRenderer>().color = new Color(1.0f, 1.0f, 0.6f);
		}

		else if (cellType == CellType.Start)
		{
			referencedCell.GetComponentInChildren<SpriteRenderer>().color = Color.green;
		}

		else if (cellType == CellType.End)
		{
			referencedCell.GetComponentInChildren<SpriteRenderer>().color = Color.red;
		}

		else if (cellType == CellType.Path)
		{
			referencedCell.GetComponentInChildren<SpriteRenderer>().color = Color.blue;
		}

		else if (cellType == CellType.Neutral)
		{
			referencedCell.GetComponentInChildren<SpriteRenderer>().color = Color.white;
		}
		else if (cellType == CellType.Wall)
		{
			referencedCell.GetComponentInChildren<SpriteRenderer>().color = Color.gray;
		}

	}

	public void GetNeighbours(Cell[,] grid)
	{
		if (i < GameManager.cols - 1)
		{
			if (grid[i + 1, j].cellType != CellType.Wall)
			{
				MonoBehaviour.print("OHSHIT");
				neighbours.Add(grid[i + 1, j]);
			}
		}

		if (i > 0)
		{
			if (grid[i - 1, j].cellType != CellType.Wall)
			{
				neighbours.Add(grid[i - 1, j]);
			}
		}

		if (j < GameManager.rows - 1)
		{
			if (grid[i, j + 1].cellType != CellType.Wall)
			{
				neighbours.Add(grid[i, j + 1]);
			}
		}

		if (j > 0)
		{
			if (grid[i, j - 1].cellType != CellType.Wall)
			{
				neighbours.Add(grid[i, j - 1]);
			}

		}

		if (i > 0 && j > 0)
		{
			if (grid[i - 1, j - 1].cellType != CellType.Wall)
			{
				neighbours.Add(grid[i - 1, j - 1]);
			}
		}

		if (i < GameManager.cols - 1 && j > 0)
		{
			if (grid[i + 1, j - 1].cellType != CellType.Wall)
			{
				neighbours.Add(grid[i + 1, j - 1]);
			}
		}

		if (i > 0  && j < GameManager.rows - 1)
		{
			if (grid[i - 1, j + 1].cellType != CellType.Wall)
			{
				neighbours.Add(grid[i - 1, j + 1]);
			}
		}

		if (i < GameManager.cols - 1 && j < GameManager.rows - 1)
		{
			if (grid[i + 1, j + 1].cellType != CellType.Wall)
			{
				neighbours.Add(grid[i + 1, j + 1]);
			}
		}


	}
}
