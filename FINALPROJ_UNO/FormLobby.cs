using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using UNOFinal.Models;

namespace UNOFinal
{
    public class FormLobby : Form
    {
        private readonly Color BG_DARK = Color.FromArgb(19, 32, 24);
        private readonly Color BG_MID = Color.FromArgb(30, 45, 36);
        private readonly Color BG_CARD = Color.FromArgb(24, 38, 28);
        private readonly Color BG_BAR = Color.FromArgb(10, 24, 16);
        private readonly Color RED = Color.FromArgb(230, 57, 70);
        private readonly Color BLUE = Color.FromArgb(33, 150, 243);
        private readonly Color GREEN_P = Color.FromArgb(76, 175, 80);
        private readonly Color BTN_BRD = Color.FromArgb(58, 90, 68);
        private readonly Color TEXT_MUT = Color.FromArgb(106, 138, 116);
        private readonly Color TEXT_MN = Color.FromArgb(224, 224, 224);

        private TextBox[] txtNames = new TextBox[3];
        private Button[][] btnToggle = new Button[3][];
        private ComboBox[] cmbDiff = new ComboBox[3];
        private bool[] isHuman = { true, false, false };
        private Color[] seatClrs;
        private PictureBox[] avatarPics = new PictureBox[3];

        public FormLobby()
        {
            seatClrs = new Color[] { RED, BLUE, GREEN_P };
            SetupForm();
            this.Load += (s, e) => BuildUI();
        }

        private void SetupForm()
        {
            this.Text = "UNO - Game Setup";
            this.BackColor = BG_DARK;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 10f);
        }
        private void Form_Load(object sender, EventArgs e)
        {
            MusicManager.PlayMenuMusic();
        }

        private void UpdateAvatarForSeat(int seat, string playerName)
        {
            if (string.IsNullOrEmpty(playerName) || playerName.StartsWith("CPU"))
                return;

            int avatarId = new DatabaseManager().GetPlayerAvatar(playerName);
            if (avatarPics[seat] != null)
                avatarPics[seat].Image = AvatarDrawer.ToBitmap(avatarId, 40);
        }

        private void BuildUI()
        {
            this.Controls.Clear();
            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height;
            int cx = W / 2;

            // Card panel width — responsive but capped
            int panelW = Math.Min(520, W - 80);
            int panelX = cx - panelW / 2;

            // ── Top bar ───────────────────────────────────────────────────────
            Panel pnlBar = new Panel
            {
                Bounds = new Rectangle(0, 0, W, 44),
                BackColor = BG_BAR
            };
            Button btnBack = MakeBtn("Back", 12, 5, 80, 34, BG_MID, TEXT_MUT);
            btnBack.Click += (s, e) => { new FormMainMenu().Show(); this.Close(); };
            Label lblTitle = new Label
            {
                Text = "Game Setup",
                ForeColor = TEXT_MN,
                Font = new Font("Segoe UI", 13f),
                Bounds = new Rectangle(106, 0, 300, 44),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            pnlBar.Controls.Add(btnBack);
            pnlBar.Controls.Add(lblTitle);
            this.Controls.Add(pnlBar);

            // ── Section label ─────────────────────────────────────────────────
            Label lblSect = new Label
            {
                Text = "CONFIGURE PLAYERS",
                ForeColor = TEXT_MUT,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Location = new Point(panelX, 66),
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblSect);

            // ── Seat rows ─────────────────────────────────────────────────────
            int rowH = 88;
            int rowGap = 10;
            int firstRowY = 90;
            string[] names = { "Player 1", "CPU 1", "CPU 2" };

            for (int i = 0; i < 3; i++)
                BuildSeatRow(i, names[i], panelX, firstRowY + i * (rowH + rowGap), panelW, rowH);

            // ── Rules box ─────────────────────────────────────────────────────
            int rulesY = firstRowY + 3 * (rowH + rowGap) + 10;
            Panel pnlRules = new Panel
            {
                Bounds = new Rectangle(panelX, rulesY, panelW, 60),
                BackColor = BG_CARD
            };
            Rounded(pnlRules, 8);
            Label lblRT = new Label
            {
                Text = "Game Rules",
                ForeColor = Color.FromArgb(139, 196, 160),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 8),
                BackColor = Color.Transparent
            };
            Label lblRB = new Label
            {
                Text = "Standard UNO rules. Wild Draw Four challenges enabled. UNO call penalty: draw 2 cards.",
                ForeColor = TEXT_MUT,
                Font = new Font("Segoe UI", 8.5f),
                Bounds = new Rectangle(12, 28, panelW - 24, 24),
                BackColor = Color.Transparent
            };
            pnlRules.Controls.Add(lblRT);
            pnlRules.Controls.Add(lblRB);
            this.Controls.Add(pnlRules);

            // ── Start button ──────────────────────────────────────────────────
            int startY = rulesY + 76;
            Button btnStart = MakeBtn("Start Game", panelX, startY, panelW, 52, RED, Color.White);
            btnStart.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            btnStart.Click += BtnStart_Click;
            this.Controls.Add(btnStart);
            MusicManager.PlayMenuMusic();
        }

        private void BuildSeatRow(int seat, string defaultName, int x, int y, int w, int h)
        {
            Panel row = new Panel
            {
                Bounds = new Rectangle(x, y, w, h),
                BackColor = Color.FromArgb(24, 38, 28)
            };
            Rounded(row, 10);
            this.Controls.Add(row);

            // ── AVATAR (created first, but click event added later) ──────────────────
            PictureBox avatar = new PictureBox
            {
                Bounds = new Rectangle(14, (h - 40) / 2, 40, 40),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Tag = seat
            };

            int avatarId = new DatabaseManager().GetPlayerAvatar(defaultName);
            avatar.Image = AvatarDrawer.ToBitmap(avatarId, 40);

            row.Controls.Add(avatar);
            avatarPics[seat] = avatar;

            // Seat circle
            Panel circle = new Panel
            {
                Bounds = new Rectangle(64, (h - 36) / 2, 36, 36),
                BackColor = Color.Transparent
            };
            Color sc = seatClrs[seat];
            circle.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(sc))
                    e.Graphics.FillEllipse(b, 0, 0, 35, 35);
                using (var font = new Font("Segoe UI", 13f, FontStyle.Bold))
                using (var b = new SolidBrush(Color.White))
                {
                    string n = (seat + 1).ToString();
                    var sz = e.Graphics.MeasureString(n, font);
                    e.Graphics.DrawString(n, font, b,
                        (35 - sz.Width) / 2f, (35 - sz.Height) / 2f);
                }
            };
            row.Controls.Add(circle);

            // Name textbox
            TextBox txt = new TextBox
            {
                Text = defaultName,
                Bounds = new Rectangle(110, (h - 28) / 2, 130, 28),
                BackColor = Color.FromArgb(30, 45, 36),
                ForeColor = TEXT_MN,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };

            // TextChanged event (uses avatar)
            txt.TextChanged += (s, e) =>
            {
                string newName = txt.Text.Trim();
                if (!string.IsNullOrEmpty(newName) && !newName.StartsWith("CPU"))
                {
                    DatabaseManager db = new DatabaseManager();
                    int avatarIdFromDb = db.GetPlayerAvatar(newName);
                    avatar.Image = AvatarDrawer.ToBitmap(avatarIdFromDb, 40);
                }
                else if (newName.StartsWith("CPU"))
                {
                    avatar.Image = AvatarDrawer.ToBitmap(1, 40);
                }
            };

            txtNames[seat] = txt;
            row.Controls.Add(txt);

            // ── AVATAR CLICK EVENT (added AFTER txt is created) ──────────────────────
            avatar.Click += (s, e) =>
            {
                string pName = txt.Text.Trim();
                if (string.IsNullOrEmpty(pName)) pName = defaultName;
                if (pName.StartsWith("CPU"))
                {
                    MessageBox.Show("CPU players cannot have avatars.", "Not Available",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int currentId = new DatabaseManager().GetPlayerAvatar(pName);
                var picker = new AvatarPickerForm(pName, currentId);
                if (picker.ShowDialog() == DialogResult.OK)
                {
                    avatar.Image = AvatarDrawer.ToBitmap(picker.SelectedAvatarId, 40);
                }
            };

            // Toggle panel
            Panel togglePanel = new Panel
            {
                Bounds = new Rectangle(260, (h - 30) / 2, 140, 30),
                BackColor = Color.FromArgb(20, 32, 24),
                BorderStyle = BorderStyle.None
            };

            Button bHuman = MakeMiniBtn("Human", 0, 0, 70, 30, BG_MID, TEXT_MUT);
            Button bCPU = MakeMiniBtn("CPU", 72, 0, 66, 30, BG_MID, TEXT_MUT);
            togglePanel.Controls.Add(bHuman);
            togglePanel.Controls.Add(bCPU);
            row.Controls.Add(togglePanel);
            btnToggle[seat] = new Button[] { bHuman, bCPU };

            // Difficulty
            ComboBox cmb = new ComboBox
            {
                Bounds = new Rectangle(w - 104, (h - 28) / 2, 90, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 45, 36),
                ForeColor = TEXT_MN,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            cmb.Items.AddRange(new object[] { "Easy", "Medium", "Hard" });
            cmb.SelectedIndex = 1;
            cmb.Visible = !isHuman[seat];
            cmbDiff[seat] = cmb;
            row.Controls.Add(cmb);

            int s2 = seat;
            bHuman.Click += (s, e) => SetSeat(s2, true);
            bCPU.Click += (s, e) => SetSeat(s2, false);
            SetSeat(seat, isHuman[seat]);
        }

        private void SetSeat(int seat, bool human)
        {
            isHuman[seat] = human;
            btnToggle[seat][0].BackColor = human ? RED : BG_MID;
            btnToggle[seat][0].ForeColor = human ? Color.White : TEXT_MUT;
            btnToggle[seat][1].BackColor = human ? BG_MID : BLUE;
            btnToggle[seat][1].ForeColor = human ? TEXT_MUT : Color.White;
            cmbDiff[seat].Visible = !human;
            txtNames[seat].ForeColor = human ? TEXT_MN : TEXT_MUT;

            if (!human && txtNames[seat].Text.StartsWith("Player"))
                txtNames[seat].Text = $"CPU {seat + 1}";
            if (human && txtNames[seat].Text.StartsWith("CPU"))
                txtNames[seat].Text = $"Player {seat + 1}";

            if (human)
            {
                string playerName = txtNames[seat].Text.Trim();
                UpdateAvatarForSeat(seat, playerName);
            }
        }
        private void BtnStart_Click(object sender, EventArgs e)
        {
            DatabaseManager db = new DatabaseManager();

            string[] originalNames = { "Player 1", "CPU 1", "CPU 2" };

            for (int i = 0; i < 3; i++)
            {
                string oldName = originalNames[i];
                string newName = txtNames[i].Text.Trim();

                if (oldName != newName && !string.IsNullOrEmpty(newName) && !newName.StartsWith("CPU"))
                {
                    int oldAvatarId = db.GetPlayerAvatar(oldName);
                    int newAvatarId = db.GetPlayerAvatar(newName);

                    if (newAvatarId == 1 && oldAvatarId != 1)
                    {
                        db.UpdatePlayerAvatar(newName, oldAvatarId);
                    }
                }
            }

            var config = new GameConfig
            {
                PlayerNames = new[] { txtNames[0].Text.Trim(), txtNames[1].Text.Trim(), txtNames[2].Text.Trim() },
                IsHuman = new[] { isHuman[0], isHuman[1], isHuman[2] },
                AIDifficulty = new[] {
                    cmbDiff[0].SelectedItem?.ToString() ?? "Medium",
                    cmbDiff[1].SelectedItem?.ToString() ?? "Medium",
                    cmbDiff[2].SelectedItem?.ToString() ?? "Medium"
                }
            };
            new FormGame(config).Show();
            this.Close();
        }

        private Button MakeBtn(string text, int x, int y, int w, int h,
                               Color back, Color fore)
        {
            var btn = new Button
            {
                Text = text,
                Bounds = new Rectangle(x, y, w, h),
                BackColor = back,
                ForeColor = fore,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btn.FlatAppearance.BorderColor = BTN_BRD;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(back, 0.1f);
            return btn;
        }

        private Button MakeMiniBtn(string text, int x, int y, int w, int h,
                                   Color back, Color fore)
        {
            var btn = new Button
            {
                Text = text,
                Bounds = new Rectangle(x, y, w, h),
                BackColor = back,
                ForeColor = fore,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void Rounded(Control c, int r)
        {
            var path = new GraphicsPath();
            int d = r * 2;
            path.AddArc(0, 0, d, d, 180, 90);
            path.AddArc(c.Width - d, 0, d, d, 270, 90);
            path.AddArc(c.Width - d, c.Height - d, d, d, 0, 90);
            path.AddArc(0, c.Height - d, d, d, 90, 90);
            path.CloseFigure();
            c.Region = new Region(path);
        }
    }

    // ── Shared config passed between forms ────────────────────────────────────
    public class GameConfig
    {
        public string[] PlayerNames { get; set; }
        public bool[] IsHuman { get; set; }
        public string[] AIDifficulty { get; set; }
    }
}