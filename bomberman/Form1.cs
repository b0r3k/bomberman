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
            this.WindowState = FormWindowState.Maximized;       //maximize the window
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            g = CreateGraphics();
            level = 0;
            buttonStart.Visible = false;        //disable buttons
            buttonStart.Enabled = false;
            button2P.Visible = false;
            button2P.Enabled = false;
            label1.Visible = false;
            Map.InitDirections();       //initialize directions for monsters
            PlayLevel(level);       //open first level
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
                    if (level == 10)        //if last level, end the game
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
                    else        //if not last level, ask to go into next
                    {
                        var wonResult = MessageBox.Show("Poustoupit do další úrovně?", "Úroveň dokončena!", MessageBoxButtons.YesNo);
                        if (wonResult == DialogResult.Yes)
                        {
                            PlayLevel(level);
                        }
                    }
                    break;
                case State.lost:
                    timer.Enabled = false;      //ask to reply the level
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
            g.Clear(Color.Black);       //clear the graphics
            if (twoPlayers) map = new Map("2board" + whatLevel.ToString() + ".txt", "icons.png", "bonus" + whatLevel.ToString() + ".txt");      //choose 1 or 2 player map
            else map = new Map("board" + whatLevel.ToString() + ".txt", "icons.png", "bonus" + whatLevel.ToString() + ".txt");
            map.DeleteGarbage();
            map.DrawMap(g, ClientSize.Width, ClientSize.Height);
            map.state = State.running;
            timer.Enabled = true;
        }        

        private void bomberman_KeyUp(object sender, KeyEventArgs e)
        {
            
        }

        private void button2P_Click(object sender, EventArgs e)     //same as 1 player, just twoPlayers = true
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
