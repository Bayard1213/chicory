using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chicory
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // проверяем если ли уже запущенный экземпляр процесса
            int prC = 0;
            foreach (Process pr in Process.GetProcesses())
                if (pr.ProcessName == "chicory") prC++;
            // если да, говорим и убиваем его
            if (prC > 1)
            {
                MessageBox.Show("Application is already running!", "Warning", MessageBoxButtons.OK,MessageBoxIcon.Warning);
                Process.GetCurrentProcess().Kill();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            MainForm.AppliedUserSetting();
        }
    }
}
