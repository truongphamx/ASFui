﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASFui
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class ASFui : Form
    {
        private bool _asfRunning;
        private ASFProcess _asf;
        private readonly bool _isLocal;

        public ASFui()
        {
            while (!Util.CheckBinary())
            {
                var result = MessageBox.Show(@"ASF binary setting not configured. Configure now?",
                    @"ASF binary not found.", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    Properties.Settings.Default.ASFBinary = "Setting not configured.";
                    var settings = new SettingsForm();
                    settings.ShowDialog();
                }
                else
                {
                    Environment.Exit(-1);
                }
            }

            if (Util.CheckIfAsfIsRunning())
            {
                MessageBox.Show(@"An instance of ASF is already running. Close it.",
                    @"ASF already running.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-2);
            }
            InitializeComponent();
            Util.CheckVersion();
            _isLocal = Properties.Settings.Default.IsLocal;
        }

        private void ASFui_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized) return;
            Hide();
            TrayIcon.Visible = true;
        }

        private void ASFui_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_asfRunning || !Properties.Settings.Default.IsLocal) return;
            _asf.Stop();
        }
        
        private void GetBotList()
        {
            cbBotList.Items.Clear();
            var status = Util.SendCommand("statusall");
            var matches = Regex.Matches(status, @"Bot (.*) is");
            foreach (Match m in matches)
            {
                cbBotList.Items.Add(m.Groups[1].Value);
            }

            if (cbBotList.Items.Count <= 0) return;
            cbBotList.SelectedIndex = 0;
            EnableElements();
        }

        #region Buttons Events
        #region Start/Stop Buttons
        private void BtnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            rtbOutput.AppendText(@"Starting ASF..." + Environment.NewLine);
            if (Properties.Settings.Default.IsLocal)
            {
                _asf = new ASFProcess(rtbOutput);
                _asf.Start();
            }
            btnStop.Enabled = true;
            btnClear.Enabled = true;
            cbBotList.Enabled = true;
            rtbOutput.Enabled = true;
            btnReloadBots.Enabled = true;
            btnReloadBots.Focus();
            _asfRunning = true;
            btnASFuiSettings.Enabled = false;
            Task.Delay(1500).ContinueWith(b => GetBotList());
            tsslCommandOutput.Text = @"Started ASF server.";
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            rtbOutput.AppendText("Stopping ASF..." + Environment.NewLine);
            if (Properties.Settings.Default.IsLocal)
            {
                _asf.Stop();
            }
            DisableElements();
            tsslCommandOutput.Text = @"Stopped ASF server.";
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            rtbOutput.Clear();
            tsslCommandOutput.Text = @"Cleared log.";
        }

        private void btnReloadBots_Click(object sender, EventArgs e)
        {
            GetBotList();
            tsslCommandOutput.Text = @"Updated Bot list.";
        }

        private void btnASFuiSettings_Click(object sender, EventArgs e)
        {
            var settings = new SettingsForm();
            settings.ShowDialog();
        }
        #endregion

        #region Cards Buttons
        private void btnFarm_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var result = Util.SendCommand(Util.GenerateCommand("farm", cbBotList.SelectedItem.ToString()));
                if (_isLocal)
                {
                    tsslCommandOutput.Text = @"!farm <" + cbBotList.SelectedItem + @">: " + result;
                }
                else
                {
                    rtbOutput.AppendText(@"!farm <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
                }
            });
        }

        private void btnLoot_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var result = Util.SendCommand(Util.GenerateCommand("loot", cbBotList.SelectedItem.ToString()));
                if (_isLocal)
                {
                    tsslCommandOutput.Text = @"!loot <" + cbBotList.SelectedItem + @">: " + result;
                }
                else
                {
                    rtbOutput.AppendText(@"!loot <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
                }
            });
        }

        private void btnLootAll_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var result = Util.SendCommand("lootall");
                if (_isLocal)
                {
                    tsslCommandOutput.Text = @"!lootall: " + result;
                }
                else
                {
                    rtbOutput.AppendText(@"!lootall: " + result + Environment.NewLine);
                }
            });
        }
        #endregion

        #region Keys Buttons
        private void btnRedeem_Click(object sender, EventArgs e)
        {
            Task.Run(() => {
                var result = Util.SendCommand(Util.GenerateCommand("redeem", cbBotList.SelectedItem.ToString(), Util.MultiToOne(tbInput.Lines)));
                if (_isLocal)
                {
                    tsslCommandOutput.Text = @"!redeem <" + cbBotList.SelectedItem + @">: " + result;
                }
                else
                {
                    rtbOutput.AppendText(@"!redeem <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
                }
            });
        }

        private void btnAddLicense_Click(object sender, EventArgs e)
        {
            Task.Run(() => {
                var result = Util.SendCommand(Util.GenerateCommand("addlicense", cbBotList.SelectedItem.ToString(), Util.MultiToOne(tbInput.Lines)));
                if (_isLocal)
                {
                    tsslCommandOutput.Text = @"!addlicense <" + cbBotList.SelectedItem + @">: " + result;
                }
                else
                {
                    rtbOutput.AppendText(@"!addlicense <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
                }
            });
        }
        #endregion

        #region Games Buttons
        private void btnOwns_Click(object sender, EventArgs e)
        {
            Task.Run(() => {
                var result = Util.SendCommand(Util.GenerateCommand("owns", cbBotList.SelectedItem.ToString(), Util.MultiToOne(tbInput.Lines)));
                if (_isLocal)
                {
                    tsslCommandOutput.Text = @"!own <" + cbBotList.SelectedItem + @">: " + result;
                }
                else
                {
                    rtbOutput.AppendText(@"!own <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
                }
            });
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var result =
                    Util.SendCommand(Util.GenerateCommand("play", cbBotList.SelectedItem.ToString(), Util.MultiToOne(tbInput.Lines)));
                if (_isLocal)
                {
                    tsslCommandOutput.Text = @"!play <" + cbBotList.SelectedItem + @">: " + result;
                }
                else
                {
                    rtbOutput.AppendText(@"!play <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
                }
            });
        }
        #endregion

        #region Chat Buttons
        private void btnLeave_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand(Util.GenerateCommand("leave", cbBotList.SelectedItem.ToString()));
            if (_isLocal)
            {
                tsslCommandOutput.Text = @"!leave <" + cbBotList.SelectedItem + @">: " + result;
            }
            else
            {
                rtbOutput.AppendText(@"!leave <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
            }
        }

        private void btnRejoin_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand(Util.GenerateCommand("rejoinchat", cbBotList.SelectedItem.ToString()));
            if (_isLocal)
            {
                tsslCommandOutput.Text = @"!rejoinchat <" + cbBotList.SelectedItem + @">: " + result;
            }
            else
            {
                rtbOutput.AppendText(@"!rejoinchat <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
            }
        }
        #endregion

        #region Bots Buttons
        private void btnStartBot_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand(Util.GenerateCommand("start", cbBotList.SelectedItem.ToString()));
            if (_isLocal)
            {
                tsslCommandOutput.Text = @"!start <" + cbBotList.SelectedItem + @">: " + result;
            }
            else
            {
                rtbOutput.AppendText(@"!start <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
            }
        }

        private void btnStopBot_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand(Util.GenerateCommand("stop", cbBotList.SelectedItem.ToString()));
            if (_isLocal)
            {
                tsslCommandOutput.Text = @"!stop <" + cbBotList.SelectedItem + @">: " + result;
            }
            else
            {
                rtbOutput.AppendText(@"!stop <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
            }
        }

        private void btnPauseBot_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand(Util.GenerateCommand("pause", cbBotList.SelectedItem.ToString()));
            if (_isLocal)
            {
                tsslCommandOutput.Text = @"!pause <" + cbBotList.SelectedItem + @">: " + result;
            }
            else
            {
                rtbOutput.AppendText(@"!pause <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
            }
        }

        private void btnStatusBot_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand(Util.GenerateCommand("status", cbBotList.SelectedItem.ToString()));
            if (_isLocal)
            {
                tsslCommandOutput.Text = @"!status <" + cbBotList.SelectedItem + @">: " + result;
            }
            else
            {
                rtbOutput.AppendText(@"!status <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
            }
        }

        private void btnStatusAll_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand("statusall");
            if (!_isLocal)
            {
                rtbOutput.AppendText(@"!statusall: " + result + Environment.NewLine);
            }
        }
        #endregion

        #region ASF Buttons
        private void btnASFHelp_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand("help");
            if (_isLocal)
            {
                tsslCommandOutput.Text = @"!help: " + result;
            }
            else
            {
                rtbOutput.AppendText(@"!help: " + result + Environment.NewLine);
            }
        }

        private void btnASFUpdate_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand("update");
            if (_isLocal)
            {
                tsslCommandOutput.Text = @"!update: " + result;
            }
            else
            {
                rtbOutput.AppendText(@"!update: " + result + Environment.NewLine);
            }
        }

        private void btnASFVersion_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand("version");
            if (_isLocal)
            {
                tsslCommandOutput.Text = @"!version: " + result;
            }
            else
            {
                rtbOutput.AppendText(@"!version: " + result + Environment.NewLine);
            }
        }

        private void btnAPI_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand("api");
            if (_isLocal)
            {
                tsslCommandOutput.Text = @"!api: " + result;
            }
            else
            {
                rtbOutput.AppendText(@"!api: " + result + Environment.NewLine);
            }
        }
        #endregion

        #region 2FA Buttons
        private void btn2FA_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand(Util.GenerateCommand("2fa", cbBotList.SelectedItem.ToString()));
            if (_isLocal)
            {
                tsslCommandOutput.Text = @"!2fa <" + cbBotList.SelectedItem + @">: " + result;
            }
            else
            {
                rtbOutput.AppendText(@"!2fa <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
            }
        }

        private void btn2FAOff_Click(object sender, EventArgs e)
        {
            var result = Util.SendCommand(Util.GenerateCommand("2faoff", cbBotList.SelectedItem.ToString()));
            if (_isLocal)
            {
                tsslCommandOutput.Text = @"!2faoff <" + cbBotList.SelectedItem + @">: " + result;
            }
            else
            {
                rtbOutput.AppendText(@"!2faoff <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
            }
        }

        private void btn2FAOk_Click(object sender, EventArgs e)
        {
            tsslCommandOutput.Text = @"!2faok <" + cbBotList.SelectedItem + @">: This could take a while.";
            Task.Run(() =>
            {
                var result = Util.SendCommand(Util.GenerateCommand("2faok", cbBotList.SelectedItem.ToString()));
                if (_isLocal)
                {
                    tsslCommandOutput.Text = @"!2faok <" + cbBotList.SelectedItem + @">: " + result;
                }
                else
                {
                    rtbOutput.AppendText(@"!2faok <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
                }
            });
        }

        private void btn2FANo_Click(object sender, EventArgs e)
        {
            tsslCommandOutput.Text = @"!2fano <" + cbBotList.SelectedItem + @">: This could take a while.";
            Task.Run(() =>
            {
                var result = Util.SendCommand(Util.GenerateCommand("2fano", cbBotList.SelectedItem.ToString()));
                if (_isLocal)
                {
                    tsslCommandOutput.Text = @"!2fano <" + cbBotList.SelectedItem + @">: " + result;
                }
                else
                {
                    rtbOutput.AppendText(@"!2fano <" + cbBotList.SelectedItem + @">: " + result + Environment.NewLine);
                }
            });
        }
        #endregion
        #endregion

        private void EnableElements()
        {
            tbInput.Enabled = true;
            btnFarm.Enabled = true;
            btnLoot.Enabled = true;
            btnLootAll.Enabled = true;
            btnRedeem.Enabled = true;
            btnAddLicense.Enabled = true;
            btnOwns.Enabled = true;
            btnPlay.Enabled = true;
            btnLeave.Enabled = true;
            btnRejoin.Enabled = true;
            btnStartBot.Enabled = true;
            btnStopBot.Enabled = true;
            btnPauseBot.Enabled = true;
            btnStatusBot.Enabled = true;
            btnStatusAll.Enabled = true;
            btnASFHelp.Enabled = true;
            btnASFUpdate.Enabled = true;
            btnASFVersion.Enabled = true;
            btnAPI.Enabled = true;
            btn2FA.Enabled = true;
            btn2FAOff.Enabled = true;
            btn2FAOk.Enabled = true;
            btn2FANo.Enabled = true;
        }

        private void DisableElements()
        {
            tbInput.Enabled = false;
            btnFarm.Enabled = false;
            btnLoot.Enabled = false;
            btnLootAll.Enabled = false;
            btnRedeem.Enabled = false;
            btnAddLicense.Enabled = false;
            btnOwns.Enabled = false;
            btnPlay.Enabled = false;
            btnLeave.Enabled = false;
            btnRejoin.Enabled = false;
            btnStartBot.Enabled = false;
            btnStopBot.Enabled = false;
            btnPauseBot.Enabled = false;
            btnStatusBot.Enabled = false;
            btnStatusAll.Enabled = false;
            btnASFHelp.Enabled = false;
            btnASFUpdate.Enabled = false;
            btnASFVersion.Enabled = false;
            btnAPI.Enabled = false;
            btn2FA.Enabled = false;
            btn2FAOff.Enabled = false;
            btn2FAOk.Enabled = false;
            btn2FANo.Enabled = false;

            cbBotList.Enabled = false;
            cbBotList.Items.Clear();
            btnReloadBots.Enabled = false;
            btnStart.Enabled = true;
            btnASFuiSettings.Enabled = true;
            _asfRunning = false;
        }

        private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            TrayIcon.Visible = false;
        }

        private void tsmiClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
