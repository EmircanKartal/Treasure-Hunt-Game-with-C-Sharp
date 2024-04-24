using System;
using System.Collections;
using System.Drawing;

namespace TreausreHuntVS
{
    public class Character
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Point location { get; set; }


        // Default constructor
        public Character()
        {
            // Default values or initialization logic can be added here
        }

        public Character(int id, string name, Point location)
        {
            this.ID = id;
            this.Name = name;
            this.location = location;
        }

        public void CalculateTheShortestWay()
        {
            // Implementation of the shortest way logic
            Console.WriteLine("Calculating the shortest way...");
        }
    }

    public class Location
    {
        private ArrayList coordinates;

        // Default constructor
        public Location()
        {
            this.coordinates = new ArrayList();
        }

        public Location(int size)
        {
            this.coordinates = new ArrayList(size);
        }

        public ArrayList Coordinates
        {
            get { return coordinates; }
            set { this.coordinates = value; }
        }

        // You can add more methods as needed
    }

    public abstract class Obstacle
    {
        public int SizeX { get; set; }
        public int SizeY { get; set; }
        public Image Obst { get; set; } // Use this for the obstacle image
        public string Type { get; set; } // Lowercase 'type' to follow C# naming conventions
        public Point Position { get; set; } // Ensure proper naming convention
        public string Direction { get; set; }
        protected Obstacle(Image obstImage, Point position)
        {

            this.Obst = obstImage;
            this.Position = position;
            SetRandomSizes(); // Ensure this method sets sizes appropriately for the derived class
            
        }

        protected abstract void SetRandomSizes(); // Define how sizes are set in derived classes
    }

    public class NonMovingObstacle : Obstacle
    {
        public NonMovingObstacle(Image obstImage, Point position) : base(obstImage, position)
        {
        }

        protected override void SetRandomSizes()
        {
            Console.WriteLine("");
        }
    }

    public class MovingObstacle : Obstacle
    {
        public MovingObstacle(Image obstImage, Point position) : base(obstImage, position)
        {
        }

        protected override void SetRandomSizes()
        {
            Console.WriteLine("");        }
        // New method to update position safely
        public void UpdatePosition(int deltaX, int deltaY)
        {
            // Create a new Point with updated coordinates and assign it back to Position
            this.Position = new Point(this.Position.X + deltaX, this.Position.Y + deltaY);
        }
        // Implement the movement logic here
        public virtual void Move()
        {
            // This method can be overridden by derived classes for specific movement patterns
            // Example implementation for any generic moving obstacle (if needed)
        }
    }

    public class Tree : Obstacle
    {
        public Tree(Image obstImage, Point position) : base(obstImage, position)
        {
            this.Type = "tree";
            SetRandomSizes() ;
        }

        protected override void SetRandomSizes()
        {
            Random random = new Random();
            int randomNumber = random.Next(2, 6);

            switch (randomNumber)
            {
                case 2:
                    this.SizeX = 2;
                    this.SizeY = 2;
                    break;

                case 3:
                    this.SizeX = 3;
                    this.SizeY = 3;
                    break;

                case 4:
                    this.SizeX = 4;
                    this.SizeY = 4;
                    break;

                case 5:
                    this.SizeX = 5;
                    this.SizeY = 5;
                    break;

                default:

                    break;
            }
        }
    }

    public class Rock : Obstacle
    {
        public Rock(Image obstImage, Point position) : base(obstImage, position)
        {
            this.Type = "rock";
        }

        protected override void SetRandomSizes()
        {
            Random random = new Random();
            int randomNumber = random.Next(2, 4);

            switch (randomNumber)
            {
                case 2:
                    this.SizeX = 2;
                    this.SizeY = 2;
                    break;

                case 3:
                    this.SizeX = 3;
                    this.SizeY = 3;
                    break;

                default:

                    break;
            }
        }
    }

    public class Chest : Obstacle
    {
       
        public string ImagePath { get; set; }
        public Chest(Image obstImage, Point position) : base(obstImage, position)
        {
            this.Type = "chest";
            this.SizeX = 1;
            this.SizeY = 1;
            
        }

        protected override void SetRandomSizes()
        {
            Console.WriteLine("Hello There");
            
        }
    }

    public class GoldenChest : Chest
    {
       
        public string ImagePath { get; set; }
        public GoldenChest(Image obstImage, Point position) : base(obstImage, position)
        {
            this.Type = "goldenchest";
            this.SizeX = 1;
            this.SizeY = 1;
        }
    }

    public class EmeraldChest : Chest
    {
   
        public string ImagePath { get; set; }
        public EmeraldChest(Image obstImage, Point position) : base(obstImage, position)
        {
            this.Type = "emeraldchest";
            this.SizeX = 1;
            this.SizeY = 1;
        }
    }

    public class SilverChest : Chest
    {
      
        public string ImagePath { get; set; }
        public SilverChest(Image obstImage, Point position) : base(obstImage, position)
        {
            this.Type = "silverchest";
            this.SizeX = 1;
            this.SizeY = 1;
        }
    }

    public class BronzeChest : Chest
    {
       
        public string ImagePath { get; set; }
        public BronzeChest(Image obstImage, Point position) : base(obstImage, position)
        {
            this.Type = "bronzechest";
            this.SizeX = 1;
            this.SizeY = 1;
        }
    }

    public class Wall : NonMovingObstacle
    {
        // Implementation of Wall class
        public Wall(Image obstImage, Point position) : base(obstImage, position)
        {
            this.Type = "wall";
            this.SizeX = 10;
            this.SizeY = 1;
        }
    }

    public class Mountain : NonMovingObstacle
    {
        // Implementation of Mountain class
        public Mountain(Image obstImage, Point position) : base(obstImage, position)
        {
            this.Type = "mountain";
            this.SizeX = 15;
            this.SizeY = 15;
        }
    }

    public class Bird : MovingObstacle
    {
        // Implementation of Bird class
        public Bird(Image obstImage, Point position) : base(obstImage, position)
        {
            this.Type = "bird";
            this.SizeX = 2;
            this.SizeY = 2;
            this.Direction = "Up";
        }

        public override void Move()
        {
            const int verticalMovement = 2; // Example vertical movement step
            if (this.Direction == "Up")
            {
                // Move up if not at the boundary (you define the boundary logic)
                this.UpdatePosition(0, -verticalMovement);
                // Update direction based on your game's logic, perhaps at boundaries
            }
            else // Assuming "Down"
            {
                // Move down
                this.UpdatePosition(0, verticalMovement);
                // Update direction as needed
            }
        }
    }

    public class Bee : MovingObstacle
    {
        // Implementation of Bee class
        public Bee(Image obstImage, Point position) : base(obstImage, position)
        {
            this.Type = "bee";
            this.SizeX = 2;
            this.SizeY = 2;
            this.Direction = "Right";
        }
        public override void Move()
        {
            const int horizontalMovement = 1; // Example horizontal movement step
            if (this.Direction == "Right")
            {
                // Move right
                this.UpdatePosition(horizontalMovement, 0);
                // Update direction as needed
            }
            else // Assuming "Left"
            {
                // Move left
                this.UpdatePosition(-horizontalMovement, 0);
                // Update direction as needed
            }
        }
    }

    public interface WinterObstacle
    {
        // Declaration of WinterObstacle interface
    }

    public interface SummerObstacle
    {
        // Declaration of SummerObstacle interface
    }

    public class GameInformation
    {
        // Implementation of GameInformation class
    }
}