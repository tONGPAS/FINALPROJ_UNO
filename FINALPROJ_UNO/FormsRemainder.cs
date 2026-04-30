using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using UNOFinal.Models;

namespace UNOFinal
{
    public class FormGame : Form
    {
        //Colors
        private readonly Color BG_TABLE = Color.FromArgb(22, 50, 35);
        private readonly Color BG_DARK = Color.FromArgb(10, 24, 16);
        private readonly Color BG_PANEL = Color.FromArgb(26, 42, 30);
        private readonly Color BG_BAR = Color.FromArgb(8, 18, 12);
        private readonly Color BG_CARDZ = Color.FromArgb(18, 34, 22);
        private readonly Color RED = Color.FromArgb(211, 47, 47);
        private readonly Color GOLD = Color.FromArgb(255, 215, 0);
        private readonly Color TEXT_MN = Color.FromArgb(224, 224, 224);
        private readonly Color TEXT_MUT = Color.FromArgb(106, 138, 116);
        private readonly Color BTN_BRD = Color.FromArgb(46, 74, 46);
        private PictureBox[] oppAvatar = new PictureBox[3];

        private readonly Dictionary<CardColor, Color> CARD_CLR =
            new Dictionary<CardColor, Color>
        {
            { CardColor.Red,    Color.FromArgb(211, 47,  47)  },
            { CardColor.Blue,   Color.FromArgb(25,  118, 210) },
            { CardColor.Green,  Color.FromArgb(46,  125, 50)  },
            { CardColor.Yellow, Color.FromArgb(245, 124, 0)   },
            { CardColor.Wild,   Color.FromArgb(33,  33,  33)  },
        };

        private readonly Color[] WILD_Q = {
            Color.FromArgb(211, 47,  47),
            Color.FromArgb(25,  118, 210),
            Color.FromArgb(46,  125, 50),
            Color.FromArgb(245, 124, 0),
        };

        // ── Card sizes ────────────────────────────────────────────────────────
        private const int CW = 72;   // hand card width
        private const int CH = 102;  // hand card height
        private const int OVL = 48;   // overlap
        private const int MCW = 80;   // center card width
        private const int MCH = 112;  // center card height
        private const int MNW = 20;   // mini card width
        private const int MNH = 28;   // mini card height

        // ── Game state ────────────────────────────────────────────────────────
        private GameConfig config;
        private GameEngine engine;
        private List<Player> players;
        private DatabaseManager db;
        private int sessionId = -1;
        private DateTime sessionStart;
        private DateTime roundStart;
        private bool gameStarted = false;
        private string lastHumanPlayer = "";
        private bool processingTurn = false;
        private int turnCount = 0;

        // ── Player event banners ──────────────────────────────────────────────
        private string[] bannerText;
        private Color[] bannerColor;
        private System.Windows.Forms.Timer bannerTimer;

        // ── UI references ─────────────────────────────────────────────────────
        private Panel pnlTop, pnlOpp, pnlCenter, pnlStatus, pnlHand, pnlBottom;
        private Label lblRound, lblTurn, lblStatus, lblActiveColor;
        private Label lblMyName, lblMyInfo;
        private Panel pnlDot;
        private Button btnDraw;

        // ── Opponent controls ─────────────────────────────────────────────────
        private Panel[] oppBox;
        private Label[] oppName, oppCardCount;
        private Panel[] oppBanner;
        private Label[] oppBannerLbl;
        private Panel[] oppMiniCardPanel;

        // ── Hand ──────────────────────────────────────────────────────────────
        private List<Rectangle> cardRects = new List<Rectangle>();
        private int hoveredCard = -1;

        public FormGame(GameConfig config)
        {
            this.config = config;
            this.db = new DatabaseManager();
            SetupForm();
            this.Load += OnLoad;
        }

        private void SetDoubleBuffered(Control c)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                ?.SetValue(c, true, null);
        }
        private void ReattachHandEvents()
        {
            pnlHand.MouseClick -= ClickHand;
            pnlHand.MouseClick += ClickHand;
            pnlHand.MouseMove -= MoveHand;
            pnlHand.MouseMove += MoveHand;
        }


        private void SetupForm()
        {
            this.Text = "UNO Card Game";
            this.BackColor = BG_TABLE;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 10f);
        }

        private void OnLoad(object s, EventArgs e)
        {
            int n = config.PlayerNames.Length;
            bannerText = new string[n];
            bannerColor = new Color[n];
            for (int i = 0; i < n; i++) bannerText[i] = "";

            bannerTimer = new System.Windows.Forms.Timer { Interval = 2400 };
            bannerTimer.Tick += (ts, te) =>
            {
                bannerTimer.Stop();
                for (int i = 0; i < bannerText.Length; i++) bannerText[i] = "";
                RefreshOppPanels();
                RefreshBottomBar();
            };

            BuildUI();
            InitGame();
        }

        private void ShowBanner(int idx, string msg, Color col)
        {
            if (idx < 0 || idx >= bannerText.Length) return;
            bannerText[idx] = msg;
            bannerColor[idx] = col;
            bannerTimer.Stop();
            bannerTimer.Start();
            RefreshOppPanels();
            RefreshBottomBar();
        }

        // ── Build UI ──────────────────────────────────────────────────────────
        private void BuildUI()
        {
            this.Controls.Clear();
            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height;

            // Top bar
            pnlTop = new Panel { Bounds = new Rectangle(0, 0, W, 46), BackColor = BG_BAR };

            pnlDot = new Panel { Bounds = new Rectangle(16, 14, 18, 18), BackColor = Color.Gray };
            pnlDot.Region = new Region(new Rectangle(0, 0, 18, 18));

            lblActiveColor = MakeLbl("Active: —", new Rectangle(40, 0, 140, 46), TEXT_MUT, 9f);
            lblRound = MakeLbl("Round 1", new Rectangle(W / 2 - 70, 0, 140, 46), TEXT_MUT, 10.5f, true);
            lblTurn = MakeLbl("Turn 1", new Rectangle(W / 2 + 76, 0, 80, 46), Color.FromArgb(50, 80, 60), 8.5f);

            Button btnMenu = MakeBtn("Menu", W - 80, 7, 68, 32, BG_PANEL, TEXT_MUT);
            btnMenu.Click += (s, e) => {
                if (MessageBox.Show("Return to main menu? Game will be lost.",
                    "Quit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                { new FormMainMenu().Show(); this.Close(); }
            };
            Button btnGuide = MakeBtn("?", W - 156, 7, 40, 32, BG_PANEL, TEXT_MUT);
            btnGuide.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnGuide.Click += (s, e) =>
            {
                var guide = new FormGuide();
                guide.ShowDialog(this);
            };
            pnlTop.Controls.Add(btnGuide);

            pnlTop.Controls.Add(pnlDot);
            pnlTop.Controls.Add(lblActiveColor);
            pnlTop.Controls.Add(lblRound);
            pnlTop.Controls.Add(lblTurn);
            pnlTop.Controls.Add(btnMenu);
            this.Controls.Add(pnlTop);

            // Opponents zone
            pnlOpp = new Panel { Bounds = new Rectangle(0, 46, W, 130), BackColor = Color.Transparent };
            SetDoubleBuffered(pnlOpp);
            this.Controls.Add(pnlOpp);
            BuildOppPanels(W);

            // Center zone
            int cy = 46 + 130 + 8;
            pnlCenter = new Panel { Bounds = new Rectangle(0, cy, W, MCH + 44), BackColor = Color.Transparent };
            SetDoubleBuffered(pnlCenter);
            pnlCenter.Paint += PaintCenter;
            pnlCenter.MouseClick += ClickCenter;
            this.Controls.Add(pnlCenter);

            // Status bar
            int sy = cy + MCH + 50;
            pnlStatus = new Panel { Bounds = new Rectangle(20, sy, W - 40, 36), BackColor = BG_DARK };
            Rounded(pnlStatus, 8);
            var accent = new Panel { Bounds = new Rectangle(0, 0, 4, 36), BackColor = GOLD };
            pnlStatus.Controls.Add(accent);
            lblStatus = MakeLbl("  Starting...", new Rectangle(10, 0, W - 52, 36),
                Color.FromArgb(160, 210, 170), 10f);
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            pnlStatus.Controls.Add(lblStatus);
            this.Controls.Add(pnlStatus);

            // Hand zone
            int hy = sy + 44;
            pnlHand = new Panel { Bounds = new Rectangle(0, hy, W, H - hy - 54), BackColor = BG_CARDZ };
            SetDoubleBuffered(pnlHand);
            pnlHand.Paint += PaintHand;
            pnlHand.MouseClick += ClickHand;
            pnlHand.MouseMove += MoveHand;
            this.Controls.Add(pnlHand);

            // Bottom bar
            pnlBottom = new Panel { Bounds = new Rectangle(0, H - 54, W, 54), BackColor = BG_BAR };

            // ── MY AVATAR (NEW) ─────────────────────────────────────────
            DatabaseManager db = new DatabaseManager();
            string myName = config.PlayerNames[0];
            int myAvatarId = db.GetPlayerAvatar(myName);
            PictureBox myAvatar = new PictureBox
            {
                Bounds = new Rectangle(12, 8, 38, 38),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = AvatarDrawer.ToBitmap(myAvatarId, 38),
                BackColor = Color.Transparent
            };
            pnlBottom.Controls.Add(myAvatar);
            // ─────────────────────────────────────────────────────────

            lblMyName = MakeLbl("", new Rectangle(60, 0, W / 2 - 60, 54), GOLD, 13f, true);  // was 20, now 60
            lblMyName.TextAlign = ContentAlignment.MiddleLeft;
            lblMyInfo = MakeLbl("", new Rectangle(W / 2, 0, W / 2 - 170, 54), TEXT_MUT, 9.5f);
            lblMyInfo.TextAlign = ContentAlignment.MiddleRight;

            btnDraw = MakeBtn("Draw Card", W - 156, 10, 136, 34, BG_PANEL, TEXT_MN);
            btnDraw.Click += DrawClick;

            pnlBottom.Controls.Add(lblMyName);
            pnlBottom.Controls.Add(lblMyInfo);
            pnlBottom.Controls.Add(btnDraw);
            this.Controls.Add(pnlBottom);
        }

        private void BuildOppPanels(int W)
        {
            pnlOpp.Controls.Clear();
            int n = config.PlayerNames.Length;
            oppBox = new Panel[n];
            oppName = new Label[n];
            oppCardCount = new Label[n];
            oppBanner = new Panel[n];
            oppBannerLbl = new Label[n];
            oppMiniCardPanel = new Panel[n];

            int count = n ;
            int boxW = Math.Min(300, (W - 60) / Math.Max(count, 1));
            int totalW = boxW * count + 16 * (count - 1);
            int sx = (W - totalW) / 2;

            Color[] dotCol = { Color.FromArgb(230, 57, 70),   
                   Color.FromArgb(33, 150, 243),   
                   Color.FromArgb(76, 175, 80) };

            for (int i = 0; i < n; i++)
            {
                int x = sx + i * (boxW + 16);
                var box = new Panel { Bounds = new Rectangle(x, 10, boxW, 108), BackColor = BG_DARK };
                Rounded(box, 10);

                // Color strip
                new Panel { Bounds = new Rectangle(0, 0, 4, 108), BackColor = dotCol[i], Parent = box };

                // ── AVATAR (NEW) ─────────────────────────────────────────
                int avatarId = db.GetPlayerAvatar(config.PlayerNames[i]);
                PictureBox avatar = new PictureBox
                {
                    Bounds = new Rectangle(10, 8, 36, 36),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = AvatarDrawer.ToBitmap(avatarId, 36),
                    BackColor = Color.Transparent
                };
                box.Controls.Add(avatar);
                oppAvatar[i] = avatar;
                // ─────────────────────────────────────────────────────────

                // Name (moved right — was 14, now 54)
                var lName = MakeLbl(config.PlayerNames[i],
                    new Rectangle(54, 7, boxW - 68, 22), TEXT_MN, 11f, true);
                box.Controls.Add(lName);
                oppName[i] = lName;

                // Type (moved right)
                string typeStr = config.IsHuman[i] ? "Human" : $"CPU · {config.AIDifficulty[i]}";
                box.Controls.Add(MakeLbl(typeStr,
                    new Rectangle(54, 27, boxW - 68, 16), Color.FromArgb(60, 90, 70), 8f));

                // Card count (moved right)
                var lCount = MakeLbl("7 cards", new Rectangle(54, 43, boxW - 68, 17), TEXT_MUT, 9f);
                box.Controls.Add(lCount);
                oppCardCount[i] = lCount;

                // Mini cards panel
                var mini = new Panel { Bounds = new Rectangle(14, 60, boxW - 28, MNH + 2), BackColor = Color.Transparent };
                int ii = i;
                mini.Paint += (s, e) =>
                {
                    int cnt = gameStarted ? players[ii].HandCount : 7;
                    DrawMiniCards(e.Graphics, mini.Width, cnt);
                };
                box.Controls.Add(mini);
                oppMiniCardPanel[i] = mini;

                // Banner
                var bnrPnl = new Panel { Bounds = new Rectangle(0, 88, boxW, 20), BackColor = Color.Transparent, Visible = false };
                var bnrLbl = MakeLbl("", new Rectangle(0, 0, boxW, 20), Color.White, 8f, true);
                bnrLbl.TextAlign = ContentAlignment.MiddleCenter;
                bnrPnl.Controls.Add(bnrLbl);
                box.Controls.Add(bnrPnl);
                oppBanner[i] = bnrPnl;
                oppBannerLbl[i] = bnrLbl;

                oppBox[i] = box;
                pnlOpp.Controls.Add(box);
            }
        }

        private void DrawMiniCards(Graphics g, int pw, int count)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int max = Math.Min(count, 13);
            int sp = max > 1 ? Math.Min(MNW - 2, (pw - MNW) / (max - 1)) : 0;
            for (int i = 0; i < max; i++)
            {
                int x = i * sp;
                using (var b = new SolidBrush(Color.FromArgb(35, 0, 0, 0)))
                    g.FillRectangle(b, x + 2, 2, MNW, MNH);
                var rp = RoundedPath(new Rectangle(x, 0, MNW, MNH), 3);
                using (var b = new SolidBrush(Color.FromArgb(175, 30, 30)))
                    g.FillPath(b, rp);
                using (var p = new Pen(Color.FromArgb(100, 255, 255, 255), 0.8f))
                    g.DrawPath(p, rp);
            }
            if (count > 13)
                using (var f = new Font("Segoe UI", 7f, FontStyle.Bold))
                using (var b = new SolidBrush(TEXT_MUT))
                    g.DrawString($"+{count - 13}", f, b, 13 * sp + MNW + 2, MNH / 2 - 6);
        }

        // ── Init game ─────────────────────────────────────────────────────────
        private void InitGame()
        {
            MusicManager.PlayGameMusic();
            players = new List<Player>();
            for (int i = 0; i < config.PlayerNames.Length; i++)
            {
                if (config.IsHuman[i])
                    players.Add(new Player(config.PlayerNames[i], true));
                else
                {
                    var d = config.AIDifficulty[i] switch
                    {
                        "Easy" => AIDifficulty.Easy,
                        "Hard" => AIDifficulty.Hard,
                        _ => AIDifficulty.Medium
                    };
                    players.Add(new AIPlayer(config.PlayerNames[i], d));
                }
            }

            engine = new GameEngine(players);
            engine.OnCardPlayed += (p, c) => SetStatus($"{p.Name} played {c}.");
            engine.OnCardsDrawn += OnDrawn;
            engine.OnPlayerSkipped += OnSkipped;
            engine.OnDirectionChanged += OnReversed;
            engine.OnUNOCalled += p => ShowBanner(players.IndexOf(p), "UNO!", Color.FromArgb(155, 115, 0));
            engine.OnRoundWon += OnRoundWon;

            sessionStart = DateTime.Now;
            roundStart = DateTime.Now;

            foreach (var p in players) if (p.IsHuman) db.GetOrCreatePlayer(p.Name);

            engine.StartGame();
            gameStarted = true;
            sessionId = db.SaveSession(GetGameMode(), "", 0, sessionStart);

            for (int i = 0; i < players.Count; i++)
            {
                int? pid = players[i].IsHuman ? db.GetOrCreatePlayer(players[i].Name) : (int?)null;
                db.SaveSessionPlayer(sessionId, pid, players[i].Name,
                    !players[i].IsHuman,
                    !players[i].IsHuman ? config.AIDifficulty[i] : null,
                    0, 0);
            }

            RefreshUI();
            ProcessTurn();
        }

        private string GetGameMode()
        {
            int h = players.Count(p => p.IsHuman);
            if (h == players.Count) return $"{players.Count}P";
            if (h == 1) return "Solo";
            return "Mixed";
        }

        // ── Turn loop ─────────────────────────────────────────────────────────
        private void ProcessTurn()
        {
            if (!gameStarted || processingTurn) return;
            if (engine.State == GameState.RoundOver || engine.State == GameState.GameOver) return;

            RefreshUI();

            if (engine.State == GameState.AITurn)
            {
                processingTurn = true;
                SetStatus($"{engine.CurrentPlayer.Name} is thinking...");
                HighlightOpp(engine.CurrentPlayer.Name, true);

                var t = new System.Windows.Forms.Timer { Interval = 1300 };
                t.Tick += (s, e) =>
                {
                    t.Stop();
                    HighlightOpp(engine.CurrentPlayer.Name, false);
                    engine.ProcessAITurn();
                    turnCount++;
                    processingTurn = false;
                    RefreshUI();
                    ProcessTurn();
                };
                t.Start();
                return;
            }

            if (engine.State == GameState.PlayerTurn)
                CheckPassDevice();

            RefreshUI();
        }

        private void HighlightOpp(string name, bool on)
        {
            for (int i = 1; i < players.Count; i++)
                if (players[i].Name == name && oppBox?[i] != null)
                    oppBox[i].BackColor = on ? Color.FromArgb(28, 46, 58) : BG_DARK;
        }

        private void CheckPassDevice()
        {
            if (!engine.CurrentPlayer.IsHuman) return;
            string cur = engine.CurrentPlayer.Name;
            if (lastHumanPlayer != "" && lastHumanPlayer != cur)
            {
                new FormPassDevice(cur, () => { lastHumanPlayer = cur; RefreshUI(); })
                    .ShowDialog(this);
            }
            else lastHumanPlayer = cur;
        }

        // ── Refresh ───────────────────────────────────────────────────────────
        private void RefreshUI()
        {
            if (!gameStarted) return;
            pnlHand.Invalidate();
            pnlHand.Refresh();
            var cur = engine.CurrentPlayer;
            lblRound.Text = $"Round {engine.RoundNumber}";
            lblTurn.Text = $"Turn {turnCount + 1}";
            pnlDot.BackColor = CARD_CLR[engine.ActiveColor];
            lblActiveColor.Text = $"Active: {engine.ActiveColor}";

            if (engine.State == GameState.PlayerTurn && cur.IsHuman)
                SetStatus($"Your turn, {cur.Name} — click a highlighted card to play, or draw.");
            else if (engine.State == GameState.AITurn)
                SetStatus($"{cur.Name} is thinking...");

            RefreshOppPanels();
            RefreshBottomBar();

            btnDraw.Enabled = engine.State == GameState.PlayerTurn && cur.IsHuman;
            btnDraw.BackColor = btnDraw.Enabled
                ? Color.FromArgb(34, 56, 36) : Color.FromArgb(18, 30, 20);
            btnDraw.ForeColor = btnDraw.Enabled ? TEXT_MN : TEXT_MUT;

            pnlCenter.Invalidate();
            pnlHand.Invalidate();
        }

        private void RefreshOppPanels()
        {
            if (oppBox == null || !gameStarted) return;
            for (int i = 0; i < players.Count; i++)
            {
                if (oppBox[i] == null) continue;
                bool active = engine.CurrentPlayer == players[i];
                int cnt = players[i].HandCount;
                bool uno = cnt == 1;

                oppBox[i].BackColor = active ? Color.FromArgb(26, 44, 56) : BG_DARK;
                oppName[i].ForeColor = active ? GOLD : uno ? Color.FromArgb(255, 160, 0) : TEXT_MN;
                oppName[i].Text = players[i].Name +
                    (active ? "  ◀ TURN" : uno ? "  UNO!" : "");
                oppCardCount[i].Text = $"{cnt} card{(cnt == 1 ? "" : "s")}";
                oppCardCount[i].ForeColor = uno ? Color.FromArgb(255, 160, 0) : TEXT_MUT;
                oppMiniCardPanel[i]?.Invalidate();

                // Banner
                string bn = bannerText[i];
                if (!string.IsNullOrEmpty(bn))
                {
                    oppBanner[i].BackColor = bannerColor[i];
                    oppBannerLbl[i].Text = bn;
                    oppBanner[i].Visible = true;
                }
                else oppBanner[i].Visible = false;
            }
        }

        private void RefreshBottomBar()
        {
            if (!gameStarted) return;
            var cur = engine.CurrentPlayer;
            bool myTurn = engine.State == GameState.PlayerTurn && cur.IsHuman;
            var me = players.FirstOrDefault(p => p.IsHuman);
            int myIdx = players.IndexOf(me ?? cur);
            string bn = myIdx >= 0 && myIdx < bannerText.Length ? bannerText[myIdx] : "";

            lblMyName.Text = myTurn ? $"  ▶  {cur.Name}" : $"  {me?.Name ?? ""}";
            lblMyName.ForeColor = myTurn ? GOLD : TEXT_MUT;
            lblMyInfo.Text = !string.IsNullOrEmpty(bn)
                ? $"{bn}  "
                : $"{me?.HandCount ?? 0} card{(me?.HandCount == 1 ? "" : "s")}  ";
            lblMyInfo.ForeColor = !string.IsNullOrEmpty(bn)
                ? Color.FromArgb(255, 140, 140) : TEXT_MUT;
        }

        // ── Paint center ──────────────────────────────────────────────────────
        private void PaintCenter(object sender, PaintEventArgs e)
        {
            if (!gameStarted) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int W = pnlCenter.Width;
            int H = pnlCenter.Height;
            int cx = W / 2, cy = H / 2;

            // Draw pile
            int dpx = cx - MCW - 52;
            int dpy = cy - MCH / 2;
            DrawCard(g, dpx, dpy, MCW, MCH, Color.FromArgb(155, 28, 28), "DRAW", false);
            DrawCenterLabel(g, "Draw pile", dpx, dpy + MCH + 4, MCW);

            // Direction
            string dir = engine.Direction == TurnDirection.Clockwise
                ? "→  Clockwise" : "←  Counter";
            using (var f = new Font("Segoe UI", 9f))
            using (var b = new SolidBrush(Color.FromArgb(70, 110, 80)))
            {
                var sz = g.MeasureString(dir, f);
                g.DrawString(dir, f, b, cx - sz.Width / 2f, cy - 10);
            }

            // Discard pile
            int discX = cx + 52;
            int discY = cy - MCH / 2;
            if (engine.TopDiscard != null)
            {
                DrawCard(g, discX, discY, MCW, MCH,
                    CARD_CLR[engine.ActiveColor],
                    GetLbl(engine.TopDiscard), true, engine.TopDiscard);
                DrawCenterLabel(g, "Discard pile", discX, discY + MCH + 4, MCW);
            }
        }

        private void DrawCenterLabel(Graphics g, string text, int x, int y, int w)
        {
            using (var f = new Font("Segoe UI", 8f))
            using (var b = new SolidBrush(TEXT_MUT))
            {
                var sz = g.MeasureString(text, f);
                g.DrawString(text, f, b, x + (w - sz.Width) / 2f, y);
            }
        }

        private void ClickCenter(object sender, EventArgs e)
        {
            if (engine.State != GameState.PlayerTurn) return;
            var me = e as MouseEventArgs;
            if (me == null) return;
            int cx = pnlCenter.Width / 2, cy = pnlCenter.Height / 2;
            if (new Rectangle(cx - MCW - 52, cy - MCH / 2, MCW, MCH).Contains(me.Location))
                DrawClick(sender, e);
        }

        // ── Paint hand ────────────────────────────────────────────────────────
        private void PaintHand(object sender, PaintEventArgs e)
        {
            if (!gameStarted || !engine.CurrentPlayer.IsHuman) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var hand = engine.CurrentPlayer.Hand;
            int W = pnlHand.Width;
            int H = pnlHand.Height;
            cardRects.Clear();
            if (hand.Count == 0) return;

            var play = engine.GetPlayableCardsForCurrentPlayer();
            int maxW = W - 80;
            int sp = hand.Count > 1
                ? Math.Min(OVL, (maxW - CW) / (hand.Count - 1)) : CW;
            int total = CW + sp * (hand.Count - 1);
            int sx = (W - total) / 2;
            int baseY = H / 2 - CH / 2 + 6;

            for (int i = 0; i < hand.Count; i++)
            {
                var card = hand[i];
                bool can = play.Contains(card);
                bool hov = hoveredCard == i && can;
                int x = sx + i * sp;
                int y = can ? baseY - 12 : baseY;
                if (hov) y -= 8;

                DrawCard(g, x, y, CW, CH, CARD_CLR[card.Color],
                    GetLbl(card), true, card, can, hov);
                cardRects.Add(new Rectangle(x, can ? baseY - 12 : baseY, CW, CH));
            }

            using (var f = new Font("Segoe UI", 8.5f, FontStyle.Italic))
            using (var b = new SolidBrush(Color.FromArgb(46, 74, 46)))
            {
                string hint = "Highlighted cards are playable  ·  Dimmed cards cannot be played";
                var sz = g.MeasureString(hint, f);
                g.DrawString(hint, f, b, (W - sz.Width) / 2f, H - 18);
            }
        }

        private void MoveHand(object sender, MouseEventArgs e)
        {
            if (!gameStarted || !engine.CurrentPlayer.IsHuman) return;

            int prev = hoveredCard;
            int next = -1;
            for (int i = cardRects.Count - 1; i >= 0; i--)
            {
                if (cardRects[i].Contains(e.Location))
                {
                    next = i;
                    break;
                }
            }
            if (next != prev)
            {
                hoveredCard = next;
                pnlHand.Invalidate();
            }
        }

        private void ClickHand(object sender, MouseEventArgs e)
        {
            if (engine.State != GameState.PlayerTurn || !engine.CurrentPlayer.IsHuman) return;
            var hand = engine.CurrentPlayer.Hand;
            var play = engine.GetPlayableCardsForCurrentPlayer();
            for (int i = cardRects.Count - 1; i >= 0; i--)
            {
                if (!cardRects[i].Contains(e.Location)) continue;
                if (i >= hand.Count) return;
                var card = hand[i];
                if (!play.Contains(card))
                {
                    ShowBanner(players.IndexOf(engine.CurrentPlayer),
                        "⚠ Can't play that card!", Color.FromArgb(110, 35, 15));
                    return;
                }
                PlayCard(card);
                return;
            }
        }

        // ── Play ──────────────────────────────────────────────────────────────
        private void PlayCard(Card card)
        {
            var p = engine.CurrentPlayer;
            if (p.HandCount == 2)
            {
                p.CallUno();
                ShowBanner(players.IndexOf(p), "UNO!", Color.FromArgb(155, 115, 0));
                if (sessionId > 0) db.LogMove(sessionId, engine.RoundNumber, p.Name, "UNO", null, null);
            }
            if (sessionId > 0) db.LogMove(sessionId, engine.RoundNumber, p.Name, "Play", card.ToString(), null);
            engine.PlayCard(p, card);
            turnCount++;
            if (engine.State == GameState.ChoosingColor) { ShowColorPicker(); return; }
            RefreshUI();
            ProcessTurn();
        }

        private void DrawClick(object sender, EventArgs e)
        {
            if (engine.State != GameState.PlayerTurn || !engine.CurrentPlayer.IsHuman) return;
            var p = engine.CurrentPlayer;
            var drawn = engine.DrawCard(p);
            if (sessionId > 0) db.LogMove(sessionId, engine.RoundNumber, p.Name, "Draw", drawn?.ToString(), null);
            SetStatus($"{p.Name} drew a card.");
            RefreshUI();
            if (drawn != null && drawn.IsPlayableOn(engine.TopDiscard, engine.ActiveColor))
            {
                if (MessageBox.Show($"You drew: {drawn}\nPlay it now?", "Play drawn card?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                { PlayCard(drawn); return; }
            }
            engine.AdvanceTurn();
            RefreshUI();
            ProcessTurn();
        }

        // ── Color picker ──────────────────────────────────────────────────────
        private void ShowColorPicker()
        {
            var f = new Form
            {
                Text = "Choose a color",
                Size = new Size(380, 210),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = BG_DARK,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            new Label
            {
                Text = "Choose the active color:",
                ForeColor = TEXT_MN,
                Font = new Font("Segoe UI", 12f),
                Bounds = new Rectangle(20, 14, 340, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Parent = f
            };

            var picks = new (string n, Color c, CardColor cc)[]
            {
                ("Red", WILD_Q[0], CardColor.Red), ("Blue", WILD_Q[1], CardColor.Blue),
                ("Green", WILD_Q[2], CardColor.Green), ("Yellow", WILD_Q[3], CardColor.Yellow),
            };
            int bx = 18;
            foreach (var (n, c, cc) in picks)
            {
                var btn = new Button
                {
                    Text = n,
                    Bounds = new Rectangle(bx, 56, 78, 100),
                    BackColor = c,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Parent = f
                };
                btn.FlatAppearance.BorderSize = 0;
                var ch = cc;
                btn.Click += (s, ev) => { f.DialogResult = DialogResult.OK; f.Tag = ch; f.Close(); };
                bx += 86;
            }
            f.ShowDialog(this);
            if (f.Tag is CardColor picked)
            {
                if (sessionId > 0) db.LogMove(sessionId, engine.RoundNumber,
                    engine.CurrentPlayer.Name, "Color", null, picked.ToString());
                engine.SetActiveColor(picked);
            }
            RefreshUI();
            ProcessTurn();
        }

        // ── Events ────────────────────────────────────────────────────────────
        private void OnDrawn(Player p, int count)
        {
            string msg = $"⚡ Drew +{count} cards!";
            SetStatus($"{p.Name} draws {count} card{(count > 1 ? "s" : "")}!");
            if (count >= 2) ShowBanner(players.IndexOf(p), msg, Color.FromArgb(155, 28, 28));
        }

        private void OnSkipped(Player p)
        {
            SetStatus($"{p.Name} is skipped!");
            ShowBanner(players.IndexOf(p), "⏭ Skipped!", Color.FromArgb(120, 65, 8));
        }

        private void OnReversed(TurnDirection d)
        {
            string msg = d == TurnDirection.Clockwise ? "↩ Reversed → Clockwise" : "↪ Reversed → Counter";
            SetStatus(msg);
            for (int i = 0; i < players.Count; i++)
                ShowBanner(i, msg, Color.FromArgb(18, 55, 105));
        }

        private void OnRoundWon(Player winner)
        {
            int pts = players.Where(p => p != winner).Sum(p => p.GetHandPoints());
            if (sessionId > 0)
                db.SaveRound(sessionId, engine.RoundNumber, winner.Name, pts,
                    (int)(DateTime.Now - roundStart).TotalSeconds);
            ShowWinScreen(winner, pts);
        }

        // ── Win screen ────────────────────────────────────────────────────────
        private void ShowWinScreen(Player winner, int pts)
        {
            MusicManager.PlayWinnerSound();

            var win = new Form
            {
                Text = "Round Over",
                Size = new Size(460, 360),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = BG_DARK,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            new Label
            {
                Text = "🏆 GAME WINNER 🏆",
                ForeColor = GOLD,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Bounds = new Rectangle(0, 20, 460, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Parent = win
            };

            new Label
            {
                Text = winner.Name,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 28f, FontStyle.Bold),
                Bounds = new Rectangle(0, 55, 460, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Parent = win
            };

            new Label
            {
                Text = $"+{pts} points earned this round",
                ForeColor = Color.FromArgb(139, 196, 160),
                Font = new Font("Segoe UI", 12f),
                Bounds = new Rectangle(0, 105, 460, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Parent = win
            };

            // Final scores
            new Label
            {
                Text = "FINAL SCORES",
                ForeColor = GOLD,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Bounds = new Rectangle(0, 140, 460, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Parent = win
            };

            int sy = 170;
            foreach (var p in players.OrderByDescending(p => p.Score))
            {
                new Label
                {
                    Text = p.Name,
                    ForeColor = p == winner ? GOLD : TEXT_MUT,
                    Font = new Font("Segoe UI", 10f, p == winner ? FontStyle.Bold : FontStyle.Regular),
                    Bounds = new Rectangle(80, sy, 200, 22),
                    BackColor = Color.Transparent,
                    Parent = win
                };
                new Label
                {
                    Text = $"{p.Score} pts",
                    ForeColor = p == winner ? GOLD : TEXT_MN,
                    Font = new Font("Segoe UI", 10f, p == winner ? FontStyle.Bold : FontStyle.Regular),
                    Bounds = new Rectangle(280, sy, 100, 22),
                    TextAlign = ContentAlignment.MiddleRight,
                    BackColor = Color.Transparent,
                    Parent = win
                };
                sy += 26;
            }

            // Single button - Back to Menu
            var btnExit = new Button
            {
                Text = "Back to Menu",
                Bounds = new Rectangle(130, sy + 20, 200, 44),
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Parent = win
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) =>
            {
                MusicManager.StopWinnerSound();
                win.Close();
            };

            win.ShowDialog(this);

            // End the game and return to main menu
            EndGame(winner);
        }
        private void EndGame(Player winner)
        {
            if (sessionId > 0)
                db.UpdateSession(sessionId, winner.Name, engine.RoundNumber);

            var sorted = players.OrderByDescending(p => p.Score).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                db.UpdateSessionPlayer(sessionId, sorted[i].Name, sorted[i].Score, i + 1);
                if (sorted[i].IsHuman)
                    db.UpdatePlayerStats(sorted[i].Name, sorted[i] == winner, sorted[i].Score);
            }

            MusicManager.PlayMenuMusic();  // Switch back to menu music
            new FormMainMenu().Show();
            this.Close();
        }

        // ── GDI+ card ─────────────────────────────────────────────────────────
        private void DrawCard(Graphics g, int x, int y, int w, int h,
            Color color, string label, bool faceUp,
            Card card = null, bool playable = false, bool hovered = false)
        {   
            //shadow
            using (var sb = new SolidBrush(Color.FromArgb(48, 0, 0, 0)))
                g.FillPath(sb, RoundedPath(new Rectangle(x + 3, y + 4, w, h), 8));
            //card background
            var path = RoundedPath(new Rectangle(x, y, w, h), 8);
            using (var b = new SolidBrush(color)) g.FillPath(b, path);
            using (var b = new SolidBrush(Color.FromArgb(22, 255, 255, 255)))
                g.FillPath(b, RoundedPath(new Rectangle(x + 3, y + 3, w - 6, h - 6), 6));

            if (playable)
                using (var p = new Pen(GOLD, 3f)) g.DrawPath(p, path);
            else if (hovered)
                using (var p = new Pen(Color.FromArgb(170, 255, 255, 255), 2f)) g.DrawPath(p, path);
            else
                using (var p = new Pen(Color.FromArgb(110, 255, 255, 255), 1.5f)) g.DrawPath(p, path);

            if (!faceUp)
            {
                using (var p = new Pen(Color.FromArgb(75, 255, 255, 255), 1f))
                    g.DrawEllipse(p, x + 6, y + 10, w - 12, h - 20);
                float fs2 = w > 40 ? 9f : 6f;
                using (var f = new Font("Segoe UI", fs2, FontStyle.Bold))
                using (var b = new SolidBrush(Color.FromArgb(140, 255, 255, 255)))
                {
                    var sz2 = g.MeasureString("UNO", f);
                    g.DrawString("UNO", f, b, x + (w - sz2.Width) / 2f, y + (h - sz2.Height) / 2f);
                }
                return;
            }
            if (card != null && card.IsWild()) { DrawWild(g, x, y, w, h, label); return; }

            using (var p = new Pen(Color.FromArgb(100, 255, 255, 255), 1.3f))
                g.DrawEllipse(p, x + 5, y + 8, w - 10, h - 16);

            float fs = label.Length > 2 ? (w > 60 ? 13f : 10f) : (w > 60 ? 26f : 20f);
            using (var f = new Font("Segoe UI", fs, FontStyle.Bold))
            using (var b = new SolidBrush(Color.White))
            {
                var sz = g.MeasureString(label, f);
                g.DrawString(label, f, b, x + (w - sz.Width) / 2f, y + (h - sz.Height) / 2f);
            }
            using (var f = new Font("Segoe UI", w > 60 ? 8f : 6f, FontStyle.Bold))
            using (var b = new SolidBrush(Color.White))
            {
                g.DrawString(label, f, b, x + 4, y + 4);
                g.DrawString(label, f, b, x + w - 16, y + h - 14);
            }
        }

        private void DrawWild(Graphics g, int x, int y, int w, int h, string label)
        {
            g.SetClip(RoundedPath(new Rectangle(x, y, w, h), 8));
            int hw = w / 2, hh = h / 2;
            using (var b = new SolidBrush(WILD_Q[0])) g.FillRectangle(b, x, y, hw, hh);
            using (var b = new SolidBrush(WILD_Q[1])) g.FillRectangle(b, x + hw, y, hw, hh);
            using (var b = new SolidBrush(WILD_Q[2])) g.FillRectangle(b, x, y + hh, hw, hh);
            using (var b = new SolidBrush(WILD_Q[3])) g.FillRectangle(b, x + hw, y + hh, hw, hh);
            g.ResetClip();
            using (var b = new SolidBrush(Color.FromArgb(155, 0, 0, 0)))
                g.FillEllipse(b, x + 7, y + 11, w - 14, h - 22);
            float fs = w > 60 ? 11f : 8f;
            using (var f = new Font("Segoe UI", fs, FontStyle.Bold))
            using (var b = new SolidBrush(Color.White))
            {
                var sz = g.MeasureString(label, f);
                g.DrawString(label, f, b, x + (w - sz.Width) / 2f, y + (h - sz.Height) / 2f);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private string GetLbl(Card c) => c.Type switch
        {
            CardType.Number => c.Value.ToString(),
            CardType.Skip => "SK",
            CardType.Reverse => "RV",
            CardType.DrawTwo => "+2",
            CardType.Wild => "W",
            CardType.WildDrawFour => "W+4",
            _ => "?"
        };

        private void SetStatus(string msg)
        {
            if (lblStatus != null && !lblStatus.IsDisposed)
                lblStatus.Text = "   " + msg;
        }

        private Label MakeLbl(string text, Rectangle bounds, Color fore,
                              float size, bool bold = false)
        {
            return new Label
            {
                Text = text,
                ForeColor = fore,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
                Bounds = bounds,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private GraphicsPath RoundedPath(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void Rounded(Control c, int r) =>
            c.Region = new Region(RoundedPath(new Rectangle(0, 0, c.Width, c.Height), r));

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
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = BTN_BRD;
            btn.FlatAppearance.BorderSize = 1;
            return btn;
        }
    }
}


// ── FormPassDevice ────────────────────────────────────────────────────────────
namespace UNOFinal
{
    public class FormPassDevice : Form
    {
        private readonly Color BG  = Color.FromArgb(10, 10, 18);
        private readonly Color RED = Color.FromArgb(230, 57, 70);

        private string nextPlayerName;
        private Action onReady;

        public FormPassDevice(string nextPlayerName, Action onReady)
        {
            this.nextPlayerName = nextPlayerName;
            this.onReady        = onReady;
            SetupForm();
            this.Load += (s, e) => BuildUI();
        }

        private void SetupForm()
        {
            this.Text            = "Pass the Device";
            this.BackColor       = BG;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState     = FormWindowState.Maximized;
            this.DoubleBuffered  = true;
        }

        private void BuildUI()
        {
            this.Controls.Clear();
            int W  = this.ClientSize.Width;
            int H  = this.ClientSize.Height;
            int cx = W / 2;
            int cy = H / 2;

            Label lblTitle = new Label
            {
                Text      = "Pass the Device",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 28f),
                AutoSize  = true,
                BackColor = Color.Transparent
            };
            lblTitle.Location = new Point(cx - lblTitle.PreferredWidth / 2, cy - 120);
            this.Controls.Add(lblTitle);

            Label lblSub = new Label
            {
                Text      = $"It's {nextPlayerName}'s turn",
                ForeColor = Color.FromArgb(76, 175, 80),
                Font      = new Font("Segoe UI", 16f),
                AutoSize  = true,
                BackColor = Color.Transparent
            };
            lblSub.Location = new Point(cx - lblSub.PreferredWidth / 2, cy - 60);
            this.Controls.Add(lblSub);

            Label lblHint = new Label
            {
                Text      = "Cards are hidden. Hand the device to the next player.",
                ForeColor = Color.FromArgb(100, 100, 100),
                Font      = new Font("Segoe UI", 12f),
                AutoSize  = true,
                BackColor = Color.Transparent
            };
            lblHint.Location = new Point(cx - lblHint.PreferredWidth / 2, cy - 10);
            this.Controls.Add(lblHint);

            Button btnReady = new Button
            {
                Text      = "I'm ready — show my cards",
                Bounds    = new Rectangle(cx - 180, cy + 50, 360, 56),
                BackColor = RED,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnReady.FlatAppearance.BorderSize = 0;
            btnReady.Click += (s, e) => { onReady?.Invoke(); this.Close(); };
            this.Controls.Add(btnReady);
        }
    }
}


// ── FormSettings ──────────────────────────────────────────────────────────────
namespace UNOFinal
{
    public class FormSettings : Form
    {
        private readonly Color BG_DARK  = Color.FromArgb(19, 32, 24);
        private readonly Color BG_BAR   = Color.FromArgb(10, 24, 16);
        private readonly Color BG_MID   = Color.FromArgb(30, 45, 36);
        private readonly Color RED      = Color.FromArgb(230, 57, 70);
        private readonly Color BTN_BRD  = Color.FromArgb(58, 90, 68);
        private readonly Color TEXT_MN  = Color.FromArgb(224, 224, 224);
        private readonly Color TEXT_MUT = Color.FromArgb(106, 138, 116);

        public FormSettings()
        {
            SetupForm();
            this.Load += (s, e) => BuildUI();
        }

        private void SetupForm()
        {
            this.Text            = "UNO - Settings";
            this.BackColor       = BG_DARK;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState     = FormWindowState.Maximized;
            this.DoubleBuffered  = true;
            this.Font            = new Font("Segoe UI", 10f);
        }

        private void BuildUI()
        {
            this.Controls.Clear();
            int W      = this.ClientSize.Width;
            int H      = this.ClientSize.Height;
            int cx     = W / 2;
            int panelW = Math.Min(500, W - 80);
            int panelX = cx - panelW / 2;
            int y      = 80;

            // Top bar
            Panel pnlBar = new Panel { Bounds = new Rectangle(0, 0, W, 44), BackColor = BG_BAR };
            Button btnBack = new Button
            {
                Text      = "Back",
                Bounds    = new Rectangle(12, 5, 80, 34),
                BackColor = BG_MID,
                ForeColor = TEXT_MUT,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderColor = BTN_BRD;
            btnBack.FlatAppearance.BorderSize  = 1;
            btnBack.Click += (s, e) => this.Close();
            Label lblTitle = new Label
            {
                Text      = "Settings",
                ForeColor = TEXT_MN,
                Font      = new Font("Segoe UI", 14f),
                Bounds    = new Rectangle(106, 0, 200, 44),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            pnlBar.Controls.Add(btnBack);
            pnlBar.Controls.Add(lblTitle);
            this.Controls.Add(pnlBar);

            // Draw Two stacking
            CheckBox chkStack = new CheckBox
            {
                Text      = "Allow Draw Two stacking (house rule)",
                ForeColor = TEXT_MN,
                Font      = new Font("Segoe UI", 11f),
                AutoSize  = true,
                Location  = new Point(panelX, y),
                BackColor = Color.Transparent,
                Checked   = false
            };
            this.Controls.Add(chkStack);

            // Animation speed
            Label lblAnim = new Label
            {
                Text      = "Card animation speed",
                ForeColor = TEXT_MUT,
                Font      = new Font("Segoe UI", 10f),
                AutoSize  = true,
                Location  = new Point(panelX, y + 50),
                BackColor = Color.Transparent
            };
            ComboBox cmbAnim = new ComboBox
            {
                Bounds        = new Rectangle(panelX, y + 74, 200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor     = BG_MID,
                ForeColor     = TEXT_MN,
                FlatStyle     = FlatStyle.Flat,
                Font          = new Font("Segoe UI", 10f)
            };
            cmbAnim.Items.AddRange(new object[] { "Fast", "Normal", "Slow" });
            cmbAnim.SelectedIndex = 1;
            this.Controls.Add(lblAnim);
            this.Controls.Add(cmbAnim);

            // Save button
            Button btnSave = new Button
            {
                Text      = "Save Settings",
                Bounds    = new Rectangle(panelX, y + 160, panelW, 52),
                BackColor = RED,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => this.Close();
            this.Controls.Add(btnSave);
        }
    }
}

namespace UNOFinal
{
    public class FormMatchDetails : Form
    {
        // ── Colors ────────────────────────────────────────────────────────────
        private readonly Color BG_DARK = Color.FromArgb(19, 32, 24);
        private readonly Color BG_BAR = Color.FromArgb(10, 24, 16);
        private readonly Color BG_PANEL = Color.FromArgb(24, 38, 28);
        private readonly Color BG_ROW_A = Color.FromArgb(28, 44, 32);
        private readonly Color BG_ROW_B = Color.FromArgb(22, 36, 26);
        private readonly Color GOLD = Color.FromArgb(255, 215, 0);
        private readonly Color RED_C = Color.FromArgb(211, 47, 47);
        private readonly Color BLUE_C = Color.FromArgb(25, 118, 210);
        private readonly Color GREEN_C = Color.FromArgb(46, 125, 50);
        private readonly Color YELLOW_C = Color.FromArgb(245, 124, 0);
        private readonly Color TEXT_MN = Color.FromArgb(224, 224, 224);
        private readonly Color TEXT_MUT = Color.FromArgb(106, 138, 116);
        private readonly Color BTN_BRD = Color.FromArgb(58, 90, 68);

        private DatabaseManager db = new DatabaseManager();
        private int sessionId;
        private string sessionWinner;
        private TabControl tabs;

        public FormMatchDetails(int sessionId, string winnerName)
        {
            this.sessionId = sessionId;
            this.sessionWinner = winnerName;
            SetupForm();
            this.Load += (s, e) => BuildUI();
        }

        private void SetupForm()
        {
            this.Text = "Match Details";
            this.BackColor = BG_DARK;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 10f);
        }

        private void BuildUI()
        {
            this.Controls.Clear();
            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height;

            // ── Top bar ───────────────────────────────────────────────────────
            Panel pnlBar = new Panel { Bounds = new Rectangle(0, 0, W, 50), BackColor = BG_BAR };
            Button btnBack = MakeBtn("Back", 12, 8, 80, 34);
            btnBack.Click += (s, e) => this.Close();

            new Label
            {
                Text = $"Match Details  —  Winner: {sessionWinner}",
                ForeColor = TEXT_MN,
                Font = new Font("Segoe UI", 14f),
                Bounds = new Rectangle(106, 0, W - 200, 50),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Parent = pnlBar
            };
            pnlBar.Controls.Add(btnBack);
            this.Controls.Add(pnlBar);

            // ── Tab control ───────────────────────────────────────────────────
            tabs = new TabControl
            {
                Bounds = new Rectangle(0, 50, W, H - 50),
                BackColor = BG_DARK,
                Font = new Font("Segoe UI", 10f)
            };
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.DrawItem += Tabs_DrawItem;
            tabs.ItemSize = new Size(160, 36);
            tabs.SizeMode = TabSizeMode.Fixed;
            this.Controls.Add(tabs);

            // Tab 1 — Match Summary
            var tabMatch = new TabPage("Match Summary")
            {
                BackColor = BG_DARK,
                Padding = new Padding(0)
            };
            tabs.TabPages.Add(tabMatch);
            BuildMatchSummaryTab(tabMatch);

            // Tab 2 — Player Profiles
            var tabPlayers = new TabPage("Player Profiles")
            {
                BackColor = BG_DARK,
                Padding = new Padding(0)
            };
            tabs.TabPages.Add(tabPlayers);
            BuildPlayerProfilesTab(tabPlayers);
        }

        // ── Custom tab drawing ────────────────────────────────────────────────
        private void Tabs_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tab = tabs.TabPages[e.Index];
            bool sel = e.Index == tabs.SelectedIndex;
            var bg = sel ? BG_PANEL : BG_BAR;
            var fg = sel ? GOLD : TEXT_MUT;
            using (var b = new SolidBrush(bg))
                e.Graphics.FillRectangle(b, e.Bounds);
            using (var f = new Font("Segoe UI", 10f, sel ? FontStyle.Bold : FontStyle.Regular))
            using (var b = new SolidBrush(fg))
                e.Graphics.DrawString(tab.Text, f, b,
                    e.Bounds.X + (e.Bounds.Width - e.Graphics.MeasureString(tab.Text, f).Width) / 2f,
                    e.Bounds.Y + 8);
            if (sel)
            {
                using (var p = new Pen(GOLD, 2f))
                    e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1,
                        e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        // ── TAB 1: Match Summary ──────────────────────────────────────────────
        private void BuildMatchSummaryTab(TabPage tab)
        {
            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height - 90;
            int col = W / 2 - 20;

            Panel scroll = new Panel
            {
                Bounds = new Rectangle(0, 0, W, H),
                AutoScroll = true,
                BackColor = BG_DARK,
                Parent = tab
            };

            int y = 16;

            // ── Session player results ────────────────────────────────────────
            y = AddSectionHeader(scroll, "Final Results", 20, y, W - 40);
            var players = db.GetSessionTargets(sessionId);
            y = BuildResultsTable(scroll, players, 20, y, W - 40);
            y += 16;

            // ── Round breakdown ───────────────────────────────────────────────
            y = AddSectionHeader(scroll, "Round Breakdown", 20, y, W - 40);
            var rounds = db.GetSessionRounds(sessionId);
            y = BuildRoundsTable(scroll, rounds, 20, y, W - 40);
            y += 16;

            // ── Per-player move stats ─────────────────────────────────────────
            y = AddSectionHeader(scroll, "Player Move Stats", 20, y, W - 40);
            var stats = db.GetSessionPlayerStats(sessionId);
            var colors = db.GetSessionPlayerColors(sessionId);
            y = BuildMoveStatsTable(scroll, stats, colors, 20, y, W - 40);
            y += 16;

            // ── Game summary numbers ──────────────────────────────────────────
            y = AddSectionHeader(scroll, "Game Summary", 20, y, W - 40);
            y = BuildGameSummary(scroll, rounds, stats, 20, y, W - 40);
            y += 30;
        }

        private int AddSectionHeader(Control parent, string title, int x, int y, int w)
        {
            new Label
            {
                Text = title,
                ForeColor = GOLD,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Bounds = new Rectangle(x, y, w, 28),
                BackColor = Color.Transparent,
                Parent = parent
            };
            new Panel
            {
                Bounds = new Rectangle(x, y + 30, w, 1),
                BackColor = Color.FromArgb(40, 70, 50),
                Parent = parent
            };
            return y + 38;
        }

        private int BuildResultsTable(Control parent, DataTable dt, int x, int y, int w)
        {
            string[] cols = { "Place", "Player", "Type", "Final Score" };
            int[] cw = { 80, w - 80 - 120 - 120, 120, 120 };
            y = BuildTableHeader(parent, cols, cw, x, y, w);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i];
                int place = Convert.ToInt32(row["Placement"]);
                string name = row["PlayerName"].ToString();
                bool isAI = Convert.ToBoolean(row["IsAI"]);
                string diff = row["AIDifficulty"]?.ToString() ?? "";
                int score = Convert.ToInt32(row["FinalScore"]);

                Color placeCol = place == 1 ? GOLD
                               : place == 2 ? Color.FromArgb(192, 192, 192)
                               : Color.FromArgb(205, 127, 50);
                string placeStr = place == 1 ? "1st" : place == 2 ? "2nd" : "3rd";
                string typeStr = isAI ? $"CPU ({diff})" : "Human";

                string[] vals = { placeStr, name, typeStr, $"{score} pts" };
                Color[] fc = { placeCol, place == 1 ? GOLD : TEXT_MN, TEXT_MUT, TEXT_MN };
                y = BuildTableRow(parent, vals, cw, x, y, w, i, fc);
            }
            return y + 8;
        }

        private int BuildRoundsTable(Control parent, DataTable dt, int x, int y, int w)
        {
            if (dt.Rows.Count == 0)
            {
                new Label
                {
                    Text = "No round data recorded.",
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 10f, FontStyle.Italic),
                    Bounds = new Rectangle(x, y, w, 28),
                    BackColor = Color.Transparent,
                    Parent = parent
                };
                return y + 36;
            }

            string[] cols = { "Round", "Winner", "Points Scored", "Duration" };
            int[] cw = { 80, w - 80 - 160 - 130, 160, 130 };
            y = BuildTableHeader(parent, cols, cw, x, y, w);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i];
                int rnum = Convert.ToInt32(row["RoundNumber"]);
                string win = row["WinnerName"].ToString();
                int pts = Convert.ToInt32(row["PointsScored"]);
                int dur = Convert.ToInt32(row["Duration"]);
                string durStr = dur > 60 ? $"{dur / 60}m {dur % 60}s" : $"{dur}s";

                string[] vals = { $"Round {rnum}", win, $"{pts} pts", durStr };
                Color[] fc = { TEXT_MUT, win == sessionWinner ? GOLD : TEXT_MN, TEXT_MN, TEXT_MUT };
                y = BuildTableRow(parent, vals, cw, x, y, w, i, fc);
            }
            return y + 8;
        }

        private int BuildMoveStatsTable(Control parent, DataTable stats,
                                        DataTable colors, int x, int y, int w)
        {
            if (stats.Rows.Count == 0)
            {
                new Label
                {
                    Text = "No move data recorded for this session.",
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 10f, FontStyle.Italic),
                    Bounds = new Rectangle(x, y, w, 28),
                    BackColor = Color.Transparent,
                    Parent = parent
                };
                return y + 36;
            }

            string[] cols = { "Player", "Played", "Drawn", "UNO Calls", "Skips", "+2s", "+4s", "Fav Color" };
            int rem = w - 200 - 70 - 70 - 90 - 70 - 70 - 70 - 110;
            int[] cw = { 200, 70, 70, 90, 70, 70, 70, 110 };
            y = BuildTableHeader(parent, cols, cw, x, y, w);

            // Build color lookup
            var colorMap = new Dictionary<string, string>();
            foreach (DataRow cr in colors.Rows)
            {
                string pname = cr["PlayerName"].ToString();
                int r = ToInt(cr["Reds"]), b = ToInt(cr["Blues"]),
                    g = ToInt(cr["Greens"]), yc = ToInt(cr["Yellows"]);
                int max = Math.Max(Math.Max(r, b), Math.Max(g, yc));
                string fav = max == 0 ? "—"
                           : max == r ? "Red" : max == b ? "Blue"
                           : max == g ? "Green" : "Yellow";
                colorMap[pname] = fav;
            }

            for (int i = 0; i < stats.Rows.Count; i++)
            {
                var row = stats.Rows[i];
                string pn = row["PlayerName"].ToString();
                string fav = colorMap.ContainsKey(pn) ? colorMap[pn] : "—";

                Color favCol = fav == "Red" ? RED_C : fav == "Blue" ? BLUE_C
                             : fav == "Green" ? GREEN_C : fav == "Yellow" ? YELLOW_C : TEXT_MUT;

                string[] vals = {
                    pn,
                    ToInt(row["CardsPlayed"]).ToString(),
                    ToInt(row["CardsDrawn"]).ToString(),
                    ToInt(row["UnoCalls"]).ToString(),
                    ToInt(row["Skips"]).ToString(),
                    ToInt(row["DrawTwos"]).ToString(),
                    ToInt(row["WildDrawFours"]).ToString(),
                    fav
                };
                Color[] fc = { TEXT_MN, TEXT_MN, TEXT_MUT, GOLD,
                               TEXT_MN, TEXT_MN, TEXT_MN, favCol };
                y = BuildTableRow(parent, vals, cw, x, y, w, i, fc);
            }
            return y + 8;
        }

        private int BuildGameSummary(Control parent, DataTable rounds,
                                     DataTable stats, int x, int y, int w)
        {
            int totalMoves = 0;
            int totalRounds = rounds.Rows.Count;
            int totalDur = 0;
            int maxPts = 0;
            string bigRound = "—";

            foreach (DataRow r in rounds.Rows)
            {
                int d = ToInt(r["Duration"]);
                int p = ToInt(r["PointsScored"]);
                totalDur += d;
                if (p > maxPts) { maxPts = p; bigRound = $"Round {r["RoundNumber"]} ({p} pts)"; }
            }
            foreach (DataRow r in stats.Rows)
                totalMoves += ToInt(r["CardsPlayed"]) + ToInt(r["CardsDrawn"]);

            string durStr = totalDur > 60 ? $"{totalDur / 60}m {totalDur % 60}s" : $"{totalDur}s";

            var summaryItems = new (string label, string value)[]
            {
                ("Total Rounds",       totalRounds.ToString()),
                ("Total Moves",        totalMoves.ToString()),
                ("Game Duration",      durStr),
                ("Biggest Round",      bigRound),
                ("Session Winner",     sessionWinner),
            };

            int itemW = (w - 16 * (summaryItems.Length - 1)) / summaryItems.Length;
            int ix = x;
            foreach (var (label, value) in summaryItems)
            {
                Panel card = new Panel
                {
                    Bounds = new Rectangle(ix, y, itemW, 70),
                    BackColor = BG_PANEL,
                    Parent = parent
                };
                Rounded(card, 8);
                new Label
                {
                    Text = value,
                    ForeColor = GOLD,
                    Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                    Bounds = new Rectangle(0, 8, itemW, 30),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Parent = card
                };
                new Label
                {
                    Text = label,
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 8.5f),
                    Bounds = new Rectangle(0, 40, itemW, 22),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Parent = card
                };
                ix += itemW + 16;
            }
            return y + 86;
        }

        // ── TAB 2: Player Profiles ────────────────────────────────────────────
        private void BuildPlayerProfilesTab(TabPage tab)
        {
            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height - 90;

            // Get players from this session
            var sessionPlayers = db.GetSessionTargets(sessionId);
            var humanPlayers = new List<string>();
            foreach (DataRow r in sessionPlayers.Rows)
                    humanPlayers.Add(r["PlayerName"].ToString());

            if (humanPlayers.Count == 0)
            {
                new Label
                {
                    Text = "No human players in this session.",
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 12f, FontStyle.Italic),
                    Bounds = new Rectangle(40, 40, W - 80, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Parent = tab
                };
                return;
            }

            // If multiple humans, add a player selector
            Panel scroll = new Panel
            {
                Bounds = new Rectangle(0, 0, W, H),
                AutoScroll = true,
                BackColor = BG_DARK,
                Parent = tab
            };

            int y = 16;

            foreach (string playerName in humanPlayers)
            {
                y = BuildPlayerProfile(scroll, playerName, 20, y, W - 40);
                y += 30;
            }
        }

        private int BuildPlayerProfile(Control parent, string playerName, int x, int y, int w)
        {
            // Section header
            new Label
            {
                Text = $"Player Profile  —  {playerName}",
                ForeColor = GOLD,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                Bounds = new Rectangle(x, y, w, 30),
                BackColor = Color.Transparent,
                Parent = parent
            };
            y += 38;

            // ── Stats cards row ───────────────────────────────────────────────
            var profile = db.GetPlayerProfile(playerName);
            if (profile.Rows.Count == 0)
            {
                new Label
                {
                    Text = "No profile data found.",
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 10f, FontStyle.Italic),
                    Bounds = new Rectangle(x, y, w, 28),
                    BackColor = Color.Transparent,
                    Parent = parent
                };
                return y + 36;
            }

            var pr = profile.Rows[0];
            int totalGames = ToInt(pr["TotalGames"]);
            int totalWins = ToInt(pr["TotalWins"]);
            int totalScore = ToInt(pr["TotalScore"]);
            double winRate = Convert.ToDouble(pr["WinRate"]);
            int avgScore = ToInt(pr["AvgScore"]);
            int losses = totalGames - totalWins;

            var statCards = new (string val, string lbl, Color col)[]
            {
                (totalGames.ToString(),  "Games Played",  TEXT_MN),
                (totalWins.ToString(),   "Total Wins",    Color.FromArgb(76, 175, 80)),
                (losses.ToString(),      "Total Losses",  Color.FromArgb(211, 47, 47)),
                ($"{winRate:F1}%",       "Win Rate",      winRate >= 50 ? Color.FromArgb(76,175,80) : Color.FromArgb(211,47,47)),
                (totalScore.ToString(),  "Total Score",   GOLD),
                (avgScore.ToString(),    "Avg Score",     TEXT_MN),
            };

            int cardW = (w - 10 * (statCards.Length - 1)) / statCards.Length;
            int cx2 = x;
            foreach (var (val, lbl, col) in statCards)
            {
                Panel card = new Panel
                {
                    Bounds = new Rectangle(cx2, y, cardW, 72),
                    BackColor = BG_PANEL,
                    Parent = parent
                };
                Rounded(card, 8);
                new Label
                {
                    Text = val,
                    ForeColor = col,
                    Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                    Bounds = new Rectangle(0, 8, cardW, 32),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Parent = card
                };
                new Label
                {
                    Text = lbl,
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 8.5f),
                    Bounds = new Rectangle(0, 42, cardW, 22),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Parent = card
                };
                cx2 += cardW + 10;
            }
            y += 88;

            // ── Charts row ────────────────────────────────────────────────────
            int chartH = 220;
            int halfW = (w - 16) / 2;

            // Win/Loss pie chart
            try
            {
                var pie = new PieChart
                {
                    Bounds = new Rectangle(x, y, halfW, chartH),
                    BackColor = BG_PANEL,
                    Parent = parent,
                    Series = new ISeries[]
                    {
                        new PieSeries<double>
                        {
                            Values = new double[] { totalWins },
                            Name   = "Wins",
                            Fill   = new SolidColorPaint(new SKColor(76, 175, 80))
                        },
                        new PieSeries<double>
                        {
                            Values = new double[] { Math.Max(losses, 0) },
                            Name   = "Losses",
                            Fill   = new SolidColorPaint(new SKColor(211, 47, 47))
                        }
                    },
                    LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom,
                };
                new Label
                {
                    Text = "Win / Loss Ratio",
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Bounds = new Rectangle(x, y + chartH + 2, halfW, 20),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Parent = parent
                };
            }
            catch { }

            // Score history line chart
            try
            {
                var history = db.GetPlayerScoreHistory(playerName);
                var scores = new List<double>();
                var labels = new List<string>();
                int gi = 1;
                foreach (DataRow r in history.Rows)
                {
                    scores.Add(Convert.ToDouble(r["FinalScore"]));
                    labels.Add($"G{gi++}");
                }

                if (scores.Count > 0)
                {
                    var line = new CartesianChart
                    {
                        Bounds = new Rectangle(x + halfW + 16, y, halfW, chartH),
                        BackColor = BG_PANEL,
                        Parent = parent,
                        Series = new ISeries[]
                        {
                            new LineSeries<double>
                            {
                                Values    = scores,
                                Name      = "Score",
                                Stroke    = new SolidColorPaint(new SKColor(255, 215, 0), 2),
                                Fill      = new SolidColorPaint(new SKColor(255, 215, 0, 30)),
                                GeometryFill   = new SolidColorPaint(new SKColor(255, 215, 0)),
                                GeometryStroke = new SolidColorPaint(new SKColor(255, 215, 0)),
                                GeometrySize   = 6
                            }
                        },
                        XAxes = new Axis[]
                        {
                            new Axis { Labels = labels, LabelsPaint = new SolidColorPaint(new SKColor(106,138,116)) }
                        },
                        YAxes = new Axis[]
                        {
                            new Axis { LabelsPaint = new SolidColorPaint(new SKColor(106,138,116)) }
                        }
                    };
                    new Label
                    {
                        Text = "Score History",
                        ForeColor = TEXT_MUT,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        Bounds = new Rectangle(x + halfW + 16, y + chartH + 2, halfW, 20),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent,
                        Parent = parent
                    };
                }
                else
                {
                    new Label
                    {
                        Text = "Not enough game history for score chart.",
                        ForeColor = TEXT_MUT,
                        Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                        Bounds = new Rectangle(x + halfW + 16, y + chartH / 2 - 10, halfW, 24),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent,
                        Parent = parent
                    };
                }
            }
            catch { }

            y += chartH + 28;

            // ── Favorite color bar chart ──────────────────────────────────────
            try
            {
                var colorData = db.GetPlayerFavoriteColors(playerName);
                if (colorData.Rows.Count > 0)
                {
                    var cr = colorData.Rows[0];
                    int red = ToInt(cr["Red"]);
                    int blue = ToInt(cr["Blue"]);
                    int green = ToInt(cr["Green"]);
                    int yell = ToInt(cr["Yellow"]);

                    var bar = new CartesianChart
                    {
                        Bounds = new Rectangle(x, y, halfW, 180),
                        BackColor = BG_PANEL,
                        Parent = parent,
                        Series = new ISeries[]
                        {
                            new ColumnSeries<double>
                            {
                                Values = new double[] { red, blue, green, yell },
                                Name   = "Cards Played",
                                Fill   = new SolidColorPaint(new SKColor(255, 215, 0, 180))
                            }
                        },
                        XAxes = new Axis[]
                        {
                            new Axis
                            {
                                Labels = new[] { "Red", "Blue", "Green", "Yellow" },
                                LabelsPaint = new SolidColorPaint(new SKColor(106,138,116))
                            }
                        },
                        YAxes = new Axis[]
                        {
                            new Axis { LabelsPaint = new SolidColorPaint(new SKColor(106,138,116)) }
                        }
                    };
                    new Label
                    {
                        Text = "Cards Played by Color (All-Time)",
                        ForeColor = TEXT_MUT,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        Bounds = new Rectangle(x, y + 182, halfW, 20),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent,
                        Parent = parent
                    };
                }
            }
            catch { }

            // ── Head to head ─────────────────────────────────────────────────
            try
            {
                var h2h = db.GetHeadToHead(playerName);
                if (h2h.Rows.Count > 0)
                {
                    int hx2 = x + halfW + 16;
                    new Label
                    {
                        Text = "Head-to-Head vs Other Players",
                        ForeColor = TEXT_MUT,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        Bounds = new Rectangle(hx2, y, halfW, 22),
                        BackColor = Color.Transparent,
                        Parent = parent
                    };

                    string[] hCols = { "Opponent", "Games", "Wins", "Losses" };
                    int[] hcw = { halfW - 60 - 60 - 60, 60, 60, 60 };
                    int hy2 = y + 28;
                    hy2 = BuildTableHeader(parent, hCols, hcw, hx2, hy2, halfW);
                    for (int i = 0; i < h2h.Rows.Count; i++)
                    {
                        var row = h2h.Rows[i];
                        string[] vals = {
                            row["Opponent"].ToString(),
                            row["GamesShared"].ToString(),
                            row["Wins"].ToString(),
                            row["Losses"].ToString()
                        };
                        Color[] fc = { TEXT_MN, TEXT_MUT,
                            Color.FromArgb(76,175,80), Color.FromArgb(211,47,47) };
                        hy2 = BuildTableRow(parent, vals, hcw, hx2, hy2, halfW, i, fc);
                    }
                }
            }
            catch { }

            y += 210;

            // ── Divider ───────────────────────────────────────────────────────
            new Panel
            {
                Bounds = new Rectangle(x, y, w, 1),
                BackColor = Color.FromArgb(30, 55, 35),
                Parent = parent
            };
            return y + 8;
        }

        // ── Table helpers ─────────────────────────────────────────────────────
        private int BuildTableHeader(Control parent, string[] cols, int[] cw,
                                     int x, int y, int w)
        {
            Panel hdr = new Panel
            {
                Bounds = new Rectangle(x, y, w, 30),
                BackColor = Color.FromArgb(12, 26, 16),
                Parent = parent
            };
            int cx = 10;
            for (int i = 0; i < cols.Length; i++)
            {
                new Label
                {
                    Text = cols[i],
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Bounds = new Rectangle(cx, 0, cw[i], 30),
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                    Parent = hdr
                };
                cx += cw[i];
            }
            return y + 32;
        }

        private int BuildTableRow(Control parent, string[] vals, int[] cw,
                                  int x, int y, int w, int rowIndex, Color[] foreColors)
        {
            Panel row = new Panel
            {
                Bounds = new Rectangle(x, y, w, 36),
                BackColor = rowIndex % 2 == 0 ? BG_ROW_A : BG_ROW_B,
                Parent = parent
            };
            int cx = 10;
            for (int i = 0; i < vals.Length; i++)
            {
                new Label
                {
                    Text = vals[i],
                    ForeColor = i < foreColors.Length ? foreColors[i] : TEXT_MN,
                    Font = new Font("Segoe UI", 9.5f),
                    Bounds = new Rectangle(cx, 0, cw[i], 36),
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                    Parent = row
                };
                cx += cw[i];
            }
            return y + 38;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private int ToInt(object val)
        {
            if (val == null || val == DBNull.Value) return 0;
            return Convert.ToInt32(val);
        }

        private Button MakeBtn(string text, int x, int y, int w, int h)
        {
            var btn = new Button
            {
                Text = text,
                Bounds = new Rectangle(x, y, w, h),
                BackColor = Color.FromArgb(30, 45, 36),
                ForeColor = TEXT_MUT,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = BTN_BRD;
            btn.FlatAppearance.BorderSize = 1;
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
}
