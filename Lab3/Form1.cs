using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.ClearAll();

            foreach (char c in textBox1.Text.ToArray())
                Program.CharList.Add(c);
            Program.CharList.Add('\ud801');
            int type = 0;
            Program.LexAnalize(ref type);
            textBox2.Text = String.Join(System.Environment.NewLine, Program.findedServiceWords);
            textBox3.Text = String.Join(System.Environment.NewLine, Program.findedOperatorChars);
            textBox4.Text = String.Join(System.Environment.NewLine, Program.findedSpecialChars);
            textBox7.Text = String.Join(System.Environment.NewLine, Program.findedStrings);
            textBox6.Text = String.Join(System.Environment.NewLine, Program.findedIdentifiers);
            textBox5.Text = String.Join(System.Environment.NewLine, Program.findedNumbers);

        }
    }
}
