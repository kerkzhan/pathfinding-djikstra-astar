using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour 
{
	enum PathfindingType
	{
		Djikstra,
		AStar
	}


	public static int cols = 100;
	public static int rows = 100;
	public float frameRate = 0f;
	public static float width;
	public static float height;
	public static Vector2 offset;
	public Canvas restartText;
	public Text text;

	public GameObject cellPrefab;

	Cell[,] grid = new Cell[cols,rows];

	List<Cell> unsettledNodes = new List<Cell>();
	List<Cell> settledNodes = new List<Cell>();
	public List<Cell> route = new List<Cell>();

	Cell start;
	Cell end;

	Vector2 bottomLeftVec;
	Vector2 edgeVector;
	PathfindingType type;

	bool restart;


	int mouseCounter;


	void Start () 
	{
		restart = false;
		restartText.gameObject.SetActive(false);

		type = PathfindingType.Djikstra;
		float x = 100 * 0.0035f * 100/cols;
		cellPrefab.GetComponent<Transform>().localScale = new Vector3(x, x, x);

		Vector2 topRightCorner = new Vector2(1.0f, 1.0f);
		Vector2 bottomLeftCorner = new Vector2(0.0f, 0.0f);
		bottomLeftVec =  Camera.main.ViewportToWorldPoint(bottomLeftCorner);
		edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);


		offset.x = edgeVector.x - bottomLeftVec.x;
		offset.y = edgeVector.y - bottomLeftVec.y;
		width = offset.y/cols;
		height = offset.y/rows;

		for (int i = 0; i < cols; i++)
		{
			for (int j = 0; j < rows; j++)
			{
				Vector3 position = new Vector3((i * width - offset.x / 2  + cellPrefab.GetComponent<Transform>().localScale.x/2) + (offset.x/8), j * height - offset.y / 2 + cellPrefab.GetComponent<Transform>().localScale.x/2 + 0.25f);
				GameObject go = Instantiate(cellPrefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);

				grid[i,j] = new Cell(i, j, go);
			}
		}		



		start = grid[0,0];
		end = grid[cols/2, rows/2];


		start.cellType = Cell.CellType.Start;
		start.Show();

		end.cellType = Cell.CellType.End;
		end.Show();
	}
	
	// Update is called once per frame
	void Update () 
	{

		if (Input.GetKey(KeyCode.Space))
		{
			unsettledNodes.Add(start);
			if (type == PathfindingType.AStar)
			{
				StartCoroutine(AStar());
			}

			else
			{
				StartCoroutine(Djikstra());
			}

			if (restart == true)
			{
				Scene scene;
				scene = SceneManager.GetActiveScene();
				SceneManager.LoadScene(scene.name);
			}
		}

		if (Input.GetKey(KeyCode.Alpha3))
		{
			type = PathfindingType.Djikstra;
			text.text = "MODE \n Djikstra";
			print(type);
		}

		if (Input.GetKey(KeyCode.Alpha4))
		{
			type = PathfindingType.AStar;
			text.text = "MODE \n AStar";
			print(type);
		}

		if (Input.GetKey(KeyCode.Alpha1))
		{
			ChangeCellToStart(GetMouseGrid());
		}

		if (Input.GetKey(KeyCode.Alpha2))
		{
			ChangeCellToEnd(GetMouseGrid());
		}

		if(Input.GetMouseButton(0))
		{
			ChangeCell(GetMouseGrid());
			
		}	

		if(Input.GetMouseButtonUp(0))
		{
			mouseCounter = 0;

		}

	}

	int[] GetMouseGrid()
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		Cell firstCell = grid[0,0];
		Cell lastCell = grid[cols-1, cols-1];

		float x = Map(mousePos.x, firstCell.referencedCell.transform.position.x - firstCell.referencedCell.transform.localScale.x/2, lastCell.referencedCell.transform.position.x + lastCell.referencedCell.transform.localScale.x/2, 0f, 1f);
		float y = Map(mousePos.y, bottomLeftVec.y, edgeVector.y, 0f, 1f);

		x = x *cols;
		y = y *cols;

		x = Mathf.FloorToInt(x);
		y = Mathf.FloorToInt(y);

		int indexX = (int) x;
		int indexY = (int) y;

		print(indexX);

		int[] mouseToGrid = new int[2];
		mouseToGrid[0] = indexX;
		mouseToGrid[1] = indexY;
		return mouseToGrid;
	}
	void ChangeCellToStart(int[] gridIndex)
	{	
		int indexX = gridIndex[0];
		int indexY = gridIndex[1];

		start.cellType = Cell.CellType.Neutral;
		start.Show();
		grid[indexX,indexY].cellType = Cell.CellType.Start;
		start = grid[indexX,indexY];
		start.Show();
	}


	void ChangeCellToEnd(int[] gridIndex)
	{
		int indexX = gridIndex[0];
		int indexY = gridIndex[1];


		end.cellType = Cell.CellType.Neutral;
		end.Show();
		grid[indexX,indexY].cellType = Cell.CellType.End;
		end = grid[indexX,indexY];
		end.Show();
	}

	void ChangeCell(int[] gridIndex)
	{
		
		int indexX = gridIndex[0];
		int indexY = gridIndex[1];

		if (mouseCounter == 0)
		{
			if (grid[indexX,indexY].cellType == Cell.CellType.Neutral)
			{
				grid[indexX,indexY].cellType = Cell.CellType.Wall;
			}

			else if (grid[indexX,indexY].cellType == Cell.CellType.Wall)
			{
				grid[indexX,indexY].cellType = Cell.CellType.Neutral;
			}
				
			grid[indexX,indexY].Show();
		}
	}

	float Map(float value, float low1, float high1, float low2, float high2)
	{
		return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
	}

	IEnumerator Djikstra()
	{
		for (int i = 0; i < cols; i++)
		{
			for (int j = 0; j < rows; j++)
			{
				grid[i,j].GetNeighbours(grid);
			}
		}

		while (unsettledNodes.Count > 0)
		{
			if (unsettledNodes.Count > 0)
			{
				int shortestCell = 0;

				for (int i = 0; i < unsettledNodes.Count; i++)
				{
					if (unsettledNodes[i].dist < unsettledNodes[shortestCell].dist)
					{
						shortestCell = i;
						print("IM HERE");
					}
				}

				Cell currentCell = unsettledNodes[shortestCell];


				unsettledNodes.Remove(currentCell);
				settledNodes.Add(currentCell);

				for (int i = 0; i < currentCell.neighbours.Count; i++)
				{
					Cell neighbour = currentCell.neighbours[i];

					if (!settledNodes.Contains(neighbour))
					{
						int tempDistance = currentCell.dist + 10;

						if (unsettledNodes.Contains(neighbour))
						{
							if (tempDistance < neighbour.dist)
							{
								neighbour.dist = tempDistance;
							}
						}

						else
						{
							neighbour.dist = tempDistance;
							unsettledNodes.Add(neighbour);
						}

						if (neighbour.previous == null)
						{
							neighbour.previous = currentCell;
						}
					}

				}

				if (currentCell == end)
				{
					Cell temp = currentCell;
					route.Add(temp);

					while(temp.previous != null)
					{
						route.Add(temp.previous);
						temp = temp.previous;
					}
					print("I WON");

					restart = true;

					restartText.gameObject.SetActive(true);
					unsettledNodes.Clear();

				}
			}

			else
			{

			}

			for (int i = 0; i < cols; i++)
			{
				for (int j = 0; j < rows; j++)
				{
					//grid[i,j].cellType = Cell.CellType.Neutral;
					grid[i,j].Show();
				}
			}

			for (int i = 0; i < settledNodes.Count; i++)
			{
				settledNodes[i].cellType = Cell.CellType.Settled;
				settledNodes[i].Show();
			}

			for (int i = 0; i < unsettledNodes.Count; i++)
			{
				unsettledNodes[i].cellType = Cell.CellType.Unsettled;
				unsettledNodes[i].Show();
			}

			for (int i = 0; i < route.Count; i++)
			{
				route[i].cellType = Cell.CellType.Path;
				route[i].Show();
			}

			start.cellType = Cell.CellType.Start;
			start.Show();

			end.cellType = Cell.CellType.End;
			end.Show();

		yield return new WaitForSeconds(frameRate);
		}
	}

	int Distance(float a, float b, float c, float d)
	{
		return (int)Mathf.Sqrt((b-a)*(b-a) + (d-c)*(d-c));
	}

	int Heuristic(Cell a, Cell b)
	{
		int d;

		d = Distance(a.i, b.i, a.j, b.j);
		return d;
	}

	IEnumerator AStar()
	{
		for (int i = 0; i < cols; i++)
		{
			for (int j = 0; j < rows; j++)
			{
				grid[i,j].GetNeighbours(grid);
			}
		}

		while (unsettledNodes.Count > 0)
		{
			if (unsettledNodes.Count > 0)
			{
				int shortestCell = 0;

				for (int i = 0; i < unsettledNodes.Count; i++)
				{
					if (unsettledNodes[i].dist < unsettledNodes[shortestCell].dist)
					{
						shortestCell = i;
					}

					if (unsettledNodes[i].dist == unsettledNodes[shortestCell].dist)
					{
						if (this.unsettledNodes[i].g > this.unsettledNodes[shortestCell].g)
						{
							shortestCell = i;
						}
					}
				}

				Cell currentCell = unsettledNodes[shortestCell];


				unsettledNodes.Remove(currentCell);
				settledNodes.Add(currentCell);

				for (int i = 0; i < currentCell.neighbours.Count; i++)
				{
					Cell neighbour = currentCell.neighbours[i];

					if (!settledNodes.Contains(neighbour))
					{
						int tempG = currentCell.g + Heuristic(neighbour, currentCell);

						if (!unsettledNodes.Contains(neighbour))
						{
							unsettledNodes.Add(neighbour);

						}

						else if (tempG >= neighbour.g)
						{
							continue;
						}

					
						neighbour.g = tempG;
						neighbour.h = Heuristic(neighbour, end);
						neighbour.dist = neighbour.g + neighbour.h;
				

						if (neighbour.previous == null)
						{
							neighbour.previous = currentCell;
						}
					}

				}

				if (currentCell == end)
				{
					Cell temp = currentCell;
					route.Add(temp);

					while(temp.previous != null)
					{
						route.Add(temp.previous);
						temp = temp.previous;
					}
					print("I WON");
					restart = true;

					restartText.gameObject.SetActive(true);
					unsettledNodes.Clear();

				}
			}

			else
			{

			}

			for (int i = 0; i < cols; i++)
			{
				for (int j = 0; j < rows; j++)
				{
					//grid[i,j].cellType = Cell.CellType.Neutral;
					grid[i,j].Show();
				}
			}

			for (int i = 0; i < settledNodes.Count; i++)
			{
				settledNodes[i].cellType = Cell.CellType.Settled;
				settledNodes[i].Show();
			}

			for (int i = 0; i < unsettledNodes.Count; i++)
			{
				unsettledNodes[i].cellType = Cell.CellType.Unsettled;
				unsettledNodes[i].Show();
			}

			for (int i = 0; i < route.Count; i++)
			{
				route[i].cellType = Cell.CellType.Path;
				route[i].Show();
			}

			start.cellType = Cell.CellType.Start;
			start.Show();

			end.cellType = Cell.CellType.End;
			end.Show();

			yield return new WaitForSeconds(frameRate);
		}
	}
}
