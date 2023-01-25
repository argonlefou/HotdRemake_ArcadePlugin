using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace ArcadeMod_Config
{
    public partial class WndMain : Form
    {
        #region WIN32

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(
              string deviceName, int modeNum, ref DEVMODE devMode);
        const int ENUM_CURRENT_SETTINGS = -1;

        const int ENUM_REGISTRY_SETTINGS = -2;

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {

            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;

        }

        #endregion

        //Quality Settings
        private int _ResolutionWidth = 1920;
        private int _ResolutionHeight = 1080;
        private int _FullscreenMode = 0;       //0=Exclusive fullscreen, 1=Fullscreen window, 2=Maximized window, 3=Windowed
        private int _ShadowQuality = 0;        //0=Low, 1=MEdium, 2=High
        private int _AntialiasingEnabled = 1;  //0=Off, 1=FXAA
        private int _BloomEnabled = 0;
        private int _MotionBlurEnabled = 0;
        private int _AmbiantOcclusionEnabled = 0;

        //Graphics Settings
        private int _VSyncEnabled = 1;
        private int _HudScale = 100;
        private int _Gamma = 100;

        //Gameplay Settings (Works for both players)
        private int _HideCrosshairs = 1;
        private int _GameDifficulty = 3;
        private int _BloodColor = 0;    //0 = RED, Green = 1

        //Sound Settings (0-100%)
        private int _MasterVolume = 100;
        private int _MusicVolume = 100;
        private int _SFXVolume = 100;
        private int _UIVolume = 100;
        private int _DialogsVolume = 100;
        private int _ShotVolume = 100;
        private int _AdvertiseSound = 1;

        //Language Settings(0=Unknown, 1=English, 2=Polish, 3=Japanese, 4=German, 5=Spanish, 6=Italian, 7=French, 8=Russian, 9=Chinese)
        private int _Language = 1;

        //These ones are custom        
        private int _HideCursor = 1;
        private int _Freeplay = 0;
        private int _CreditsToStart = 1;
        private int _CreditsToContinue = 1;
        private int _InitialLife = 4;
        private int _MaxLife = 5;
        private int _InputType = 0;    //0=Mouse (1P), 1=DemulShooter(2P)

        private string _ConfFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\ArcadeMod_Config.ini";

        public WndMain()
        {
            InitializeComponent();
            ListAvailableScreenResolutions();
            ReadConf(_ConfFilePath);
            SetGuiItemsFromValues();
        }
        
        private void ListAvailableScreenResolutions()
        {
            DEVMODE vDevMode = new DEVMODE();
            int i = 0;
            while (EnumDisplaySettings(null, i, ref vDevMode))
            {
                string res = vDevMode.dmPelsWidth + "x" + vDevMode.dmPelsHeight;
                if (!CheckIfResolutionAlreadyExists(res))
                    Cbox_Resolution.Items.Add(res);
                i++;
            }
        }

        private void SetGuiItemsFromValues()
        {
            Cbox_Freeplay.SelectedIndex = _Freeplay;
            Cbox_CreditsToStart.SelectedIndex = _CreditsToStart - 1;
            Cbox_CreditsToContinue.SelectedIndex = _CreditsToContinue - 1;
            Cbox_Difficulty.SelectedIndex = _GameDifficulty;
            Cbox_InitialLife.SelectedIndex = _InitialLife - 1;
            Cbox_MaxLife.SelectedIndex = _MaxLife - 3;
            Cbox_AdvertiseSound.SelectedIndex = _AdvertiseSound;
            Cbox_BloodColor.SelectedIndex = _BloodColor;
            Cbox_Language.SelectedIndex = _Language - 1;
            Cbox_InputType.SelectedIndex = _InputType;
            if (_HideCrosshairs == 1)
                Chk_Crosshair.Checked = true;
            else
                Chk_Crosshair.Checked = false;
            if (_HideCursor == 1)
                Chk_MouseCursor.Checked = true;
            else
                Chk_MouseCursor.Checked = false;


            //Resolution:
            string res = _ResolutionWidth + "x" + _ResolutionHeight;
            if (CheckIfResolutionAlreadyExists(res))
                Cbox_Resolution.Text = res;
            Cbox_Fullscreen.SelectedIndex = _FullscreenMode;
            Cbox_ShadowQuality.SelectedIndex = _ShadowQuality;
            Cbox_Aliasing.SelectedIndex = _AntialiasingEnabled;
            Cbox_Blur.SelectedIndex = _MotionBlurEnabled;
            Cbox_Bloom.SelectedIndex = _BloomEnabled;
            Cbox_AmbiantOcclusion.SelectedIndex = _AmbiantOcclusionEnabled;
            Cbox_Vsync.SelectedIndex = _VSyncEnabled;
            TrackBar_HUD.Value = _HudScale;
            TrackBar_Gamma.Value = _Gamma;

            TrackBar_Master.Value = _MasterVolume;
            TrackBar_UI.Value = _UIVolume;
            TrackBar_SFX.Value = _SFXVolume;
            TrackBar_BGM.Value = _MusicVolume;
            TrackBar_VCE.Value = _DialogsVolume;
            TrackBar_Shot.Value = _ShotVolume;
        }

        private void Cbox_Resolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Cbox_Resolution.Text.Length > 0)
                Btn_Save.Enabled = true;
        }

        private bool CheckIfResolutionAlreadyExists(string Resolution)
        {
            for (int i = 0; i < Cbox_Resolution.Items.Count; i++)
            {
                if (Cbox_Resolution.Items[i].ToString().Equals(Resolution))
                    return true;
            }
            return false;
        }

        #region FILE I/O

        private void ReadConf(string ConfigFilePath)
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(ConfigFilePath))
                    {
                        String line = sr.ReadLine();
                        String[] buffer;
                        int i = 0;

                        while (line != null)
                        {
                            if (!line.StartsWith(";"))
                            {
                                buffer = line.Split('=');
                                if (buffer.Length == 2)
                                {
                                    String StrKey = buffer[0].Trim();
                                    String StrValue = buffer[1].Trim();

                                    if (this.GetType().GetField("_" + StrKey, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) != null)
                                    {
                                        if (int.TryParse(StrValue, out i))
                                            this.GetType().GetField("_" + StrKey, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(this, i);
                                        else
                                            MessageBox.Show("Error parsing " + StrKey + " value in INI file : " + StrValue + " is not valid", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                    else
                                        MessageBox.Show("Field not found : _" + StrKey, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            line = sr.ReadLine();
                        }
                        sr.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading " + ConfigFilePath + " : " + ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                MessageBox.Show("Config file not found ! A new one will be created", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }

        /// <summary>
        /// Write Conf file
        /// </summary>
        public bool WriteConf(String ConfigFilePath)
        {
            try
            {
                using (StreamWriter sr = new StreamWriter(ConfigFilePath, false))
                {
                    string[] res_s = Cbox_Resolution.Text.Split('x');
                    int i = 0;
                    sr.WriteLine(";Quality Settings");
                    sr.WriteLine("ResolutionWidth=" + res_s[0]);
                    sr.WriteLine("ResolutionHeight=" + res_s[1]);
                    sr.WriteLine("FullscreenMode=" + Cbox_Fullscreen.SelectedIndex.ToString());
                    sr.WriteLine("ShadowQuality=" + Cbox_ShadowQuality.SelectedIndex.ToString());
                    sr.WriteLine("AntialiasingEnabled=" + Cbox_Aliasing.SelectedIndex.ToString());
                    sr.WriteLine("BloomEnabled=" + Cbox_Bloom.SelectedIndex.ToString());
                    sr.WriteLine("MotionBlurEnabled=" + Cbox_Blur.SelectedIndex.ToString());
                    sr.WriteLine("AmbiantOcclusionEnabled=" + Cbox_AmbiantOcclusion.SelectedIndex.ToString());
                    sr.WriteLine("");
                    sr.WriteLine(";Graphics Settings");
                    sr.WriteLine("VSyncEnabled=" + Cbox_Vsync.SelectedIndex.ToString());
                    sr.WriteLine("HudScale=" + TrackBar_HUD.Value.ToString());
                    sr.WriteLine("Gamma=" + TrackBar_Gamma.Value.ToString());
                    sr.WriteLine("");
                    sr.WriteLine(";Gameplay Settings");
                    i = Chk_Crosshair.Checked ? 1 : 0;
                    sr.WriteLine("HideCrosshairs=" + i.ToString());
                    sr.WriteLine("GameDifficulty=" + Cbox_Difficulty.SelectedIndex.ToString());
                    sr.WriteLine("BloodColor=" + Cbox_BloodColor.SelectedIndex.ToString());
                    sr.WriteLine("");
                    sr.WriteLine(";Sound Settings");
                    sr.WriteLine("MasterVolume=" + TrackBar_Master.Value.ToString());
                    sr.WriteLine("MusicVolume=" + TrackBar_BGM.Value.ToString());
                    sr.WriteLine("SFXVolume=" + TrackBar_SFX.Value.ToString());
                    sr.WriteLine("UIVolume=" + TrackBar_UI.Value.ToString());
                    sr.WriteLine("DialogsVolume=" + TrackBar_VCE.Value.ToString());
                    sr.WriteLine("ShotVolume=" + TrackBar_Shot.Value.ToString());
                    sr.WriteLine("AdvertiseSound=" + Cbox_AdvertiseSound.SelectedIndex.ToString());
                    sr.WriteLine("");
                    sr.WriteLine(";Language Settings");
                    sr.WriteLine("Language=" + (Cbox_Language.SelectedIndex + 1).ToString());
                    sr.WriteLine("");
                    sr.WriteLine(";These ones are custom"); 
                    i = Chk_MouseCursor.Checked ? 1 : 0;
                    sr.WriteLine("HideCursor=" + i.ToString());
                    sr.WriteLine("Freeplay=" + Cbox_Freeplay.SelectedIndex.ToString());
                    sr.WriteLine("CreditsToStart=" + (Cbox_CreditsToStart.SelectedIndex +1).ToString());
                    sr.WriteLine("CreditsToContinue=" + (Cbox_CreditsToContinue.SelectedIndex +1).ToString());
                    sr.WriteLine("InitialLife=" + (Cbox_InitialLife.SelectedIndex + 1).ToString());
                    sr.WriteLine("MaxLife=" + (Cbox_MaxLife.SelectedIndex + 3).ToString());
                    sr.WriteLine("InputType=" + Cbox_InputType.SelectedIndex.ToString());
                    sr.Close();
                }
                return true;
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error writing confguration file : " + ConfigFilePath + "\n\n" + Ex.Message.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }


        #endregion

        private void Btn_Save_Click(object sender, EventArgs e)
        {
            if (WriteConf(_ConfFilePath))
                MessageBox.Show("Configuration file succesfully saved !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        
    }
}
