using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bomberman
{
    public partial class bomberman : Form
    {
        public bomberman()
        {
            InitializeComponent();
        }
        Map map;
        Graphics g;
        int level;
        bool twoPlayers = false;

        private void buttonStart_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            g = CreateGraphics();
            level = 0;
            buttonStart.Visible = false;
            buttonStart.Enabled = false;
            button2P.Visible = false;
            button2P.Enabled = false;
            label1.Visible = false;
            Map.InitDirections();
            PlayLevel(level);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            switch (map.state)
            {
                case State.running:
                    map.MoveAll();
                    map.DeleteGarbage();
                    map.DrawMap(g, ClientSize.Width, ClientSize.Height);
                    break;
                case State.won:
                    timer.Enabled = false;
                    level++;
                    if (level == 10)
                    {
                        level = 0;
                        var endResult = MessageBox.Show("Hrát znovu?", "Gratulujeme! Dokončili jste hru!", MessageBoxButtons.YesNo);
                        if (endResult == DialogResult.Yes)
                        {
                            PlayLevel(level);
                        }
                        else
                        {
                            this.Close();
                        }
                    }
                    else
                    {
                        var wonResult = MessageBox.Show("Poustoupit do další úrovně?", "Úroveň dokončena!", MessageBoxButtons.YesNo);
                        if (wonResult == DialogResult.Yes)
                        {
                            PlayLevel(level);
                        }
                    }
                    break;
                case State.lost:
                    timer.Enabled = false;
                    var lostResult = MessageBox.Show("Zkusit úroveň znovu?", "Prohráli jste!", MessageBoxButtons.YesNo);
                    if (lostResult == DialogResult.Yes)
                    {
                        PlayLevel(level);
                    }
                    else
                    {
                        this.Close();
                    }
                    break;
                default:
                    break;
            }
        }

        void PlayLevel(int whatLevel)
        {
            g.Clear(Color.Black);
            if (twoPlayers) map = new Map("2board" + whatLevel.ToString() + ".txt", "icons.png", "bonus" + whatLevel.ToString() + ".txt");
            else map = new Map("board" + whatLevel.ToString() + ".txt", "icons.png", "bonus" + whatLevel.ToString() + ".txt");
            map.DeleteGarbage();
            map.DrawMap(g, ClientSize.Width, ClientSize.Height);
            map.state = State.running;
            timer.Enabled = true;
        }

        ArrowPressed arrowPressed = ArrowPressed.none;

        /*protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up)
            {
                arrowPressed = ArrowPressed.up;
                return true;
            }
            if (keyData == Keys.Down)
            {
                arrowPressed = ArrowPressed.down;
                return true;
            }
            if (keyData == Keys.Left)
            {
                arrowPressed = ArrowPressed.left;
                return true;
            }
            if (keyData == Keys.Right)
            {
                arrowPressed = ArrowPressed.right;
                return true;
            }
            if (keyData == Keys.Enter)
            {
                arrowPressed = ArrowPressed.enter;
                return true;
            }
            if (keyData == Keys.Space)
            {
                arrowPressed = ArrowPressed.space;
                return true;
            }
            if (keyData == Keys.W)
            {
                arrowPressed = ArrowPressed.wkey;
                return true;
            }
            if (keyData == Keys.A)
            {
                arrowPressed = ArrowPressed.akey;
                return true;
            }
            if (keyData == Keys.S)
            {
                arrowPressed = ArrowPressed.skey;
                return true;
            }
            if (keyData == Keys.D)
            {
                arrowPressed = ArrowPressed.dkey;
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }*/

        private void bomberman_KeyUp(object sender, KeyEventArgs e)
        {
            arrowPressed = ArrowPressed.none;
        }

        private void button2P_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            g = CreateGraphics();
            level = 0;
            twoPlayers = true;
            buttonStart.Visible = false;
            buttonStart.Enabled = false;
            button2P.Visible = false;
            button2P.Enabled = false;
            label1.Visible = false;
            Map.InitDirections();
            PlayLevel(level);
        }
    }
}
