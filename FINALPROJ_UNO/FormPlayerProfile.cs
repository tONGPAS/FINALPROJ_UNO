using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using UNOFinal.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace UNOFinal
{
    public class FormPlayerProfile : Form
    {
        private readonly Color BG_DARK = Color.FromArgb(19, 32, 24);
        private readonly Color BG_BAR = Color.FromArgb(10, 24, 16);
        private readonly Color BG_PANEL = Color.FromArgb(24, 38, 28);
        private readonly Color BG_ROW_A = Color.FromArgb(28, 44, 32);
        private readonly Color BG_ROW_B = Color.FromArgb(22, 36, 26);
        private readonly Color GOLD = Color.FromArgb(255, 215, 0);
        private readonly Color GREEN_C = Color.FromArgb(76, 175, 80);
        private readonly Color RED_C = Color.FromArgb(211, 47, 47);
        private readonly Color BLUE_C = Color.FromArgb(25, 118, 210);
        private readonly Color YELLOW_C = Color.FromArgb(245, 124, 0);
        private readonly Color TEXT_MN = Color.FromArgb(224, 224, 224);
        private readonly Color TEXT_MUT = Color.FromArgb(106, 138, 116);
        private readonly Color BTN_BRD = Color.FromArgb(58, 90, 68);

        private DatabaseManager db = new DatabaseManager();
        private string playerName;

        public FormPlayerProfile(string playerName)
        {
            this.playerName = playerName;
            SetupForm();
            this.Load += (s, e) => BuildUI();
        }

        private void SetupForm()
        {
            this.Text = $"Player Profile - {playerName}";
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

            //Top bar
            Panel pnlBar = new Panel { Bounds = new Rectangle(0, 0, W, 50), BackColor = BG_BAR };
            Button btnBack = MakeBtn("Back", 12, 8, 80, 34);
            btnBack.Click += (s, e) => this.Close();

            new Label
            {
                Text = $"Player Profile  —  {playerName}",
                ForeColor = GOLD,
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                Bounds = new Rectangle(106, 0, W - 200, 50),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Parent = pnlBar
            };
            pnlBar.Controls.Add(btnBack);
            this.Controls.Add(pnlBar);

            // ── Scrollable content ────────────────────────────────────────────
            Panel scroll = new Panel
            {
                Bounds = new Rectangle(0, 50, W, H - 50),
                AutoScroll = true,
                BackColor = BG_DARK
            };
            this.Controls.Add(scroll);

            int pad = 30;
            int cw = W - pad * 2;
            int y = 20;

            // ── AVATAR SECTION ────────────────────────────────────────────────
            int avatarId = db.GetPlayerAvatar(playerName);
            PictureBox avatar = new PictureBox
            {
                Bounds = new Rectangle(pad, y, 80, 80),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = AvatarDrawer.ToBitmap(avatarId, 80),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Tag = playerName
            };
            // Click avatar to change it
            avatar.Click += (s, ev) =>
            {
                string pname = avatar.Tag.ToString();
                int currentId = db.GetPlayerAvatar(pname);
                var picker = new AvatarPickerForm(pname, currentId);
                if (picker.ShowDialog() == DialogResult.OK)
                {
                    avatar.Image = AvatarDrawer.ToBitmap(picker.SelectedAvatarId, 80);
                    // Refresh the profile to show updated stats if needed
                    BuildUI();
                }
            };
            scroll.Controls.Add(avatar);

            // Player name label next to avatar
            Label lblPlayerName = new Label
            {
                Text = playerName,
                ForeColor = GOLD,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Bounds = new Rectangle(pad + 100, y + 20, cw - 120, 40),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            scroll.Controls.Add(lblPlayerName);

            y += 100;  // Move down after avatar section
            // ───────────────────────────────────────────────────────────────────

            // ── Stat cards ────────────────────────────────────────────────────
            var profile = db.GetPlayerProfile(playerName);
            if (profile.Rows.Count > 0)
            {
                var pr = profile.Rows[0];
                int games = ToInt(pr["TotalGames"]);
                int wins = ToInt(pr["TotalWins"]);
                int losses = games - wins;
                double winRate = Convert.ToDouble(pr["WinRate"]);
                int totalScore = ToInt(pr["TotalScore"]);
                int avgScore = ToInt(pr["AvgScore"]);
                string since = Convert.ToDateTime(pr["CreatedAt"]).ToString("MMM dd, yyyy");

                var cards = new (string val, string lbl, Color col)[]
                {
                    (games.ToString(),       "Games Played",  TEXT_MN),
                    (wins.ToString(),        "Total Wins",    GREEN_C),
                    (losses.ToString(),      "Total Losses",  RED_C),
                    ($"{winRate:F1}%",       "Win Rate",      winRate >= 50 ? GREEN_C : RED_C),
                    (totalScore.ToString(),  "Total Score",   GOLD),
                    (avgScore.ToString(),    "Avg Score / Game", TEXT_MN),
                    (since,                  "Playing Since", TEXT_MUT),
                };

                int cardW = (cw - 12 * (cards.Length - 1)) / cards.Length;
                int cx = pad;
                foreach (var (val, lbl, col) in cards)
                {
                    Panel card = new Panel
                    {
                        Bounds = new Rectangle(cx, y, cardW, 76),
                        BackColor = BG_PANEL,
                        Parent = scroll
                    };
                    Rounded(card, 8);
                    new Label
                    {
                        Text = val,
                        ForeColor = col,
                        Font = new Font("Segoe UI", val.Length > 6 ? 11f : 16f, FontStyle.Bold),
                        Bounds = new Rectangle(0, 8, cardW, 34),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent,
                        Parent = card
                    };
                    new Label
                    {
                        Text = lbl,
                        ForeColor = TEXT_MUT,
                        Font = new Font("Segoe UI", 8f),
                        Bounds = new Rectangle(0, 44, cardW, 24),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent,
                        Parent = card
                    };
                    cx += cardW + 12;
                }
                y += 92;

                // ── Charts row 1: Win/Loss pie + Score history line ───────────
                int chartH = 260;
                int half = (cw - 20) / 2;

                // Win/Loss Pie
                Panel piePnl = new Panel
                {
                    Bounds = new Rectangle(pad, y, half, chartH),
                    BackColor = BG_PANEL,
                    Parent = scroll
                };
                Rounded(piePnl, 10);
                new Label
                {
                    Text = "Win / Loss Ratio",
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Bounds = new Rectangle(0, 6, half, 22),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Parent = piePnl
                };

                try
                {
                    var pie = new PieChart
                    {
                        Bounds = new Rectangle(10, 26, half - 20, chartH - 36),
                        BackColor = Color.Transparent,
                        Parent = piePnl,
                        Series = new ISeries[]
                        {
                            new PieSeries<double>
                            {
                                Values     = new double[] { Math.Max(wins, 0.01) },
                                Name       = $"Wins ({wins})",
                                Fill       = new SolidColorPaint(new SKColor(76, 175, 80)),
                                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                                DataLabelsSize  = 14,
                            },
                            new PieSeries<double>
                            {
                                Values     = new double[] { Math.Max(losses, 0.01) },
                                Name       = $"Losses ({losses})",
                                Fill       = new SolidColorPaint(new SKColor(211, 47, 47)),
                                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                                DataLabelsSize  = 14,
                            }
                        },
                        LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom,
                        LegendTextPaint = new SolidColorPaint(new SKColor(139, 196, 160)),
                    };
                }
                catch (Exception ex)
                {
                    new Label
                    {
                        Text = $"Chart error: {ex.Message}",
                        ForeColor = TEXT_MUT,
                        Font = new Font("Segoe UI", 8f),
                        AutoSize = true,
                        Location = new Point(10, 40),
                        BackColor = Color.Transparent,
                        Parent = piePnl
                    };
                }

                // Score History Line
                Panel linePnl = new Panel
                {
                    Bounds = new Rectangle(pad + half + 20, y, half, chartH),
                    BackColor = BG_PANEL,
                    Parent = scroll
                };
                Rounded(linePnl, 10);
                new Label
                {
                    Text = "Score History (All Games)",
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Bounds = new Rectangle(0, 6, half, 22),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Parent = linePnl
                };

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

                    if (scores.Count >= 1)
                    {
                        new CartesianChart
                        {
                            Bounds = new Rectangle(10, 26, half - 20, chartH - 36),
                            BackColor = Color.Transparent,
                            Parent = linePnl,
                            Series = new ISeries[]
                            {
                                new LineSeries<double>
                                {
                                    Values         = scores,
                                    Name           = "Score",
                                    Stroke         = new SolidColorPaint(new SKColor(255, 215, 0), 3),
                                    Fill           = new SolidColorPaint(new SKColor(255, 215, 0, 40)),
                                    GeometryFill   = new SolidColorPaint(new SKColor(255, 215, 0)),
                                    GeometryStroke = new SolidColorPaint(new SKColor(200, 160, 0), 2),
                                    GeometrySize   = 8,
                                    DataLabelsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                                    DataLabelsSize  = 11,
                                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                                }
                            },
                            XAxes = new Axis[]
                            {
                                new Axis
                                {
                                    Labels      = labels,
                                    LabelsPaint = new SolidColorPaint(new SKColor(106, 138, 116)),
                                    TicksPaint  = new SolidColorPaint(new SKColor(50, 80, 50)),
                                }
                            },
                            YAxes = new Axis[]
                            {
                                new Axis
                                {
                                    LabelsPaint = new SolidColorPaint(new SKColor(106, 138, 116)),
                                    TicksPaint  = new SolidColorPaint(new SKColor(50, 80, 50)),
                                }
                            },
                            LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden,
                        };
                    }
                    else
                    {
                        new Label
                        {
                            Text = "Play more games to see score history.",
                            ForeColor = TEXT_MUT,
                            Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                            Bounds = new Rectangle(10, chartH / 2 - 10, half - 20, 30),
                            TextAlign = ContentAlignment.MiddleCenter,
                            BackColor = Color.Transparent,
                            Parent = linePnl
                        };
                    }
                }
                catch (Exception ex)
                {
                    new Label
                    {
                        Text = $"Chart error: {ex.Message}",
                        ForeColor = TEXT_MUT,
                        Font = new Font("Segoe UI", 8f),
                        AutoSize = true,
                        Location = new Point(10, 40),
                        BackColor = Color.Transparent,
                        Parent = linePnl
                    };
                }

                y += chartH + 20;

                // ── Charts row 2: Color bar + Action card bar ─────────────────
                int chartH2 = 240;

                // Color bar chart
                Panel colorPnl = new Panel
                {
                    Bounds = new Rectangle(pad, y, half, chartH2),
                    BackColor = BG_PANEL,
                    Parent = scroll
                };
                Rounded(colorPnl, 10);
                new Label
                {
                    Text = "Cards Played by Color",
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Bounds = new Rectangle(0, 6, half, 22),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Parent = colorPnl
                };

                try
                {
                    var colorData = db.GetPlayerFavoriteColors(playerName);
                    if (colorData.Rows.Count > 0)
                    {
                        var cr = colorData.Rows[0];
                        new CartesianChart
                        {
                            Bounds = new Rectangle(10, 26, half - 20, chartH2 - 36),
                            BackColor = Color.Transparent,
                            Parent = colorPnl,
                            Series = new ISeries[]
                            {
                                new ColumnSeries<double>
                                {
                                    Values = new double[]
                                    {
                                        ToInt(cr["Red"]), ToInt(cr["Blue"]),
                                        ToInt(cr["Green"]), ToInt(cr["Yellow"])
                                    },
                                    Name             = "Cards",
                                    DataLabelsPaint  = new SolidColorPaint(SKColors.White),
                                    DataLabelsSize   = 12,
                                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                                    Fill             = new LinearGradientPaint(
                                        new SKColor(255, 215, 0, 200),
                                        new SKColor(255, 152, 0, 200))
                                }
                            },
                            XAxes = new Axis[]
                            {
                                new Axis
                                {
                                    Labels      = new[] { "Red", "Blue", "Green", "Yellow" },
                                    LabelsPaint = new SolidColorPaint(new SKColor(106, 138, 116)),
                                }
                            },
                            YAxes = new Axis[]
                            {
                                new Axis { LabelsPaint = new SolidColorPaint(new SKColor(106, 138, 116)) }
                            },
                            LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden,
                        };
                    }
                }
                catch (Exception ex)
                {
                    new Label
                    {
                        Text = $"Chart error: {ex.Message}",
                        ForeColor = TEXT_MUT,
                        Font = new Font("Segoe UI", 8f),
                        AutoSize = true,
                        Location = new Point(10, 40),
                        BackColor = Color.Transparent,
                        Parent = colorPnl
                    };
                }

                // Action card breakdown
                Panel actionPnl = new Panel
                {
                    Bounds = new Rectangle(pad + half + 20, y, half, chartH2),
                    BackColor = BG_PANEL,
                    Parent = scroll
                };
                Rounded(actionPnl, 10);
                new Label
                {
                    Text = "Action Cards Used",
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Bounds = new Rectangle(0, 6, half, 22),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Parent = actionPnl
                };

                try
                {
                    var actionData = db.GetPlayerActionCards(playerName);
                    if (actionData.Rows.Count > 0)
                    {
                        var ar = actionData.Rows[0];
                        new CartesianChart
                        {
                            Bounds = new Rectangle(10, 26, half - 20, chartH2 - 36),
                            BackColor = Color.Transparent,
                            Parent = actionPnl,
                            Series = new ISeries[]
                            {
                                new ColumnSeries<double>
                                {
                                    Values = new double[]
                                    {
                                        ToInt(ar["Skips"]),   ToInt(ar["Reverses"]),
                                        ToInt(ar["DrawTwos"]), ToInt(ar["WildDrawFours"]),
                                        ToInt(ar["Wilds"]),   ToInt(ar["Numbers"])
                                    },
                                    Name            = "Used",
                                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                                    DataLabelsSize  = 11,
                                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                                    Fill            = new LinearGradientPaint(
                                        new SKColor(33, 150, 243, 200),
                                        new SKColor(46, 125, 50, 200))
                                }
                            },
                            XAxes = new Axis[]
                            {
                                new Axis
                                {
                                    Labels      = new[] { "Skip", "Reverse", "+2", "+4", "Wild", "Numbers" },
                                    LabelsPaint = new SolidColorPaint(new SKColor(106, 138, 116)),
                                }
                            },
                            YAxes = new Axis[]
                            {
                                new Axis { LabelsPaint = new SolidColorPaint(new SKColor(106, 138, 116)) }
                            },
                            LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden,
                        };
                    }
                }
                catch (Exception ex)
                {
                    new Label
                    {
                        Text = $"Chart error: {ex.Message}",
                        ForeColor = TEXT_MUT,
                        Font = new Font("Segoe UI", 8f),
                        AutoSize = true,
                        Location = new Point(10, 40),
                        BackColor = Color.Transparent,
                        Parent = actionPnl
                    };
                }

                y += chartH2 + 20;

                // ── Head-to-head table ────────────────────────────────────────
                Panel h2hPnl = new Panel
                {
                    Bounds = new Rectangle(pad, y, cw, 200),
                    BackColor = BG_PANEL,
                    Parent = scroll
                };
                Rounded(h2hPnl, 10);
                new Label
                {
                    Text = "Head-to-Head Record vs Other Players",
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Bounds = new Rectangle(12, 8, cw - 24, 24),
                    BackColor = Color.Transparent,
                    Parent = h2hPnl
                };

                try
                {
                    var h2h = db.GetHeadToHead(playerName);
                    if (h2h.Rows.Count > 0)
                    {
                        int[] hw = { cw - 120 - 120 - 120 - 24, 120, 120, 120 };
                        string[] hc = { "Opponent", "Games Together", "Wins", "Losses" };
                        int hy2 = 38;
                        // Header
                        Panel hdr = new Panel
                        {
                            Bounds = new Rectangle(12, hy2, cw - 24, 28),
                            BackColor = Color.FromArgb(12, 26, 16),
                            Parent = h2hPnl
                        };
                        int hx2 = 8;
                        for (int i = 0; i < hc.Length; i++)
                        {
                            new Label
                            {
                                Text = hc[i],
                                ForeColor = TEXT_MUT,
                                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                                Bounds = new Rectangle(hx2, 0, hw[i], 28),
                                TextAlign = ContentAlignment.MiddleLeft,
                                BackColor = Color.Transparent,
                                Parent = hdr
                            };
                            hx2 += hw[i];
                        }
                        hy2 += 30;

                        for (int i = 0; i < h2h.Rows.Count; i++)
                        {
                            var row = h2h.Rows[i];
                            int shared = ToInt(row["GamesShared"]);
                            int w2 = ToInt(row["Wins"]);
                            int l2 = ToInt(row["Losses"]);

                            Panel rp = new Panel
                            {
                                Bounds = new Rectangle(12, hy2, cw - 24, 34),
                                BackColor = i % 2 == 0 ? BG_ROW_A : BG_ROW_B,
                                Parent = h2hPnl
                            };
                            string[] vals = { row["Opponent"].ToString(),
                                             shared.ToString(), w2.ToString(), l2.ToString() };
                            Color[] fc = { TEXT_MN, TEXT_MUT, GREEN_C, RED_C };
                            int rx2 = 8;
                            for (int j = 0; j < vals.Length; j++)
                            {
                                new Label
                                {
                                    Text = vals[j],
                                    ForeColor = fc[j],
                                    Font = new Font("Segoe UI", 10f, j == 0 ? FontStyle.Regular : FontStyle.Bold),
                                    Bounds = new Rectangle(rx2, 0, hw[j], 34),
                                    TextAlign = ContentAlignment.MiddleLeft,
                                    BackColor = Color.Transparent,
                                    Parent = rp
                                };
                                rx2 += hw[j];
                            }
                            hy2 += 36;
                        }
                        h2hPnl.Height = hy2 + 12;
                    }
                    else
                    {
                        new Label
                        {
                            Text = "No head-to-head data yet. Play against other human players!",
                            ForeColor = TEXT_MUT,
                            Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                            Bounds = new Rectangle(12, 40, cw - 24, 30),
                            TextAlign = ContentAlignment.MiddleCenter,
                            BackColor = Color.Transparent,
                            Parent = h2hPnl
                        };
                    }
                }
                catch { }

                y += h2hPnl.Height + 30;
            }
            else
            {
                new Label
                {
                    Text = $"No profile data found for {playerName}.",
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 12f, FontStyle.Italic),
                    Bounds = new Rectangle(pad, y, cw, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Parent = scroll
                };
            }
        }

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