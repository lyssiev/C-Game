using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework
{
    class Enemies 
    {
        //objective 2
        // creating variables/attributes
        protected int damage;
        protected int health;
        protected string name;
        protected bool defeated;
        protected int linesOfDialogue;
        protected List<string> dialogue = new List<string>();
        protected List<string> questions = new List<string>();
        protected List<string> answers = new List<string>();

        public Enemies(string name, string[,] Questions , int health, int damage)
        {
            //assigns using parameters when the object is instantiated
            defeated = false;
            this.name = name;
            this.health = health;
            this.damage = damage;
            SetUpEnemy(Questions);
        }
        protected virtual void SetUpEnemy(string[,] Questions)
        {
            int j;
            Random rnd = new Random();
            int length = Questions.GetLength(1); //random question picked
            for (int i = 0; i < health; i++)//adds random question into the lists
            {
                j = rnd.Next(0, length);
                questions.Add(Questions[0, j]);
                answers.Add(Questions[1, j]);
            }
            SetUpDialogue();
        }
        protected void SetUpDialogue()
        {
            using (StreamReader sr = new StreamReader("Dialogue/" + name + ".txt")) //reads from file and adds lines of dialogue to a list
            {
                linesOfDialogue = 0;
                while (sr.Peek() >= 0)
                {
                    dialogue.Add(sr.ReadLine());
                    linesOfDialogue++;
                }
            }
        }
        protected void SayDialogue()
        {
            Console.ForegroundColor = ConsoleColor.Blue; //changes to blue to allow the user to distinguish between other text (as this is quite a text heavy sequence)
            Random random = new Random();
            int line = random.Next(1, linesOfDialogue);
            Console.WriteLine("\n" + name + ": " + dialogue[line]);//outputs dialogue
            Console.ResetColor();
        }

        protected void DisplayAsciiArt() //reads art from file and outputs to console
        {
            try
            {
                using (StreamReader sr = new StreamReader("Art/" + name + "Art.txt"))
                {
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error: " + e.ToString());
            }
        }

        public virtual bool Fight(ref int userHealth)
        {
            DisplayAsciiArt();
            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine("YOU HAVE ENCOUNTERED AN ENEMY: " + name);
            Console.ForegroundColor = ConsoleColor.Blue; //changes to blue
            Console.WriteLine(name + ": " + dialogue[0]);//outputs first line of dialogue
            Console.ResetColor();
            Console.WriteLine("Answer these questions to defeat them!\n");
            while (health >= 0 && !Program.dead)
            {
                SayDialogue();
                if (health != 0 && questions.Count != 0 && !Program.dead)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nYour question: " + questions[health - 1] + "\n"); //asks question in red
                    Console.ResetColor();
                    Console.WriteLine("The enemy has " + health + "HP left. You have " + Program.userHealth + " HP left.");
                    Console.WriteLine("\n(F)ight or (S)kip?");
                    string choice = Console.ReadLine().ToUpper();
                    switch (choice)
                    {
                        case "F": //if the fight option is picked
                            {
                                Console.WriteLine("Your answer: ");
                                string userAnswer = Console.ReadLine().ToUpper();
                                if (userAnswer == answers[health - 1].ToUpper()) //if the user inputs the correct answer
                                {
                                    Console.WriteLine("That is correct! " + name + " took 1HP damage.");
                                    questions.Remove(questions[health - 1]); //removes the question and answer from the lists
                                    answers.Remove(answers[health - 1]);
                                    health--;
                                }
                                else //user takes damage
                                {
                                    Console.WriteLine("That is incorrect! You took " + damage + "HP damage.");
                                    userHealth--;
                                }
                                break;
                            }
                        case "S"://if skip is picked
                            {
                                Console.WriteLine("If you skip a question, you will lose 3 health. Proceed? (y/n)");
                                string skipChoice = Console.ReadLine().ToUpper();
                                if (skipChoice == "Y")
                                {
                                    questions.Remove(questions[health - 1]); //removes the question
                                    answers.Remove(answers[health - 1]);
                                    userHealth -= 3; //decreases the health by 3
                                    health--;
                                }
                                break;
                            }
                        default: //validation
                            {
                                Console.WriteLine("You have entered an invalid letter. Please try again.");
                                break;
                            }
                    }

                }
                else //when the enemy has 1 hp left and the last question has been entered
                {
                    Console.WriteLine("The enemy is defeated!");
                    health--;
                    defeated = true;
                }

                if (userHealth <= 0) //sets the global variable to true to stop the game.
                {
                    Program.dead = true;
                }

            }
            return defeated;

        }

    }
    class Bosses : Enemies //inherited class for 'Bosses' which includes a drop
    {
        public string drop;
        public Bosses(string name, string[,] Questions, string drop , int health, int damage) : base(name, Questions , health, damage)
        {
            this.drop = drop;
        }

    }
    class FinalBoss : Enemies //final boss inherited class
    {
        public FinalBoss(string name, string[,] Questions , int health, int damage) : base(name, Questions , health, damage)
        {
            SetUpEnemy(Questions);
            Console.WriteLine("You use Zeus' lightning bolt to charge the Pacman controller. \nThe crown is placed on your head, as you become the king of the robots!");
        }
        protected override void SetUpEnemy(string[,] Questions) //adds all questions as a final test
        {
            int length = Questions.GetLength(1);
            for (int i = 0; i < length; i++)
            {
                questions.Add(Questions[0, i]);
                answers.Add(Questions[1, i]);
            }
            SetUpDialogue();
        }

    }

    class Map
    {
        private Dictionary<int, Dictionary<int, int>> map; //decalaring a dictionary called map, used throughout to check if rooms are connected or locked
        public int playerPosition = 1;
        private int area = 0;
        public List<string> inventory = new List<string>();//contains all items dropped by the bosses
        private List<int> unlockedRooms = new List<int>();//checks when outputting map
        public Dictionary<string, string> ReadDialogueFile(int area) //reads all dialogue from the files into the dictionary
        {
            string path = "Area/area" + area + ".txt";
            Dictionary<string, string> dialogue = new Dictionary<string, string>();
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    while (sr.Peek() >= 0)
                    {
                        int commaPosition = 0;
                        string line = sr.ReadLine();
                        bool commaFound = false;
                        while (!commaFound)
                        {
                            if (line[commaPosition] == ',') //finds comma
                            {
                                commaFound = true;
                            }
                            else
                            {
                                commaPosition++;
                            }
                        }
                        string key = "";
                        string value = "";
                        for (int i = 0; i < line.Length; i++)
                        {
                            if (i < commaPosition && i != commaPosition)
                            {
                                key += line[i]; //splits all before the comma into the key
                            }
                            else if (i > commaPosition && i != commaPosition)
                            {
                                value += line[i]; //splits all after the comma into the value
                            }

                        }

                        dialogue.Add(key, value); //adds to dictionary

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error: ", e.ToString());
            }
            return dialogue;
        }


        public void UnlockRoom(int roomUnlocked) //unlocks the room in the dictionary
        {

            int[] roomsToUnlock = new int[10];
            int counter = 0;

            foreach (var room in map)
            {
                foreach (var subdict in map[room.Key])
                {
                    if (subdict.Key == roomUnlocked && subdict.Value == 1) //finds all instances of the room that needs to be unlocked in the dictionary
                    {
                        {
                            roomsToUnlock[counter] = room.Key;
                            counter++;

                        }
                    }
                }
            }
            for (int i = 0; i < counter; i++)
            {
                map[roomsToUnlock[i]].Remove(roomUnlocked); //removes and adds a new value with '0' meaning unlocked
                map[roomsToUnlock[i]].Add(roomUnlocked, 0);
            }
            unlockedRooms.Add(roomUnlocked);

        }

        public void StartArea() //allows the user to travel to each 'area' of the map
        {
            Program.PlayMusic("gamemusic");
            Console.WriteLine("Push enter to continue.");
            Console.ReadLine();
            Console.Clear();
            bool repeat = true;
            while (repeat == true) //keeps asking if the player is not moved
            {
                playerPosition = 1;
                DisplayMap();
                Console.WriteLine("\nYou are in the starting room!\nFrom here you can travel in your time machine to each area.");
                Console.WriteLine("Please enter up/down/left/right to enter each area.");
                string input = Console.ReadLine().ToLower();
                switch (input)
                {
                    case "up":
                        {
                            Program.PlayMusic("arcademusic");
                            repeat = UpdatePlayerPos(7); //moves the player and returns if it is possible
                            area = 3;
                            break;
                        }
                    case "down":
                        {
                            Program.PlayMusic("medievalmusic");
                            area = 2;
                            repeat = UpdatePlayerPos(5);
                            break;
                        }
                    case "left":
                        {
                            Program.PlayMusic("robotmusic");
                            area = 4;
                            repeat = UpdatePlayerPos(10);    
                            break;
                        }
                    case "right":
                        {
                            area = 1;
                            Program.PlayMusic("greekmusic");
                            repeat = UpdatePlayerPos(2);
                            break;
                        }
                    default: //validation
                        {
                            Console.WriteLine("Invalid input. Please try again. Push enter.");
                            Console.ReadLine();
                            break;
                        }
                }
            }
        }

        public void MoveInArea() //allows the user to move from room to room, look, show the map or get help
        {
            while (!Program.dead && Program.userHealth > 0)
            {
                if (playerPosition == 1) //if they are in room 1
                {
                    StartArea();
                }
                else
                {
                    bool move = false;
                    Dictionary<string, string> areaDialogue = ReadDialogueFile(area);
                    while (move == false && playerPosition != 1 && Program.dead == false)
                    {
                        Console.Clear();
                        Console.WriteLine("Info --- Room: " + playerPosition + " -  Health: " + Program.userHealth + " - Score: " + Program.score + " ---"); //displays information
                        Console.WriteLine(areaDialogue[Convert.ToString(playerPosition)]);
                        Console.WriteLine("\nEither (M)ove, (L)ook, (D)isplay your map or (H)elp?");
                        string choice = Console.ReadLine().ToLower();
                        switch (choice)
                        {
                            case "m":
                                {
                                    Console.WriteLine("\nWhich room would you like to move to? Please enter the number from the map.");
                                    string moveChoice = Console.ReadLine();
                                    if (Int32.TryParse(moveChoice, out int roomNumber))
                                    {
                                        UpdatePlayerPos(roomNumber); //moves player
                                        move = true;
                                    }
                                    else //validation
                                    {
                                        Console.WriteLine("Please enter a valid input.");
                                    };
                                    break;
                                }
                            case "l":
                                {
                                    Console.WriteLine("Look up/down/left/right?");
                                    string lookChoice = Console.ReadLine().ToLower();
                                    string output = OutputDialogue(area, lookChoice, areaDialogue  ); //outputs dialogue when looking
                                    Console.WriteLine(output);
                                    if (!Program.dead) //stops repeated output if dead
                                    {
                                        Console.WriteLine("Press enter to continue.");
                                        Console.ReadLine();
                                    }
                                    break;
                                }
                            case "d": //display map
                                {
                                    DisplayMap();
                                    Console.WriteLine("Press enter to continue.");
                                    Console.ReadLine();
                                    break;
                                }
                            case "h": //help, objective 7(ii)
                                {
                                    Console.WriteLine("Select a room to move to. From there, you can look around and search for enemies.\nYou will only find enemies when looking around, not moving.\nEach enemy increases your score by 1 point, and each boss increases it by 2. Look at the score guide on your map to find out how many points it takes to unlock a certain room.");
                                    Console.WriteLine("Press enter to continue.");
                                    Console.ReadLine();
                                    break;
                                }
                            default: //validation
                                {
                                    Console.WriteLine("You have entered an invalid input. Try again.");
                                    Console.WriteLine("Press enter to continue.");
                                    Console.ReadLine();
                                    break;
                                }

                        }
                    }
                }
            }
        }


        public void PlayEnemy( ) //used to instantiate enemies based on areas
        {
            // objective 2(i)
            // objective 2(iii)
            Enemies enemy = null;
            if (area == 1)
            {
                enemy = new Enemies("Cyclops", Question.Questions , 3, 1); //objective 5(ii)
            }
            else if (area == 2)
            {
                enemy = new Enemies("Armour stand", Question.Questions , 3, 1);
            }
            else if (area == 3)
            {
                enemy = new Enemies("Ghost", Question.Questions , 3, 1);
            }
            bool defeated = enemy.Fight(ref Program.userHealth);
            if (defeated && Program.userHealth > 0)
            {
                Program.score++;
                enemy = null; //object lifecycle
            }

        }

        private bool CheckBossDefeated(Bosses enemy) //checks if the user has defeated the boss of the area by checking if they have its drop in their inventory
        {
            //objective 5(iii)
            bool itemLeft = false;
            if (inventory.Contains(enemy.drop))
            {
                itemLeft = true;
            }
            return itemLeft;
        }

        public void PlayBoss(ref int score) //instantiates bosses based on area
        {
            // objective 2(i)
            // objective 2(iii)
            Bosses enemy = null;
            Random random = new Random(); //objective 4(i), 5(i)
            int damage = random.Next(3);
            if (playerPosition == 4)
            {
                enemy = new Bosses("Zeus", Question.Questions, "Lightning bolt" , 5, damage);
            }
            else if (playerPosition == 6)
            {
                enemy = new Bosses("King", Question.Questions, "Crown" , 5, damage);
            }
            else if (playerPosition == 9)
            {
                enemy = new Bosses("Pacman", Question.Questions, "Game controller" , 5, damage);
            }

            if (false == CheckBossDefeated(enemy)) //playing the boss and adding their drop to the inventory
            {
                bool defeated = enemy.Fight(ref Program.userHealth);
                if (defeated == true && Program.userHealth > 0)
                {
                    Console.WriteLine("\nYou have found a " + enemy.drop + ". It has been added to your inventory. Push enter to continue.");  //objective 5(iii)
                    inventory.Add(Convert.ToString(enemy.drop));
                    score += 2;
                    Console.ReadLine();
                }

            }
            enemy = null; //object lifecycle
        }
        private string OutputDialogue(int area, string direction, Dictionary<string, string> dialogue  )
        {
            Random random = new Random();
            string output = "";
            int chance = random.Next(4); //chance of enemy appearing
            direction = direction.ToLower();
            if (direction == "up" || direction == "down" || direction == "left" || direction == "right")
            {
                if (chance == 1) //random enemy appears, , objective 2(ii)
                {
                    PlayEnemy();
                }
                else //outputs dialogue from dictionary
                {
                    string key = direction + playerPosition;
                    foreach (var value in dialogue)
                    {
                        if (value.Key == key)
                        {
                            output = value.Value;
                        }
                    }
                }
            }
            else //validation
            {
                Console.WriteLine("You have entered an invalid input. Push enter.");
            }
            return output;
        }
        public bool CheckScore(int newPos, int score) //checks if door can be unlocked
        {
            bool unlock = false;
            if ((newPos == 4 && score >= 2) || (newPos == 5 && score >= 3) || (newPos == 7 && score >= 4) || (newPos == 9 && score >= 6))
            {
                unlock = true;
            }

            return unlock;
        }

        private void PlayUltimateBoss() //instantiates ultimate boss
        {
            FinalBoss robot = new FinalBoss("Robot", Question.Questions, (Question.Questions.Length)/2 , 3); //objective 5(iv)
            bool defeated = robot.Fight(ref Program.userHealth);
            if (defeated == true && Program.userHealth > 0)
            {
                Console.WriteLine("Your remaining health was added to your point score.");
                Program.score += Program.userHealth;
                Program.userHealth = 0; //sets the health to zero to break out of the game loop
            }
        }

        public bool UpdatePlayerPos(int newPos) //moves player to a room if it is not locked
        {
            bool found = false;
            bool unlocked = true;
            Dictionary<int, int> valuePairs = new Dictionary<int, int>(map[playerPosition]); //sub-dictionary of the main map
            List<int> keys = new List<int>(); //split into lists as the dictionary has been changed
            List<int> values = new List<int>();

            foreach (var key in valuePairs) //adds to list
            {
                keys.Add(key.Key);
                values.Add(key.Value);
            }
            int counter = 0;
            if (newPos == 10 && inventory.Count >= 3) //checks if they are moving to the final ultimate boss room
            {
                Console.WriteLine("You have all of the pieces to defeat the robot!");
                PlayUltimateBoss();
                unlocked = false;
            }
            else
            {
                foreach (var locked in keys)//goes through each value in the list
                {
                    if (values[counter] == 0 && locked == newPos) //checks if the room is unlocked and is the room that the player wants to move to
                    {
                        playerPosition = newPos;
                        found = true;
                        unlocked = false;
                    }
                    else if (values[counter] == 1 && locked == newPos) //checks if the room is locked and is the new position
                    {
                        unlocked = CheckScore(newPos, Program.score);
                        if (unlocked == false)
                        {
                            Console.WriteLine("This door is locked. Please come back later.");
                            found = true;
                        }
                        else
                        {
                           found = UserUnlockRoom(newPos); //asks the user if they want to unlock the room
                        }
                    }
                    counter++;

                }
                if (!found && playerPosition != newPos) //if there is not a path
                {
                    Console.WriteLine("You cannot enter this area from here.");
                    Console.ReadLine();
                }
                if ((playerPosition == 4 || playerPosition == 6 || playerPosition == 9)) //if moving to a boss battle room
                {
                    PlayBoss(ref Program.score);
                }
            }
            return unlocked;
        }

        private bool UserUnlockRoom(int newPos)//asks user if they want to unlock the room
        {
            bool found = false;
            Console.WriteLine("You can unlock this room as your score is " + Program.score + ". Unlock? (y/n)");
            if (Console.ReadLine().ToUpper() == "Y")
            {
                UnlockRoom(newPos);
                DisplayLockArt();
                found = true;
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("I don't see why you wouldn't, but come back later if you want to. Push enter to continue.");
                Console.ReadLine();
            }
            return found;
        }

        private void DisplayLockArt()//reads lock from file
        {
            using (StreamReader sr = new StreamReader("lockopen.txt"))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
            Console.WriteLine("Room unlocked! You may now move to this room. (Push enter to continue.)");
        }
        public Map()//defined graph as dictionary of dictionaries set in constructor
        {
            //objective 3(i)

            map = new Dictionary<int, Dictionary<int, int>>
            {
                { 1 , new Dictionary<int, int>
                    { {2, 0}, {5, 1}, {7, 1}, {10, 1} }
                },
                { 2 , new Dictionary<int, int>
                    { {1, 0}, {3, 0}, {4, 1}}
                },
                { 3 , new Dictionary<int, int>
                    { {2, 0}, {4, 1} }
                },
                { 4 , new Dictionary<int, int>
                    { {2, 0}, {3, 0} }
                },
                { 5 , new Dictionary<int, int>
                    { {1, 0}, {6, 0} }
                },
                { 6 , new Dictionary<int, int>
                    { {5, 0} }
                },
                { 7 , new Dictionary<int, int>
                    { {1, 0}, {8, 0}, {9, 1} }
                },
                { 8 , new Dictionary<int, int>
                    { {7, 0}, {9, 1} }
                },
                { 9 , new Dictionary<int, int>
                    { {8, 0}, {7, 0} }
                },
                { 10 , new Dictionary<int, int>
                    { {1, 0 } }
                }

            };

        }
        public void DisplayMap() //displays the map depending on which rooms are unlocked in the list 'unlockedRooms'
        {
            Console.Clear();
            Console.WriteLine("-.-.-.-.-.- YOUR MAP: -.-.-.-.-.-");
            Console.WriteLine("\nYou can access the map by choosing (D)isplay when asked.\nLocked: *");
            Console.WriteLine("Scores needed to unlock each door:");
            Console.WriteLine(" - Door 4: 2 points \n - Door 5: 3 points \n - Door 7: 4 points \n - Door 9: 6 points \n - Door 10: All bosses defeated\n");
            using (StreamReader sr = new StreamReader("map.txt"))
            {
                string map = sr.ReadToEnd();
                string newmap = "";
                if (unlockedRooms.Count > 0)
                {
                    for (int i = 0; i < map.Length - 1; i++)
                    {
                        string nextChar = Convert.ToString(map[i + 1]);
                        if (int.TryParse(nextChar, out int nextCharInt))
                        {
                            if (map[i] == '*' && unlockedRooms.Contains(nextCharInt) == true)
                            {
                                newmap += " ";
                            }
                            else
                            {
                                newmap += map[i];
                            }
                        }
                        else
                        {
                            newmap += map[i];
                        }
                    }
                    newmap += map[map.Length - 1];
                    Console.WriteLine(newmap);
                }
                else
                {
                    Console.WriteLine(map);
                }
            }
        }
    }
    class Question
    {
        private static bool fileWritten = false;
        public static string[,] Questions;
        //Importing the questions with a file either from kahoot or pre-written using the function in the program
        private static void GetQuestionsFile(ref bool questionsLoaded) //objective 1(i)
        {
            string path;

            if (!fileWritten)//if the user is entering their own
            {
                Console.WriteLine("Please enter your file name. It must be in the 'bin, debug' folder of this program.");
                path = Console.ReadLine();
            }
            else //when called using the WriteQuestions() method
            {
                path = "WriteLines.txt";
            }
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {

                    int lineNumber = 0;
                    int numberOfLines = File.ReadAllLines(path).Length;//number of lines using system.io
                    Questions = new string[2, numberOfLines];
                    while (sr.Peek() >= 0) //while not at end of file
                    {
                        string line = sr.ReadLine();
                        string question = "";
                        string answer = "";
                        int commaPosition = 0;
                        bool commaFound = false;
                        while (!commaFound)
                        {
                            if (line[commaPosition] == ',') //finds where the comma is in the line
                            {
                                commaFound = true;
                            }
                            else
                            {
                                commaPosition++;
                            }
                        }
                        for (int i = 0; i < line.Length; i++) //assigning the string before the comma to 'question' and after to 'answer'
                        {
                            if (i < commaPosition && i != commaPosition)
                            {
                                question += line[i];
                            }
                            else if (i > commaPosition && i != commaPosition)
                            {
                                answer += line[i];
                            }

                        }
                        //adding into 2d array
                        Questions[0, lineNumber] = question;
                        Questions[1, lineNumber] = answer;
                        lineNumber++;

                    }
                    Console.WriteLine("Questions added! Push enter to continue.");
                    Console.ReadLine();
                    questionsLoaded = true;
                }
            }
            catch (Exception e) //validation
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message + ". Push enter.");
                questionsLoaded = false; //cannot play game if this is false
                Console.ReadLine();
            }


        }

        private static void WriteQuestions(ref bool questionsLoaded) //objective 1(ii)
        {
            Console.WriteLine("How many questions would you like to enter?");
            string initalInput = Console.ReadLine();
            int questionNumbers;
            if (int.TryParse(initalInput, out questionNumbers))
            {
                string text = "";
                for (int i = 0; i < questionNumbers; i++)
                {
                    Console.WriteLine("Question " + (i + 1) + ": ");//asks for the questions
                    text += Console.ReadLine() + ",";
                    if (i != questionNumbers - 1)
                    {
                        Console.WriteLine("Answer " + (i + 1) + ": ");
                        text += Console.ReadLine() + Environment.NewLine;//asks for answers
                    }
                    else
                    {
                        Console.WriteLine("Answer " + (i + 1) + ": "); //last line without an empty line in the array
                        text += Console.ReadLine();
                    }

                }
                using (StreamWriter outputFile = new StreamWriter("WriteLines.txt"))//writes to a file to be loaded in LoadFile() [objective 1(iia)
                {
                    outputFile.WriteLine(text);
                }
                fileWritten = true;
                GetQuestionsFile(ref questionsLoaded);

            }
            else //validation
            {
                Console.WriteLine("You have entered an invalid number. Push enter.");
                Console.ReadLine();
            }
        }
        public bool Menu(ref bool endGame, ref bool questionsLoaded) //start menu 
        {
            Program.PlayMusic("gamemusic");
            bool playGame = false;
            Console.Clear();
            Console.WriteLine("---------------------Welcome to the game!---------------------");
            Console.WriteLine("Play game (P), Load file (L), Write questions (W), Close game (C)");
            string input = Console.ReadLine().ToUpper();
            switch (input)
            {
                case "L": //loads questions
                    {
                        GetQuestionsFile(ref questionsLoaded);
                        break;
                    }
                case "C": //closes program
                    {
                        Console.WriteLine("Thank you for playing the game!");
                        endGame = true;
                        break;
                    }
                case "W": //write questions
                    {
                        WriteQuestions(ref questionsLoaded);
                        break;
                    }
                case "P": //plays game if there are questions in the dictionary
                    {
                        if (questionsLoaded == true)
                        {
                            playGame = true;
                        }
                        else
                        {
                            Console.WriteLine("Please enter your questions before beginning the game!");
                            Console.ReadLine();
                        }
                        break;
                    }
                default://validation
                    {
                        Console.WriteLine("Not a valid input. Please try again.");
                        Console.ReadLine();
                        break;
                    }

            }
            return playGame;
        }
    }

    internal class Program
    {
        //global variables
        public static int score;
        public static int userHealth;
        public static bool dead;

        public static void PlayMusic(string filename) //plays music from file
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer();
            player.SoundLocation = ("Music/" + filename + ".wav");
            player.Play();
        }
        private static void PlayGame() //plays the game
        {
            Console.WriteLine("Welcome to the game! You have arrived from the future to save our planet from an evil robot takeover.\nSelect a room to move to. From there, you can look around and search for enemies.\nYou will only find enemies when looking around, not moving.");
            Console.WriteLine("You must travel to each area, solving the puzzles along the way and defeating any enemies you come across with your knowledge.");
            Map map = new Map(); //sets up map

            //sets variables up for new game
            userHealth = 30; //objective 5(v)
            score = 0; //objective 6(i)
            dead = false;
            map.inventory.Clear();
            map.playerPosition = 1;


            while (!dead && userHealth >= 0) //loops until dead 
            {
                if (userHealth > 0)
                {
                    map.MoveInArea(); 
                }
                else
                {
                    Console.WriteLine("You have won the game! Well done, the world is now safe from the robots.\nYour score was: " + score + "."); //objective 6(i)
                    Console.ReadLine();
                    userHealth--; //breaks out of the loop
                }

            }
            Console.Clear();
            if (dead == true)
            {
                Console.WriteLine("You died! Your score: " + score + " point(s)!"); //objective 6(i)
                Console.ReadLine();
            }

        }
        static void Main(string[] args)
        {
            bool gameover = false;
            bool playGame;
            bool questionsLoaded = false;
            Question questions = new Question();
            while (!gameover)//continues until close game is selected
            { 
                playGame = questions.Menu(ref gameover, ref questionsLoaded);
                if (playGame == true)
                {
                    PlayGame();
                }
            }
            Console.ReadLine();
        }
    }
}







