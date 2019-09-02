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

    enum ArrowPressed { none, left, up, right, down, enter, space, wkey, akey, skey, dkey };

    abstract class Man : MovingElement
    {
        public int bombMax;
        public int bombNow;
        public bool alive = false;
        public void SolveBonus(int fromX, int fromY)
        {
            char ch = map.bonuses[fromX, fromY];
            switch (ch)
            {
                case 'R':
                    this.range++;
                    break;
                case 'r':
                    this.range--;
                    break;
                case 'S':
                    if (this.speed > 0) this.speed--;
                    break;
                case 'B':
                    this.bombMax++;
                    break;
                /*case 'T':
                    this.bombTimer = this.bombTimer + 2;
                    break;
                case 't':
                    this.bombTimer = this.bombTimer - 2;
                    break;*/
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
            this.speed = 1;
        }

        int timer = 1;
        public override void MakeStep()
        {
            int newX = x;
            int newY = y;

            timer--;
            if (timer <= 0)
            {
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
                timer = this.speed;
            }
            /*switch (map.arrowPressed)
            {
                case ArrowPressed.none:
                    break;
                case ArrowPressed.left:
                    newX = x - 1;
                    break;
                case ArrowPressed.up:
                    newY = y - 1;
                    break;
                case ArrowPressed.right:
                    newX = x + 1;
                    break;
                case ArrowPressed.down:
                    newY = y + 1;
                    break;
                case ArrowPressed.enter:
                    if (this.bombNow < this.bombMax)
                    {
                        map.PutBomb(x, y, range, bombTimer, map.player1);
                        this.bombNow++;
                    }
                    break;
                default:
                    break;
            }*/
            if (map.IsFree(newX, newY))
            {
                map.Move(x, y, newX, newY);
                if (map.bonuses[newX, newY] != '0')
                {
                    SolveBonus(newX, newY);
                    map.bonuses[newX, newY] = '0';
                }
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
            this.bombTimer = 10;
            this.bombMax = 1;
            this.alive = isAlive;
            this.bombNow = 0;
            this.speed = 1;
        }

        int timer = 1;
        public override void MakeStep()
        {
            int newX = x;
            int newY = y;

            timer--;
            if (timer <= 0)
            {
                if (Keyboard.IsKeyDown(Key.W)) newY = y - 1;
                else if (Keyboard.IsKeyDown(Key.D)) newX = x + 1;
                else if (Keyboard.IsKeyDown(Key.S)) newY = y + 1;
                else if (Keyboard.IsKeyDown(Key.A)) newX = x - 1;
                else if (Keyboard.IsKeyDown(Key.Space))
                {
                    if (this.bombNow < this.bombMax)
                    {
                        map.PutBomb(x, y, range, bombTimer, map.player2);
                        this.bombNow++;
                    }
                }
                timer = this.speed;
            }

            if (map.IsFree(newX, newY))
            {
                map.Move(x, y, newX, newY);
                if (map.bonuses[newX, newY] != '0')
                {
                    SolveBonus(newX, newY);
                    map.bonuses[newX, newY] = '0';
                }
            }
        }
    }   

    public struct vector
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
            if (monsterTimer == 0)
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

        public void SolveBomb(int fromX, int fromY)
        {
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
                    if ((map.monsterCount == 0) && (map.player1.alive || map.player2.alive)) map.state = State.won;
                    break;
                case 'c':
                    map.CreateBonus(fromX, fromY);
                    break;
                default:
                    break;
            }
            map.EmptySpace(fromX, fromY);
        }

        public void ExplodeBomb(int fromX, int fromY, int fromRange)
        {
            this.owner.bombNow--;
            for (int i = 0; i <= fromRange; i++)
            {
                if (!(i <= fromX) || map.IsWall(fromX - i, fromY)) break;
                else SolveBomb(fromX - i, fromY);
            }
            for (int i = 0; i <= fromRange; i++)
            {
                if (!(i < map.width - fromX) || map.IsWall(fromX + i, fromY)) break;
                else SolveBomb(fromX + i, fromY);
            }
            for (int i = 0; i <= fromRange; i++)
            {
                if (!(i <= fromY) || map.IsWall(fromX, fromY - i)) break;
                else SolveBomb(fromX, fromY - i);
            }
            for (int i = 0; i <= fromRange; i++)
            {
                if (!(i < map.height - fromY) || map.IsWall(fromX, fromY + i)) break;
                else SolveBomb(fromX, fromY + i);
            }
            map.DeleteBomb(fromX, fromY);
        }

        public override void MakeStep()
        {
            bombTimer--;
            if (bombTimer == 0)
            {
                ExplodeBomb(x, y, range);
            }
        }
    }

    public enum State { notstarted, running, won, lost };

    class Map
    {
        private char[,] board;
        private bool[,] bombs;
        public char[,] bonuses;
        public int width;
        public int height;
        public int monsterCount;
        int prob;
        string str;
        Random rnd;

        public State state = State.notstarted;

        Bitmap[] icons;
        int sx; // velikost kosticky ikonek

        public Player1 player1;
        public Player2 player2;
        public List<MovingElement> MovingElementsNotMan;
        public Queue<int> ToDelete;

        public ArrowPressed arrowPressed;


        public Map(string pathMap, string pathIcons, string pathBonus)
        {
            LoadIcons(pathIcons);
            LoadMap(pathMap);
            LoadBonus(pathBonus);
            state = State.running;
        }

        public void PutBomb(int fromX, int fromY, int fromRange, int fromTimer, Man fromMan)
        {
            bombs[fromX, fromY] = true;
            Bomb bomb = new Bomb(this, fromX, fromY, fromRange, fromTimer, fromMan);
            MovingElementsNotMan.Add(bomb);
        }

        public void DeleteBomb(int fromX, int fromY)
        {
            // najit pohyblivyPrvek a vyhodit ho ze seznamu :
            for (int i = 0; i < MovingElementsNotMan.Count; i++)
            {
                if ((MovingElementsNotMan[i].isBomb) && (MovingElementsNotMan[i].x == fromX) && (MovingElementsNotMan[i].y == fromY))
                {
                    ToDelete.Enqueue(i);
                    bombs[fromX, fromY] = false;
                    //MovingElementsNotMan[i].delete = true;
                    //map.MovingElementsNotMan.RemoveAt(i); // 1. vyhodit ze seznamu pohyblivych prvku...
                    break;
                }
            }
        }

        public void Move(int fromX, int fromY, int toX, int toY)
        {
            char c = board[fromX, fromY];
            board[fromX, fromY] = ' ';
            board[toX, toY] = c;

            // podivat se, jestli tam nestal hrdina:
            if (c == 'M')
            {
                player1.x = toX;
                player1.y = toY;
                return; // kdyz na [zY,zX] stoji hrdina, tak tam nic jineho nestoji
            }
           if (c == 'N')
            {
                player2.x = toX;
                player2.y = toY;
                return; // kdyz na [zY,zX] stoji hrdina, tak tam nic jineho nestoji
            }

            // najit pripadny pohyblivyPrvek a zmenit mu polohu :
            foreach (MovingElement mov in MovingElementsNotMan)
            {
                if ((mov.x == fromX) && (mov.y == fromY))
                {
                    mov.x = toX;
                    mov.y = toY;
                    break; // jakmile tam stoji jeden, tak uz tam nikdo jiny nestoji
                }
            }

        }

        public void ChangeDir(int fromX, int fromY, int dir)
        {
            board[fromX, fromY] = directions[dir];
        }

        public void DeleteMovingElement(int fromX, int fromY)
        {
            // najit pohyblivyPrvek a vyhodit ho ze seznamu :
            for (int i = 0; i < MovingElementsNotMan.Count; i++)
            {
                if ((MovingElementsNotMan[i].x == fromX) && (MovingElementsNotMan[i].y == fromY))
                {
                    //MovingElementsNotMan[i].delete = true;
                    ToDelete.Enqueue(i);
                    //MovingElementsNotMan.RemoveAt(i); // 1. vyhodit ze seznamu pohyblivych prvku...
                    board[fromX, fromY] = ' ';                    // 2. ...a z planu!
                    break;
                }
            }
        }

        public void CreateBonus(int fromX, int fromY)
        {
            //s určitou pravděpodobností dát na místo bonusy (rychlost, víc bomb, dosah bomb)
            prob = rnd.Next(0, 40);
            if (prob < 4)
            {
                bonuses[fromX, fromY] = str[prob];
            }
        }

        public void LoadMap(string path)
        {
            MovingElementsNotMan = new List<MovingElement>();

            System.IO.StreamReader sr = new System.IO.StreamReader(path);
            width = int.Parse(sr.ReadLine());
            height = int.Parse(sr.ReadLine());
            bombs = new bool[width, height];
            board = new char[width, height];
            player1 = new Player1(this, 0, 0, false);
            player2 = new Player2(this, 0, 0, false);
            ToDelete = new Queue<int>();
            monsterCount = 0;

            for (int y = 0; y < height; y++)
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

                    // vytvorit pripadne pohyblive objekty:
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

        public void LoadIcons(string path)
        {
            Bitmap bmp = new Bitmap(path);
            this.sx = bmp.Height;
            int iconNumber = bmp.Width / sx; // predpokladam, ze to jsou kosticky v rade
            icons = new Bitmap[iconNumber];
            for (int i = 0; i < iconNumber; i++)
            {
                Rectangle rect = new Rectangle(i * sx, 0, sx, sx);
                icons[i] = bmp.Clone(rect, System.Drawing.Imaging.PixelFormat.DontCare);
            }
        }

        public void LoadBonus(string path) //přepsat tak, aby bylo součástí mapy a cestu k bonusům dostávala jako další parametr, jen načítala do nového pole
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
            str = "RrSBTt";
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

            // urcit LHR vyrezu:
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
                    int mx = dx + x; // index do mapy
                    int my = dy + y; // index do mapy

                    char c = board[mx, my];
                    int pictureIndex = " cMB<^>vXDEe".IndexOf(c); // 0..
                    if (c == 'N')
                    {
                        pictureIndex = 2;
                    }
                    if (((c == 'M') || (c == 'N')) && bombs[mx, my])
                    {
                        pictureIndex = 9;
                    }
                    else if ((c == ' ') && bombs[mx, my])
                    {
                        pictureIndex = 3;
                    }
                    else if (bonuses[mx, my] != '0')
                    {
                        pictureIndex = 10;
                    }
                    g.DrawImage(icons[pictureIndex], x * sx, y * sx);
                }
            }
        }

        public void MoveAll()
        {
            //this.arrowPressed = arrowPressed;
            foreach (MovingElement mov in MovingElementsNotMan)
            {
                mov.MakeStep();
            }
            if (player1.alive)
            {
                player1.MakeStep();
            }
            if (player2.alive)
            {
                player2.MakeStep();
            }

            if (!(player1.alive || player2.alive))
            {
                state = State.lost;
            }
        }

        public void DeleteGarbage()
        {
            int i = 0;
            while (ToDelete.Count > 0)
            {
                MovingElementsNotMan.RemoveAt(ToDelete.Dequeue() - i);
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

        public static void InitDirections()
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