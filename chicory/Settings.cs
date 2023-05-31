using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace chicory
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - (this.Width + 100), Screen.PrimaryScreen.Bounds.Height - (this.Height + 100));
        }
        // при загрузке формы показываем значения настроек пользователя
        private void Settings_Load(object sender, EventArgs e)
        {
            comboBox1.Text = Properties.Settings.Default.WaitingTime.ToString();
            checkBox1.Checked = Properties.Settings.Default.ShowTip;
            checkBox2.Checked = Properties.Settings.Default.StartEnable;
        }
        // запрещаем тыкать в поле что либо кроме int и control
        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && !Char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void comboBox1_Enter(object sender, EventArgs e)
        {
            comboBox1.BackColor = Color.White;
        }
        // выставляем значения по дефолту
        private void defaultButton_Click(object sender, EventArgs e)
        {
            comboBox1.Text = "0";
            checkBox1.Checked = true;
            checkBox2.Checked = false;
        }
        // сохраняем настройки беря данные с полей
        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                Properties.Settings.Default.WaitingTime = uint.Parse(comboBox1.Text);
                MessageBox.Show("Changes saved","Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch 
            {
                comboBox1.BackColor= Color.LightPink;
                MessageBox.Show("The timeout field can only contain integers", "Incorrect value input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            Properties.Settings.Default.ShowTip = checkBox1.Checked;
            Properties.Settings.Default.StartEnable = checkBox2.Checked;
            // сохраняем настройки в Properties
            Properties.Settings.Default.Save();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        // ставим ограничение на максимальное время таймера
        private void comboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            uint x;
            if (uint.TryParse(comboBox1.Text, out x))
            {
                if (x > 720) comboBox1.Text = "720";
            }
            else
                comboBox1.Text = "720";
        }
    }
}
