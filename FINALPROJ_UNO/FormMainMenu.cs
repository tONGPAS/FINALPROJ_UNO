using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UNOFinal
{
    public class FormMainMenu : Form
    {
        private readonly Color BG_DARK  = Color.FromArgb(26, 58, 42);
        private readonly Color BG_BAR   = Color.FromArgb(10, 24, 16);
        private readonly Color RED      = Color.FromArgb(230, 57, 70);
        private readonly Color BTN_DARK = Color.FromArgb(30, 45, 36);
        private readonly Color BTN_BRD  = Color.FromArgb(46, 74, 56);
        private readonly Color TEXT_DIM = Color.FromArgb(74, 107, 88);
        private readonly Color TEXT_SUB = Color.FromArgb(139, 196, 160);

        private readonly Color[] CARD_COLORS = {
            Color.FromArgb(230, 57,  70),
            Color.FromArgb(33,  150, 243),
            Color.FromArgb(76,  175, 80),
            Color.FromArgb(255, 152, 0),
        };

        public FormMainMenu()
        {
            SetupForm();
            this.Load += (s, e) => BuildUI();
        }

        private void SetupForm()
        {
            this.Text            = "UNO Card Game";
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
            int cy = H / 2;

            // ── Top bar ───────────────────────────────────────────────────────
            Panel pnlBar = new Panel
            {
                Bounds    = new Rectangle(0, 0, W, 44),
                BackColor = BG_BAR
            };
            Label lblBarTitle = new Label
            {
                Text      = "UNO Card Game",
                ForeColor = TEXT_DIM,
                Font      = new Font("Segoe UI", 9f),
                Bounds    = new Rectangle(16, 0, 300, 44),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            Button btnX = new Button
            {
                Text      = "✕",
                Bounds    = new Rectangle(W - 50, 2, 44, 40),
                BackColor = Color.Transparent,
                ForeColor = TEXT_SUB,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 13f),
                Cursor    = Cursors.Hand
            };
            btnX.FlatAppearance.BorderSize             = 0;
            btnX.FlatAppearance.MouseOverBackColor     = RED;
            btnX.Click += (s, e) => Application.Exit();
            pnlBar.Controls.Add(lblBarTitle);
            pnlBar.Controls.Add(btnX);
            this.Controls.Add(pnlBar);

            // ── Card decoration ───────────────────────────────────────────────
            Panel pnlCards = new Panel
            {
                Bounds    = new Rectangle(cx - 110, cy - 230, 220, 90),
                BackColor = Color.Transparent
            };
            pnlCards.Paint += PnlCards_Paint;
            this.Controls.Add(pnlCards);

            // ── Subtitle ──────────────────────────────────────────────────────
            Label lblSub = new Label
            {
                Text      = "CARD GAME",
                ForeColor = TEXT_SUB,
                Font      = new Font("Segoe UI", 10f),
                Bounds    = new Rectangle(cx - 100, cy - 134, 200, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblSub);

            // ── Buttons ───────────────────────────────────────────────────────
            int btnW   = 280;
            int btnH   = 52;
            int btnX2  = cx - btnW / 2;
            int startY = cy - 90;
            int gap    = 64;

            MakeBtn("New Game",      btnX2, startY,            btnW, btnH, RED,      true,  (s, e) => { new FormLobby().Show(); this.Hide(); });
            MakeBtn("Leaderboard",   btnX2, startY + gap,      btnW, btnH, BTN_DARK, false, (s, e) => { new FormLeaderboard().Show(); this.Hide(); });
            MakeBtn("Match History", btnX2, startY + gap * 2,  btnW, btnH, BTN_DARK, false, (s, e) => { new FormMatchHistory().Show(); this.Hide(); });
            MakeBtn("Settings",      btnX2, startY + gap * 3,  btnW, btnH, BTN_DARK, false, (s, e) => { new FormSettings().ShowDialog(); });
            MakeBtn("Exit",          btnX2, startY + gap * 4 + 12, btnW, 42, BTN_DARK, false, (s, e) =>
            {
                if (MessageBox.Show("Exit the game?", "Exit",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Application.Exit();
            });

            // ── Version label ─────────────────────────────────────────────────
            Label lblVer = new Label
            {
                Text      = ".",
                ForeColor = TEXT_DIM,
                Font      = new Font("Segoe UI", 8.5f),
                Bounds    = new Rectangle(0, H - 30, W, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblVer);
            MusicManager.PlayMenuMusic();
        }

        private void MakeBtn(string text, int x, int y, int w, int h,
                             Color back, bool primary, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text      = text,
                Bounds    = new Rectangle(x, y, w, h),
                BackColor = back,
                ForeColor = primary ? Color.White : Color.FromArgb(200, 220, 210),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 12f),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            btn.FlatAppearance.BorderColor        = primary ? RED : BTN_BRD;
            btn.FlatAppearance.BorderSize         = 1;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(back, 0.1f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(back, 0.05f);
            btn.Click += onClick;
            Rounded(btn, 8);
            this.Controls.Add(btn);
        }

        private void PnlCards_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            string[] letters = { "U", "N", "O", "!" };
            int[]    angles  = { -8, -2, 3, 8 };
            int cw = 44, ch = 62;
            for (int i = 0; i < 4; i++)
            {
                int x = i * 54;
                g.TranslateTransform(x + cw / 2f, ch / 2f);
                g.RotateTransform(angles[i]);
                g.TranslateTransform(-cw / 2f, -ch / 2f);
                using (var b = new SolidBrush(CARD_COLORS[i]))
                    g.FillRectangle(b, 0, 0, cw, ch);
                using (var p = new Pen(Color.White, 2f))
                    g.DrawRectangle(p, 1, 1, cw - 2, ch - 2);
                using (var font = new Font("Segoe UI", 20f, FontStyle.Bold))
                using (var b = new SolidBrush(Color.White))
                {
                    var sz = g.MeasureString(letters[i], font);
                    g.DrawString(letters[i], font, b,
                        (cw - sz.Width) / 2f, (ch - sz.Height) / 2f);
                }
                g.ResetTransform();
            }
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

        public void ReturnToMenu() { this.Show(); }
    }
}
