using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TreausreHuntVS
{
    public partial class Form1 : Form
    {
        private string playerName;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GameForm gameForm = new GameForm(playerName);
            gameForm.Show();
            this.Hide();
        }
        private bool isConfirmationAsked = false;

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Check if we've already asked for confirmation
            if (!isConfirmationAsked)
            {
                // Ask user for confirmation before closing
                DialogResult result = MessageBox.Show("Are you sure you want to close?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Close the application gracefully
                    isConfirmationAsked = true; // Prevent further confirmation
                    Application.Exit();
                }
                else
                {
                    // Prevent the form from closing if the user cancels
                    e.Cancel = true;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            playerName = textBox1.Text;
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            textBox1.Text = "";
        }
    }
}
