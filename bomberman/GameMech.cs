using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace bomberman
{
    abstract class Element
    {
    }

    abstract class MovingElement : Element
    {
        public Map map;
        public int x;
        public int y;
        public int direction;
        public int range;
        public int speed;
        public int bombTimer;
        public bool isBomb;
        public Man owner;
        public abstract void MakeStep();
        public bool rotated;
    }

    abstract class Man : MovingElement
    {
        public int bombMax;
        public int bombNow;
        public bool alive = false;
        public void SolveBonus(int fromX, int fromY)        //says what to do when collecting a bonus
        {
            char ch = map.bonuses[fromX, fromY];
            switch (ch)
            {
                case 'R':
                    this.range++;
                    break;
                case 'r':
                    if (this.range > 1) this.range--;
                    break;
                case 'S':
                    if (this.speed > 0) this.speed--;
                    break;
                case 'B':
                    this.bombMax++;
                    break;
                default:
                    break;
            }
        }
    }

    class Player1 : Man
    {
        public Player1(Map map, int standingX, int standingY, bool isAlive)
        {
            this.map = map;
            this.x = standingX;
            this.y = standingY;
            this.range = 1;
            this.bombTimer = 20;
            this.bombMax = 1;
            this.alive = isAlive;
            this.bombNow = 0;
            this.speed = 2;
        }

        int timer = 2;
        public override void MakeStep()
        {
            int newX = x;
            int newY = y;

            if (Keyboard.IsKeyDown(Key.Up)) newY = y - 1;
            else if (Keyboard.IsKeyDown(Key.Right)) newX = x + 1;
            else if (Keyboard.IsKeyDown(Key.Down)) newY = y + 1;
            else if (Keyboard.IsKeyDown(Key.Left)) newX = x - 1;
            else if (Keyboard.IsKeyDown(Key.Enter))
            {
                if (this.bombNow < this.bombMax)
                {
                    map.PutBomb(x, y, range, bombTimer, map.player1);
                    this.bombNow++;
                }
            }

            timer--;
            if (timer == 0)
            {
                if (map.IsFree(newX, newY))
                {
                    map.Move(x, y, newX, newY);
                    if (map.bonuses[newX, newY] != '0')
                    {
                        SolveBonus(newX, newY);     //collect the bonus if there's any
                        map.bonuses[newX, newY] = '0';
                    }
                }
                timer = this.speed;
            }
        }
    }

    class Player2 : Man
    {
        public Player2(Map map, int standingX, int standingY, bool isAlive)
        {
            this.map = map;
            this.x = standingX;
            this.y = standingY;
            this.range = 1;
            this.bombTimer = 20;
            this.bombMax = 1;
            this.alive = isAlive;
            this.bombNow = 0;
            this.speed = 2;
        }

        int timer = 2;
        public override void MakeStep()
        {
            int newX = x;
            int newY = y;
            
            if (Keyboard.IsKeyDown(Key.W)) newY = y - 1;        //Keyboard.IsKeyDown uses Windows.Input to detect keys, references used
            else if (Keyboard.IsKeyDown(Key.D)) newX = x + 1;
            else if (Keyboard.IsKeyDown(Key.S)) newY = y + 1;
            else if (Keyboard.IsKeyDown(Key.A)) newX = x - 1;
            else if (Keyboard.IsKeyDown(Key.Space))
            {
                if (this.bombNow < this.bombMax)        //puts bomb if the player has any left
                {
                    map.PutBomb(x, y, range, bombTimer, map.player2);
                    this.bombNow++;
                }
            }

            timer--;
            if (timer == 0)
            {
                if (map.IsFree(newX, newY))
                {
                    map.Move(x, y, newX, newY);
                    if (map.bonuses[newX, newY] != '0')
                    {
                        SolveBonus(newX, newY);
                        map.bonuses[newX, newY] = '0';
                    }
                }
                timer = this.speed;
            }
        }
    }   

    public struct vector            //struct for working with coordinates and directions
    {
        public int x, y;
        public vector(int a, int b)
        {
            x = a;
            y = b;
        }
    }

    class Monster : MovingElement
    {
        int monsterTimer;
        public Monster(Map map, int standingX, int standingY, char chDirection)
        {
            this.map = map;
            this.x = standingX;
            this.y = standingY;
            this.rotated = false;
            direction = ">v<^".IndexOf(chDirection);
            monsterTimer = 3;
        }

        public override void MakeStep()
        {
            void OneStep()
            {
                int newX;
                int newY;
                newX = x + Map.vectors[this.direction].x;
                newY = y + Map.vectors[this.direction].y;
                if (map.IsPlayer(newX, newY)) map.KillPlayer(newX, newY);
                map.Move(x, y, newX, newY);
            }
            void RotateLeft()
            {
                this.direction = (this.direction + 3) % 4;
                map.ChangeDir(x, y, direction);
            }
            void RotateRight()
            {
                this.direction = (this.direction + 1) % 4;
                map.ChangeDir(x, y, direction);
            }

            monsterTimer--;
            if (monsterTimer == 0)      //makes a step once in 3 ticks
            {
                if (!(map.RightFree(x, y, direction)))
                {
                    if (!(map.ForwardFree(x, y, direction))) RotateLeft();
                    else OneStep();
                    rotated = false;
                }
                else if (rotated)
                {
                    OneStep();
                    rotated = false;
                }
                else
                {
                    RotateRight();
                    rotated = true;
                }
                monsterTimer = 3;
            }
        }
    }

    class Bomb : MovingElement
    {
        public Bomb(Map map, int standingX, int standingY, int thisRange, int thisTimer, Man fromMan)
        {
            this.map = map;
            this.x = standingX;
            this.y = standingY;
            this.range = thisRange;
            this.bombTimer = thisTimer;
            this.isBomb = true;
            this.owner = fromMan;
        }

        bool wasClay = false;       //bomb stops on the first clay

        public void SolveBomb(int fromX, int fromY)     //decides what to do with single frames where bomb exploded
        {
            map.explosions[fromX, fromY] = true;        //for displaying the explosions
            wasClay = false;
            char ch = map.WhatsThere(fromX, fromY);
            switch (ch)
            {
                case 'M':
                    map.player1.alive = false;
                    break;
                case 'N':
                    map.player2.alive = false;
                    break;
                case '^':
                case 'v':
                case '<':
                case '>':
                    map.DeleteMovingElement(fromX, fromY);
                    map.monsterCount--;
                    if ((map.monsterCount == 0) && (map.player1.alive || map.player2.alive)) map.state = State.won;     //if someone is alive and monsters not, the level is won
                    break;
                case 'c':
                    map.CreateBonus(fromX, fromY);      //create bonus after destroying clay
                    wasClay = true;     //stop the bomb
                    break;
                default:
                    break;
            }
            map.EmptySpace(fromX, fromY);       //when there exploded bomb, there'll be nothing
        }

        public void ExplodeBomb(int fromX, int fromY, int fromRange)
        {
            this.owner.bombNow--;       //frees the player to use the bomb again
            for (int i = 0; i <= fromRange; i++)        //explodes on up to range frames, stops at first wall or clay (or end of map)
            {
                if (!(i <= fromX) || map.IsWall(fromX - i, fromY)) break;
                else SolveBomb(fromX - i, fromY);
                if (wasClay) break;
            }
            for (int i = 0; i <= fromRange; i++)
            {
                if (!(i < map.width - fromX) || map.IsWall(fromX + i, fromY)) break;
                else SolveBomb(fromX + i, fromY);
                if (wasClay) break;
            }
            for (int i = 0; i <= fromRange; i++)
            {
                if (!(i <= fromY) || map.IsWall(fromX, fromY - i)) break;
                else SolveBomb(fromX, fromY - i);
                if (wasClay) break;
            }
            for (int i = 0; i <= fromRange; i++)
            {
                if (!(i < map.height - fromY) || map.IsWall(fromX, fromY + i)) break;
                else SolveBomb(fromX, fromY + i);
                if (wasClay) break;
            }
            map.DeleteBomb(fromX, fromY);       //deletes the bomb from moving elements
        }

        public override void MakeStep()
        {
            bombTimer--;
            if (bombTimer == 0)     //explodes after the timer reaches 0
            {
                ExplodeBomb(x, y, range);
            }
        }
    }

    public enum State { notstarted, running, won, lost };

    class Map
    {
        private char[,] board;      //map itself
        private bool[,] bombs;      //special array for bombs (there can be a bomb and player on a place at the same time)
        public char[,] bonuses;     //special array for bonus
        public bool[,] explosions;      //special array for displaying explosions
        public int width;
        public int height;
        public int monsterCount;
        int prob;
        Random rnd;

        public State state = State.notstarted;

        Bitmap[] icons;
        int sx;         //icons size

        public Player1 player1;
        public Player2 player2;
        public List<MovingElement> MovingElementsNotMan;
        public Queue<int> ToDelete;


        public Map(string pathMap, string pathIcons, string pathBonus)
        {
            LoadIcons(pathIcons);
            LoadMap(pathMap);
            LoadBonus(pathBonus);
            state = State.running;
        }

        public void PutBomb(int fromX, int fromY, int fromRange, int fromTimer, Man fromMan)        //puts bomb on a place, bomb remembers all values needed
        {
            bombs[fromX, fromY] = true;
            Bomb bomb = new Bomb(this, fromX, fromY, fromRange, fromTimer, fromMan);
            MovingElementsNotMan.Add(bomb);
        }

        public void DeleteBomb(int fromX, int fromY)
        {
            for (int i = 0; i < MovingElementsNotMan.Count; i++)        //finds thing that's bomb and on the right place - the bomb
            {
                if ((MovingElementsNotMan[i].isBomb) && (MovingElementsNotMan[i].x == fromX) && (MovingElementsNotMan[i].y == fromY))
                {
                    ToDelete.Enqueue(i);        //adds it to the queue to delete from moving elements list
                    bombs[fromX, fromY] = false;        //deletes from bombs array
                    break;
                }
            }
        }

        public void Move(int fromX, int fromY, int toX, int toY)        //move things
        {
            char c = board[fromX, fromY];       //remember what are you
            board[fromX, fromY] = ' ';      //empty the place you've just left
            board[toX, toY] = c;        //move

            if (c == 'M')       //if there's player, there's nothing else
            {
                player1.x = toX;
                player1.y = toY;
                return;
            }
           if (c == 'N')
            {
                player2.x = toX;
                player2.y = toY;
                return;
            }
           
           foreach (MovingElement mov in MovingElementsNotMan)      //move every element in the list
            {
                if ((mov.x == fromX) && (mov.y == fromY))
                {
                    mov.x = toX;
                    mov.y = toY;
                    break;
                }
            }

        }

        public void ChangeDir(int fromX, int fromY, int dir)        //changes the direction of the monster
        {
            board[fromX, fromY] = directions[dir];
        }

        public void DeleteMovingElement(int fromX, int fromY)
        {
            for (int i = 0; i < MovingElementsNotMan.Count; i++)
            {
                if ((MovingElementsNotMan[i].x == fromX) && (MovingElementsNotMan[i].y == fromY))       //find the thing that's on the place
                {
                    ToDelete.Enqueue(i);        //add it to the queue to delete
                    board[fromX, fromY] = ' ';      //empty the place on the map
                    break;
                }
            }
        }

        public void CreateBonus(int fromX, int fromY)       //with 10% probability adds a bonus to the place
        {
            prob = rnd.Next(0, 40);
            if (prob < 4)
            {
                bonuses[fromX, fromY] = "SBRr"[prob];
            }
        }

        public void LoadMap(string path)        //loads the map from the input .txt
        {
            MovingElementsNotMan = new List<MovingElement>();

            System.IO.StreamReader sr = new System.IO.StreamReader(path);
            width = int.Parse(sr.ReadLine());
            height = int.Parse(sr.ReadLine());
            bombs = new bool[width, height];
            board = new char[width, height];
            explosions = new bool[width, height];
            player1 = new Player1(this, 0, 0, false);
            player2 = new Player2(this, 0, 0, false);
            ToDelete = new Queue<int>();
            monsterCount = 0;

            for (int y = 0; y < height; y++)        //no bombs at the beginning
            {
                for (int x = 0; x < width; x++)
                {
                    bombs[x, y] = false;
                }
            }

            for (int y = 0; y < height; y++)
            {
                string line = sr.ReadLine();
                for (int x = 0; x < width; x++)
                {
                    char ch = line[x];
                    board[x, y] = ch;
                    
                    switch (ch)
                    {
                        case 'M':
                            this.player1 = new Player1(this, x, y, true);
                            break;

                        case 'N':
                            this.player2 = new Player2(this, x, y, true);
                            break;

                        case '<':
                        case '^':
                        case '>':
                        case 'v':
                            monsterCount++;
                            Monster monster = new Monster(this, x, y, ch);
                            MovingElementsNotMan.Add(monster);
                            break;

                        default:
                            break;
                    }
                }
            }
            sr.Close();
        }

        public void LoadIcons(string path)      //loads icon pictures from the file
        {
            Bitmap bmp = new Bitmap(path);
            this.sx = bmp.Height;
            int iconNumber = bmp.Width / sx;        //cuts the long line into the squares
            icons = new Bitmap[iconNumber];
            for (int i = 0; i < iconNumber; i++)
            {
                Rectangle rect = new Rectangle(i * sx, 0, sx, sx);
                icons[i] = bmp.Clone(rect, System.Drawing.Imaging.PixelFormat.DontCare);
            }
        }

        public void LoadBonus(string path)   //load where are bonuses at the beginning from another .txt file to a special array
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(path);
            bonuses = new char[width, height];

            for (int y = 0; y < height; y++)
            {
                string line = sr.ReadLine();
                for (int x = 0; x < width; x++)
                {
                    char ch = line[x];
                    bonuses[x, y] = ch;
                }
            }
            sr.Close();
            rnd = new Random();
        }

        public void DrawMap(Graphics g, int windowWidthPixels, int windowHeightPixels)
        {
            int windowWidth = windowWidthPixels / sx;
            int windowHeight = windowHeightPixels / sx;

            if (windowWidth > width)
                windowWidth = width;

            if (windowHeight > height)
                windowHeight = height;

            int dx = player1.x - windowWidth / 2;
            if (dx < 0)
                dx = 0;
            if (dx + windowWidth - 1 >= this.width)
                dx = this.width - windowWidth;

            int dy = player1.y - windowHeight / 2;
            if (dy < 0)
                dy = 0;
            if (dy + windowHeight - 1 >= this.height)
                dy = this.height - windowHeight;

            for (int x = 0; x < windowWidth; x++)
            {
                for (int y = 0; y < windowHeight; y++)
                {
                    int mx = dx + x;
                    int my = dy + y;

                    char c = board[mx, my];
                    int pictureIndex = "^>v<MN..cX...... ".IndexOf(c);      //choose index of picture based on the map
                    if (explosions[mx, my])     //if there's explosion, there's nothing else
                    {
                        pictureIndex = 15;
                        explosions[mx, my] = false;     //once it exploded, it disappears
                    }
                    else if (((c == 'M') || (c == 'N')) && bombs[mx, my])       //show that there's man&bomb at the same time
                    {
                        pictureIndex += 2;
                    }
                    else if ((c == ' ') && bombs[mx, my])       //show there's only bomb
                    {
                        pictureIndex = 14;
                    }
                    else if ((bonuses[mx, my] != '0') && ("^>v<MN..cX...... ".IndexOf(c) > 3))      //show bonus
                    {
                        pictureIndex = 10 + "SBRr".IndexOf(bonuses[mx, my]);
                    }
                    g.DrawImage(icons[pictureIndex], x * sx, y * sx);       //draw the map
                }
            }
        }

        public void MoveAll()
        { 
            foreach (MovingElement mov in MovingElementsNotMan)     //move everything (bombs and monsters)
            {
                mov.MakeStep();
            }
            if (player1.alive)      //move player1 if alive
            {
                player1.MakeStep();
            }
            if (player2.alive)      //move player2 if alive
            {
                player2.MakeStep();
            }

            if (!(player1.alive || player2.alive))      //if noone's alive, you lost
            {
                state = State.lost;
            }
        }

        public void DeleteGarbage()     //delete from list after moving everything (can't do it sooner)
        {
            int i = 0;
            while (ToDelete.Count > 0)      //while there's something to delete
            {
                MovingElementsNotMan.RemoveAt(ToDelete.Dequeue() - i);      //delete on the place from the list, minus i - that's how many are deleted already
                i++;
            }
        }

        public void KillPlayer(int x, int y)
        {
            if (board[x, y] == 'M') player1.alive = false;
            if (board[x, y] == 'N') player2.alive = false;
        }

        public void EmptySpace(int x, int y)
        {
            board[x, y] = ' ';
        }

        public char WhatsThere(int x, int y)
        {
            return board[x, y];
        }

        public bool IsFree(int x, int y)
        {
            return (board[x, y] == ' ');
        }

        public bool IsClay(int x, int y)
        {
            return (board[x, y] == 'c');
        }

        public bool IsWall(int x, int y)
        {
            return (board[x, y] == 'X');
        }

        public bool IsPlayer(int x, int y)
        {
            return ((board[x, y] == 'M') || (board[x, y] == 'N'));
        }

        public static string directions;
        public static vector[] vectors;

        public static void InitDirections()     //initilizes directions for monsters
        {
            directions = ">v<^";
            vectors = new vector[4];
            vectors[0] = new vector(0, 1);
            vectors[1] = new vector(1, 0);
            vectors[2] = new vector(0, -1);
            vectors[3] = new vector(-1, 0);
        }

        public bool ForwardFree(int a, int b, int dir)
        {
            if (IsFree(a + vectors[dir].x, b + vectors[dir].y) || IsPlayer(a + vectors[dir].x, b + vectors[dir].y))
            {
                if (bombs[a + vectors[dir].x, b + vectors[dir].y] == false) return true;
                else return false;
            }
            else return false;
        }

        public bool RightFree(int a, int b, int dir)
        {
            if (IsFree(a + vectors[(dir + 1) % 4].x, b + vectors[(dir + 1) % 4].y) || IsPlayer(a + vectors[(dir + 1) % 4].x, b + vectors[(dir + 1) % 4].y))
            {
                if (bombs[a + vectors[(dir + 1) % 4].x, b + vectors[(dir + 1) % 4].y] == false) return true;
                else return false;
            }
            else return false;
        }

        public bool LeftFree(int a, int b, int dir)
        {
            if ((IsFree((a + vectors[(dir + 3) % 4].x), (b + vectors[(dir + 3) % 4].y))) || (IsPlayer((a + vectors[(dir + 3) % 4].x), (b + vectors[(dir + 3) % 4].y))))
                if (!(bombs[(a + vectors[(dir + 3) % 4].x), (b + vectors[(dir + 3) % 4].y)])) return true;
                else return false;
            else return false;
        }
    }
}