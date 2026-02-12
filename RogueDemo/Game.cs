using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RogueCore;

namespace RogueDemo
{
    public class Player
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;
        public int Water { get; set; } = 100;
        public int Food { get; set; } = 100;
        public int Ammo { get; set; } = 50;
        public int Battery { get; set; } = 100;
        public int Experience { get; set; } = 0;
        public int Level { get; set; } = 1;
        public List<string> Implants { get; set; } = new List<string>();
        public List<string> Mutations { get; set; } = new List<string>();
        public List<string> Inventory { get; set; } = new List<string>();
        
        public Player(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Enemy
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Health { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int Damage { get; set; }
        public bool IsHostile { get; set; } = true;
        public bool IsReprogrammed { get; set; } = false; // For robots that can be hacked
        
        public Enemy(string type, string name, int health, int damage, int x, int y)
        {
            Type = type;
            Name = name;
            Health = health;
            Damage = damage;
            X = x;
            Y = y;
        }
    }

    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; } // weapon, implant, consumable, etc.
        
        public Item(string name, string description, string type, int x, int y)
        {
            Name = name;
            Description = description;
            Type = type;
            X = x;
            Y = y;
        }
    }

    public class GameLocation
    {
        public string Name { get; set; }
        public string Type { get; set; } // shelter, wasteland, ruins, etc.
        public string Description { get; set; }
        public Dungeon Dungeon { get; set; }
        public List<Enemy> Enemies { get; set; } = new List<Enemy>();
        public List<Item> Items { get; set; } = new List<Item>();
        public List<string> Events { get; set; } = new List<string>();
        
        public GameLocation(string name, string type, string description, int width, int height)
        {
            Name = name;
            Type = type;
            Description = description;
            Dungeon = new Dungeon(width, height);
        }
    }

    public class Game
    {
        private Screen screen;
        private Message messageLog;
        private Player player;
        private GameLocation currentLocation;
        private List<GameLocation> locations = new List<GameLocation>();
        private Random random = new Random();
        private bool gameRunning = true;
        private bool inCombat = false;
        private List<Enemy> activeEnemies = new List<Enemy>();
        
        public Game(Screen screen)
        {
            this.screen = screen;
            this.messageLog = new Message(screen.ScreenHeight - 4, 4);
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Create initial shelter location
            var startingShelter = new GameLocation("Outpost Alpha", "shelter", 
                "A fortified underground shelter with basic amenities", 
                screen.ScreenWidth - 2, screen.ScreenHeight - 6);
            
            // Generate the shelter layout
            GenerateShelterLayout(startingShelter);
            
            // Add some NPCs and items to the shelter
            startingShelter.Enemies.Add(new Enemy("NPC", "Trader Joe", 50, 5, 10, 5));
            startingShelter.Items.Add(new Item("Plasma Rifle", "A high-tech energy weapon", "weapon", 15, 8));
            startingShelter.Items.Add(new Item("Water Filter", "Filters contaminated water", "consumable", 20, 10));
            startingShelter.Items.Add(new Item("Neural Implant MK-I", "Basic neural enhancement", "implant", 25, 12));
            
            locations.Add(startingShelter);
            
            // Create starting player position
            player = new Player(5, 5);
            currentLocation = startingShelter;
            
            // Add some random wasteland locations
            CreateWastelandLocations();
            
            // Show welcome message
            messageLog.Add("Welcome to Rogue B. Star!");
            messageLog.Add("You are a stranger in a post-apocalyptic world.");
            messageLog.Add("Find the legendary 'B. Star' artifact and uncover the truth.");
            messageLog.Add("Press SPACE to continue...");
        }

        private void GenerateShelterLayout(GameLocation location)
        {
            var dungeon = location.Dungeon;
            
            // Fill with walls initially
            var wall = new Cell
            {
                Character = new Char('#', Color.Gray, Color.Black),
                Solid = true,
                Visible = true
            };
            
            dungeon.FillRect(new Rectangle(0, 0, dungeon.Width, dungeon.Height), wall);
            
            // Create interior space
            var floor = new Cell
            {
                Character = new Char('.', Color.DarkGray, Color.Black),
                Solid = false,
                Visible = true
            };
            
            dungeon.FillRect(new Rectangle(1, 1, dungeon.Width - 2, dungeon.Height - 2), floor);
            
            // Add some rooms
            CreateRoom(dungeon, 3, 3, 8, 6); // Living quarters
            CreateRoom(dungeon, 12, 3, 8, 6); // Trading area
            CreateRoom(dungeon, 21, 3, 8, 6); // Workshop
            CreateRoom(dungeon, 3, 10, 12, 6); // Storage
            CreateRoom(dungeon, 16, 10, 10, 6); // Medical bay
            
            // Add doorways
            CreateDoorway(dungeon, 11, 5); // Between living quarters and trading
            CreateDoorway(dungeon, 20, 5); // Between trading and workshop
            CreateDoorway(dungeon, 9, 12); // Between living quarters and storage
            CreateDoorway(dungeon, 20, 12); // Between workshop and medical
        }

        private void CreateRoom(Dungeon dungeon, int x, int y, int width, int height)
        {
            var floor = new Cell
            {
                Character = new Char('.', Color.Silver, Color.Black),
                Solid = false,
                Visible = true
            };
            
            dungeon.FillRect(new Rectangle(x, y, width, height), floor);
        }

        private void CreateDoorway(Dungeon dungeon, int x, int y)
        {
            var doorway = new Cell
            {
                Character = new Char('+', Color.Yellow, Color.Black),
                Solid = false, // Make doors passable in this simple version
                Visible = true
            };
            
            dungeon.SetCell(x, y, doorway);
        }

        private void CreateWastelandLocations()
        {
            // Create several wasteland areas
            for (int i = 0; i < 3; i++)
            {
                var wasteland = new GameLocation($"Wasteland Sector {(char)('A' + i)}", "wasteland",
                    "Radioactive wasteland with ruins and dangers", 
                    screen.ScreenWidth - 2, screen.ScreenHeight - 6);
                
                GenerateWastelandLayout(wasteland);
                
                // Add enemies and items to wasteland
                if (i == 0) // First wasteland
                {
                    wasteland.Enemies.Add(new Enemy("Mutant", "Radioactive Beast", 80, 15, 15, 10));
                    wasteland.Enemies.Add(new Enemy("Robot", "Scavenger Bot", 60, 10, 25, 15));
                    wasteland.Items.Add(new Item("RadAway", "Reduces radiation", "consumable", 20, 8));
                }
                else if (i == 1) // Second wasteland
                {
                    wasteland.Enemies.Add(new Enemy("Cyborg", "Rogue Cyborg", 100, 20, 12, 12));
                    wasteland.Enemies.Add(new Enemy("Mutant", "Giant Spider", 70, 25, 30, 20));
                    wasteland.Items.Add(new Item("Energy Cell", "Power source for implants", "consumable", 18, 15));
                }
                else // Third wasteland
                {
                    wasteland.Enemies.Add(new Enemy("Corp.Agent", "Corporate Agent", 120, 25, 20, 18));
                    wasteland.Items.Add(new Item("B. Star Fragment", "Part of the legendary artifact", "artifact", 25, 12));
                    wasteland.Events.Add("You sense something important nearby...");
                }
                
                locations.Add(wasteland);
            }
        }

        private void GenerateWastelandLayout(GameLocation location)
        {
            var dungeon = location.Dungeon;
            
            // Create a more complex wasteland layout with tunnels and rooms
            var floor = new Cell
            {
                Character = new Char('.', Color.Olive, Color.Black),
                Solid = false,
                Visible = true
            };
            
            var wall = new Cell
            {
                Character = new Char('#', Color.Brown, Color.Black),
                Solid = true,
                Visible = true
            };
            
            // Fill with walls initially
            dungeon.FillRect(new Rectangle(0, 0, dungeon.Width, dungeon.Height), wall);
            
            // Create some open areas
            for (int i = 0; i < 8; i++)
            {
                int roomX = random.Next(2, dungeon.Width - 10);
                int roomY = random.Next(2, dungeon.Height - 8);
                int roomWidth = random.Next(4, 8);
                int roomHeight = random.Next(3, 6);
                
                dungeon.FillRect(new Rectangle(roomX, roomY, roomWidth, roomHeight), floor);
            }
            
            // Create connecting tunnels between rooms
            ConnectRoomsRandomly(dungeon, floor);
        }

        private void ConnectRoomsRandomly(Dungeon dungeon, Cell floor)
        {
            // Simple tunnel connection - connect some random positions
            for (int i = 0; i < 10; i++)
            {
                int startX = random.Next(1, dungeon.Width - 1);
                int startY = random.Next(1, dungeon.Height - 1);
                int endX = random.Next(1, dungeon.Width - 1);
                int endY = random.Next(1, dungeon.Height - 1);
                
                var start = new Point(startX, startY);
                var end = new Point(endX, endY);
                
                dungeon.FillLine(start, end, floor);
            }
        }

        public void Render()
        {
            screen.ClearScreen();
            
            // Update FOV for player
            currentLocation.Dungeon.UpdateFov(new Point(player.X, player.Y), 8);
            
            // Display dungeon
            currentLocation.Dungeon.Display(screen, 1);
            
            // Draw player
            var playerChar = new Char('@', Color.Cyan, Color.Black);
            screen.SetChar(player.X, player.Y + 1, playerChar);
            
            // Draw enemies that are in FOV
            foreach (var enemy in currentLocation.Enemies.Where(e => 
                IsInPlayerFOV(e.X, e.Y)))
            {
                char enemySymbol = enemy.Type switch
                {
                    "Mutant" => 'M',
                    "Robot" => 'R',
                    "Cyborg" => 'C',
                    "Corp.Agent" => 'A',
                    "NPC" => 'N',
                    _ => 'E'
                };
                
                var enemyColor = enemy.IsReprogrammed ? Color.Green : Color.Red;
                var enemyChar = new Char(enemySymbol, enemyColor, Color.Black);
                screen.SetChar(enemy.X, enemy.Y + 1, enemyChar);
            }
            
            // Draw items that are in FOV
            foreach (var item in currentLocation.Items.Where(i => 
                IsInPlayerFOV(i.X, i.Y)))
            {
                char itemSymbol = item.Type switch
                {
                    "weapon" => ')',
                    "implant" => '%',
                    "consumable" => '!',
                    "artifact" => '*',
                    _ => '?'
                };
                
                var itemChar = new Char(itemSymbol, Color.Yellow, Color.Black);
                screen.SetChar(item.X, item.Y + 1, itemChar);
            }
            
            // Draw UI elements
            DrawUI();
            
            // Draw message log
            messageLog.ShowMore(screen);
        }

        private bool IsInPlayerFOV(int x, int y)
        {
            // Check if a position is within the player's field of view
            // This is a simplified check - in a real implementation we'd check the visibility flag
            int distX = Math.Abs(x - player.X);
            int distY = Math.Abs(y - player.Y);
            return (distX + distY) <= 8; // Approximate FOV check
        }

        private void DrawUI()
        {
            // Draw top status bar
            screen.Print(0, 0, $"HP:{player.Health}/{player.MaxHealth} W:{player.Water} F:{player.Food} A:{player.Ammo} B:{player.Battery}");
            
            // Draw location info
            screen.Print(screen.ScreenWidth - 30, 0, currentLocation.Name);
            
            // Draw bottom status
            string actionPrompt = inCombat ? "[A]ttack [R]un [U]se Item" : "[WASD] Move [I]nv [E]xplore";
            screen.Print(0, screen.ScreenHeight - 1, actionPrompt);
        }

        public void ProcessInput(KeyInfo key)
        {
            if (!gameRunning) return;
            
            bool moved = false;
            
            switch (key.KeyCode)
            {
                case Key.W:
                case Key.Up:
                    MovePlayer(0, -1);
                    moved = true;
                    break;
                case Key.S:
                case Key.Down:
                    MovePlayer(0, 1);
                    moved = true;
                    break;
                case Key.A:
                case Key.Left:
                    MovePlayer(-1, 0);
                    moved = true;
                    break;
                case Key.D:
                case Key.Right:
                    MovePlayer(1, 0);
                    moved = true;
                    break;
                case Key.I:
                    ShowInventory();
                    break;
                case Key.E:
                    ExploreNearby();
                    break;
                case Key.Space:
                    // Action key - interact with items/enemies
                    InteractWithEnvironment();
                    break;
                case Key.H:
                    // Attempt to hack robots
                    HackNearbyRobot();
                    break;
                case Key.M:
                    // Show mutations
                    ShowMutations();
                    break;
                case Key.Escape:
                    gameRunning = false;
                    MessageBox.Show("Game exited by player");
                    break;
            }
            
            if (moved)
            {
                HandleTimePassage();
                CheckEncounters();
            }
            
            Render();
        }

        private void MovePlayer(int deltaX, int deltaY)
        {
            int newX = player.X + deltaX;
            int newY = player.Y + deltaY;
            
            // Check if the move is valid (not a wall)
            var targetCell = currentLocation.Dungeon.GetCell(newX, newY);
            if (!targetCell.Solid)
            {
                // Check for enemies at destination
                var enemyAtDestination = currentLocation.Enemies
                    .FirstOrDefault(e => e.X == newX && e.Y == newY && e.IsHostile);
                
                if (enemyAtDestination != null)
                {
                    // Combat encounter
                    EngageEnemy(enemyAtDestination);
                    return;
                }
                
                // Valid move
                player.X = newX;
                player.Y = newY;
                
                // Check for items at new position
                var itemHere = currentLocation.Items
                    .FirstOrDefault(i => i.X == player.X && i.Y == player.Y);
                
                if (itemHere != null)
                {
                    PickUpItem(itemHere);
                }
            }
            else
            {
                messageLog.Add("You bump into a wall.");
            }
        }

        private void EngageEnemy(Enemy enemy)
        {
            inCombat = true;
            messageLog.Add($"Combat started with {enemy.Name}!");
            
            // Simple combat system - player attacks first
            int playerDamage = random.Next(10, 25);
            enemy.Health -= playerDamage;
            messageLog.Add($"You hit {enemy.Name} for {playerDamage} damage!");
            
            if (enemy.Health <= 0)
            {
                messageLog.Add($"{enemy.Name} defeated!");
                currentLocation.Enemies.Remove(enemy);
                
                // Gain experience
                player.Experience += enemy.Type switch
                {
                    "Mutant" => 15,
                    "Robot" => 20,
                    "Cyborg" => 25,
                    "Corp.Agent" => 30,
                    _ => 10
                };
                
                CheckLevelUp();
            }
            else
            {
                // Enemy counterattacks
                int enemyDamage = random.Next(enemy.Damage / 2, enemy.Damage);
                player.Health -= enemyDamage;
                messageLog.Add($"{enemy.Name} hits you for {enemyDamage} damage!");
                
                if (player.Health <= 0)
                {
                    GameOver();
                    return;
                }
            }
            
            inCombat = false;
        }

        private void CheckLevelUp()
        {
            int expThreshold = player.Level * 50;
            if (player.Experience >= expThreshold)
            {
                player.Level++;
                player.MaxHealth += 10;
                player.Health = player.MaxHealth; // Heal on level up
                messageLog.Add($"Level up! Now level {player.Level}");
            }
        }

        private void PickUpItem(Item item)
        {
            player.Inventory.Add(item.Name);
            currentLocation.Items.Remove(item);
            messageLog.Add($"Picked up {item.Name}: {item.Description}");
        }

        private void InteractWithEnvironment()
        {
            // Look for items nearby
            var nearbyItems = currentLocation.Items
                .Where(i => Math.Abs(i.X - player.X) <= 1 && Math.Abs(i.Y - player.Y) <= 1)
                .ToList();
            
            if (nearbyItems.Any())
            {
                var item = nearbyItems.First();
                PickUpItem(item);
            }
            else
            {
                messageLog.Add("Nothing to interact with here.");
            }
        }

        private void HackNearbyRobot()
        {
            var nearbyRobots = currentLocation.Enemies
                .Where(e => e.Type == "Robot" && 
                           Math.Abs(e.X - player.X) <= 2 && 
                           Math.Abs(e.Y - player.Y) <= 2 &&
                           !e.IsReprogrammed)
                .ToList();
            
            if (nearbyRobots.Any())
            {
                var robot = nearbyRobots.First();
                
                // Simple hacking attempt
                int hackChance = random.Next(1, 101);
                if (hackChance <= (player.Implants.Contains("Neural Interface") ? 70 : 30))
                {
                    robot.IsReprogrammed = true;
                    robot.IsHostile = false;
                    messageLog.Add($"Successfully hacked {robot.Name}! It's now friendly.");
                }
                else
                {
                    messageLog.Add($"Failed to hack {robot.Name}. It attacks!");
                    // Robot becomes more hostile
                    robot.Damage += 5;
                }
            }
            else
            {
                messageLog.Add("No hackable robots nearby.");
            }
        }

        private void ShowInventory()
        {
            if (player.Inventory.Any())
            {
                string invList = string.Join(", ", player.Inventory);
                messageLog.Add($"Inventory: {invList}");
            }
            else
            {
                messageLog.Add("Inventory is empty.");
            }
        }

        private void ShowMutations()
        {
            if (player.Mutations.Any())
            {
                string mutList = string.Join(", ", player.Mutations);
                messageLog.Add($"Mutations: {mutList}");
            }
            else
            {
                messageLog.Add("No active mutations.");
            }
        }

        private void ExploreNearby()
        {
            // Find unexplored areas nearby
            bool foundSomething = false;
            
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -2; dy <= 2; dy++)
                {
                    int checkX = player.X + dx;
                    int checkY = player.Y + dy;
                    
                    var cell = currentLocation.Dungeon.GetCell(checkX, checkY);
                    if (cell.Character.Character == '#' && random.Next(1, 101) <= 20) // 20% chance to find secret
                    {
                        // Convert wall to floor (secret passage)
                        var floor = new Cell
                        {
                            Character = new Char('.', Color.DarkGreen, Color.Black),
                            Solid = false,
                            Visible = true
                        };
                        
                        currentLocation.Dungeon.SetCell(checkX, checkY, floor);
                        messageLog.Add("You found a secret passage!");
                        foundSomething = true;
                        break;
                    }
                }
                if (foundSomething) break;
            }
            
            if (!foundSomething)
            {
                messageLog.Add("You search but find nothing unusual.");
            }
        }

        private void HandleTimePassage()
        {
            // Time passes, resources deplete
            player.Water = Math.Max(0, player.Water - 1);
            player.Food = Math.Max(0, player.Food - 1);
            player.Battery = Math.Max(0, player.Battery - 1);
            
            // Check for resource depletion consequences
            if (player.Water == 0)
            {
                player.Health -= 2;
                messageLog.Add("You're dehydrated! (-2 HP)");
            }
            
            if (player.Food == 0)
            {
                player.Health -= 1;
                messageLog.Add("You're starving! (-1 HP)");
            }
            
            // Regenerate health if conditions are met
            if (player.Water > 20 && player.Food > 20 && currentLocation.Type == "shelter")
            {
                if (player.Health < player.MaxHealth)
                {
                    player.Health = Math.Min(player.MaxHealth, player.Health + 1);
                    messageLog.Add("Resting in shelter. Health +1");
                }
            }
            
            // Check for death
            if (player.Health <= 0)
            {
                GameOver();
            }
        }

        private void CheckEncounters()
        {
            // Random encounters in wastelands
            if (currentLocation.Type == "wasteland" && random.Next(1, 101) <= 10) // 10% chance per move
            {
                TriggerRandomEncounter();
            }
        }

        private void TriggerRandomEncounter()
        {
            string[] encounterTypes = { "radiation storm", "mutant attack", "bandit ambush", "robot patrol", "anomaly" };
            string encounter = encounterTypes[random.Next(encounterTypes.Length)];
            
            messageLog.Add($"Random encounter: {encounter}!");
            
            switch (encounter)
            {
                case "radiation storm":
                    int radDamage = random.Next(5, 15);
                    player.Health -= radDamage;
                    messageLog.Add($"Radiation exposure! -{radDamage} HP");
                    if (player.Mutations.Count == 0 || random.Next(1, 101) <= 30)
                    {
                        // 30% chance of gaining mutation if no mutations yet, or random chance
                        string[] mutations = { "Radiation Resistance", "Regeneration", "Night Vision", "Toxic Blood" };
                        string newMutation = mutations[random.Next(mutations.Length)];
                        player.Mutations.Add(newMutation);
                        messageLog.Add($"You've developed: {newMutation}");
                    }
                    break;
                    
                case "mutant attack":
                    // Add a temporary enemy
                    var mutant = new Enemy("Mutant", "Angry Mutant", 50, 12, player.X + 1, player.Y + 1);
                    currentLocation.Enemies.Add(mutant);
                    messageLog.Add("A mutant appears and attacks!");
                    break;
                    
                case "bandit ambush":
                    var bandit = new Enemy("Cyborg", "Bandit Leader", 75, 18, player.X + 2, player.Y);
                    currentLocation.Enemies.Add(bandit);
                    messageLog.Add("Bandits ambush you!");
                    break;
                    
                case "robot patrol":
                    var robot = new Enemy("Robot", "Patrol Bot", 60, 10, player.X - 1, player.Y + 1);
                    currentLocation.Enemies.Add(robot);
                    messageLog.Add("A security robot detects you!");
                    break;
                    
                case "anomaly":
                    messageLog.Add("You discover a strange anomaly. Gain 10 XP.");
                    player.Experience += 10;
                    break;
            }
        }

        private void GameOver()
        {
            gameRunning = false;
            MessageBox.Show($"Game Over!\n\nFinal Stats:\nLevel: {player.Level}\nExperience: {player.Experience}\nLocation: {currentLocation.Name}");
        }

        public bool IsGameRunning()
        {
            return gameRunning;
        }
    }
}