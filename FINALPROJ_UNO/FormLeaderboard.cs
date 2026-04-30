using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using UNOFinal.Models;

namespace UNOFinal
{
    public class FormLeaderboard : Form
    {
        private readonly Color BG_DARK   = Color.FromArgb(19, 32, 24);
        private readonly Color BG_BAR    = Color.FromArgb(10, 24, 16);
        private readonly Color BG_LIST   = Color.FromArgb(24, 38, 28);
        private readonly Color BG_ROW_A  = Color.FromArgb(28, 44, 32);
        private readonly Color BG_ROW_B  = Color.FromArgb(22, 36, 26);
        private readonly Color RED       = Color.FromArgb(230, 57, 70);
        private readonly Color GOLD      = Color.FromArgb(255, 215, 0);
        private readonly Color SILVER    = Color.FromArgb(192, 192, 192);
        private readonly Color BRONZE    = Color.FromArgb(205, 127, 50);
        private readonly Color TEXT_MN   = Color.FromArgb(224, 224, 224);
        private readonly Color TEXT_MUT  = Color.FromArgb(106, 138, 116);
        private readonly Color BTN_BRD   = Color.FromArgb(58, 90, 68);

        private DatabaseManager db = new DatabaseManager();
        private ListView        lvLeaderboard;
        private Panel           pnlStats;
        private Label           lblStats;

        public FormLeaderboard()
        {
            SetupForm();
            this.Load += (s, e) => BuildUI();
        }

        private void SetupForm()
        {
            this.Text            = "UNO - Leaderboard";
            this.BackColor       = BG_DARK;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState     = FormWindowState.Maximized;
            this.DoubleBuffered  = true;
            this.Font            = new Font("Segoe UI", 10f);
        }
        private void Form_Load(object sender, EventArgs e)
        {
            MusicManager.PlayMenuMusic();
        }

        private void BuildUI()
        {
            this.Controls.Clear();
            int W  = this.ClientSize.Width;
            int H  = this.ClientSize.Height;
            int cx = W / 2;
            int panelW = Math.Min(1000, W - 80);
            int panelX = cx - panelW / 2;

            // ── Top bar ───────────────────────────────────────────────────────
            Panel pnlBar = new Panel
            {
                Bounds    = new Rectangle(0, 0, W, 50),
                BackColor = BG_BAR
            };
            Button btnBack = MakeBtn("Back", 12, 8, 80, 34);
            btnBack.Click += (s, e) => { new FormMainMenu().Show(); this.Close(); };

            Label lblTitle = new Label
            {
                Text      = "Leaderboard",
                ForeColor = TEXT_MN,
                Font      = new Font("Segoe UI", 15f),
                Bounds    = new Rectangle(106, 0, 300, 50),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            Button btnRefresh = MakeBtn("Refresh", W - 110, 8, 90, 34);
            btnRefresh.Click += (s, e) => LoadLeaderboard();

            pnlBar.Controls.Add(btnBack);
            pnlBar.Controls.Add(lblTitle);
            pnlBar.Controls.Add(btnRefresh);
            this.Controls.Add(pnlBar);

            // ── Stats summary panel ───────────────────────────────────────────
            pnlStats = new Panel
            {
                Bounds    = new Rectangle(panelX, 64, panelW, 60),
                BackColor = Color.FromArgb(16, 28, 20)
            };
            Rounded(pnlStats, 8);
            lblStats = new Label
            {
                Text      = "Loading stats...",
                ForeColor = TEXT_MUT,
                Font      = new Font("Segoe UI", 10f),
                Bounds    = new Rectangle(0, 0, panelW, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            pnlStats.Controls.Add(lblStats);
            this.Controls.Add(pnlStats);

            // ── Column headers ────────────────────────────────────────────────
            // ── Column headers ────────────────────────────────────────────────
            Panel pnlHeader = new Panel
            {
                Bounds = new Rectangle(0, 134, this.ClientSize.Width, 34),  // Full width
                BackColor = Color.FromArgb(10, 24, 16)
            };
            Rounded(pnlHeader, 6);

            // Fixed column widths (must match LoadLeaderboard)
            int avatarW = 50;
            int rankW = 50;
            int winsW = 60;
            int gamesW = 60;
            int winRateW = 80;
            int actionsW = 160;
            int nameW = this.ClientSize.Width - avatarW - rankW - winsW - gamesW - winRateW - actionsW - 30;

            int[] colWidths = { avatarW, rankW, nameW, winsW, gamesW, winRateW, actionsW };
            string[] colNames = { "", "Rank", "Player", "Wins", "Games", "Win Rate", "Score / Actions" };

            int hx = 10;
            for (int i = 0; i < colNames.Length; i++)
            {
                new Label
                {
                    Text = colNames[i],
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Bounds = new Rectangle(hx, 0, colWidths[i], 34),
                    TextAlign = i == 0 ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                    Parent = pnlHeader
                };
                hx += colWidths[i];
            }
            this.Controls.Add(pnlHeader);

            // ── Leaderboard rows panel ────────────────────────────────────────
            Panel pnlRows = new Panel
            {
                Bounds = new Rectangle(0, 174, this.ClientSize.Width, this.ClientSize.Height - 190),
                BackColor = Color.Transparent,
                AutoScroll = true
            };
            this.Controls.Add(pnlRows);
            this.Tag = pnlRows;

            LoadLeaderboard();
            MusicManager.PlayMenuMusic();
        }

        private void LoadLeaderboard()
        {
            var pnlRows = (Panel)this.Tag;
            if (pnlRows == null) return;
            pnlRows.Controls.Clear();

            try
            {
                DataTable dt = db.GetLeaderboard();

                if (dt.Rows.Count == 0)
                {
                    pnlRows.Controls.Add(new Label
                    {
                        Text = "No players yet. Play a game to see rankings here!",
                        ForeColor = TEXT_MUT,
                        Font = new Font("Segoe UI", 11f, FontStyle.Italic),
                        Bounds = new Rectangle(0, 40, pnlRows.Width, 40),
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                    lblStats.Text = "No data yet - play some games first!";
                    return;
                }

                int panelW = this.ClientSize.Width;  // Use full form width
                int rowH = 48;

                // MUST MATCH the headers in BuildUI
                int avatarW = 50;
                int rankW = 50;
                int winsW = 60;
                int gamesW = 60;
                int winRateW = 80;
                int actionsW = 160;
                int nameW = panelW - avatarW - rankW - winsW - gamesW - winRateW - actionsW - 30;

                int totalGames = 0, totalWins = 0;
                string topPlayer = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    int rank = i + 1;
                    string name = row["Name"].ToString();
                    int wins = Convert.ToInt32(row["TotalWins"]);
                    int games = Convert.ToInt32(row["TotalGames"]);
                    int score = Convert.ToInt32(row["TotalScore"]);
                    double wr = Convert.ToDouble(row["WinRate"]);

                    if (i == 0) topPlayer = name;
                    totalGames += games;
                    totalWins += wins;

                    Panel rowPnl = new Panel
                    {
                        Bounds = new Rectangle(0, i * (rowH + 4), panelW - 20, rowH),
                        BackColor = i % 2 == 0 ? BG_ROW_A : BG_ROW_B
                    };

                    int currentX = 10;

                    // Avatar
                    int avatarId = db.GetPlayerAvatar(name);
                    PictureBox avatar = new PictureBox
                    {
                        Bounds = new Rectangle(currentX, 4, 40, 40),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Image = AvatarDrawer.ToBitmap(avatarId, 40),
                        BackColor = Color.Transparent,
                        Cursor = Cursors.Hand,
                        Tag = name
                    };
                    avatar.Click += (s, ev) =>
                    {
                        string pname = avatar.Tag.ToString();
                        int currentId = db.GetPlayerAvatar(pname);
                        var picker = new AvatarPickerForm(pname, currentId);
                        if (picker.ShowDialog() == DialogResult.OK)
                        {
                            avatar.Image = AvatarDrawer.ToBitmap(picker.SelectedAvatarId, 40);
                            LoadLeaderboard();
                        }
                    };
                    rowPnl.Controls.Add(avatar);
                    currentX += avatarW;

                    // Rank
                    Color rankColor = rank == 1 ? GOLD : rank == 2 ? SILVER : rank == 3 ? BRONZE : TEXT_MUT;
                    string rankText = rank <= 3 ? (rank == 1 ? "1st" : rank == 2 ? "2nd" : "3rd") : rank.ToString();
                    Label lblRank = new Label
                    {
                        Text = rankText,
                        ForeColor = rankColor,
                        Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                        Bounds = new Rectangle(currentX, 0, rankW, rowH),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    rowPnl.Controls.Add(lblRank);
                    currentX += rankW;

                    // Name
                    Label lblName = new Label
                    {
                        Text = name,
                        ForeColor = rank == 1 ? GOLD : TEXT_MN,
                        Font = new Font("Segoe UI", 11f, rank == 1 ? FontStyle.Bold : FontStyle.Regular),
                        Bounds = new Rectangle(currentX, 0, nameW, rowH),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    rowPnl.Controls.Add(lblName);
                    currentX += nameW;

                    // Wins
                    Label lblWins = new Label
                    {
                        Text = wins.ToString(),
                        ForeColor = TEXT_MN,
                        Bounds = new Rectangle(currentX, 0, winsW, rowH),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    rowPnl.Controls.Add(lblWins);
                    currentX += winsW;

                    // Games
                    Label lblGames = new Label
                    {
                        Text = games.ToString(),
                        ForeColor = TEXT_MUT,
                        Bounds = new Rectangle(currentX, 0, gamesW, rowH),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    rowPnl.Controls.Add(lblGames);
                    currentX += gamesW;

                    // Win Rate
                    Color wrColor = wr >= 60 ? Color.FromArgb(76, 175, 80)
                                 : wr >= 40 ? Color.FromArgb(255, 193, 7)
                                 : Color.FromArgb(239, 83, 80);
                    Label lblWr = new Label
                    {
                        Text = $"{wr:F1}%",
                        ForeColor = wrColor,
                        Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                        Bounds = new Rectangle(currentX, 0, winRateW, rowH),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    rowPnl.Controls.Add(lblWr);
                    currentX += winRateW;

                    // Score
                    Label lblScore = new Label
                    {
                        Text = $"{score:N0}",
                        ForeColor = TEXT_MN,
                        Bounds = new Rectangle(currentX, 0, 70, rowH),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    rowPnl.Controls.Add(lblScore);

                    // Profile button
                    Button btnProfile = new Button
                    {
                        Text = "Profile",
                        Bounds = new Rectangle(panelW - 124, 10, 62, 28),
                        BackColor = Color.FromArgb(20, 50, 30),
                        ForeColor = Color.FromArgb(139, 196, 160),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 8f),
                        Cursor = Cursors.Hand,
                        Tag = name
                    };
                    btnProfile.FlatAppearance.BorderColor = Color.FromArgb(46, 90, 56);
                    btnProfile.FlatAppearance.BorderSize = 1;
                    btnProfile.Click += (s, ev) =>
                    {
                        string pname = ((Button)s).Tag?.ToString();
                        new FormPlayerProfile(pname).ShowDialog(this);
                        LoadLeaderboard();
                    };
                    rowPnl.Controls.Add(btnProfile);

                    // Edit button
                    Button btnEdit = new Button
                    {
                        Text = "Edit",
                        Bounds = new Rectangle(panelW - 58, 10, 46, 28),
                        BackColor = Color.FromArgb(30, 45, 36),
                        ForeColor = TEXT_MUT,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 8f),
                        Cursor = Cursors.Hand,
                        Tag = name
                    };
                    btnEdit.FlatAppearance.BorderColor = BTN_BRD;
                    btnEdit.FlatAppearance.BorderSize = 1;
                    btnEdit.Click += BtnEdit_Click;
                    rowPnl.Controls.Add(btnEdit);

                    pnlRows.Controls.Add(rowPnl);
                }

                double avgWR = totalGames > 0 ? totalWins * 100.0 / totalGames : 0;
                lblStats.Text = $"Top player: {topPlayer}   |   Total games: {totalGames}   |   Total wins: {totalWins}   |   Overall win rate: {avgWR:F1}%";
            }
            catch (Exception ex)
            {
                lblStats.Text = $"Error loading data: {ex.Message}";
            }
        }

        // ── Update: Edit player name ──────────────────────────────────────────
        private void BtnEdit_Click(object sender, EventArgs e)
        {
            string oldName = ((Button)sender).Tag?.ToString();
            if (string.IsNullOrEmpty(oldName)) return;

            Form editForm = new Form
            {
                Text            = "Edit Player Name",
                Size            = new Size(360, 180),
                StartPosition   = FormStartPosition.CenterParent,
                BackColor       = BG_DARK,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false
            };

            new Label
            {
                Text      = "New player name:",
                ForeColor = TEXT_MN,
                Font      = new Font("Segoe UI", 10f),
                Bounds    = new Rectangle(20, 20, 320, 24),
                BackColor = Color.Transparent,
                Parent    = editForm
            };

            TextBox txt = new TextBox
            {
                Text      = oldName,
                Bounds    = new Rectangle(20, 50, 310, 28),
                BackColor = Color.FromArgb(30, 45, 36),
                ForeColor = TEXT_MN,
                Font      = new Font("Segoe UI", 11f),
                Parent    = editForm
            };

            Button btnSave = new Button
            {
                Text      = "Save",
                Bounds    = new Rectangle(20, 96, 140, 38),
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Parent    = editForm
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, ev) =>
            {
                string newName = txt.Text.Trim();
                if (string.IsNullOrEmpty(newName))
                {
                    MessageBox.Show("Name cannot be empty.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (newName == oldName)
                {
                    editForm.Close();
                    return;
                }
                try
                {
                    db.UpdatePlayerName(oldName, newName);
                    MessageBox.Show($"Player renamed to '{newName}'.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    editForm.Close();
                    LoadLeaderboard();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button btnCancel = new Button
            {
                Text      = "Cancel",
                Bounds    = new Rectangle(180, 96, 140, 38),
                BackColor = Color.FromArgb(30, 45, 36),
                ForeColor = TEXT_MUT,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f),
                Cursor    = Cursors.Hand,
                Parent    = editForm
            };
            btnCancel.FlatAppearance.BorderColor = BTN_BRD;
            btnCancel.FlatAppearance.BorderSize  = 1;
            btnCancel.Click += (s, ev) => editForm.Close();

            editForm.ShowDialog(this);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private Button MakeBtn(string text, int x, int y, int w, int h)
        {
            var btn = new Button
            {
                Text      = text,
                Bounds    = new Rectangle(x, y, w, h),
                BackColor = Color.FromArgb(30, 45, 36),
                ForeColor = TEXT_MUT,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = BTN_BRD;
            btn.FlatAppearance.BorderSize  = 1;
            return btn;
        }

        private void Rounded(Control c, int r)
        {
            var path = new GraphicsPath();
            int d    = r * 2;
            path.AddArc(0, 0, d, d, 180, 90);
            path.AddArc(c.Width - d, 0, d, d, 270, 90);
            path.AddArc(c.Width - d, c.Height - d, d, d, 0, 90);
            path.AddArc(0, c.Height - d, d, d, 90, 90);
            path.CloseFigure();
            c.Region = new Region(path);
        }
    }
}
