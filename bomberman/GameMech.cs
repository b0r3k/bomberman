using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int bombTimer;
        public int bombMax;
        public abstract void MakeStep();
    }

    enum ArrowPressed { none, left, up, right, down, enter, space };

    class Man : MovingElement
    {
        public Man(Map map, int standingX, int standingY)
        {
            this.map = map;
            this.x = standingX;
            this.y = standingY;
            this.range = 3;
            this.bombTimer = 10;
            this.bombMax = 1;
        }

        public override void MakeStep()
        {
            int newX = x;
            int newY = y;
            switch (map.arrowPressed)
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
                    map.PutBomb(x, y, range, bombTimer);
                    break;
                default:
                    break;
            }
            if (map.IsFree(newX, newY))
            {
                map.Move(x, y, newX, newY);
            }
        }
    }

    class Monster : MovingElement
    {
        public Monster(Map map, int standingX, int standingY, char chDirection)
        {
            this.map = map;
            this.x = standingX;
            this.y = standingY;

            direction = "<^>v".IndexOf(chDirection);
        }

        public override void MakeStep()
        {
            //
        }
    }

    class Bomb : MovingElement
    {
        public Bomb(Map map, int standingX, int standingY, int thisRange, int thisTimer)
        {
            this.map = map;
            this.x = standingX;
            this.y = standingY;
            this.range = thisRange;
            this.bombTimer = thisTimer;
        }

        public override void MakeStep()
        {
            bombTimer = bombTimer - 1;
            if (bombTimer == 0)
            {
                map.Explode(x, y, range);
            }
        }
    }

    public enum State { notstarted, running, won, lost };

    class Map
    {
        private char[,] board;
        private bool[,] bombs;
        int width;
        int height;

        public State state = State.notstarted;


        Bitmap[] icons;
        int sx; // velikost kosticky ikonek

        public Man man;
        public List<MovingElement> MovingElementsNotMan;

        public ArrowPressed arrowPressed;


        public Map(string pathMap, string pathIcons)
        {
            LoadIcons(pathIcons);
            LoadMap(pathMap);
            state = State.running;
        }

        public void PutBomb(int fromX, int fromY, int fromRange, int fromTimer)
        {
            bombs[fromX, fromY] = true;
            Bomb bomb = new Bomb(this, fromX, fromY, fromRange, fromTimer);
            MovingElementsNotMan.Add(bomb);
        }

        public void Explode(int fromX, int fromY, int fromRange)
        {
            for (int i = 1; i <= fromRange; i++)
            {
                if ((i <= fromX) && board[fromX - i, fromY] == 'c')
                {
                    board[fromX - i, fromY] = ' ';
                }
                if ((i < width - fromX) && board[fromX + i, fromY] == 'c')
                {
                    board[fromX + i, fromY] = ' ';
                }
                if ((i <= fromY) && board[fromX, fromY - i] == 'c')
                {
                    board[fromX, fromY - i] = ' ';
                }
                if ((i < height - fromY) && board[fromX, fromY + i] == 'c')
                {
                    board[fromX, fromY + i] = ' ';
                }
            }
            bombs[fromX, fromY] = false;
            DeleteMovingElement(fromX, fromY);
        }

        public void Move(int fromX, int fromY, int toX, int toY)
        {
            char c = board[fromX, fromY];
            board[fromX, fromY] = ' ';
            board[toX, toY] = c;

            // podivat se, jestli tam nestal hrdina:
            if (c == 'M')
            {
                man.x = toX;
                man.y = toY;
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

        public void DeleteMovingElement(int fromX, int fromY)
        {
            // najit pohyblivyPrvek a vyhodit ho ze seznamu :
            for (int i = 0; i < MovingElementsNotMan.Count; i++)
            {
                if ((MovingElementsNotMan[i].x == fromX) && (MovingElementsNotMan[i].y == fromY))
                {
                    MovingElementsNotMan.RemoveAt(i); // 1. vyhodit ze seznamu pohyblivych prvku...
                    board[fromX, fromY] = ' ';                    // 2. ...a z planu!
                    break;
                }
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
                            this.man = new Man(this, x, y);
                            break;

                        case '<':
                        case '^':
                        case '>':
                        case 'v':
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

        public void DrawMap(Graphics g, int windowWidthPixels, int windowHeightPixels)
        {
            int windowWidth = windowWidthPixels / sx;
            int windowHeight = windowHeightPixels / sx;

            if (windowWidth > width)
                windowWidth = width;

            if (windowHeight > height)
                windowHeight = height;

            // urcit LHR vyrezu:
            int dx = man.x - windowWidth / 2;
            if (dx < 0)
                dx = 0;
            if (dx + windowWidth - 1 >= this.width)
                dx = this.width - windowWidth;

            int dy = man.y - windowHeight / 2;
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
                    if ((c == 'M') && bombs[mx, my])
                    {
                        pictureIndex = 9;
                    }
                    else if ((c == ' ') && bombs[mx, my])
                    {
                        pictureIndex = 3;
                    }
                    g.DrawImage(icons[pictureIndex], x * sx, y * sx);
                }
            }
        }

        public void MoveAll(ArrowPressed arrowPressed)
        {
            this.arrowPressed = arrowPressed;
            foreach (MovingElement mov in MovingElementsNotMan)
            {
                mov.MakeStep();
            }

            man.MakeStep();
        }

        public bool IsFree(int x, int y)
        {
            return (board[x, y] == ' ');
        }

        public bool IsClay(int x, int y)
        {
            return (board[x, y] == 'c');
        }
    }
}