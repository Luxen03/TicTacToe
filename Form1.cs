using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TicTacToe.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TicTacToe {
    public partial class Form1 : Form {
        private static Form1 only;
        private enum Whose { None, Player, Enemy };
        private static Whose turn = Whose.Player;
        private Slot[,] board = {
            {new Slot(), new Slot(), new Slot() },
            {new Slot(), new Slot(), new Slot() },
            {new Slot(), new Slot(), new Slot() }
        };
        class Slot {
            public int x;
            public int y;
            public int botPreference;
            public Whose whose;
            public System.Windows.Forms.Button button;
            public void Clicked() {
                //change turn
                if (only.board[x, y].whose != Whose.None || Available(only.board).Count == 0) return;
                only.board[x, y].whose = turn;
                //update board
                for (int column = 0; column < 3; column++) for (int row = 0; row < 3; row++) {
                    Slot slot = only.board[column, row];
                    slot.button.BackgroundImageLayout = ImageLayout.Stretch;
                    switch (slot.whose) {
                        case Whose.Player: slot.button.BackgroundImage = Resources.X; break;
                        case Whose.Enemy: slot.button.BackgroundImage = Resources.O; break;
                    }
                }
                if (IsWinning(only.board, turn)) {
                    //temporary win
                    for (int column = 0; column < 3; column++) for (int row = 0; row < 3; row++) only.board[column, row].whose = turn;
                    for (int column = 0; column < 3; column++) for (int row = 0; row < 3; row++) {
                        Slot slot = only.board[column, row];
                        slot.button.BackgroundImageLayout = ImageLayout.Stretch;
                        switch (slot.whose) {
                            case Whose.Player: slot.button.BackgroundImage = Resources.X; break;
                            case Whose.Enemy: slot.button.BackgroundImage = Resources.O; break;
                        }
                    }
                }
                turn = (turn == Whose.Player) ? Whose.Enemy : Whose.Player;
            }
            public Slot Clone() {
                return MemberwiseClone() as Slot;
            }
        }

        public Form1() {
            only = this;
            InitializeComponent();
        }

        private static bool IsWinning(Slot[,] virtualBoard, Whose who) {
            //horizontal and vertical
            for (int grid1 = 0; grid1 < 3; grid1++) {
                int count1 = 0;
                int count2 = 0;
                for (int grid2 = 0; grid2 < 3; grid2++) {
                    if (virtualBoard[grid1, grid2].whose == who) count1++;
                    if (virtualBoard[grid2, grid1].whose == who) count2++;
                }
                if (count1 > 2 || count2 > 2) return true;
            }
            //diagonal
            for (int cases = 0; cases < 2; cases++) if (virtualBoard[0, cases * 2].whose == who && virtualBoard[1, 1].whose == who && virtualBoard[2, 2 - cases * 2].whose == who) return true;
            return false;
        }

        private static List<Slot> Available(Slot[,] virtualBoard) {
            List<Slot> availableMoves = new List<Slot>();
            for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) if (virtualBoard[i, j].whose == Whose.None) availableMoves.Add(virtualBoard[i, j]);
            return availableMoves;
        }

        private static void BotPlay() {
            int x = -1;
            int y = -1;
            float best = -9999999;
            for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) if (only.board[i, j].whose == Whose.None) {
                //make virtual board
                Slot[,] virtuals = only.board.Clone() as Slot[,];
                for (int i2 = 0; i2 < 3; i2++) for (int j2 = 0; j2 < 3; j2++) virtuals[i2, j2] = virtuals[i2, j2].Clone();
                //take turns
                virtuals[i, j].whose = turn;
                Whose newVirtualTurn = (turn == Whose.Player) ? Whose.Enemy : Whose.Player;
                //get best foresight
                float foresight = Foresight(virtuals, newVirtualTurn, 1);
                Console.WriteLine($"Foresight value of {i} {j} is {foresight}");
                if (foresight > best) {
                    Console.WriteLine("Best choice!");
                    x = i; y = j;
                    best = foresight;
                }
            }
            Console.WriteLine($"BOT CHOSE {x} {y}");
            only.board[x, y].Clicked();
        }

        private static void DebugDisplay(Slot[,] display) {
            for (int column = 0; column < 3; column++) for (int row = 0; row < 3; row++) {
                Slot slot = display[column, row];
                slot.button.BackgroundImageLayout = ImageLayout.Stretch;
                switch (slot.whose) {
                    case Whose.Player: slot.button.BackgroundImage = Resources.X; break;
                    case Whose.Enemy: slot.button.BackgroundImage = Resources.O; break;
                    case Whose.None: slot.button.BackgroundImage = null; break;
                }
            }
            only.Refresh();
        }

        private static float Foresight(Slot[,] virtualBoard, Whose virtualTurn ,int depth) {
            //Console.WriteLine($"Foresight Depth : {depth}");
            float worst_best = 0;
            int cases = 0;
            if (IsWinning(virtualBoard, Whose.Player)) return 0;
            if (IsWinning(virtualBoard, Whose.Enemy)) return 2;
            if (Available(virtualBoard).Count() < 1) return 1;
            for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) if (virtualBoard[i, j].whose == Whose.None) {
                //make virtual board
                Slot[,] virtuals = virtualBoard.Clone() as Slot[,];
                for (int i2 = 0; i2 < 3; i2++) for (int j2 = 0; j2 < 3; j2++) virtuals[i2, j2] = virtuals[i2, j2].Clone();
                //take turns
                virtuals[i, j].whose = virtualTurn;
                Whose newVirtualTurn = (virtualTurn == Whose.Player) ? Whose.Enemy : Whose.Player;
                //get worst_best foresight
                //virtualTurn = Whose.Enemy;
                float resulting = Foresight(virtuals, newVirtualTurn, depth + 1);
                //DebugDisplay(virtuals);
                switch(virtualTurn) {
                    case Whose.Player:
                        worst_best += resulting;
                        cases += 2;
                        break;
                    case Whose.Enemy:
                        if (resulting > worst_best) worst_best = resulting;
                        break;
                }
            }
            if (virtualTurn == Whose.Player) return worst_best / cases;
            return worst_best;
        }

        private void Form1_Load(object sender, EventArgs e) {
            Size = new Size(316, 340);
            for (int column = 0; column < 3; column++) for (int row = 0; row < 3; row++) {
                System.Windows.Forms.Button button = new System.Windows.Forms.Button();
                button.Size = new Size(100, 100);
                button.Location = new Point(column * 100, row * 100);
                Slot slot = only.board[column, row];
                slot.x = column;
                slot.y = row;
                button.Click += (object buttonSender, EventArgs buttonE) => {
                    Console.WriteLine($"PLAYER CHOSE {slot.x} {slot.y}");
                    slot.Clicked();
                    if (Available(only.board).Count != 0) BotPlay();
                };
                Controls.Add(button);
                only.board[column, row].button = button;
            }
        }
    }
}
