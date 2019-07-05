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

        private void buttonStart_Click(object sender, EventArgs e)
        {
            g = CreateGraphics();
            map = new Map("board.txt.", "icons.png");
            timer.Enabled = true;
            buttonStart.Visible = false;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            switch (map.state)
            {
                case State.running:
                    map.MoveAll(arrowPressed);
                    map.DrawMap(g, ClientSize.Width, ClientSize.Height);
                    break;
                case State.won:
                    timer.Enabled = false;
                    MessageBox.Show("Vyhráli jste!");
                    break;
                case State.lost:
                    timer.Enabled = false;
                    MessageBox.Show("Prohráli jste!");
                    break;
                default:
                    break;
            }
        }

        ArrowPressed arrowPressed = ArrowPressed.none;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
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
        }

        private void bomberman_KeyUp(object sender, KeyEventArgs e)
        {
            arrowPressed = ArrowPressed.none;
        }
    }
}
