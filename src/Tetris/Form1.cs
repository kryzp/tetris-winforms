using System.Runtime.InteropServices;

namespace Tetris
{
	public partial class Form1 : Form
	{
		private enum PieceType
		{
			EMPTY,
			SHADOW,
			LEFT_L,
			RIGHT_L,
			DIAG_L,
			DIAG_R,
			LONG_UP,
			JOINER,
			BLOCK,
			MAX,
		}

		private bool IsSolidPiece(PieceType p) => p != PieceType.EMPTY && p != PieceType.SHADOW && p != PieceType.MAX;

		private string[] tetrominos = new string[(int)PieceType.MAX];

		public const int WIDTH = 10;
		public const int HEIGHT = 20;

		public const int X_OFFSET = 10;
		public const int Y_OFFSET = 10;

		public const int STORAGE_X_OFFSET = X_OFFSET + (WIDTH * CELL_SIZE) + X_OFFSET;
		public const int STORAGE_Y_OFFSET = Y_OFFSET;

		public const int CELL_SIZE = 48;

		private int currentScore = 0;
		private int highScore = 0;

		private bool gamePaused = false; // pause the game

		private Graphics g;

		private int moveDownTickLimit = 20; // how many ticks must pass to move a block one unit down
		private int minimumMoveDownTickLimit = 5; // how many ticks must pass at minimum to move a block one unit down (after all the speeding up)
		private int moveDownTickCounter = 0; // ticks up and wraps around at the tick limit

		private int finalizeSettleTime = 50; // how long it takes to settle
		private int finalizeSettleCounter = 0;

		private PieceType currentPiece = PieceType.LEFT_L;
		private int currentPieceX = 0;
		private int currentPieceY = 0;
		private int currentPieceRotation; // 0 = 0 deg, 1 = 90 deg cw, 2 = 180 deg cw, 3 = 270 deg cw, and it wraps around at 4

		private PieceType inStorage = PieceType.EMPTY; // current piece in storage

		private PieceType[] board = new PieceType[WIDTH * HEIGHT];

		// ctor
		public Form1()
		{
			InitializeComponent();

			g = CreateGraphics();

			this.Paint += RenderBoard;

			this.gameTickTimer.Tick += GameTick;
			this.gameTickTimer.Interval = 1;
			this.gameTickTimer.Start();
		}

		// loads... the form?
		private void Form1_Load(object sender, EventArgs e)
		{
			ResetGame();
		}

		private void ResetGame()
		{
			// clear board
			for (int i = 0; i < WIDTH * HEIGHT; i++)
			{
				board[i] = PieceType.EMPTY;
			}

			// ensure width of the window is what it should be
			Width = X_OFFSET + (WIDTH * CELL_SIZE) + X_OFFSET + (4 * (CELL_SIZE + X_OFFSET));
			Height = Y_OFFSET + (HEIGHT * CELL_SIZE) + 8 * Y_OFFSET;

			// set up all the basic information about the game
			SetUpTetrominoData();
			ResetCurrentPieceData();

			this.gamePaused = false;
			this.currentPiece = PieceType.LEFT_L;
			this.highScore = this.highScore > this.currentScore ? this.highScore : this.currentScore;
			this.currentScore = 0;
			this.inStorage = PieceType.EMPTY;
			this.scoreLabel.Text = "0";
			this.highscoreLabel.Text = "Highscore: " + highScore.ToString();

			UpdateUpNextUIInfo();
		}

		// each tetromino has a 16 character string assigned to it which represents how it is laid out in the game world
		private void SetUpTetrominoData()
		{
			// left l tetromino
			tetrominos[(int)PieceType.LEFT_L] =
				"  X " +
				"  X " +
				" XX " +
				"    ";

			// right l tetromino
			tetrominos[(int)PieceType.RIGHT_L] =
				" X  " +
				" X  " +
				" XX " +
				"    ";

			// diag l tetromino
			tetrominos[(int)PieceType.DIAG_L] =
				" X  " +
				" XX " +
				"  X " +
				"    ";

			// diag r tetromino
			tetrominos[(int)PieceType.DIAG_R] =
				"  X " +
				" XX " +
				" X  " +
				"    ";

			// long up tetromino
			tetrominos[(int)PieceType.LONG_UP] =
				"  X " +
				"  X " +
				"  X " +
				"  X ";

			// "joiner" tetromino (literally no clue what it's called)
			tetrominos[(int)PieceType.JOINER] =
				"  X " +
				" XX " +
				"  X " +
				"    ";

			// block tetromino
			tetrominos[(int)PieceType.BLOCK] =
				"    " +
				" XX " +
				" XX " +
				"    ";
		}

		// draws a piece
		private void DrawPiece(int x, int y, int r, PieceType shapeType, PieceType displayType, PieceType[] outputGfx)
		{
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int globalX = x + j;
					int globalY = y + i;

					int primaryIdx = CalcTetrominoLocalIdx(j, i, r);
					int foreignIdx = (globalY * WIDTH) + globalX;

					char c = tetrominos[(int)shapeType][primaryIdx];

					if (c == 'X')
						outputGfx[foreignIdx] = displayType;
				}
			}
		}

		// responsible for drawing the entire board
		private void RenderBoard(object? sender, PaintEventArgs e)
		{
			// render board without current piece
			PieceType[] outputGraphics = new PieceType[WIDTH * HEIGHT];
			for (int i = 0; i < WIDTH * HEIGHT; i++)
				outputGraphics[i] = board[i];

			// render current piece "preview" at the bottom
			int tempBefore = currentPieceY;
			while (!IsCurrentPiecePositionInvalid(0, 1, currentPieceRotation))
				currentPieceY++;
			int previewPos = currentPieceY;
			currentPieceY = tempBefore;

			// add shadow / preview piece
			DrawPiece(currentPieceX, previewPos, currentPieceRotation, currentPiece, PieceType.SHADOW, outputGraphics);

			// render current piece "over" the board
			DrawPiece(currentPieceX, currentPieceY, currentPieceRotation, currentPiece, currentPiece, outputGraphics);

			// output the board to the screen
			for (int i = 0; i < HEIGHT; i++)
			{
				for (int j = 0; j < WIDTH; j++)
				{
					int idx = (i * WIDTH) + j;

					int xp = (j * CELL_SIZE) + X_OFFSET;
					int yp = (i * CELL_SIZE) + Y_OFFSET;

					PieceType pieceHere = outputGraphics[idx];

					g.FillRectangle(
						new SolidBrush(GetColourFromType(pieceHere)),
						new Rectangle(xp, yp, CELL_SIZE, CELL_SIZE)
					);
				}
			}

			// draw storage
			RenderStorage();
		}

		// draws the top right box containing what is currently in the players tetromino storage
		private void RenderStorage()
		{
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int localIdx = (i * 4) + j;

					int xp = (j * CELL_SIZE) + STORAGE_X_OFFSET;
					int yp = (i * CELL_SIZE) + STORAGE_Y_OFFSET;

					Brush cellBrush = new SolidBrush(Color.Black);

					if (IsSolidPiece(inStorage))
					{
						char cc = tetrominos[(int)inStorage][localIdx];
						cellBrush = new SolidBrush(cc == 'X' ? GetColourFromType(inStorage) : Color.Black);
					}

					g.FillRectangle(cellBrush, new Rectangle(xp, yp, CELL_SIZE, CELL_SIZE));
				}
			}
		}

		// input booleans to stop from a key being registered as being pressed multiple times when held down
		private bool isLeftDown = false;
		private bool isRightDown = false;
		private bool isXDown = false;
		private bool isZDown = false;
		private bool isCDown = false;

		// game tick function
		private void GameTick(object sender, EventArgs e)
		{
			if (gamePaused)
				return;

			// check if current piece should be settled down
			if (IsCurrentPiecePositionInvalid(0, 1, currentPieceRotation))
			{
				finalizeSettleCounter++;

				if (finalizeSettleCounter > finalizeSettleTime)
				{
					finalizeSettleCounter = 0;

					SettleCurrentPiece();
					ResetCurrentPieceData();

					if (CheckCurrentPieceForIntersectionWithOtherPieces())
						LoseGame();

					Invalidate();
				}
			}
			else
			{
				finalizeSettleCounter = 0;

				moveDownTickCounter++;

				int limit = moveDownTickLimit + (int)((float)Math.Min(1f, (float)currentScore / 35f) * (float)(minimumMoveDownTickLimit - moveDownTickLimit));

				if (IsKeyDown(Keys.Down))
					limit = (int)((float)moveDownTickLimit * 0.2f);

				if (moveDownTickCounter > limit)
				{
					moveDownTickCounter = 0;
					currentPieceY++;

					Invalidate();
				}
			}

			// quick skip down
			if (IsKeyDown(Keys.X))
			{
				if (!isXDown)
				{
					// keep going down until we hit an invalid position (aka: it has a tetromino piece or it is out of bounds)
					while (!IsCurrentPiecePositionInvalid(0, 1, currentPieceRotation))
						currentPieceY++;

					SettleCurrentPiece();
					ResetCurrentPieceData();

					if (CheckCurrentPieceForIntersectionWithOtherPieces())
						LoseGame();

					isXDown = true;
				}

				Invalidate();
			}
			else
			{
				isXDown = false;
			}

			// storage
			if (IsKeyDown(Keys.C))
			{
				if (!isCDown)
				{
					isCDown = true;

					PieceType tmp = inStorage;
					inStorage = currentPiece;

					if (tmp != PieceType.EMPTY)
					{
						currentPiece = tmp;
					}
					else
					{
						FetchNextPiece();
					}

					ResetCurrentPieceData();
				}

				Invalidate();
			}
			else
			{
				isCDown = false;
			}

			// rotational movement
			if (IsKeyDown(Keys.Z))
			{
				if (!isZDown && !IsCurrentPiecePositionInvalid(0, 0, currentPieceRotation + 1))
				{
					currentPieceRotation++;
					isZDown = true;
				}

				Invalidate();
			}
			else
			{
				isZDown = false;
			}

			// horizontal movement left
			if (IsKeyDown(Keys.Left))
			{
				if (!isLeftDown && !IsCurrentPiecePositionInvalid(-1, 0, currentPieceRotation))
				{
					currentPieceX--;
					isLeftDown = true;
				}

				Invalidate();
			}
			else
			{
				isLeftDown = false;
			}

			// horizontal movement right
			if (IsKeyDown(Keys.Right))
			{
				if (!isRightDown && !IsCurrentPiecePositionInvalid(+1, 0, currentPieceRotation))
				{
					currentPieceX++;
					isRightDown = true;
				}

				Invalidate();
			}
			else
			{
				isRightDown = false;
			}

			// check if any rows that have been created are full so that we can remove them
			CheckForRowsToCancel();
		}

		// assigns a colour to a tetromino piece type
		private Color GetColourFromType(PieceType type)
		{
			return type switch
			{
				PieceType.EMPTY => Color.Black,
				PieceType.SHADOW => Color.DarkGray,
				PieceType.LEFT_L => Color.Blue,
				PieceType.RIGHT_L => Color.Orange,
				PieceType.DIAG_L => Color.Cyan,
				PieceType.DIAG_R => Color.Yellow,
				PieceType.JOINER => Color.Magenta,
				PieceType.BLOCK => Color.Red,
				PieceType.LONG_UP => Color.Green
			};
		}

		// assigns a name to a tetromino piece type
		private string GetStringNameFromType(PieceType type)
		{
			return type switch
			{
				PieceType.EMPTY => "",
				PieceType.SHADOW => "",
				PieceType.LEFT_L => "Left L",
				PieceType.RIGHT_L => "Right L",
				PieceType.DIAG_L => "Left Diagonal",
				PieceType.DIAG_R => "Right Diagonal",
				PieceType.JOINER => "Joiner",
				PieceType.BLOCK => "Block",
				PieceType.LONG_UP => "Pole"
			};
		}

		// essentially returns a regular index into the current tetrominos string
		// but applies some rotation functions to it which flip the x and y
		// axis around to effectively rotate it
		// i basically expanded rotation matrices then added some constants to make it fit and look normal and not explode the application
		private int CalcTetrominoLocalIdx(int px, int py, int rot)
		{
			int idx = 0;

			switch (rot % 4)
			{
				case 0: // 0 deg cw
					idx = (py * 4) + px;
					break;

				case 1: // 90 deg cw
					idx = py - (px * 4);
					idx += 12;
					break;

				case 2: // 180 deg cw
					idx = -(py * 4) - px;
					idx += 15;
					break;

				case 3: // 270 deg cw
					idx = -py + (px * 4);
					idx += 3;
					break;
			}

			return idx;
		}

		// checks if the current piece is colliding at its position and given a rotation, in addition to a position offset (dx, dy)
		private bool IsCurrentPiecePositionInvalid(int dx, int dy, int rot)
		{
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int globalX = currentPieceX + j + dx;
					int globalY = currentPieceY + i + dy;

					int primaryIdx = CalcTetrominoLocalIdx(j, i, rot);
					int foreignIdx = (globalY * WIDTH) + globalX;

					char c = tetrominos[(int)currentPiece][primaryIdx];

					// only check if that part of the tetromino is actually a physical part of it
					if (c == 'X')
					{
						// out of bounds?
						if (globalX < 0 || globalX >= WIDTH || globalY < 0 || globalY >= HEIGHT)
							return true;

						// in the same position as an existing settled piece on the board?
						if (this.board[foreignIdx] != PieceType.EMPTY)
							return true;
					}
				}
			}

			return false;
		}

		// settles the current piece
		private void SettleCurrentPiece()
		{
			// write current piece directly into the board
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int globalX = currentPieceX + j;
					int globalY = currentPieceY + i;

					int primaryIdx = CalcTetrominoLocalIdx(j, i, currentPieceRotation);
					int foreignIdx = (globalY * WIDTH) + globalX;

					char c = tetrominos[(int)currentPiece][primaryIdx];

					if (c == 'X')
						this.board[foreignIdx] = currentPiece;
				}
			}

			FetchNextPiece();
		}

		// move onto next piece
		private void FetchNextPiece()
		{
			// move current piece forward by one
			currentPiece = (PieceType)(((int)currentPiece + 1) % ((int)PieceType.MAX));

			if (currentPiece == PieceType.EMPTY)
				currentPiece = (PieceType)((int)currentPiece + 2); // skip EMPTY and SHADOW

			// update ui
			UpdateUpNextUIInfo();
		}

		// looooonnnnnggg name but it's only used once so whatever
		private bool CheckCurrentPieceForIntersectionWithOtherPieces()
		{
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int globalX = currentPieceX + j;
					int globalY = currentPieceY + i;

					int localIdx = CalcTetrominoLocalIdx(j, i, currentPieceRotation);
					int globalIdx = (globalY * WIDTH) + globalX;

					bool bbLocal = tetrominos[(int)currentPiece][localIdx] == 'X';
					bool bbGlobal = IsSolidPiece(board[globalIdx]);

					if (bbLocal && bbGlobal)
						return true;
				}
			}

			return false;
		}

		// make sure the up next label is updated properly
		private void UpdateUpNextUIInfo()
		{
			PieceType nextPiece = (PieceType)(((int)currentPiece + 1) % ((int)PieceType.MAX));

			if (nextPiece == PieceType.EMPTY)
				nextPiece = (PieceType)((int)nextPiece + 2); // skip EMPTY and SHADOW

			upNextLabel.Text = "Up Next: " + GetStringNameFromType(nextPiece);
		}

		// check for rows that can be canceled out
		private void CheckForRowsToCancel()
		{
			List<int> rowsToCancel = new List<int>();

			// identify all rows that need to be removed
			// and keep track of them in the list
			for (int i = 0; i < HEIGHT; i++)
			{
				bool full = true;

				for (int j = 0; j < WIDTH; j++)
				{
					if (board[(i * WIDTH) + j] == PieceType.EMPTY)
					{
						full = false;
						break;
					}
				}

				if (full)
					rowsToCancel.Add(i);
			}

			// loop through list moving all the elements above each row that needs
			// to be removed into said row
			foreach (var r in rowsToCancel)
			{
				IncrementScore(1);

				for (int i = r; i > 0; i--)
				{
					for (int j = 0; j < WIDTH; j++)
					{
						board[(i * WIDTH) + j] = board[((i - 1) * WIDTH) + j];
					}
				}
			}
		}

		// ...loses the game?
		private void LoseGame()
		{
			gamePaused = true;

			string extra = currentScore > highScore ? ", this is a new highscore!" : ", unfortunately this isn't a new highscore...";

			if (currentScore == highScore)
				extra = ", this is exactly your current highscore!";

			MessageBox.Show("Game Lost! Score: " + currentScore.ToString() + extra, "Game Lost!", MessageBoxButtons.OK);

			ResetGame();
		}

		// increments the current score by n and updates the score label
		private void IncrementScore(int n = 1)
		{
			this.currentScore += n;
			this.scoreLabel.Text = currentScore.ToString();
		}

		// reset current piece back to the top when getting a new one
		private void ResetCurrentPieceData()
		{
			this.currentPieceX = 3;
			this.currentPieceY = 0;
			this.currentPieceRotation = 0;
		}

		// called when the resetbutton is clicked (note: this also happens when spacebar is pressed!)
		private void resetButton_Click(object sender, EventArgs e)
		{
			gamePaused = true;

			DialogResult dr = MessageBox.Show("Are you sure you want to reset the game?", "Reset", MessageBoxButtons.YesNo);

			// only reset if player has confirmed so
			if (dr == DialogResult.Yes)
				ResetGame();

			gamePaused = false;
		}

		// helper input functions
		// written in this weird way because the built-in
		// key checking interrupts other processes and isn't
		// really that good for games and such
		// so instead i load in the user32.dll which gives
		// me access to GetKeyState() which actually polls the OS
		// on the state of the keyboard, then i wrap it in a helper
		// "IsKeyDown" function to make it more readable.
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern short GetKeyState(int keyCode);
		public static bool IsKeyDown(Keys key) => Convert.ToBoolean(GetKeyState((int)key) & 0x8000);
	}
}
