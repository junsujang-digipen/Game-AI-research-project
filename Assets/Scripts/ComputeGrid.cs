using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
namespace WFC {

	public class ComputeGrid : MonoBehaviour {
		public int row;
		public int col;

		private int MAXSIZE = 30;

		private List<List<CellInformation>> _cellInfo = new List<List<CellInformation>>();

		private List<List<GameObject>> _textList = new List<List<GameObject>>();

		[SerializeField]
		private Tilemap _tilemap;

		[SerializeField]
		private AllCells _allCells;

		[SerializeField]
		private PlaceCell _placeCell;

		[SerializeField]
		private Canvas _canvas;

		[SerializeField]
		private GameObject _textPrefab;

		[SerializeField]
		private Transform _TileGridTrans;

		enum BaseCellType : int {
			none = -1,
			noWall = 2,
			E = 3,
			N = 5,
			W = 7,
			S = 11,
			EN = E * N,
			EW = E * W,
			ES = E * S,
			NW = N * W,
			NS = N * S,
			WS = W * S,
			ENW = EN * W,
			ENS = EN * S,
			EWS = EW * S,
			NWS = NW * S,
			ENWS = ENW * S
		}
		private List<List<BaseCellType>> confirmedBaseCellInfo = new List<List<BaseCellType>>();
		void Start() {
			// Scale the tile grid
			float scale = 10.0f / row;
			_TileGridTrans.localScale = new Vector3(scale * 2, scale * 2, scale * 2);

			for (int i = 0; i < row; ++i) {
				this._cellInfo.Add(new List<CellInformation>());
				this.confirmedBaseCellInfo.Add(new List<BaseCellType>());
				for (int j = 0; j < col; ++j) {
					this._cellInfo[i].Add(new CellInformation { });
					this.confirmedBaseCellInfo[i].Add(BaseCellType.none);
				}
			}

			// Create the text grid
			Vector3 startPos = _tilemap.CellToWorld(new Vector3Int(0, 0, 0));
			for (int i = 0; i < MAXSIZE; ++i) {
				if (_textList.Count <= i) {
					_textList.Add(new List<GameObject>());
				}
				for (int j = 0; j < MAXSIZE; ++j) {
					if (_textList[i].Count <= j) {
						_textList[i].Add(Instantiate(_textPrefab, _canvas.transform));
					}
					if (j < col && i < row) {
						_textList[i][j].transform.position = Camera.main.WorldToScreenPoint(startPos + (new Vector3(j * 2 + 1, i * 2 + 1) * scale));
						_textList[i][j].GetComponent<TMPro.TextMeshProUGUI>().text = "" + (i * col + j);
						_textList[i][j].GetComponent<TMPro.TextMeshProUGUI>().fontSize = 40 * scale;
					} else {
						_textList[i][j].transform.position = new Vector3(-100, -100);
					}
				}
			}
		}

		public void SetDimentions(float size) {
			Unset();
			row = col = (int)size;
			confirmedBaseCellInfo.Clear();
			_cellInfo.Clear();
			Start();
		}

		private void UpdateText() {
			for (int i = 0; i < row; ++i) {
				for (int j = 0; j < col; ++j) {
					int canadits = _cellInfo[i][j].candidates.Count;
					if (canadits > 0)
						_textList[i][j].GetComponent<TMPro.TextMeshProUGUI>().text = "" + canadits;
					else
						_textList[i][j].GetComponent<TMPro.TextMeshProUGUI>().text = "1";
				}
			}
		}

		public void BasicSolve() {
			debugSolve = false;
			oneStepDebugSolve = false;
			Unset();
			SetUp();
			tileUpdate();
			solve();
		}
		public void DebugSolve() {
			debugSolve = true;
			oneStepDebugSolve = false;
			Unset();
			SetUp();
			tileUpdate();
		}
		public void OneStepDebugSolve() {
			oneStep = true;
			if (oneStepDebugSolve == false) {
				oneStepDebugSolve = true;
				debugSolve = true;
				Unset();
				SetUp();
				tileUpdate();
			}
		}
		private bool debugSolve = false;
		private bool oneStepDebugSolve = false;
		private bool oneStep = false;
		void Update() {
			/*if(Input.GetMouseButtonDown(0)) 
			{
				BasicSolve();
			}
			if(Input.GetMouseButtonDown(1)) 
			{
				DebugSolve();
			}
			if(Input.GetMouseButtonDown(2)) 
			{
				OneStepDebugSolve();
			}*/
			if (Input.GetKeyUp(KeyCode.Escape)) {
				Application.Quit();
            }
			if (oneStepDebugSolve == true) {
				if (oneStep == true) {
					oneStep = false;
					if (solve() == true) {
						oneStepDebugSolve = false;
						debugSolve = false;
					}
				}
			} else if (debugSolve == true) {
				if (solve() == true) debugSolve = false;
			}

		}
		void tileUpdate() {
			for (int i = 0; i < row; ++i) {
				for (int j = 0; j < col; ++j) {
					for (int candidateIndex = 0; candidateIndex < _cellInfo[i][j].candidates.Count; candidateIndex++) {
						if (!_cellInfo[i][j].isChoosen && !checkNearbyTiles(i, j, _allCells._cells[_cellInfo[i][j].candidates[candidateIndex].cellType])) {
							_cellInfo[i][j].candidates.RemoveAt(candidateIndex); //remove that candidate
							candidateIndex--;
						}
					}
				}
			}
		}

		bool checkNearbyTiles(int targetRow, int targetCol, Cell candidate) //target row and column should be origin's located.
		{
			for (int baseCellIndex = 0; baseCellIndex < candidate._cells.Count; baseCellIndex++) // Solve all the basecell in a candidate cell
			{
				int newRow = targetRow + candidate._cells[baseCellIndex]._offset.y;
				int newCol = targetCol + candidate._cells[baseCellIndex]._offset.x;
				if (newRow < 0 || newRow >= row || newCol < 0 || newCol >= col || confirmedBaseCellInfo[newRow][newCol] != BaseCellType.none) return false; // When exceed end of grid.
				if (newRow != row - 1) {
					int targetNorth = (int)confirmedBaseCellInfo[newRow + 1][newCol];
					bool selfNorthWall = candidate._cells[baseCellIndex].Top();
					bool targetSouthWall = targetNorth % (int)BaseCellType.S == 0;
					if (targetNorth != -1 && selfNorthWall != targetSouthWall) {
						return false;
					}
				}
				if (newRow != 0) {
					int targetSouth = (int)confirmedBaseCellInfo[newRow - 1][newCol];
					bool selfSouthWall = candidate._cells[baseCellIndex].Bottom();
					bool targetNorthWall = targetSouth % (int)BaseCellType.N == 0;
					if (targetSouth != -1 && selfSouthWall != targetNorthWall) {
						return false;
					}
				}
				if (newCol != col - 1) {
					int targetEast = (int)confirmedBaseCellInfo[newRow][newCol + 1];
					bool selfEastWall = candidate._cells[baseCellIndex].Right();
					bool targetWestWall = targetEast % (int)BaseCellType.W == 0;
					if (targetEast != -1 && selfEastWall != targetWestWall) {
						return false;
					}
				}
				if (newCol != 0) {
					int targetWest = (int)confirmedBaseCellInfo[newRow][newCol - 1];
					bool selfWestWall = candidate._cells[baseCellIndex].Left();
					bool targetEastWall = targetWest % (int)BaseCellType.E == 0;
					if (targetWest != -1 && selfWestWall != targetEastWall) {
						return false;
					}
				}
			}
			return true;
		}

		// Solve the grid
		public bool solve() {
			int i = 0, j = 0;
			while (getLowestEntropyCell(ref i, ref j) == true) {
				// check Candidate counts
				//Debug.Log("-----------------------------------");
				int type = Random.Range(0, _cellInfo[i][j].candidates.Count - 1);
				ConfirmCell(i, j, type);
				//Debug.Log("placing cellType: " + cellInfo[i][j].candidates[0].cellType);
				//Debug.Log("-----------------------------------");

				tileUpdate();
				UpdateText();
				if (debugSolve == true) {
					//wait(5.0f);
					return false;
				}
			}
			return true;
		}
		private void SetUp() {
			for (int i = 0; i < row; ++i) {
				for (int j = 0; j < col; ++j) {
					for (int k = 0; k < _allCells._cells.Count; ++k) {
						_cellInfo[i][j].candidates.Add(new CellCandidate(k, 0));
					}
				}
			}
		}
		private void Unset() {
			//clear
			_tilemap.ClearAllTiles();
			for (int i = 0; i < row; ++i) {
				for (int j = 0; j < col; ++j) {
					ReSetBaseCell(i, j);
				}
			}
		}
		// Apply the cell in the position
		public void ConfirmCell(int i, int j, int candidateID) {
			//Debug.Log("i: " + i + ", j: " + j + ", cellType: " + candidateID);
			int CellID = _cellInfo[i][j].candidates[candidateID].cellType;
			//LogCell(CellID);

			foreach (BaseCell baseCell in _allCells._cells[CellID]._cells) {
				int newI = i + baseCell._offset.y;
				int newJ = j + baseCell._offset.x;
				//Debug.Log("new i: " + newI + ", new j: " + newJ);
				SetBaseCell(newI, newJ, GetBaseCellType(baseCell));
			}
			_placeCell.PlaceCellOnGrid(new Vector3Int(j, i, 0), _allCells._cells[CellID]);
		}
		private void ReSetBaseCell(int i, int j) {
			_cellInfo[i][j].isChoosen = false;
			_cellInfo[i][j].candidates.Clear();
			confirmedBaseCellInfo[i][j] = BaseCellType.none;
		}
		private void SetBaseCell(int i, int j, BaseCellType BaseCellID) {
			_cellInfo[i][j].isChoosen = true;
			_cellInfo[i][j].candidates.Clear();
			confirmedBaseCellInfo[i][j] = BaseCellID;
		}
		private BaseCellType GetBaseCellType(BaseCell baseCell) {
			int type = 1;
			type *= baseCell.Top() == true ? (int)BaseCellType.N : 1;
			type *= baseCell.Right() == true ? (int)BaseCellType.E : 1;
			type *= baseCell.Left() == true ? (int)BaseCellType.W : 1;
			type *= baseCell.Bottom() == true ? (int)BaseCellType.S : 1;
			type = type == 1 ? 2 : type;
			return (BaseCellType)type;
		}
		private bool getLowestEntropyCell(ref int r, ref int c) {
			int lowest = int.MaxValue;
			bool success = false;
			for (int i = 0; i < row; ++i) {
				for (int j = 0; j < col; ++j) {
					if (this._cellInfo[i][j].isChoosen == false
					&& this._cellInfo[i][j].candidates.Count < lowest) {
						lowest = this._cellInfo[i][j].candidates.Count;
						r = i;
						c = j;
						success = true;
					}
				}
			}
			return success;
		}
		private IEnumerator wait(float time) {
			Debug.Log("Wait");

			yield return new WaitForSeconds(time);

			Debug.Log("Wait end");
		}
		private void LogCell(int CellID) {
			int r = 0;
			int c = 0;
			foreach (BaseCell baseCell in _allCells._cells[CellID]._cells) {
				r = r < baseCell._offset.y ? baseCell._offset.y : r;
				c = c < baseCell._offset.x ? baseCell._offset.x : c;
				Debug.Log("Cell contents: x = " + baseCell._offset.x + " y = " + baseCell._offset.y);
			}
		}
	}
}