using System;
using System.Drawing;
using System.Windows.Forms;
using UNOFinal.Models;

namespace UNOFinal
{
    public class AvatarPickerForm : Form
    {
        private int selectedAvatarId;
        private string playerName;
        private DatabaseManager db;
        private TableLayoutPanel avatarGrid;

        public int SelectedAvatarId => selectedAvatarId;

        public AvatarPickerForm(string player, int currentAvatarId)
        {
            playerName = player;
            selectedAvatarId = currentAvatarId;
            db = new DatabaseManager();

            BuildUI();
            LoadAvatars();
        }

        private void BuildUI()
        {
            this.Text = $"Choose Avatar - {playerName}";
            this.Size = new Size(550, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(19, 32, 24);

            Label title = new Label
            {
                Text = "Select Your Avatar",
                ForeColor = Color.FromArgb(255, 215, 0),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Bounds = new Rectangle(0, 20, 550, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(title);

            Label subtitle = new Label
            {
                Text = "Click any avatar to select it",
                ForeColor = Color.FromArgb(106, 138, 116),
                Font = new Font("Segoe UI", 10),
                Bounds = new Rectangle(0, 55, 550, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(subtitle);

            avatarGrid = new TableLayoutPanel
            {
                Bounds = new Rectangle(20, 90, 510, 420),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                BackColor = Color.FromArgb(24, 38, 28)
            };
            avatarGrid.ColumnCount = 4;
            avatarGrid.RowCount = 4;

            for (int i = 0; i < 4; i++)
            {
                avatarGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
                avatarGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            }

            this.Controls.Add(avatarGrid);

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Bounds = new Rectangle(200, 520, 150, 35),
                BackColor = Color.FromArgb(30, 45, 36),
                ForeColor = Color.FromArgb(224, 224, 224),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(58, 90, 68);
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void LoadAvatars()
        {
            for (int avatarId = 1; avatarId <= 16; avatarId++)
            {
                PictureBox avatarBox = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Margin = new Padding(5),
                    BackColor = Color.FromArgb(30, 45, 36),
                    Tag = avatarId,
                    Cursor = Cursors.Hand,
                    Dock = DockStyle.Fill
                };

                avatarBox.Image = AvatarDrawer.ToBitmap(avatarId, 90);

                if (avatarId == selectedAvatarId)
                {
                    avatarBox.BorderStyle = BorderStyle.Fixed3D;
                    avatarBox.BackColor = Color.FromArgb(76, 175, 80);
                }

                avatarBox.Click += AvatarBox_Click;

                int row = (avatarId - 1) / 4;
                int col = (avatarId - 1) % 4;
                avatarGrid.Controls.Add(avatarBox, col, row);
            }
        }

        private void AvatarBox_Click(object sender, EventArgs e)
        {
            PictureBox clicked = sender as PictureBox;
            if (clicked != null)
            {
                selectedAvatarId = (int)clicked.Tag;
                db.UpdatePlayerAvatar(playerName, selectedAvatarId);
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}