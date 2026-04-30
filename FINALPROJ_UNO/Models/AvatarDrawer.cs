using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace UNOFinal.Models
{
    public static class AvatarDrawer
    {
        //this order 1 -> 2  -> 3 -> 4
        //           5 -> .....
        public static readonly string[] AvatarNames = {
            "Spade",   "Heart",   "Diamond", "Club",     
            "Cat",     "Dog",     "Fox",     "Frog",     
            "Fire",    "Thunder", "Water",   "Leaf",     
            "Crown",   "Star",    "Joystick","Dice"      
        };

        public static readonly Color[] AvatarColors = {
            Color.FromArgb(60,  60,  80),   //spade    
            Color.FromArgb(200, 50,  60),   //heart    
            Color.FromArgb(50,  130, 200),  //diamond  
            Color.FromArgb(50,  140, 80),   //club     
            Color.FromArgb(255, 140, 0),    //cat      
            Color.FromArgb(120, 80,  50),   //dog      
            Color.FromArgb(220, 80,  30),   //fox      
            Color.FromArgb(40,  160, 80),   //frog     
            Color.FromArgb(220, 60,  30),   //fire     
            Color.FromArgb(80,  120, 220),  //thunder  
            Color.FromArgb(30,  140, 200),  //water    
            Color.FromArgb(60,  160, 70),   //leaf     
            Color.FromArgb(180, 130, 20),   //crown    
            Color.FromArgb(150, 50,  180),  //star     
            Color.FromArgb(40,  100, 180),  //joystick 
            Color.FromArgb(160, 80,  40),   //dice   
        };

        
        //NOTE!! avatarId is 1-based (1-16)
        public static void Draw(Graphics g, int avatarId, int x, int y, int size)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            int idx = Math.Max(0, Math.Min(15, avatarId - 1));
            Color bg = AvatarColors[idx];

            
            DrawCircle(g, x, y, size, bg);

            
            switch (idx)
            {
                case 0: DrawSpade(g, x, y, size); break;
                case 1: DrawHeart(g, x, y, size); break;
                case 2: DrawDiamond(g, x, y, size); break;
                case 3: DrawClub(g, x, y, size); break;
                case 4: DrawCat(g, x, y, size); break;
                case 5: DrawDog(g, x, y, size); break;
                case 6: DrawFox(g, x, y, size); break;
                case 7: DrawFrog(g, x, y, size); break;
                case 8: DrawFire(g, x, y, size); break;
                case 9: DrawThunder(g, x, y, size); break;
                case 10: DrawWater(g, x, y, size); break;
                case 11: DrawLeaf(g, x, y, size); break;
                case 12: DrawCrown(g, x, y, size); break;
                case 13: DrawStar(g, x, y, size); break;
                case 14: DrawJoystick(g, x, y, size); break;
                case 15: DrawDice(g, x, y, size); break;
            }
        }

        
        public static Bitmap ToBitmap(int avatarId, int size)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
                Draw(g, avatarId, 0, 0, size);
            return bmp;
        }

        
        private static void DrawCircle(Graphics g, int x, int y, int size, Color bg)
        {
            
            using (var b = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                g.FillEllipse(b, x + 2, y + 3, size, size);

            using (var b = new SolidBrush(bg))
                g.FillEllipse(b, x, y, size, size);

            
            using (var b = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                g.FillEllipse(b, x + size / 6, y + size / 8, size / 2, size / 3);

            
            using (var p = new Pen(Color.FromArgb(80, 255, 255, 255), 1.5f))
                g.DrawEllipse(p, x, y, size, size);
        }

        
        private static void DrawSymbol(Graphics g, int x, int y, int size,
                                       string symbol, float fontSize)
        {
            using (var f = new Font("Segoe UI Symbol", fontSize, FontStyle.Bold))
            using (var b = new SolidBrush(Color.White))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(symbol, f, b,
                    new RectangleF(x, y, size, size), sf);
            }
        }

        private static void DrawText(Graphics g, int x, int y, int size,
                                     string text, float fontSize)
        {
            using (var f = new Font("Segoe UI", fontSize, FontStyle.Bold))
            using (var b = new SolidBrush(Color.White))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(text, f, b,
                    new RectangleF(x, y, size, size), sf);
            }
        }

        // ── CARD SUITS ────────────────────────────────────────────────────────
        private static void DrawSpade(Graphics g, int x, int y, int size)
            => DrawSymbol(g, x, y, size, "♠", size * 0.42f);

        private static void DrawHeart(Graphics g, int x, int y, int size)
            => DrawSymbol(g, x, y, size, "♥", size * 0.42f);

        private static void DrawDiamond(Graphics g, int x, int y, int size)
            => DrawSymbol(g, x, y, size, "♦", size * 0.42f);

        private static void DrawClub(Graphics g, int x, int y, int size)
            => DrawSymbol(g, x, y, size, "♣", size * 0.42f);

        // ── ANIMALS ───────────────────────────────────────────────────────────
        private static void DrawCat(Graphics g, int x, int y, int size)
        {
            int cx = x + size / 2;
            int cy = y + size / 2;
            float r = size * 0.28f;

            // Face
            using (var b = new SolidBrush(Color.FromArgb(255, 200, 120)))
                g.FillEllipse(b, cx - r, cy - r * 0.9f, r * 2, r * 1.8f);

            // Ears
            using (var b = new SolidBrush(Color.FromArgb(255, 200, 120)))
            {
                var earL = new PointF[] {
                    new PointF(cx - r * 0.7f, cy - r * 0.8f),
                    new PointF(cx - r * 1.1f, cy - r * 1.6f),
                    new PointF(cx - r * 0.1f, cy - r * 0.9f)
                };
                var earR = new PointF[] {
                    new PointF(cx + r * 0.7f, cy - r * 0.8f),
                    new PointF(cx + r * 1.1f, cy - r * 1.6f),
                    new PointF(cx + r * 0.1f, cy - r * 0.9f)
                };
                g.FillPolygon(b, earL);
                g.FillPolygon(b, earR);
            }

            // Eyes
            using (var b = new SolidBrush(Color.FromArgb(60, 40, 80)))
            {
                g.FillEllipse(b, cx - r * 0.55f, cy - r * 0.2f, r * 0.35f, r * 0.35f);
                g.FillEllipse(b, cx + r * 0.2f, cy - r * 0.2f, r * 0.35f, r * 0.35f);
            }

            // Nose
            using (var b = new SolidBrush(Color.FromArgb(220, 120, 140)))
                g.FillEllipse(b, cx - r * 0.12f, cy + r * 0.15f, r * 0.24f, r * 0.18f);

            // Whiskers
            using (var p = new Pen(Color.White, 1f))
            {
                g.DrawLine(p, cx - r * 0.1f, cy + r * 0.22f, cx - r * 0.9f, cy + r * 0.1f);
                g.DrawLine(p, cx - r * 0.1f, cy + r * 0.28f, cx - r * 0.9f, cy + r * 0.38f);
                g.DrawLine(p, cx + r * 0.1f, cy + r * 0.22f, cx + r * 0.9f, cy + r * 0.1f);
                g.DrawLine(p, cx + r * 0.1f, cy + r * 0.28f, cx + r * 0.9f, cy + r * 0.38f);
            }
        }

        private static void DrawDog(Graphics g, int x, int y, int size)
        {
            int cx = x + size / 2;
            int cy = y + size / 2;
            float r = size * 0.28f;

            // Face
            using (var b = new SolidBrush(Color.FromArgb(210, 170, 110)))
                g.FillEllipse(b, cx - r, cy - r * 0.9f, r * 2, r * 1.8f);

            // Floppy ears
            using (var b = new SolidBrush(Color.FromArgb(160, 110, 60)))
            {
                g.FillEllipse(b, cx - r * 1.4f, cy - r * 0.7f, r * 0.8f, r * 1.2f);
                g.FillEllipse(b, cx + r * 0.6f, cy - r * 0.7f, r * 0.8f, r * 1.2f);
            }

            // Eyes
            using (var b = new SolidBrush(Color.FromArgb(60, 40, 20)))
            {
                g.FillEllipse(b, cx - r * 0.55f, cy - r * 0.2f, r * 0.35f, r * 0.35f);
                g.FillEllipse(b, cx + r * 0.2f, cy - r * 0.2f, r * 0.35f, r * 0.35f);
            }

            // Nose
            using (var b = new SolidBrush(Color.FromArgb(60, 40, 30)))
                g.FillEllipse(b, cx - r * 0.2f, cy + r * 0.1f, r * 0.4f, r * 0.28f);
        }

        private static void DrawFox(Graphics g, int x, int y, int size)
        {
            int cx = x + size / 2;
            int cy = y + size / 2;
            float r = size * 0.28f;

            // Face
            using (var b = new SolidBrush(Color.FromArgb(230, 110, 40)))
                g.FillEllipse(b, cx - r, cy - r * 0.8f, r * 2, r * 1.7f);

            // Pointy ears
            using (var b = new SolidBrush(Color.FromArgb(230, 110, 40)))
            {
                g.FillPolygon(b, new PointF[] {
                    new PointF(cx - r * 0.3f, cy - r * 0.8f),
                    new PointF(cx - r * 0.9f, cy - r * 1.7f),
                    new PointF(cx - r * 0.05f, cy - r * 0.9f)
                });
                g.FillPolygon(b, new PointF[] {
                    new PointF(cx + r * 0.3f, cy - r * 0.8f),
                    new PointF(cx + r * 0.9f, cy - r * 1.7f),
                    new PointF(cx + r * 0.05f, cy - r * 0.9f)
                });
            }

            // White muzzle
            using (var b = new SolidBrush(Color.FromArgb(240, 220, 200)))
                g.FillEllipse(b, cx - r * 0.5f, cy + r * 0.0f, r, r * 0.8f);

            // Eyes
            using (var b = new SolidBrush(Color.FromArgb(40, 30, 20)))
            {
                g.FillEllipse(b, cx - r * 0.5f, cy - r * 0.15f, r * 0.3f, r * 0.3f);
                g.FillEllipse(b, cx + r * 0.2f, cy - r * 0.15f, r * 0.3f, r * 0.3f);
            }
        }

        private static void DrawFrog(Graphics g, int x, int y, int size)
        {
            int cx = x + size / 2;
            int cy = y + size / 2;
            float r = size * 0.28f;

            // Body
            using (var b = new SolidBrush(Color.FromArgb(60, 180, 80)))
                g.FillEllipse(b, cx - r, cy - r * 0.6f, r * 2, r * 1.6f);

            // Big eyes on top
            using (var b = new SolidBrush(Color.FromArgb(80, 200, 100)))
            {
                g.FillEllipse(b, cx - r * 0.8f, cy - r * 0.9f, r * 0.7f, r * 0.7f);
                g.FillEllipse(b, cx + r * 0.1f, cy - r * 0.9f, r * 0.7f, r * 0.7f);
            }
            using (var b = new SolidBrush(Color.FromArgb(30, 30, 30)))
            {
                g.FillEllipse(b, cx - r * 0.62f, cy - r * 0.78f, r * 0.34f, r * 0.34f);
                g.FillEllipse(b, cx + r * 0.28f, cy - r * 0.78f, r * 0.34f, r * 0.34f);
            }

            // Smile
            using (var p = new Pen(Color.FromArgb(30, 100, 30), 2f))
                g.DrawArc(p, cx - r * 0.4f, cy + r * 0.2f, r * 0.8f, r * 0.4f, 0, 180);
        }

        // ── ELEMENTS ──────────────────────────────────────────────────────────
        private static void DrawFire(Graphics g, int x, int y, int size)
        {
            int cx = x + size / 2;
            int cy = y + size / 2;
            float r = size * 0.32f;

            var flamePath = new GraphicsPath();
            flamePath.AddBezier(
                cx, cy + r,
                cx - r * 0.8f, cy,
                cx - r * 0.5f, cy - r * 0.5f,
                cx, cy - r * 1.2f);
            flamePath.AddBezier(
                cx, cy - r * 1.2f,
                cx + r * 0.5f, cy - r * 0.5f,
                cx + r * 0.8f, cy,
                cx, cy + r);

            using (var b = new LinearGradientBrush(
                new PointF(cx, cy + r), new PointF(cx, cy - r * 1.2f),
                Color.FromArgb(255, 80, 0), Color.FromArgb(255, 220, 0)))
                g.FillPath(b, flamePath);

            // Inner flame
            var innerPath = new GraphicsPath();
            innerPath.AddBezier(
                cx, cy + r * 0.3f,
                cx - r * 0.3f, cy - r * 0.2f,
                cx, cy - r * 0.6f,
                cx, cy - r * 0.9f);
            innerPath.AddBezier(
                cx, cy - r * 0.9f,
                cx + r * 0.1f, cy - r * 0.6f,
                cx + r * 0.3f, cy - r * 0.1f,
                cx, cy + r * 0.3f);
            using (var b = new SolidBrush(Color.FromArgb(180, 255, 200, 0)))
                g.FillPath(b, innerPath);
        }

        private static void DrawThunder(Graphics g, int x, int y, int size)
        {
            int cx = x + size / 2;
            int cy = y + size / 2;
            float r = size * 0.3f;

            var bolt = new PointF[]
            {
                new PointF(cx + r * 0.2f, cy - r * 1.1f),
                new PointF(cx - r * 0.1f, cy - r * 0.1f),
                new PointF(cx + r * 0.3f, cy - r * 0.1f),
                new PointF(cx - r * 0.2f, cy + r * 1.1f),
                new PointF(cx + r * 0.15f, cy + r * 0.1f),
                new PointF(cx - r * 0.15f, cy + r * 0.1f),
            };

            using (var b = new SolidBrush(Color.FromArgb(255, 230, 50)))
                g.FillPolygon(b, bolt);
            using (var p = new Pen(Color.FromArgb(200, 180, 20), 1f))
                g.DrawPolygon(p, bolt);
        }

        private static void DrawWater(Graphics g, int x, int y, int size)
        {
            int cx = x + size / 2;
            int cy = y + size / 2;
            float r = size * 0.3f;

            var drop = new GraphicsPath();
            drop.AddBezier(
                cx, cy - r * 1.1f,
                cx + r * 0.8f, cy - r * 0.2f,
                cx + r * 0.8f, cy + r * 0.5f,
                cx, cy + r * 1.0f);
            drop.AddBezier(
                cx, cy + r * 1.0f,
                cx - r * 0.8f, cy + r * 0.5f,
                cx - r * 0.8f, cy - r * 0.2f,
                cx, cy - r * 1.1f);

            using (var b = new LinearGradientBrush(
                new PointF(cx, cy - r), new PointF(cx, cy + r),
                Color.FromArgb(100, 180, 255), Color.FromArgb(20, 100, 200)))
                g.FillPath(b, drop);

            // Shine
            using (var b = new SolidBrush(Color.FromArgb(120, 255, 255, 255)))
                g.FillEllipse(b, cx - r * 0.35f, cy - r * 0.6f, r * 0.3f, r * 0.4f);
        }

        private static void DrawLeaf(Graphics g, int x, int y, int size)
        {
            int cx = x + size / 2;
            int cy = y + size / 2;
            float r = size * 0.3f;

            var leaf = new GraphicsPath();
            leaf.AddBezier(
                cx, cy + r,
                cx - r * 1.0f, cy,
                cx - r * 0.8f, cy - r * 1.0f,
                cx, cy - r);
            leaf.AddBezier(
                cx, cy - r,
                cx + r * 0.8f, cy - r * 1.0f,
                cx + r * 1.0f, cy,
                cx, cy + r);

            using (var b = new LinearGradientBrush(
                new PointF(cx - r, cy), new PointF(cx + r, cy),
                Color.FromArgb(80, 200, 80), Color.FromArgb(40, 140, 40)))
                g.FillPath(b, leaf);

            // Stem
            using (var p = new Pen(Color.FromArgb(40, 120, 40), 2f))
                g.DrawLine(p, cx, cy + r, cx, cy - r * 0.8f);

            // Vein
            using (var p = new Pen(Color.FromArgb(60, 160, 60), 1f))
            {
                g.DrawLine(p, cx, cy - r * 0.4f, cx - r * 0.5f, cy + r * 0.2f);
                g.DrawLine(p, cx, cy - r * 0.1f, cx + r * 0.5f, cy + r * 0.4f);
            }
        }

        // ── GAMING ────────────────────────────────────────────────────────────
        private static void DrawCrown(Graphics g, int x, int y, int size)
        {
            int cx = x + size / 2;
            int cy = y + size / 2;
            float r = size * 0.3f;

            var crown = new PointF[]
            {
                new PointF(cx - r, cy + r * 0.4f),
                new PointF(cx - r, cy - r * 0.2f),
                new PointF(cx - r * 0.5f, cy + r * 0.1f),
                new PointF(cx, cy - r),
                new PointF(cx + r * 0.5f, cy + r * 0.1f),
                new PointF(cx + r, cy - r * 0.2f),
                new PointF(cx + r, cy + r * 0.4f),
            };

            using (var b = new LinearGradientBrush(
                new PointF(cx, cy - r), new PointF(cx, cy + r * 0.4f),
                Color.FromArgb(255, 220, 50), Color.FromArgb(200, 140, 0)))
                g.FillPolygon(b, crown);
            using (var p = new Pen(Color.FromArgb(180, 120, 0), 1.5f))
                g.DrawPolygon(p, crown);

            // Jewels
            using (var b = new SolidBrush(Color.FromArgb(200, 50, 80)))
                g.FillEllipse(b, cx - r * 0.12f, cy - r * 0.85f, r * 0.24f, r * 0.24f);
            using (var b = new SolidBrush(Color.FromArgb(50, 100, 200)))
            {
                g.FillEllipse(b, cx - r * 0.88f, cy + r * 0.0f, r * 0.2f, r * 0.2f);
                g.FillEllipse(b, cx + r * 0.68f, cy + r * 0.0f, r * 0.2f, r * 0.2f);
            }
        }

        private static void DrawStar(Graphics g, int x, int y, int size)
        {
            int cx = x + size / 2;
            int cy = y + size / 2;
            float r = size * 0.35f;
            float ir = r * 0.42f;

            var pts = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double angle = (i * 36 - 90) * Math.PI / 180;
                float rad = i % 2 == 0 ? r : ir;
                pts[i] = new PointF(
                    cx + (float)(rad * Math.Cos(angle)),
                    cy + (float)(rad * Math.Sin(angle)));
            }

            using (var b = new LinearGradientBrush(
                new PointF(cx, cy - r), new PointF(cx, cy + r),
                Color.FromArgb(255, 230, 60), Color.FromArgb(220, 150, 0)))
                g.FillPolygon(b, pts);
            using (var p = new Pen(Color.FromArgb(180, 120, 0), 1f))
                g.DrawPolygon(p, pts);
        }

        private static void DrawJoystick(Graphics g, int x, int y, int size)
        {
            int cx = x + size / 2;
            int cy = y + size / 2;
            float r = size * 0.3f;

            // Controller body
            using (var b = new SolidBrush(Color.FromArgb(60, 80, 120)))
                g.FillRoundedRectangle(b,
                    cx - r, cy - r * 0.4f, r * 2, r * 1.2f, (int)(r * 0.4f));

            // Stick
            using (var b = new SolidBrush(Color.FromArgb(100, 130, 180)))
                g.FillEllipse(b, cx - r * 0.25f, cy - r * 1.0f, r * 0.5f, r * 0.5f);
            using (var p = new Pen(Color.FromArgb(100, 130, 180), r * 0.15f))
                g.DrawLine(p, cx, cy - r * 0.75f, cx, cy - r * 0.4f);

            // Buttons
            using (var b = new SolidBrush(Color.FromArgb(220, 60, 60)))
                g.FillEllipse(b, cx + r * 0.4f, cy - r * 0.2f, r * 0.3f, r * 0.3f);
            using (var b = new SolidBrush(Color.FromArgb(60, 180, 80)))
                g.FillEllipse(b, cx + r * 0.65f, cy, r * 0.3f, r * 0.3f);
        }

        private static void DrawDice(Graphics g, int x, int y, int size)
        {
            int cx = x + size / 2;
            int cy = y + size / 2;
            float r = size * 0.28f;

            // Dice body
            var rect = new RectangleF(cx - r, cy - r, r * 2, r * 2);
            using (var b = new SolidBrush(Color.FromArgb(240, 230, 210)))
                g.FillRoundedRectangle(b, cx - r, cy - r, r * 2, r * 2, (int)(r * 0.3f));
            using (var p = new Pen(Color.FromArgb(100, 80, 50), 1.5f))
                g.DrawRoundedRectangle(p, cx - r, cy - r, r * 2, r * 2, (int)(r * 0.3f));

            // Dots (showing 5)
            float d = r * 0.2f;
            using (var b = new SolidBrush(Color.FromArgb(60, 40, 20)))
            {
                g.FillEllipse(b, cx - r * 0.55f, cy - r * 0.55f, d, d); // top-left
                g.FillEllipse(b, cx + r * 0.35f, cy - r * 0.55f, d, d); // top-right
                g.FillEllipse(b, cx - r * 0.1f, cy - r * 0.1f, d, d); // center
                g.FillEllipse(b, cx - r * 0.55f, cy + r * 0.35f, d, d); // bottom-left
                g.FillEllipse(b, cx + r * 0.35f, cy + r * 0.35f, d, d); // bottom-right
            }
        }

        // ── Graphics extension helpers ─────────────────────────────────────────
        private static void FillRoundedRectangle(this Graphics g, Brush brush,
            float x, float y, float w, float h, int radius)
        {
            var path = GetRoundedRect(x, y, w, h, radius);
            g.FillPath(brush, path);
        }

        private static void DrawRoundedRectangle(this Graphics g, Pen pen,
            float x, float y, float w, float h, int radius)
        {
            var path = GetRoundedRect(x, y, w, h, radius);
            g.DrawPath(pen, path);
        }

        private static GraphicsPath GetRoundedRect(float x, float y,
            float w, float h, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}