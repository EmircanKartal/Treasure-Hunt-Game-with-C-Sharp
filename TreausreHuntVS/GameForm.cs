using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;
using System.Threading;
using System.Drawing.Drawing2D;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;
using System.Media;
using System.IO;
using TreausreHuntVS.Properties;
using WinForms = System.Windows.Forms;




namespace TreausreHuntVS
{
    public partial class GameForm : Form
    {
        private List<(bool, string)> cellImageList = new List<(bool, string)>();
        private Point playerPosition;
        private Point previousPlayerPosition;
        private const int cellWidth = 75; // Adjusted for better visibility
        private const int cellHeight = 75; // Adjusted for better visibility
        private const int rows = 100; // Grid size
        private const int columns = 100; // Grid size
        private int[,] matrix = new int[rows, columns];
        Dictionary<(int, int), string> extendedMatrix;
        private Image explorer;
        private Image fogImage;
        private Image rockImage;
        private Image treeImage;
        private Image coinImage;
        private Image chestImage;
        private Image chestGolden;
        private Image chestSilver;
        private Image chestEmerald;
        private Image chestBronze;
        private Image wintertreeImage;
        private Image summertreeImage;
        private Image winterMountainImage;
        private Image summerMountainImage;
        private Image beeImage;
        private Image birdImage;
        private string playerName;
        private WindowsMediaPlayer player = new WindowsMediaPlayer();
        private WindowsMediaPlayer soundEffectsPlayer = new WindowsMediaPlayer();
        private bool[,] visitedCells;
        private Point cameraPosition;
        public int treasureFound = 0;
        public int ChestCounter = 0;
        public int TotalChestCount = 20;
        public int NumberOfCoin = 8;
        public Point initposition = new Point(0, 0);
        public Tree tree;
        public Graphics g1;
        private List<Obstacle> obstacles = new List<Obstacle>();
        private bool isMinimap = false;
        private bool isRestarting = false;
        private RichTextBox logTextBox;
        private DateTime startTime;
        WinForms::Timer timer = new WinForms::Timer();


        public GameForm(String playerName)
        {
            extendedMatrix = InitializeMapping(50, 50, "default");

            this.DoubleBuffered = true;
            InitializeComponent();
            InitializeMatrix();
            Thread thread = new Thread(InitializeRandomObstacles);
            thread.Start();
            thread.Join();
            
            richTextBox1.AppendText("Logger\n\n");
            // Start the timer
            startTime = DateTime.Now;
            timer.Start();

            richTextBox1.GotFocus += (sender, e) => {
                this.ActiveControl = null; // Redirect focus to the form or another control
            };
            this.PlayerName.Text = "PLAYER: " + playerName;
            this.playerName = playerName;
            this.label2.Text = "Treasures Founded: " + treasureFound + "/" + TotalChestCount;
            label1.Visible = true;
            label1.Text = "ID: " + GenerateRandomID();
            Random rand = new Random();
            int position1 = rand.Next(0, 50);
            int position2 = rand.Next(0, 50);

            playerPosition = new Point(position1, position2); // Starting position of the player

            // Preload images

            explorer = Resource.Explorer;
            fogImage = Resource.fog;
            rockImage = Resource.rock;
            treeImage = Resource.tree;
            coinImage = Resource.coin;
            chestGolden = Resource.GoldChest;
            chestSilver = Resource.silverChest;
            chestEmerald = Resource.emeraldChest;
            chestBronze = Resource.BronzeChest;
            wintertreeImage = Resource.winterTree;
            summertreeImage = Resource.summerTree;
            winterMountainImage = Resource.winterMountain;
            summerMountainImage = Resource.summerMountain;
            beeImage = Resource.bee;
            birdImage = Resource.bird;
            tree = new Tree(Resource.tree, initposition);
            Random rnd = new Random();
            string[] chestImages = {
                 @"C:\Dev\TreausreHuntVS\TreausreHuntVS\Assets\GoldChest.jpg",
                 @"C:\Dev\TreausreHuntVS\TreausreHuntVS\Assets\BronzeChest.jpg",
                 @"C:\Dev\TreausreHuntVS\TreausreHuntVS\Assets\silverChest.jpg",
                 @"C:\Dev\TreausreHuntVS\TreausreHuntVS\Assets\emeraldChest.jpg"
             };
            chestImage = Image.FromFile(chestImages[rnd.Next(chestImages.Length)]);
            this.KeyPreview = true;

            // Subscribe to key down event to capture key presses
            this.KeyDown += new KeyEventHandler(GameForm_KeyDown);
            this.trackBar1.ValueChanged += new EventHandler(volumeTrackBar_ValueChanged);
            this.pictureBox4.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox4_MouseClick);
            pictureBox3.Paint += pictureBox3_Paint; // Add this line

            PlaySoundtrack(@"C:\Dev\TreausreHuntVS\TreausreHuntVS\Assets\tavernmusic.mp3");



            visitedCells = new bool[rows, columns];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    visitedCells[i, j] = true; // Start all cells as unvisited (covered in fog)
                }
            }

            ClearFogAroundPlayer(playerPosition.X, playerPosition.Y); // Initial call to clear fog around the player

            UpdateVisibleArea(); // Ensure the visible area is initialized based on player start position
            GameEventTimer.Start();
        }
        static void getInput()
        {
            Console.WriteLine("Please enter the n");
        }





        static Dictionary<(int, int), string> InitializeMapping(int rows, int columns, string defaultValue)
        {
            Dictionary<(int, int), string> mapping = new Dictionary<(int, int), string>();

            // Initialize all coordinates with default value
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    mapping[(i, j)] = defaultValue;
                }
            }

            return mapping;
        }

        // Method to retrieve the mapped string for given coordinates
        
        private void InitializeMatrix()
        {
            Random random = new Random();

            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    // Populate the matrix with either 1 or 0 based on some condition (e.g., random)
                    matrix[i, j] = 0; // Generates random 0
                }
            }


            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    Console.Write(matrix[i, j] + " ");
                }
                Console.WriteLine("\n");

            }
        }

        void UpdateGame()
        {
            foreach (var obstacle in obstacles)
            {
                if (obstacle is MovingObstacle)
                {
                    ((MovingObstacle)obstacle).Move();
                }
            }

            // Redraw or refresh the game area to reflect the updated positions
        }
        private void InitializeRandomObstacles()
        {
            InitializeMovingObstacles();
            Random random = new Random();
            for (int i = 0; i < 30; i++)
            {
                int x = random.Next(columns);
                int y = random.Next(rows);
                Point position = new Point(x, y);

                Obstacle bird = new Bird(birdImage, position);
                obstacles.Add(bird);

                Obstacle bee = new Bee(beeImage, position);
                obstacles.Add(bee);
            }
            
            for (int i = 0; i < 30; i++)
            {

                int chestType = random.Next(5, 9);
                int x = random.Next(columns);
                int y = random.Next(rows);
                Point position = new Point(x, y);
                Obstacle goldChest = new GoldenChest(Resource.GoldChest, position);
                Obstacle bronzeChest = new BronzeChest(Resource.BronzeChest, position);
                Obstacle emeraldChest = new EmeraldChest(Resource.emeraldChest, position);
                Obstacle silverChest = new SilverChest(Resource.silverChest, position);
                
                
                switch (chestType)
                {

                    case 5:
                        if (matrix[x,y] != 0)
                        {
                            i--;
                            continue;
                        }
                        obstacles.Add(goldChest);
                        matrix[x, y] = 2;
                        extendedMatrix[(x, y)] = "goldenchest";

                        ChestCounter++;
                        break;
                    case 6:
                        if (matrix[x, y] != 0)
                        {
                            i--;
                            continue;
                        }
                        obstacles.Add(emeraldChest);
                        matrix[x, y] = 2;
                        extendedMatrix[(x, y)] = "emeraldchest";

                        ChestCounter++;
                        break;
                    case 7:
                        if (matrix[x, y] != 0)
                        {
                            i--;
                            continue;
                        }
                        obstacles.Add(silverChest);
                        extendedMatrix[(x, y)] = "silverchest";

                        matrix[x, y] = 2;
                        ChestCounter++;
                        break;
                    case 8:
                        if (matrix[x, y] != 0)
                        {
                            i--;
                            continue;
                        }
                        obstacles.Add(bronzeChest);
                        extendedMatrix[(x, y)] = "bronzechest";

                        matrix[x, y] = 2;
                        ChestCounter++;
                        break;
                    default:
                        break; // This default case should never be hit with the current logic
                }
            }
            TotalChestCount = ChestCounter;
            for (int i = 0; i < 10; i++) // Place 20 obstacles at random
            {
                // Generate random position
                int x = random.Next(columns / 2, columns);
                int y = random.Next(rows);
                Point position = new Point(x, y);
                Obstacle treeExample = new Tree(Resource.summerTree, new Point(x, y));
                Obstacle rockExample = new Rock(Resource.rock, new Point(x, y));
                Obstacle wallExample = new Wall(Resource.wall, new Point(x, y));
                Obstacle mountainExample = new Mountain(Resource.summerMountain, new Point(x, y));


                int obstacleType = random.Next(1, 5); // Randomly choose between tree, rock, or chests
                switch (obstacleType)
                {
                    case 1:
                        if (matrix[x, y] != 0)
                        {
                            i--;
                            continue;
                        }
                        for (int k = treeExample.Position.X; k < treeExample.Position.X + treeExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = treeExample.Position.Y; j < treeExample.Position.Y + treeExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                if(matrix[k, j] == 2)
                                {
                                    i--;
                                    continue;
                                }
                                
                            }
                        }
                        obstacles.Add(treeExample);
                        for (int k = treeExample.Position.X; k < treeExample.Position.X + treeExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = treeExample.Position.Y; j < treeExample.Position.Y + treeExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                matrix[k, j] = 1;
                                //Console.WriteLine(k + " " + j);
                                extendedMatrix[(k, j)] = "tree";
                            }
                        }


                        matrix[x, y] = 1; // Mark the obstacle in the matrix
                        extendedMatrix[(x, y)] = "tree";
                        break;
                    case 2:
                        if (matrix[x, y] != 0)
                        {
                            i--;
                            continue;
                        }
                        for (int k = rockExample.Position.X; k < rockExample.Position.X + rockExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = rockExample.Position.Y; j < rockExample.Position.Y + treeExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                               if( matrix[k, j] == 2)
                                {
                                    i--;
                                    continue;
                                }
                             
                            }
                        }
                        obstacles.Add(rockExample);
                        for (int k = rockExample.Position.X; k < rockExample.Position.X + rockExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = rockExample.Position.Y; j < rockExample.Position.Y + treeExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                matrix[k, j] = 1;
                                extendedMatrix[(k, j)] = "rock";
                            }
                        }

                        matrix[x, y] = 1;
                        extendedMatrix[(x, y)] = "rock";

                        break;
                    case 3:
                        if (matrix[x, y] != 0)
                        {
                            i--;
                            continue;
                        }
                        for (int k = wallExample.Position.X; k < wallExample.Position.X + wallExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = wallExample.Position.Y; j < wallExample.Position.Y + wallExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                if(matrix[k, j] == 2)
                                {
                                    i--;
                                    continue;
                                }
                                
                            }
                        }
                        obstacles.Add(wallExample);
                        for (int k = wallExample.Position.X; k < wallExample.Position.X + wallExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = wallExample.Position.Y; j < wallExample.Position.Y + wallExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                matrix[k, j] = 1;
                                extendedMatrix[(k, j)] = "wall";
                            }
                        }

                        matrix[x, y] = 1;
                        extendedMatrix[(x, y)] = "wall";

                        break;
                    case 4:
                        if (matrix[x, y] != 0)
                        {
                            i--;
                            continue;
                        }
                        for (int k = mountainExample.Position.X; k < mountainExample.Position.X + mountainExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = mountainExample.Position.Y; j < mountainExample.Position.Y + mountainExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                if(matrix[k, j] == 2)
                                {
                                    i--;
                                    continue;
                                }
                                
                            }
                        }
                        obstacles.Add(mountainExample);
                        for (int k = mountainExample.Position.X; k < mountainExample.Position.X + mountainExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = mountainExample.Position.Y; j < mountainExample.Position.Y + mountainExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                matrix[k, j] = 1;
                                extendedMatrix[(k, j)] = "mountain";
                            }
                        }
                        matrix[x, y] = 1;
                        extendedMatrix[(x, y)] = "mountain";
                        break;
                }

            }
            for (int i = 0; i < 10; i++) // Place 20 obstacles at random
            {
                // Generate random position
                int x = random.Next(0,columns/2);
                int y = random.Next(rows);
                Point position = new Point(x, y);
                Obstacle treeExample = new Tree(Resource.winterTree, new Point(x, y));
                Obstacle rockExample = new Rock(Resource.rock, new Point(x, y));
                Obstacle wallExample = new Wall(Resource.wall, new Point(x, y));
                Obstacle mountainExample = new Mountain(Resource.winterMountain, new Point(x, y));


                int obstacleType = random.Next(1, 5); // Randomly choose between tree, rock, or chests
                switch (obstacleType)
                {
                    case 1:
                        if (matrix[x, y] != 0)
                        {
                            i--;
                            continue;
                        }
                        for (int k = treeExample.Position.X; k < treeExample.Position.X + treeExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = treeExample.Position.Y; j < treeExample.Position.Y + treeExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                if(matrix[k, j] == 2)
                                {
                                    i--;
                                    continue;
                                }
                                
                            }
                        }
                        obstacles.Add(treeExample);
                        for (int k = treeExample.Position.X; k < treeExample.Position.X + treeExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = treeExample.Position.Y; j < treeExample.Position.Y + treeExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                matrix[k, j] = 1;
                                //Console.WriteLine(k + " " + j);
                                extendedMatrix[(k, j)] = "tree";
                            }
                        }


                        matrix[x, y] = 1; // Mark the obstacle in the matrix
                        extendedMatrix[(x, y)] = "tree";
                        break;
                    case 2:
                        if (matrix[x, y] != 0)
                        {
                            i--;
                            continue;
                        }
                        for (int k = rockExample.Position.X; k < rockExample.Position.X + rockExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = rockExample.Position.Y; j < rockExample.Position.Y + treeExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                if(matrix[k, j] == 2)
                                {
                                    i--;
                                    continue;
                                    
                                }

                            }
                        }

                        obstacles.Add(rockExample);
                        for (int k = rockExample.Position.X; k < rockExample.Position.X + rockExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = rockExample.Position.Y; j < rockExample.Position.Y + treeExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                matrix[k, j] = 1;
                                extendedMatrix[(k, j)] = "rock";
                            }
                        }

                        matrix[x, y] = 1;
                        extendedMatrix[(x, y)] = "rock";

                        break;
                    case 3:
                        if (matrix[x, y] != 0)
                        {
                            i--;
                            continue;
                        }
                        obstacles.Add(wallExample);
                        for (int k = wallExample.Position.X; k < wallExample.Position.X + wallExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = wallExample.Position.Y; j < wallExample.Position.Y + wallExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                matrix[k, j] = 1;
                                extendedMatrix[(k, j)] = "wall";
                            }
                        }

                        matrix[x, y] = 1;
                        extendedMatrix[(x, y)] = "wall";

                        break;
                    case 4:
                        if (matrix[x, y] != 0)
                        {
                            i--;
                            continue;
                        }
                        obstacles.Add(mountainExample);
                        for (int k = mountainExample.Position.X; k < mountainExample.Position.X + mountainExample.SizeX && k < matrix.GetLength(0); k++)
                        {
                            for (int j = mountainExample.Position.Y; j < mountainExample.Position.Y + mountainExample.SizeY && j < matrix.GetLength(1); j++)
                            {
                                matrix[k, j] = 1;
                                extendedMatrix[(k, j)] = "mountain";
                            }
                        }
                        matrix[x, y] = 1;
                        extendedMatrix[(x, y)] = "mountain";
                        break;
                }
            }
        }
        void UpdateObstaclePositions()
        {
            foreach (var obstacle in obstacles)
            {
                if (obstacle is MovingObstacle movingObstacle)
                {
                    movingObstacle.Move();
                }
            }
        }
        private void InitializeMovingObstacles()
        {
            // Initialize a Bee and a Bird near (0,0) and add them to the obstacles list
            Obstacle bee = new Bee(beeImage, new Point(6, 6));
            obstacles.Add(bee);

            Obstacle bird = new Bird(birdImage, new Point(12, 6));
            obstacles.Add(bird);
        }
        private void gameUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateObstaclePositions();
            this.Invalidate(); // Forces the form to redraw, updating the positions visually
        }

        private void ScanAndLogObstaclesNearPlayer()
        {
            int scanRadius = 3; // Adjust the scan radius as needed
            for (int dx = -scanRadius; dx <= scanRadius; dx++)
            {
                for (int dy = -scanRadius; dy <= scanRadius; dy++)
                {
                    int checkX = playerPosition.X + dx;
                    int checkY = playerPosition.Y + dy;

                    // Ensure we are within bounds
                    if (checkX >= 0 && checkX < columns && checkY >= 0 && checkY < rows)
                    {
                        string obstacleType;
                        if (extendedMatrix.TryGetValue((checkX, checkY), out obstacleType) && obstacleType != "default")
                        {
                            // Log the discovery
                            LogObstacleDiscovery(checkX, checkY, obstacleType);
                        }
                    }
                }
            }
        }

        private void LogObstacleDiscovery(int x, int y, string obstacleType)
        {
            string logMessage = $"At ({x},{y}), Explorer found a {obstacleType}!\n";

            // Check if invoking is required (cross-thread UI update)
            if (richTextBox1.InvokeRequired)
            {
                // If required, invoke on UI thread
                richTextBox1.Invoke(new Action(() =>
                {
                    // Append text
                    richTextBox1.AppendText(logMessage);

                    // Set selection start to the end of text
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;

                    // Scroll to the caret (bottom)
                    richTextBox1.ScrollToCaret();
                }));
            }
            else
            {
                // If not required, directly append text
                richTextBox1.AppendText(logMessage);

                // Set selection start to the end of text
                richTextBox1.SelectionStart = richTextBox1.Text.Length;

                // Scroll to the caret (bottom)
                richTextBox1.ScrollToCaret();
            }
        }
        private void PlaySoundtrack(string filePath)
        {
            player.URL = filePath;
            player.settings.setMode("loop", true); // Loop the soundtrack
            player.controls.play();
            player.settings.volume = 5;

        }
        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Store previous position before moving
            previousPlayerPosition = playerPosition;

            // Determine new position based on key press, but do not move the player yet
            Point newPosition = playerPosition;

            switch (e.KeyCode)
            {
                case Keys.W:
                    if (playerPosition.Y - 1 >= 0 && (matrix[playerPosition.X, playerPosition.Y - 1] == 0 || matrix[playerPosition.X, playerPosition.Y - 1] == 2))
                    {
                        playerPosition.Y -= 1; // It's safe to move up
                        CollectChestIfPresent(playerPosition.X, playerPosition.Y);
                        ScanAndLogObstaclesNearPlayer();
                    }
                    break;
                case Keys.A:
                    if (playerPosition.X - 1 >= 0 && (matrix[playerPosition.X - 1, playerPosition.Y] == 0 || matrix[playerPosition.X - 1, playerPosition.Y] == 2))
                    {
                        playerPosition.X -= 1; // It's safe to move left
                        CollectChestIfPresent(playerPosition.X, playerPosition.Y);
                    }
                    break;
                case Keys.S:
                    if (playerPosition.Y + 1 < rows && (matrix[playerPosition.X, playerPosition.Y + 1] == 0 || matrix[playerPosition.X, playerPosition.Y + 1] == 2))
                    {
                        playerPosition.Y += 1; // It's safe to move down
                        CollectChestIfPresent(playerPosition.X, playerPosition.Y);
                    }
                    break;
                case Keys.D:
                    if (playerPosition.X + 1 < columns && (matrix[playerPosition.X + 1, playerPosition.Y] == 0 || matrix[playerPosition.X + 1, playerPosition.Y] == 2))
                    {
                        playerPosition.X += 1; // It's safe to move right
                        CollectChestIfPresent(playerPosition.X, playerPosition.Y);
                    }
                    break;
                case Keys.G:
                    if (playerPosition.X + 1 < columns || playerPosition.Y + 1 < rows || playerPosition.X - 1 >= 0 || playerPosition.Y - 1 >= 0)
                    {
                        if (playerPosition.X + 1 < columns && (matrix[playerPosition.X + 1, playerPosition.Y] == 0 || matrix[playerPosition.X + 1, playerPosition.Y] == 2))
                        {
                            ScanAndMoveTowardsChest();
                        }
                    }
                    break;
                case Keys.M:
                    pictureBox3.Visible = !pictureBox3.Visible;

                    if (pictureBox3.Visible)
                    {
                        shouldDrawMap = true;
                        pictureBox3.Invalidate(); // Trigger the Paint event
                    }
                    break;
                case Keys.Tab:
                    Sound.Visible = !Sound.Visible;
                    trackBar1.Visible = !trackBar1.Visible;

                    if (Sound.Visible || trackBar1.Visible)
                    {
                        trackBar1.Minimum = 0;
                        trackBar1.Maximum = 100;
                        trackBar1.Value = player.settings.volume;
                    }

                    break;
            }

            // Only proceed if the position has actually changed
            if (newPosition != playerPosition)
            {
                ClearFogAroundPlayer(playerPosition.X, playerPosition.Y);
                InvalidatePlayerPosition(previousPlayerPosition);
                InvalidatePlayerPosition(playerPosition);
                UpdateVisibleArea(); // Ensure the visible area is updated with the player's new position

            }

        }
        private void ScanAndMoveTowardsChest()
        {
            bool chestFound = false;
            Point chestPosition = new Point(-1, -1);
            int searchRadius = 3; // For a 7x7 area, we scan 3 cells in each direction from the player

            // Scan the 7x7 area for a chest
            for (int i = -searchRadius; i <= searchRadius; i++)
            {
                for (int j = -searchRadius; j <= searchRadius; j++)
                {
                    int x = playerPosition.X + i;
                    int y = playerPosition.Y + j;

                    // Check bounds and for chest
                    if (x >= 0 && x < columns && y >= 0 && y < rows && matrix[x, y] == 2)
                    {
                        chestFound = true;
                        chestPosition = new Point(x, y);
                        break;
                    }
                }
                if (chestFound) break;
            }

            // If a chest is found, determine the path and move towards it
            if (chestFound)
            {
                ScanAndLogObstaclesNearPlayer();
                MoveTowardsChest(chestPosition);
            }
            else
            {
                ScanAndLogObstaclesNearPlayer();    
                MoveRandomly(); // Move randomly if no chest is found within the search area
            }
        }

        private void MoveTowardsChest(Point chestPosition)
        {
            // Calculate the difference in X and Y positions between the player and the chest
            int dx = chestPosition.X - playerPosition.X;
            int dy = chestPosition.Y - playerPosition.Y;

            // Determine the step in X direction
            if (dx != 0)
            {
                int stepX = dx / Math.Abs(dx); // This will be 1 if dx > 0 (move right) or -1 if dx < 0 (move left)
                                               // Check if next step in X is within bounds
                if (playerPosition.X + stepX >= 0 && playerPosition.X + stepX < columns)
                {
                    playerPosition.X += stepX;
                }
            }
            // If not moving in X or after moving in X, check and possibly move in Y direction
            if (dy != 0 && playerPosition.X == chestPosition.X) // Only move in Y if we haven't moved in X or are aligned with the chest in X
            {
                int stepY = dy / Math.Abs(dy); // This will be 1 if dy > 0 (move down) or -1 if dy < 0 (move up)
                                               // Check if next step in Y is within bounds
                if (playerPosition.Y + stepY >= 0 && playerPosition.Y + stepY < rows)
                {
                    playerPosition.Y += stepY;
                }
            }

            // After moving, clear fog and check for chest collection
            ClearFogAroundPlayer(playerPosition.X, playerPosition.Y);
            InvalidatePlayerPosition(playerPosition); // You might need to update this method to actually redraw the player at the new position.
            CollectChestIfPresent(playerPosition.X, playerPosition.Y);
            UpdateVisibleArea();
        }
        private void CollectChestIfPresent(int x, int y)
        {
            if (matrix[x, y] == 2)
            {
                CollectChest(x, y);
            }
        }


        // Method to handle chest collection
        private void CollectChest(int x, int y)
        {
            matrix[x, y] = 0; // Mark the chest as collected; adjust this as needed for your game's logic
            ChestCounter--; // If you're keeping track of remaining chests
            treasureFound++; // Increment coins or chests found
            UpdateProgressBar(); // Update the progress bar to reflect the new state

            // Play the sound effect
            PlayTreasureCollectingSound();
        }

        // Method to play the treasure collecting sound from resources
        private void PlayTreasureCollectingSound()
        {
            // Extract the resource to a temporary file
            string tempFile = Path.GetTempFileName() + ".mp3"; // Create a temp file with .mp3 extension
            File.WriteAllBytes(tempFile, Resource.Bonus_Points_Sound); // Write the MP3 resource to the temp file

            // Play the sound using WindowsMediaPlayer
            soundEffectsPlayer.URL = tempFile;
            soundEffectsPlayer.controls.play();

            // Optionally, you could delete the temporary file after playback
            // However, consider when and how to safely do this, as premature deletion could stop the playback.
        }


           
        private void DrawImage(Obstacle obst1, Point point, int cellWidth, int cellHeight, Graphics g)
        {
            Image image = obst1.Obst;
            float imageScaleFactor = Math.Min((float)(cellWidth) / image.Width, (float)cellHeight / image.Height);

            // Calculate the scaled image dimensions
            int imageScaledWidth = (int)(image.Width * imageScaleFactor);
            int imageScaledHeight = (int)(image.Height * imageScaleFactor);
            for (int i = point.X; i < point.X + imageScaledWidth; i++)
            {
                for (int j = point.Y; j < point.Y + imageScaledHeight; j++)
                {
                    if (i >= 0 && i < matrix.GetLength(0) && j >= 0 && j < matrix.GetLength(1))
                    {
                        matrix[i, j] = 1;
                        extendedMatrix[(i, j)] = obst1.Type ;
                    }
                }
            }
            g.DrawImage(image, new Rectangle(point.X, point.Y, imageScaledWidth * obst1.SizeX, imageScaledHeight * obst1.SizeY));
           
        }


        private void DrawImageInRandomCells(Obstacle obst1, int cellWidth, int cellHeight, int span, Graphics g)
        {
            // Image for the obstacle
            Image image = obst1.Obst;

            // Calculate the scale factor to fit the image within a cell
            float imageScaleFactor = Math.Min((float)(cellWidth * span) / image.Width, (float)cellHeight / image.Height);

            // Calculate the scaled image dimensions
            int imageScaledWidth = (int)(image.Width * imageScaleFactor);
            int imageScaledHeight = (int)(image.Height * imageScaleFactor);

            // Randomly select a position for the image
            Random random = new Random();
            int imageColumn = random.Next(columns - span + 1);  // Random column index, ensuring space for 'span' cells
            int imageRow = random.Next(rows);       // Random row index

            // Check if the entire image will fit within the grid
            if (imageColumn + span > columns || imageRow >= rows)
            {
                // If not, call the method recursively to try again
                DrawImageInRandomCells(obst1, cellWidth, cellHeight, span, g);
                return;
            }

            // Check if all cells are empty before drawing the image
            bool canDrawImage = true;
            for (int i = 0; i < span; i++)
            {
                int currentColumn = imageColumn + i;
                int currentRow = imageRow;
                int cellIndex = currentRow * columns + currentColumn;

                // Check if the index is valid and if the cell is already filled
                if (cellIndex < cellImageList.Count && cellImageList[cellIndex].Item1)
                {
                    canDrawImage = false;
                    break;
                }
            }

            // Check if all cells are within the grid and empty
            if (canDrawImage && imageColumn + span <= columns)
            {
                for (int i = 0; i < span; i++)
                {
                    int currentColumn = imageColumn + i;
                    int currentRow = imageRow;
                    int cellIndex = currentRow * columns + currentColumn;

                    // Check if the index is valid
                    if (cellIndex < cellImageList.Count)
                    {
                        cellImageList[cellIndex] = (true, obst1.Type);
                    }
                }

                // Draw the scaled and centered image within its cells
                int imageX = imageColumn * cellWidth + (cellWidth * span - imageScaledWidth) / 2;
                int imageY = imageRow * cellHeight + (cellHeight - imageScaledHeight) / 2;

                // Draw the image only if all conditions are met

                g.DrawImage(image, new Rectangle(imageX, imageY, imageScaledWidth * obst1.SizeX, imageScaledHeight * obst1.SizeY));
            }
            else
            {
                // If not all conditions are met, call the method recursively to try again
                DrawImageInRandomCells(obst1, cellWidth, cellHeight, span, g);
            }
        }

        private void InvalidatePlayerPosition(Point position)
        {
            // Invalidate only the cell where the player is/moves to
            //GridPanel.Invalidate(new Rectangle(position.X * cellWidth - cameraPosition.X * cellWidth,
            //                                   position.Y * cellHeight - cameraPosition.Y * cellHeight,
            //                                  cellWidth, cellHeight));
        }
        // In this part i want you to add a function that
        // find the path between the actual location and nearest
        // coin to collect them all. System work like this:
        // Scan all 8 squares around the character, if there is no coin image than
        // move to random mostly right or down direction to search coin in the possible coin locations.
        // in 3x3 parts i can see so search only in the squares that have not fog or dark color.
        // Furthermore i we only know the number of the coins and keep searching until collect all coins.
        // Use NumberOfCoin variable that i declared below to count coins 

        void startGame()
        {
            Console.WriteLine("[G] tuşuna basıldı.");

            bool coinFound = false;
            Point nearestCoinPosition = new Point(-1, -1); // Placeholder for the nearest coin position

            // Scan the surroundings (3x3 area centered on the player)
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int checkX = playerPosition.X + i;
                    int checkY = playerPosition.Y + j;

                    // Ensure we are within bounds
                    if (checkX >= 0 && checkX < columns && checkY >= 0 && checkY < rows)
                    {
                        // Assume a method exists to check if a cell contains a coin and is not fogged
                        if (CellContainsCoin(checkX, checkY) && !IsFogged(checkX, checkY))
                        {
                            coinFound = true;
                            nearestCoinPosition = new Point(checkX, checkY);
                            break; // Break the inner loop
                        }
                    }
                }
                if (coinFound) break; // Break the outer loop if a coin is found
            }

            // If a coin was found, move towards it
            if (coinFound)
            {
                HandlePlayerMovement(nearestCoinPosition);
                UpdateProgressBar();
            }
            else
            {
                // No coin found in the immediate vicinity, move randomly mostly right or down
                MoveRandomly();
            }

            // Additional logic to handle movement and fog clearing goes here
        }

        // You will need to implement or adjust the following helper methods:
        // - CellContainsCoin(int x, int y): Checks if a given cell contains a coin.
        // - IsFogged(int x, int y): Checks if a given cell is covered by fog.
        // - MoveRandomly(): Moves the player in a random direction, preferring right or down, while avoiding fog and obstacles.
        private bool CellContainsCoin(int x, int y)
        {
            int cellIndex = y * columns + x; // Convert 2D cell coordinates to a 1D list index
            if (cellIndex >= 0 && cellIndex < cellImageList.Count)
            {
                return cellImageList[cellIndex].Item2 == "C:\\Dev\\TreausreHuntVS\\TreausreHuntVS\\Assets\\coin.jpg"; // Check if the cell's image is a coin
            }
            return false;
        }
        private bool IsFogged(int x, int y)
        {
            return visitedCells[y, x]; // Returns true if the cell is fogged
        }
        private void MoveRandomly()
        {
            Random random = new Random();
            List<Point> possibleMoves = new List<Point>();

            // Check all directions for possible moves: right (D), down (S), left (A), and up (W)
            var directions = new List<(int dx, int dy)>
    {
        (1, 0), // Right
        (0, 1), // Down
        (-1, 0), // Left
        (0, -1)  // Up
    };

            foreach (var dir in directions)
            {
                int newX = playerPosition.X + dir.dx;
                int newY = playerPosition.Y + dir.dy;

                // Check if the new position is within bounds, not fogged, and either empty or contains a chest
                if (newX >= 0 && newX < columns && newY >= 0 && newY < rows
                    && !IsFogged(newX, newY)
                    && (matrix[newX, newY] == 0 || matrix[newX, newY] == 2))
                {
                    possibleMoves.Add(new Point(newX, newY));
                }
            }

            // Prioritize chest if available
            foreach (var move in possibleMoves)
            {
                if (matrix[move.X, move.Y] == 2) // If there's a chest in the possible moves
                {
                    HandlePlayerMovement(move);
                    return; // Stop looking for other moves, prioritize the chest
                }
            }

            // If no chest is prioritized, move randomly among the possible moves
            if (possibleMoves.Count > 0)
            {
                int index = random.Next(possibleMoves.Count);
                HandlePlayerMovement(possibleMoves[index]);
            }
            else
            {
                // If no moves are possible (surrounded by obstacles or fog), consider additional logic or handling
            }
        }

        private void HandlePlayerMovement(Point newPosition)
        {
            // Check if the new position is different from the current position
            if (newPosition != playerPosition)
            {
                // Store previous position before moving
                Point previousPosition = playerPosition;

                // Invalidate and update player position
                InvalidatePlayerPosition(previousPosition);
                playerPosition = newPosition;

                // Clear fog around the new player position
                ClearFogAroundPlayer(playerPosition.X, playerPosition.Y);

                // Invalidate the new player position
                InvalidatePlayerPosition(playerPosition);
            }
        }
        private void FillSquare(Graphics g, int x, int y, int size, Color fillColor)
        {
            // Define brush for filling the square
            Brush fillBrush = new SolidBrush(fillColor);

            // Draw and fill the square
            g.FillRectangle(fillBrush, x, y, size, size);

            // Dispose of the brush to free resources
            fillBrush.Dispose();
        }
        // Example method to remove an image by clearing its area
        private void ClearImageArea(Graphics g, int x, int y, Color fillColor)
        {
            // Assume cellWidth and cellHeight are the dimensions of the grid cells
            Brush backgroundBrush = new SolidBrush(fillColor);

            // Convert grid coordinates (x, y) to pixel coordinates
            int pixelX = x * cellWidth;
            int pixelY = y * cellHeight;

            // Clear the specific area where the image was drawn
            g.FillRectangle(backgroundBrush, pixelX, pixelY, cellWidth, cellHeight);

            // Dispose of the brush after use
            backgroundBrush.Dispose();

            // Force the form to repaint the area
            this.Invalidate(new Rectangle(pixelX, pixelY, cellWidth, cellHeight));
        }

        private void ClearFogAroundPlayer(int x, int y)
        {
            for (int i = -3; i <= 3; i++)
            {
                for (int j = -3; j <= 3; j++)
                {
                    int newX = x + i;
                    int newY = y + j;
                    if (newX >= 0 && newX < columns && newY >= 0 && newY < rows && visitedCells[newY, newX])
                    {
                        visitedCells[newY, newX] = false;
                        InvalidatePlayerPosition(new Point(newX, newY)); // Invalidate fog-cleared area
                    }
                }
            }
        }
        private void DrawObstaclesWholeMap(Graphics g)
        {

            foreach (var obstacle in obstacles)
            {
                // Calculate the obstacle's position relative to the camera
                int relativeX = obstacle.Position.X * cellWidth;
                int relativeY = obstacle.Position.Y * cellHeight;

                // Check if the obstacle is within the visible viewport
                if (relativeX >= 0 && relativeX < pictureBox3.Width && relativeY >= 0 && relativeY < pictureBox3.Height)
                {
                    
                       // DrawImage(obstacle, new Point(relativeX, relativeY), cellWidth / 5, cellHeight / 5, g);
              }
            }
            
        }
        private void DrawObstacles(Graphics g)
        {
            foreach (var obstacle in obstacles)
            {
                // Calculate the obstacle's position relative to the camera
                int relativeX = (obstacle.Position.X - cameraPosition.X) * cellWidth;
                int relativeY = (obstacle.Position.Y - cameraPosition.Y) * cellHeight;

                // Check if the obstacle is within the visible viewport
                if (relativeX >= 0 && relativeX < pictureBox3.Width && relativeY >= 0 && relativeY < pictureBox3.Height)
                {
                    if (isMinimap)

                    {
                        DrawImage(obstacle, new Point(relativeX, relativeY), cellWidth/5, cellHeight/5, g);

                        //g.DrawImage(obstacle.Obst, new Rectangle(relativeX, relativeY, cellWidth / 5, cellHeight / 5));
                    }
                    else
                    {
                        // Draw the obstacle
                        //DrawImage(obstacle, new Point(relativeX, relativeY), cellWidth, cellHeight, g);

                        //g.DrawImage(obstacle.Obst, new Rectangle(relativeX, relativeY, cellWidth, cellHeight));
                    }

                }
            }
            isMinimap = false;
        }


        // This method updates the visible area around the player
        private void UpdateVisibleArea()
        {
            // Adjust camera position based on character position, allowing some margin
            int margin = 2; // Margin cells before camera starts to follow

            if (playerPosition.X < cameraPosition.X + margin)
            {
                cameraPosition.X = Math.Max(0, playerPosition.X - margin);
            }
            else if (playerPosition.X >= cameraPosition.X + 10 - margin)
            {
                cameraPosition.X = Math.Min(columns - 10, playerPosition.X - 10 + margin + 1);
            }

            if (playerPosition.Y < cameraPosition.Y + margin)
            {
                cameraPosition.Y = Math.Max(0, playerPosition.Y - margin);
            }
            else if (playerPosition.Y >= cameraPosition.Y + 10 - margin)
            {
                cameraPosition.Y = Math.Min(rows - 10, playerPosition.Y - 10 + margin + 1);
            }

            //GridPanel.Invalidate(); // Refresh the grid panel to apply changes
        }

        // Call UpdateVisibleArea in your player movement logic, right after updating the playerPosition


        private bool isConfirmationAsked = false;

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isRestarting)
            {
                // Yeniden başlatma işlemi için form kapanıyor, uygulamayı kapatma
                return; // Direkt olarak metoddan çık
            }

            // Eğer bu noktaya gelinmişse, normal kapatma işlemi yapılıyor demektir
            if (!isConfirmationAsked)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to close?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    isConfirmationAsked = true; // Prevent further confirmation
                    System.Windows.Forms.Application.Exit(); // Uygulamayı tamamen kapat
                }
                else
                {
                    e.Cancel = true; // Kapatmayı iptal et
                }
            }
        }

        private string GenerateRandomID()
        {
            Random random = new Random();
            int id = random.Next(1000, 9999); // Generates a number between 1000 and 9999
            return id.ToString();
        }

        private void volumeTrackBar_ValueChanged(object sender, EventArgs e)
        {
            // Adjust the volume of the player according to the TrackBar's value
            player.settings.volume = trackBar1.Value;
        }
        private void InitializeProgressBar()
        {
            // Set the maximum value of the progress bar to the total number of coins
            progressBar1.Minimum= 0;
            progressBar1.Maximum = TotalChestCount; // Assuming NumberOfCoin is the total number of coins in the game
            // Set the initial value of the progress bar
            progressBar1.Value = treasureFound;
        }

        private void UpdateProgressBar()
        {
            // Calculate the percentage of chests found
            double percentage = (double)treasureFound / TotalChestCount * 100;

            // Update the progress bar value to the calculated percentage
            progressBar1.Value = (int)percentage;

            // Update the label to show the progress
            this.label2.Text = "Treasures Founded: " + treasureFound + "/" + TotalChestCount + ", " + ChestCounter + " Remain";

            // Check if all chests are collected
            if (treasureFound == TotalChestCount)
            {
                // Stop the timer
                timer.Stop();

                // Calculate the elapsed time
                TimeSpan elapsedTime = DateTime.Now - startTime;
                // Format the elapsed time
                string elapsedTimeString = $"{elapsedTime.Minutes} min {elapsedTime.Seconds} sec";

                // Show message box congratulating the player
                MessageBox.Show(this,
                $"Congrats you collected all the chests in the map. It took {elapsedTimeString} to do it",
                "All Chests Collected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            
        }
        }


        private void GameEvent(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void testPaint(object sender, PaintEventArgs e)
        {
            // Get graphics object for drawing
            Graphics g = e.Graphics;

            // Define the transparency value (alpha channel)
            int transparency = 200; // You can adjust this value as needed (0 to 255)

            // Create transparent colors with the desired transparency
            Color transparentYellow = Color.FromArgb(transparency, Color.Yellow);
            Color transparentAliceBlue = Color.FromArgb(transparency, Color.AliceBlue);

            // Create SolidBrush objects with the transparent colors
            Brush SummerBrush = new SolidBrush(transparentYellow);
            Brush WinterBrush = new SolidBrush(transparentAliceBlue);
            Brush FogBrush = new SolidBrush(Color.Gray);

            // Define pen for drawing borders (optional)
            Pen pen = new Pen(Color.Black, 1); // Adjust color and thickness as needed

            // Get size of the GridPanel
            int width = pictureBox1.Width;
            int height = pictureBox1.Height;

            Image rock = Resource.rock;
            Image explorer = Resource.Explorer;
            Image IceandFireBg = Resource.Ice_FireBackground1;

            Rectangle destRect = new Rectangle(0, 0, 377, 661);
            //g.DrawImage(explorer, destRect);

            // Calculate the scale factor to fit the explorer image within a cell
            float scaleFactor = Math.Min((float)cellWidth / explorer.Width, (float)cellHeight / explorer.Height);

            // Calculate the scaled image dimensions
            int scaledWidth = (int)(explorer.Width * scaleFactor);
            int scaledHeight = (int)(explorer.Height * scaleFactor);

            // Define the visible area around the player
            int visibleRows = 10; // Number of rows to display
            int visibleColumns = 10; // Number of columns to display

            // Calculate the starting point for rows and columns
            int startRow = Math.Max(0, playerPosition.Y - (visibleRows / 2));
            int startColumn = Math.Max(0, playerPosition.X - (visibleColumns / 2));

            // Adjust startRow and startColumn to not exceed grid bounds
            startRow = Math.Min(startRow, rows - visibleRows);
            startColumn = Math.Min(startColumn, columns - visibleColumns);

            // Calculate the ending point for rows and columns
            int endRow = Math.Min(startRow + visibleRows, rows);
            int endColumn = Math.Min(startColumn + visibleColumns, columns);
            
            // Loop through only the visible area
            for (int i = startRow; i < endRow; i++)
            {
                for (int j = startColumn; j < endColumn; j++)
                {
                    int x = (j - startColumn) * cellWidth; // Adjust X based on the visible window
                    int y = (i - startRow) * cellHeight; // Adjust Y based on the visible window

                    // Your existing logic for drawing cells, fog, etc.
                    Brush brush = (j < columns / 2) ? WinterBrush : SummerBrush;
                    g.DrawRectangle(Pens.Black, x + 1, y + 1, cellWidth, cellHeight);
                    //Console.WriteLine("Selam testPaint");
                    g.FillRectangle(brush, x, y, cellWidth, cellHeight);

                    Random rng = new Random(); // Ideally, this should be a class-level variable to avoid re-initialization

                    if (matrix[i, j] != 0) // There's an obstacle at this position
                    {
                        // Loop through the obstacles list and draw each one
                        foreach (var obstacle in obstacles)

                        {
                            // Determine the type of obstacle and select the appropriate image
                            System.Drawing.Image obstacleImage = obstacle.Obst;
                            

                            if (obstacleImage != null)
                            {
                                // Calculate position based on obstacle's location
                                Point pos = obstacle.Position; // Assuming your obstacle objects have a Position property
                                int m = (pos.X - startColumn) * cellWidth; // Adjust X based on the visible window
                                int n = (pos.Y - startRow) * cellHeight; // Adjust Y based on the visible window

                                // Draw the obstacle image if it's within the current viewport
                                if (pos.X >= startColumn && pos.X < endColumn && pos.Y >= startRow && pos.Y < endRow)
                                {
                                    DrawImage(obstacle,new Point(m,n),cellWidth,cellHeight,g);
                                    
                                    //g.DrawImage(obstacleImage, m, n, cellWidth, cellHeight); //elma1
                                }
                            }
                        }

                        
                    }
                    if (visitedCells[i, j])
                    {
                        Brush darkBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
                        g.DrawImage(fogImage, x, y, cellWidth, cellHeight);
                        g.FillRectangle(darkBrush, x, y, cellWidth, cellHeight);
                        g.DrawRectangle(Pens.Black, x + 1, y + 1, cellWidth, cellHeight);
                    }
                    g.DrawRectangle(Pens.Black, x + 1, y + 1, cellWidth, cellHeight);

                }
            }
            // Calculate the explorer's position to be centered within its cell
            // This block is moved outside the loop to focus on drawing the explorer
            int explorerCenterX = ((playerPosition.X - startColumn) * cellWidth) + (cellWidth / 2);
            int explorerCenterY = ((playerPosition.Y - startRow) * cellHeight) + (cellHeight / 2);
            int explorerDrawX = explorerCenterX - (scaledWidth / 2);
            int explorerDrawY = explorerCenterY - (scaledHeight / 2);

            // Draw the explorer image centered within its current cell
            g.DrawImage(explorer, explorerDrawX, explorerDrawY, scaledWidth, scaledHeight);

            //DraImageInRandomCells(tree, cellWidth, cellHeight, 3, g);
            


        }

        private bool shouldDrawMap = false;

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            pictureBox3.Visible = !pictureBox3.Visible;

            if (pictureBox3.Visible)
            {
                shouldDrawMap = true;
                pictureBox3.Invalidate(); // Trigger the Paint event
            }
        }

        private void wholeMap(Graphics g)
        {
            // Graphics object for drawing on pictureBox3
            g.Clear(pictureBox3.BackColor); // Clear existing drawings

            // Define the transparency value (alpha channel)
            int transparency = 200; // You can adjust this value as needed (0 to 255)

            // Create transparent colors with the desired transparency
            Color transparentYellow = Color.FromArgb(transparency, Color.Yellow);
            Color transparentAliceBlue = Color.FromArgb(transparency, Color.AliceBlue);

            // Create SolidBrush objects with the transparent colors
            Brush summerBrush = new SolidBrush(transparentYellow);
            Brush winterBrush = new SolidBrush(transparentAliceBlue);
            Pen gridPen = new Pen(Color.Black); // Pen for drawing grid lines
            //Image treeImage1 = Image.FromFile(@"C:\Dev\TreausreHuntVS\TreausreHuntVS\Assets\tree.jpg");

            int totalWidth = pictureBox3.Width;
            int totalHeight = pictureBox3.Height;

            // Scale factors to fit the grid within pictureBox3
            float scaleX = totalWidth / (float)(columns * cellWidth);
            float scaleY = totalHeight / (float)(rows * cellHeight);
            float scale = Math.Min(scaleX, scaleY);

            int scaledCellWidth = (int)(cellWidth * scale);
            int scaledCellHeight = (int)(cellHeight * scale);
            
            // Drawing the map with cells, fog, and the explorer
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    int x = j * scaledCellWidth;
                    int y = i * scaledCellHeight;
                    
                    
                    Brush brush = (j < columns / 2) ? winterBrush : summerBrush;
                    g.FillRectangle(brush, x, y, scaledCellWidth, scaledCellHeight);
                    
                    if (visitedCells[i, j])
                    {
                        Brush fogBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
                        g.FillRectangle(fogBrush, x, y, scaledCellWidth, scaledCellHeight);
                    }

                    // Drawing grid lines
                    g.DrawRectangle(gridPen, x, y, scaledCellWidth, scaledCellHeight);
                }
            }
            


            // Positioning and drawing the explorer
            float explorerScale = Math.Min((float)scaledCellWidth / explorer.Width, (float)scaledCellHeight / explorer.Height);
            int explorerScaledWidth = (int)(explorer.Width * explorerScale);
            int explorerScaledHeight = (int)(explorer.Height * explorerScale);
            int explorerX = (playerPosition.X * scaledCellWidth) + (scaledCellWidth - explorerScaledWidth) / 2;
            int explorerY = (playerPosition.Y * scaledCellHeight) + (scaledCellHeight - explorerScaledHeight) / 2;

            g.DrawImage(explorer, explorerX, explorerY, explorerScaledWidth, explorerScaledHeight);
            //matrix[explorerX, explorerY] = 1;
            DrawObstaclesWholeMap(g);

            //g.DrawImage(treeImage1, rockX, rockY, explorerScaledWidth, explorerScaledHeight);
            //int treeImageX = (10 * scaledCellWidth) + (scaledCellWidth - (scaledCellWidth * tree.SizeX)) / 2;  // Adjust the position as needed
            //int treeImageY = (10 * scaledCellHeight) + (scaledCellHeight - (scaledCellHeight * tree.SizeY)) / 2; // Adjust the position as needed
            //g.DrawImage(treeImage1, treeImageX, treeImageY, (scaledCellWidth * tree.SizeX), (scaledCellHeight * tree.SizeY));
            //DrawImageInRandomCells(tree, cellWidth/5, cellHeight/5, 3, g);

        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            if (shouldDrawMap)
            {
                wholeMap(e.Graphics); // Your existing map drawing logic
            
                isMinimap = true;
                // Now draw the obstacles relative to the camera's current position
                //DrawObstacles(e.Graphics);
            }
        }

        private void pictureBox4_MouseClick(object sender, MouseEventArgs e)
        {
            //  MessageBox.Show("PictureBox4 clicked!");

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            // Ensure the TrackBar accommodates the full volume range before setting its value
            trackBar1.Minimum = 0; // This line may be redundant if already set to 0
            trackBar1.Maximum = TotalChestCount;


            // Toggle the visibility of the volumeTrackBar
            trackBar1.Visible = !trackBar1.Visible;
            Sound.Visible = !Sound.Visible;

            // Optionally, update the TrackBar's value to reflect the current volume
            trackBar1.Value = player.settings.volume;


        }

        private void pictureBox5_MouseClick(object sender, MouseEventArgs e)
        {
            isRestarting = true; // Yeniden başlatma işlemi başlatılıyor

            // Mevcut formu kapat
            this.Close();

            // Yeni bir form örneği oluştur
            Form1 restart = new Form1(); // Oyuncu adınızı buraya girin veya bir önceki formdan alın.

            // Yeni formu göster
            restart.Show();
        }
    }
}