using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using UNOFinal.Models;

namespace UNOFinal
{
    public class FormGuide : Form
    {
        private readonly Color BG_DARK = Color.FromArgb(19, 32, 24);
        private readonly Color GOLD = Color.FromArgb(255, 215, 0);
        private readonly Color TEXT_MN = Color.FromArgb(224, 224, 224);
        private readonly Color TEXT_MUT = Color.FromArgb(106, 138, 116);
        private readonly Color RED = Color.FromArgb(211, 47, 47);
        private readonly Color BLUE = Color.FromArgb(25, 118, 210);
        private readonly Color GREEN = Color.FromArgb(46, 125, 50);
        private readonly Color YELLOW = Color.FromArgb(245, 124, 0);
        private readonly Color DARK_GRAY = Color.FromArgb(33, 33, 33);

        private readonly Dictionary<CardColor, Color> CARD_CLR = new Dictionary<CardColor, Color>
        {
            { CardColor.Red,    Color.FromArgb(211, 47, 47) },
            { CardColor.Blue,   Color.FromArgb(25, 118, 210) },
            { CardColor.Green,  Color.FromArgb(46, 125, 50) },
            { CardColor.Yellow, Color.FromArgb(245, 124, 0) },
            { CardColor.Wild,   Color.FromArgb(33, 33, 33) }
        };

        public FormGuide()
        {
            SetupForm();
            this.Load += (s, e) => BuildUI();
        }

        private void SetupForm()
        {
            this.Text = "How to Play UNO";
            this.BackColor = BG_DARK;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
        }

        private void BuildUI()
        {
            this.Controls.Clear();

            // Top bar
            Panel topBar = new Panel
            {
                Bounds = new Rectangle(0, 0, this.ClientSize.Width, 50),
                BackColor = Color.FromArgb(10, 24, 16)
            };

            Button btnClose = new Button
            {
                Text = "✕",
                Bounds = new Rectangle(this.ClientSize.Width - 50, 5, 40, 40),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(139, 196, 160),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            Label lblTitle = new Label
            {
                Text = "HOW TO PLAY UNO",
                ForeColor = GOLD,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Bounds = new Rectangle(20, 5, 300, 40),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            topBar.Controls.Add(btnClose);
            topBar.Controls.Add(lblTitle);
            this.Controls.Add(topBar);

            // Scroll panel
            Panel scrollPanel = new Panel
            {
                Bounds = new Rectangle(0, 50, this.ClientSize.Width, this.ClientSize.Height - 50),
                AutoScroll = true,
                BackColor = BG_DARK
            };
            this.Controls.Add(scrollPanel);

            int y = 20;
            int contentWidth = scrollPanel.Width - 40;

            // Title emoji
            Label lblEmoji = new Label
            {
                Text = "🎮",
                ForeColor = GOLD,
                Font = new Font("Segoe UI", 48),
                Bounds = new Rectangle((contentWidth - 400) / 2, y, 400, 85),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            scrollPanel.Controls.Add(lblEmoji);
            y += 80;

            // Objective
            y = AddSection(scrollPanel, y, contentWidth, "🎯 OBJECTIVE",
                "Be the first player to empty your hand. The winner earns points\nbased on cards left in opponents' hands.");

            // Card Types with actual drawn cards
            y = AddCardTypesSection(scrollPanel, y, contentWidth);

            // Basic Rules
            y = AddSection(scrollPanel, y, contentWidth, "📜 BASIC RULES",
                "• Match the top card by COLOR, NUMBER, or SYMBOL\n" +
                "• If you can't play, DRAW a card — if it's playable, you may play it\n" +
                "• The game automatically calls UNO when you have ONE card left\n" +
                "• First to empty their hand WINS the round", 85);

            // Action Cards
            y = AddActionCardsSection(scrollPanel, y, contentWidth);

            // AI Difficulty
            y = AddSection(scrollPanel, y, contentWidth, "🤖 AI DIFFICULTY LEVELS",
                "• EASY: Plays the first valid card\n" +
                "• MEDIUM: Prefers action cards over number cards\n" +
                "• HARD: Strategic — tracks colors, saves Wild cards, plays optimally", 90);

            // Scoring
            y = AddScoringSection(scrollPanel, y, contentWidth);

            scrollPanel.AutoScrollMinSize = new Size(0, y + 50);
        }

        private int AddSection(Panel parent, int y, int width, string title, string content, int contentHeight = 70)
        {
            Label lblTitle = new Label
            {
                Text = title,
                ForeColor = GOLD,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Bounds = new Rectangle(20, y, width - 40, 30),
                BackColor = Color.Transparent
            };
            parent.Controls.Add(lblTitle);

            Label lblContent = new Label
            {
                Text = content,
                ForeColor = TEXT_MN,
                Font = new Font("Segoe UI", 10),
                Bounds = new Rectangle(35, y + 35, width - 70, contentHeight),
                BackColor = Color.Transparent
            };
            parent.Controls.Add(lblContent);

            
            return y + 30 + contentHeight + 25;
        }

        private int AddCardTypesSection(Panel parent, int y, int width)
        {
            Label lblTitle = new Label
            {
                Text = "🃏 CARD TYPES",
                ForeColor = GOLD,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Bounds = new Rectangle(20, y, width - 40, 30),
                BackColor = Color.Transparent
            };
            parent.Controls.Add(lblTitle);
            y += 45;

            
            var cards = new (Card card, string name, string displayText)[]
            {
                (new Card(CardColor.Green, CardType.Number, 7), "Number", "7"),
                (new Card(CardColor.Blue, CardType.Skip), "Skip", "SK"),
                (new Card(CardColor.Yellow, CardType.Reverse), "Reverse", "RV"),
                (new Card(CardColor.Red, CardType.DrawTwo), "Draw Two", "+2"),
                (new Card(CardColor.Wild, CardType.Wild), "Wild", "W"),
                (new Card(CardColor.Wild, CardType.WildDrawFour), "Wild +4", "W+4")
            };

            int cardW = 90;
            int cardH = 110;
            int startX = (width - (cards.Length * cardW + (cards.Length - 1) * 15)) / 2;
            if (startX < 20) startX = 20;

            for (int i = 0; i < cards.Length; i++)
            {
                int x = startX + i * (cardW + 15);
                var cardInfo = cards[i];

                //always use PictureBox for card! 
                PictureBox cardPic = new PictureBox
                {
                    Bounds = new Rectangle(x, y, cardW, cardH),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Transparent
                };

                
                cardPic.Paint += (s, e) =>
                {
                    DrawGuideCard(e.Graphics, 0, 0, cardW, cardH, cardInfo.card, cardInfo.displayText);
                };
                parent.Controls.Add(cardPic);

                
                Label nameLabel = new Label
                {
                    Text = cardInfo.name,
                    ForeColor = TEXT_MUT,
                    Font = new Font("Segoe UI", 8),
                    Bounds = new Rectangle(x, y + cardH + 5, cardW, 20),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };
                parent.Controls.Add(nameLabel);
            }

            return y + cardH + 40;
        }

        private void DrawGuideCard(Graphics g, int x, int y, int w, int h, Card card, string label)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            
            using (var shadow = new SolidBrush(Color.FromArgb(48, 0, 0, 0)))
            {
                var shadowPath = RoundedPath(new Rectangle(x + 2, y + 2, w, h), 6);
                g.FillPath(shadow, shadowPath);
            }

            
            Color cardColor = card.IsWild() ? DARK_GRAY : CARD_CLR[card.Color];
            using (var brush = new SolidBrush(cardColor))
            {
                var cardPath = RoundedPath(new Rectangle(x, y, w, h), 6);
                g.FillPath(brush, cardPath);
            }

            
            using (var innerBrush = new SolidBrush(Color.FromArgb(22, 255, 255, 255)))
            {
                var innerPath = RoundedPath(new Rectangle(x + 3, y + 3, w - 6, h - 6), 4);
                g.FillPath(innerBrush, innerPath);
            }

            
            using (var pen = new Pen(Color.FromArgb(110, 255, 255, 255), 1.5f))
            {
                var borderPath = RoundedPath(new Rectangle(x, y, w, h), 6);
                g.DrawPath(pen, borderPath);
            }

            
            if (card.IsWild())
            {
                var wildColors = new[] { RED, BLUE, GREEN, YELLOW };
                int hw = w / 2, hh = h / 2;
                g.SetClip(RoundedPath(new Rectangle(x, y, w, h), 6));
                using (var brush = new SolidBrush(wildColors[0])) g.FillRectangle(brush, x, y, hw, hh);
                using (var brush = new SolidBrush(wildColors[1])) g.FillRectangle(brush, x + hw, y, hw, hh);
                using (var brush = new SolidBrush(wildColors[2])) g.FillRectangle(brush, x, y + hh, hw, hh);
                using (var brush = new SolidBrush(wildColors[3])) g.FillRectangle(brush, x + hw, y + hh, hw, hh);
                g.ResetClip();

                
                using (var brush = new SolidBrush(Color.FromArgb(155, 0, 0, 0)))
                    g.FillEllipse(brush, x + 7, y + 11, w - 14, h - 22);
            }

            
            float fontSize = label.Length > 2 ? (w > 60 ? 13f : 10f) : (w > 60 ? 26f : 20f);
            using (var font = new Font("Segoe UI", fontSize, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                var sz = g.MeasureString(label, font);
                g.DrawString(label, font, brush, x + (w - sz.Width) / 2, y + (h - sz.Height) / 2);
            }

            
            using (var font = new Font("Segoe UI", w > 60 ? 8f : 6f, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString(label, font, brush, x + 4, y + 4);
                g.DrawString(label, font, brush, x + w - 16, y + h - 14);
            }
        }

        private int AddActionCardsSection(Panel parent, int y, int width)
        {
            Label lblTitle = new Label
            {
                Text = "⚡ ACTION CARDS EFFECTS",
                ForeColor = GOLD,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Bounds = new Rectangle(20, y, width - 40, 30),
                BackColor = Color.Transparent
            };
            parent.Controls.Add(lblTitle);
            y += 35;

            string[] effects = {
                "SKIP: Next player loses their turn",
                "REVERSE: Direction of play reverses",
                "DRAW TWO (+2): Next player draws 2 cards and loses turn",
                "WILD: Choose the next color to play",
                "WILD DRAW FOUR (+4): Choose color + next player draws 4 cards"
            };

            for (int i = 0; i < effects.Length; i++)
            {
                Label lblEffect = new Label
                {
                    Text = "   • " + effects[i],
                    ForeColor = TEXT_MN,
                    Font = new Font("Segoe UI", 10),
                    Bounds = new Rectangle(35, y + i * 28, width - 70, 26),
                    BackColor = Color.Transparent
                };
                parent.Controls.Add(lblEffect);
            }

            return y + effects.Length * 28 + 20;
        }

        private int AddScoringSection(Panel parent, int y, int width)
        {
            Label lblTitle = new Label
            {
                Text = "🏆 SCORING SYSTEM",
                ForeColor = GOLD,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Bounds = new Rectangle(20, y, width - 40, 30),
                BackColor = Color.Transparent
            };
            parent.Controls.Add(lblTitle);
            y += 35;

            string[] scoring = {
                "Number cards (0-9): Face value (0-9 points)",
                "Skip, Reverse, Draw Two: 20 points each",
                "Wild, Wild Draw Four: 50 points each",
                "Winner gets points from ALL cards left in opponents' hands"
            };

            for (int i = 0; i < scoring.Length; i++)
            {
                Label lblScore = new Label
                {
                    Text = "   • " + scoring[i],
                    ForeColor = TEXT_MN,
                    Font = new Font("Segoe UI", 10),
                    Bounds = new Rectangle(35, y + i * 28, width - 70, 26),
                    BackColor = Color.Transparent
                };
                parent.Controls.Add(lblScore);
            }

            return y + scoring.Length * 28 + 20;
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
    }
}