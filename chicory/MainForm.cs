using chicory.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace chicory
{
    public partial class MainForm : Form
    {
        // переменные для хранения настроек пользователя
        public static uint UserAcPowerValueIndex, UserDcPowerValueIndex, UserAcVideoValueIndex, UserDcVideoValueIndex;
        // тема оформления 
        private static uint UserLightTheme;
        // токены сабгрупп в реестре
        public static Guid GUID_SLEEP_SUBGROUP = new Guid("238C9FA8-0AAD-41ED-83f4-97BE242C8F20");
        public static Guid GUID_STANDBY_TIMEOUT = new Guid("29F6C1DB-86DA-48C5-9FDB-F2B67B1F44DA");

        public static Guid GUID_VIDEO_SUBGROUP = new Guid("7516b95f-f776-4464-8c53-06167f40cc99");
        public static Guid GUID_VIDEO_IDLE = new Guid("3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e");
        // идем до адреса активной схемы питания пользователя
        private static string AdressToPreferredPlan = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ControlPanel\\NameSpace\\{025A5937-A6BE-4686-A844-36FE4BEC8B6D}";
        private static string GuidToPreferredPlan = Registry.GetValue(AdressToPreferredPlan, "PreferredPlan", null).ToString();
        private static string AdressACDCPowerValueIndex = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Power\\User\\PowerSchemes\\" + GuidToPreferredPlan + "\\" + GUID_SLEEP_SUBGROUP + "\\" + GUID_STANDBY_TIMEOUT;
        private static string AdressACDCVideoValueIndex = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Power\\User\\PowerSchemes\\" + GuidToPreferredPlan + "\\" + GUID_VIDEO_SUBGROUP + "\\" + GUID_VIDEO_IDLE;
        // тут храним статут ожидания
        public static bool WaitingActive = false;
        private static bool TimerActive = false;
        public MainForm()
        {
            InitializeComponent();
            FormMinimized();
            // записываем тему оформления
            UserLightTheme = Convert.ToUInt32(Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme",null));
            if (UserLightTheme == 0) notifyIcon1.Icon = Resources.icon_off_to_dark;
            else notifyIcon1.Icon = Resources.icon_off_to_light;

            //регистрируем хоткей
            if (!UserDllHelper.RegisterHotKey(this.Handle.ToInt32(), 1, UserDllHelper.MOD_CONTROL, (uint)Keys.OemCloseBrackets))
                throw(new ArgumentException("Hot key error"));
            if (Properties.Settings.Default.StartEnable is true)
            {
                SwichStandby();
            }
            //MessageBox.Show
            //(
            //    "WaitingTime  " + Properties.Settings.Default.WaitingTime
            //    + "\nShowTip  " + Properties.Settings.Default.ShowTip.ToString()
            //    + "\nStartEnable  " + Properties.Settings.Default.StartEnable.ToString()
            //    + "\nUserLightTheme   " + UserLightTheme.ToString()
            //    + "\nAcVal   " + UserAcPowerValueIndex
            //    + "\nDcVal   " + UserDcPowerValueIndex
            //);

        }
        // возвращаем сохраненные настройки пользователя
        public static void AppliedUserSetting()
        {
            if (WaitingActive is true)
            {
                PowerDllHelper.SetPowerSettings(GUID_SLEEP_SUBGROUP, GUID_STANDBY_TIMEOUT, UserAcPowerValueIndex, UserDcPowerValueIndex);
                PowerDllHelper.SetPowerSettings(GUID_VIDEO_SUBGROUP, GUID_VIDEO_IDLE, UserAcVideoValueIndex, UserDcVideoValueIndex);
            }
        }
        // получаем значения полей для схемы спящего режима AC - питание от батареи, DC - питание от сети 
        private void SaveUserAcDcValueIndex()
        {
            UserAcPowerValueIndex = Convert.ToUInt32(Registry.GetValue(AdressACDCPowerValueIndex, "ACSettingIndex", null));
            UserDcPowerValueIndex = Convert.ToUInt32(Registry.GetValue(AdressACDCPowerValueIndex, "DCSettingIndex", null));

            UserAcVideoValueIndex = Convert.ToUInt32(Registry.GetValue(AdressACDCVideoValueIndex, "ACSettingIndex", null));
            UserDcVideoValueIndex = Convert.ToUInt32(Registry.GetValue(AdressACDCVideoValueIndex, "DCSettingIndex", null));
        }
        // функция для выставления нужных значений при включении/выключении режима ожидания
        private void AppliedAcDcValue(Guid subgroup, Guid settingsField, uint valueAc, uint valueDc, Icon notifyIcon, string notifyText, bool waitingStatus)
        {
            PowerDllHelper.SetPowerSettings(subgroup, settingsField, valueAc, valueDc);
            notifyIcon1.Icon = notifyIcon;
            notifyIcon1.Text = notifyText;
            WaitingActive = waitingStatus;
        }
        // сворачиваем окно
        private void FormMinimized()
        {
            WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            notifyIcon1.Visible = true;
        }
        // выносим отдельно балун что бы не мусорить
        private void ShowBallonTip(string ballonString)
        {
            if (Properties.Settings.Default.ShowTip == true)
            {
                notifyIcon1.ShowBalloonTip(
                    10000,
                    "Waiting status",
                    ballonString,
                    ToolTipIcon.Info
                );
            }
        }
        // переключение режима ожидания
        private void SwichStandby()
        {
            // если мы еще не в режиме ожидания включаем его
            if (WaitingActive is false)
            {
                // если таймера не стоит
                if (Properties.Settings.Default.WaitingTime == 0)
                {
                    // сохраняем настройки юзера
                    SaveUserAcDcValueIndex();
                    //меняем иконку в зависимости от темы оформления
                    if (UserLightTheme == 0) {
                        AppliedAcDcValue(GUID_SLEEP_SUBGROUP, GUID_STANDBY_TIMEOUT, 0, 0, Resources.icon_on_to_dark, "chicory (on)", true);
                        AppliedAcDcValue(GUID_VIDEO_SUBGROUP, GUID_VIDEO_IDLE, 0, 0, Resources.icon_on_to_dark, "chicory (on)", true);
                    }
                    else if (UserLightTheme == 1)
                    {
                        AppliedAcDcValue(GUID_SLEEP_SUBGROUP, GUID_STANDBY_TIMEOUT, 0, 0, Resources.icon_on_to_dark, "chicory (on)", true);
                        AppliedAcDcValue(GUID_VIDEO_SUBGROUP, GUID_VIDEO_IDLE, 0, 0, Resources.icon_on_to_light, "chicory (on)", true);
                    } 
                }
                // если есть таймер
                else
                {
                    timer1.Interval = (int)Properties.Settings.Default.WaitingTime * 60000;
                    SaveUserAcDcValueIndex();
                    if (UserLightTheme == 0)
                    {
                        AppliedAcDcValue(GUID_SLEEP_SUBGROUP, GUID_STANDBY_TIMEOUT, 0, 0, Resources.icon_on_to_dark, "chicory (on)", true);
                        AppliedAcDcValue(GUID_VIDEO_SUBGROUP, GUID_VIDEO_IDLE, 0, 0, Resources.icon_on_to_dark, "chicory (on)", true);
                    }
                    else if (UserLightTheme == 1)
                    {
                        AppliedAcDcValue(GUID_SLEEP_SUBGROUP, GUID_STANDBY_TIMEOUT, 0, 0, Resources.icon_on_to_dark, "chicory (on)", true);
                        AppliedAcDcValue(GUID_VIDEO_SUBGROUP, GUID_VIDEO_IDLE, 0, 0, Resources.icon_on_to_dark, "chicory (on)", true);
                    };
                    timer1.Start();
                    TimerActive = true;
                }
                ShowBallonTip("Standby enabled");

            }
            // и соответственно когда уже ожидаем - выключаем
            else if (WaitingActive is true)
            {
                if (TimerActive is false)
                {
                    if (UserLightTheme == 0)
                    {
                        AppliedAcDcValue(GUID_SLEEP_SUBGROUP, GUID_STANDBY_TIMEOUT, UserAcPowerValueIndex, UserDcPowerValueIndex, Resources.icon_off_to_dark, "chicory (off)", false);
                        AppliedAcDcValue(GUID_VIDEO_SUBGROUP, GUID_VIDEO_IDLE, UserAcVideoValueIndex, UserDcVideoValueIndex, Resources.icon_off_to_dark, "chicory (off)", false);
                    }
                    else if (UserLightTheme == 1) 
                    { 
                        AppliedAcDcValue(GUID_SLEEP_SUBGROUP, GUID_STANDBY_TIMEOUT, UserAcPowerValueIndex, UserDcPowerValueIndex, Resources.icon_off_to_light, "chicory (off)", false);
                        AppliedAcDcValue(GUID_VIDEO_SUBGROUP, GUID_VIDEO_IDLE, UserAcVideoValueIndex, UserDcVideoValueIndex, Resources.icon_off_to_light, "chicory (off)", false);
                    }
                }
                else if (TimerActive is true)
                {
                    if (UserLightTheme == 0)
                    {
                        AppliedAcDcValue(GUID_SLEEP_SUBGROUP, GUID_STANDBY_TIMEOUT, UserAcPowerValueIndex, UserDcPowerValueIndex, Resources.icon_off_to_dark, "chicory (off)", false);
                        AppliedAcDcValue(GUID_VIDEO_SUBGROUP, GUID_VIDEO_IDLE, UserAcVideoValueIndex, UserDcVideoValueIndex, Resources.icon_off_to_dark, "chicory (off)", false);
                    }
                    else if (UserLightTheme == 1) 
                    {
                        AppliedAcDcValue(GUID_SLEEP_SUBGROUP, GUID_STANDBY_TIMEOUT, UserAcPowerValueIndex, UserDcPowerValueIndex, Resources.icon_off_to_light, "chicory (off)", false);
                        AppliedAcDcValue(GUID_VIDEO_SUBGROUP, GUID_VIDEO_IDLE, UserAcVideoValueIndex, UserDcVideoValueIndex, Resources.icon_off_to_light, "chicory (off)", false);
                    }
                       
                    timer1.Stop();
                }
                ShowBallonTip("Standby disabled");
            }
        }

        // на лкм запускаем ожидание
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SwichStandby();
            }
        }
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings SettingsForm = new Settings();
            SettingsForm.Show();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UserDllHelper.HideFromAltTab(this.Handle);
        }

        // по завершению таймера делаем как было
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (UserLightTheme == 0)
            {
                AppliedAcDcValue(GUID_SLEEP_SUBGROUP, GUID_STANDBY_TIMEOUT, UserAcPowerValueIndex, UserDcPowerValueIndex, Resources.icon_off_to_dark, "chicory (off)", false);
                AppliedAcDcValue(GUID_VIDEO_SUBGROUP, GUID_VIDEO_IDLE, UserAcVideoValueIndex, UserDcVideoValueIndex, Resources.icon_off_to_dark, "chicory (off)", false);
            }
            else if (UserLightTheme == 1) 
            {
                AppliedAcDcValue(GUID_SLEEP_SUBGROUP, GUID_STANDBY_TIMEOUT, UserAcPowerValueIndex, UserDcPowerValueIndex, Resources.icon_off_to_light, "chicory (off)", false);
                AppliedAcDcValue(GUID_VIDEO_SUBGROUP, GUID_VIDEO_IDLE, UserAcVideoValueIndex, UserDcVideoValueIndex, Resources.icon_off_to_light, "chicory (off)", false);
            }
        }

        // при закрытии приложения возвращаем, все зависимости от статуса ожидания, настройки пользователя
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppliedUserSetting();
            Application.Exit();
        }
        //обработчик сообщений для окна
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case UserDllHelper.WM_HOTKEY:
                    SwichStandby();
                    break;
            }
            base.WndProc(ref m);
        }
    }
}
