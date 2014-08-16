using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QA_Tool
{
    public partial class Password : Form
    {
        string _gPassword = string.Empty;

        public Password()
        {
            InitializeComponent();
        }

        public string PasswordForm
        {
            get
            {
                return _gPassword;
            }

            set
            {
                _gPassword = value;
            }
        }
                
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void btnClose_Click(object sender, EventArgs e)
        {   
            
        }

        private void Password_FormClosing(object sender, FormClosingEventArgs e)
        {
            PasswordForm = textBox1.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PasswordForm = textBox1.Text;
            this.Close();
        }
    }
}
