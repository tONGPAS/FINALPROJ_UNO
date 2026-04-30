using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using UNOFinal.Models;

namespace UNOFinal
{
    public class FormMatchHistory : Form
    {
        private readonly Color BG_DARK  = Color.FromArgb(19, 32, 24);
        private readonly Color BG_BAR   = Color.FromArgb(10, 24, 16);
        private readonly Color BG_ROW_A = Color.FromArgb(28, 44, 32);
        private readonly Color BG_ROW_B = Color.FromArgb(22, 36, 26);
        private readonly Color RED      = Color.FromArgb(230, 57, 70);
        private readonly Color GOLD     = Color.FromArgb(255, 215, 0);
        private readonly Color TEXT_MN  = Color.FromArgb(224, 224, 224);
        private readonly Color TEXT_MUT = Color.FromArgb(106, 138, 116);
        private readonly Color BTN_BRD  = Color.FromArgb(58, 90, 68);
        private readonly Color GREEN_C  = Color.FromArgb(46, 125, 50);

        private DatabaseManager db = new DatabaseManager();
        private Panel pnlRows;
        private Label lblCount;

        public FormMatchHistory()
        {
            SetupForm();
            this.Load += (s, e) => BuildUI();
        }

        private void SetupForm()
        {
            this.Text            = "UNO - Match History";
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
            int W      = this.ClientSize.Width;
            int H      = this.ClientSize.Height;
            int cx     = W / 2;
            int panelW = Math.Min(900, W - 80);
            int panelX = cx - panelW / 2;

            // ── Top bar ───────────────────────────────────────────────────────
            Panel pnlBar = new Panel
            {
                Bounds    = new Rectangle(0, 0, W, 50),
                BackColor = BG_BAR
            };
            Button btnBack = MakeBtn("Back", 12, 8, 80, 34, Color.FromArgb(30, 45, 36), TEXT_MUT);
            btnBack.Click += (s, e) => { new FormMainMenu().Show(); this.Close(); };

            Label lblTitle = new Label
            {
                Text      = "Match History",
                ForeColor = TEXT_MN,
                Font      = new Font("Segoe UI", 15f),
                Bounds    = new Rectangle(106, 0, 300, 50),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            lblCount = new Label
            {
                Text      = "",
                ForeColor = TEXT_MUT,
                Font      = new Font("Segoe UI", 9f),
                Bounds    = new Rectangle(W / 2 - 100, 0, 200, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Button btnRefresh = MakeBtn("Refresh", W - 210, 8, 90, 34,
                Color.FromArgb(30, 45, 36), TEXT_MUT);
            btnRefresh.Click += (s, e) => LoadHistory();

            Button btnClearAll = MakeBtn("Clear All", W - 110, 8, 90, 34,
                Color.FromArgb(80, 20, 20), Color.FromArgb(220, 100, 100));
            btnClearAll.Click += BtnClearAll_Click;

            pnlBar.Controls.Add(btnBack);
            pnlBar.Controls.Add(lblTitle);
            pnlBar.Controls.Add(lblCount);
            pnlBar.Controls.Add(btnRefresh);
            pnlBar.Controls.Add(btnClearAll);
            this.Controls.Add(pnlBar);

            // ── Column headers ────────────────────────────────────────────────
            Panel pnlHeader = new Panel
            {
                Bounds    = new Rectangle(panelX, 60, panelW, 34),
                BackColor = Color.FromArgb(10, 24, 16)
            };
            Rounded(pnlHeader, 6);

            int[] colW    = { 160, 80, panelW - 160 - 80 - 140 - 80 - 70, 140, 80, 70 };
            string[] cols = { "Date", "Mode", "Players", "Winner", "Rounds", "" };
            int hx = 12;
            for (int i = 0; i < cols.Length; i++)
            {
                new Label
                {
                    Text      = cols[i],
                    ForeColor = TEXT_MUT,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Bounds    = new Rectangle(hx, 0, colW[i], 34),
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                    Parent    = pnlHeader
                };
                hx += colW[i];
            }
            this.Controls.Add(pnlHeader);

            // ── Rows ──────────────────────────────────────────────────────────
            pnlRows = new Panel
            {
                Bounds     = new Rectangle(panelX, 100, panelW, H - 116),
                BackColor  = Color.Transparent,
                AutoScroll = true
            };
            this.Controls.Add(pnlRows);

            LoadHistory();
            MusicManager.PlayMenuMusic();
        }

        private void LoadHistory()
        {
            pnlRows.Controls.Clear();

            try
            {
                DataTable dt = db.GetAllMatchHistory();

                if (dt.Rows.Count == 0)
                {
                    new Label
                    {
                        Text      = "No games played yet. Come back after a match!",
                        ForeColor = TEXT_MUT,
                        Font      = new Font("Segoe UI", 11f, FontStyle.Italic),
                        Bounds    = new Rectangle(0, 60, pnlRows.Width, 40),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent,
                        Parent    = pnlRows
                    };
                    lblCount.Text = "0 games";
                    return;
                }

                lblCount.Text = $"{dt.Rows.Count} game{(dt.Rows.Count == 1 ? "" : "s")} recorded";

                int panelW = pnlRows.Width;
                int[] colW = { 160, 80, panelW - 160 - 80 - 140 - 80 - 140, 140, 80, 140 };
                int rowH   = 48;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row    = dt.Rows[i];
                    int sessionId  = Convert.ToInt32(row["SessionId"]);
                    string date    = Convert.ToDateTime(row["StartTime"]).ToString("MMM dd yyyy  h:mm tt");
                    string mode    = row["GameMode"].ToString();
                    string players = row["Players"].ToString();
                    string winner  = row["WinnerName"].ToString();
                    int rounds     = Convert.ToInt32(row["TotalRounds"]);

                    Panel rowPnl = new Panel
                    {
                        Bounds    = new Rectangle(0, i * (rowH + 4), panelW, rowH),
                        BackColor = i % 2 == 0 ? BG_ROW_A : BG_ROW_B,
                        Tag       = sessionId
                    };
                    Rounded(rowPnl, 6);

                    // Date
                    new Label
                    {
                        Text      = date,
                        ForeColor = TEXT_MUT,
                        Font      = new Font("Segoe UI", 9f),
                        Bounds    = new Rectangle(12, 0, colW[0], rowH),
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = Color.Transparent,
                        Parent    = rowPnl
                    };
                    int rx = 12 + colW[0];

                    // Mode badge
                    Color modeColor = mode == "Solo"
                        ? Color.FromArgb(33, 150, 243)
                        : mode.Contains("P")
                        ? Color.FromArgb(76, 175, 80)
                        : Color.FromArgb(156, 39, 176);
                    new Label
                    {
                        Text      = mode,
                        ForeColor = modeColor,
                        Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                        Bounds    = new Rectangle(rx, 0, colW[1], rowH),
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = Color.Transparent,
                        Parent    = rowPnl
                    };
                    rx += colW[1];

                    // Players
                    new Label
                    {
                        Text      = players,
                        ForeColor = TEXT_MN,
                        Font      = new Font("Segoe UI", 9.5f),
                        Bounds    = new Rectangle(rx, 0, colW[2], rowH),
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = Color.Transparent,
                        Parent    = rowPnl
                    };
                    rx += colW[2];

                    // Winner
                    new Label
                    {
                        Text      = winner,
                        ForeColor = GOLD,
                        Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                        Bounds    = new Rectangle(rx, 0, colW[3], rowH),
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = Color.Transparent,
                        Parent    = rowPnl
                    };
                    rx += colW[3];

                    // Rounds
                    new Label
                    {
                        Text      = rounds.ToString(),
                        ForeColor = TEXT_MUT,
                        Font      = new Font("Segoe UI", 10f),
                        Bounds    = new Rectangle(rx, 0, colW[4], rowH),
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = Color.Transparent,
                        Parent    = rowPnl
                    };
                    rx += colW[4];

                    // Delete button
                    // Details button
                    // Details button
                    Button btnDetails = new Button
                    {
                        Text = "Details",
                        Bounds = new Rectangle(rx + 4, 10, 64, 28),
                        BackColor = Color.FromArgb(25, 60, 35),
                        ForeColor = Color.FromArgb(139, 196, 160),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 8f),
                        Cursor = Cursors.Hand,
                        Tag = new object[] { sessionId, winner },
                        Parent = rowPnl
                    };
                    btnDetails.FlatAppearance.BorderColor = Color.FromArgb(46, 90, 56);
                    btnDetails.FlatAppearance.BorderSize = 1;
                    btnDetails.Click += (s, ev) =>
                    {
                        var tag = (object[])((Button)s).Tag;
                        var details = new FormMatchDetails(
                            Convert.ToInt32(tag[0]),
                            tag[1].ToString());
                        details.ShowDialog(this);
                    };

                    // Delete button
                    Button btnDel = new Button
                    {
                        Text = "Del",
                        Bounds = new Rectangle(rx + 74, 10, 52, 28),
                        BackColor = Color.FromArgb(60, 20, 20),
                        ForeColor = Color.FromArgb(220, 100, 100),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 8f),
                        Cursor = Cursors.Hand,
                        Tag = sessionId,
                        Parent = rowPnl
                    };
                    btnDel.FlatAppearance.BorderColor = Color.FromArgb(100, 30, 30);
                    btnDel.FlatAppearance.BorderSize = 1;
                    btnDel.Click += BtnDelete_Click;


                    pnlRows.Controls.Add(rowPnl);
                }
            }
            catch (Exception ex)
            {
                new Label
                {
                    Text      = $"Error loading history: {ex.Message}",
                    ForeColor = RED,
                    Font      = new Font("Segoe UI", 10f),
                    Bounds    = new Rectangle(0, 40, pnlRows.Width, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Parent    = pnlRows
                };
            }
        }

        // ── Delete single session ─────────────────────────────────────────────
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            int sessionId = Convert.ToInt32(((Button)sender).Tag);
            var result    = MessageBox.Show(
                "Delete this match record? This cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                db.DeleteSession(sessionId);
                LoadHistory();
            }
        }

        // ── Delete all sessions ───────────────────────────────────────────────
        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Clear ALL match history? This cannot be undone.",
                "Confirm Clear All",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                db.DeleteAllSessions();
                LoadHistory();
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private Button MakeBtn(string text, int x, int y, int w, int h,
                               Color back, Color fore)
        {
            var btn = new Button
            {
                Text      = text,
                Bounds    = new Rectangle(x, y, w, h),
                BackColor = back,
                ForeColor = fore,
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
