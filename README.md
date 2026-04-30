

# 🃏 UNO! Card Game

**A complete UNO card game built with C# .NET Windows Forms**

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Windows Forms](https://img.shields.io/badge/Windows_Forms-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)

---

## 📖 Overview

This is a fully playable UNO card game for up to 3 players (Human and AI). Built with C# .NET Windows Forms, it features a complete UNO rule engine, 3 AI difficulty levels, database-driven player profiles, and full analytics including leaderboards, match history, and player statistics.

**No Visual Studio Designer was used** — every UI element is hand-coded using GDI+.

---

## ✨ Features

### 🎮 Gameplay
- Full 108-card UNO deck with all card types (Number, Skip, Reverse, Draw Two, Wild, Wild Draw Four)
- Hot-seat multiplayer for up to 3 players
- 3 AI difficulty levels (Easy, Medium, Hard)
- Automatic UNO calling when down to 1 card
- Official UNO rules for card effects, turn order, and scoring

### 🤖 AI System
- **Easy**: Plays first valid card
- **Medium**: Prefers action cards over number cards
- **Hard**: Strategic — tracks opponent colors, saves Wild cards

### 🗄️ Database (SQL Server LocalDB)
- 5 normalized tables (Players, GameSessions, SessionPlayers, Rounds, MoveLogs)
- Full CRUD operations
- Every move is logged for analytics

### 📊 Analytics
- Leaderboard with win rates and total scores
- Match history with delete functionality
- Player profiles with:
  - Win/Loss pie chart
  - Score history line chart
  - Color preference bar chart
  - Action card breakdown
  - Head-to-head records

### 🎨 GDI+ Graphics
- All cards drawn programmatically (rounded rectangles, shadows, gradients)
- 16 custom avatars (card suits, animals, elements, gaming icons)
- Rounded corners on all UI elements
- Hover effects and playable card highlighting

### 🔊 Audio
- Background music for menus and gameplay
- Winner sound effects

---

## 🛠️ Tech Stack

| Technology | Purpose |
|------------|---------|
| C# .NET 8 | Programming language |
| Windows Forms | UI framework |
| GDI+ | Custom graphics rendering |
| SQL Server LocalDB | Database |
| Microsoft.Data.SqlClient | Database connectivity |
| LiveChartsCore | Analytics charts |
| NAudio | Background music |

---

## 📁 Project Structure

```
FINALPROJ_UNO/
├── Models/
│   ├── Card.cs              # Card properties and validation
│   ├── Deck.cs              # Deck management (shuffle, draw, discard)
│   ├── Player.cs            # Base player class
│   ├── AIPlayer.cs          # AI with 3 difficulty levels
│   ├── GameEngine.cs        # Core game rules and turn management
│   ├── DatabaseManager.cs   # All database operations
│   └── AvatarDrawer.cs      # GDI+ avatar rendering
├── Forms/
│   ├── FormMainMenu.cs      # Main menu
│   ├── FormLobby.cs         # Player setup
│   ├── FormGame.cs          # Main game board
│   ├── FormLeaderboard.cs   # Leaderboard
│   ├── FormMatchHistory.cs  # Match history
│   ├── FormMatchDetails.cs  # Detailed match analytics
│   ├── FormPlayerProfile.cs # Player statistics with charts
│   ├── FormGuide.cs         # How-to-play guide
│   ├── AvatarPickerForm.cs  # Avatar selection grid
│   └── MusicManager.cs      # Background music management
└── Program.cs               # Application entry point
```

---

## 🎮 How to Play

1. Launch the application
2. Click **New Game**
3. Configure players (Human/CPU toggles, CPU difficulty)
4. Click **Start Game**
5. Play UNO! Match cards by color, number, or symbol
6. Call UNO automatically when down to 1 card
7. First to empty their hand wins the round

**Card Types:**
| Card | Effect |
|------|--------|
| Skip | Next player loses turn |
| Reverse | Direction reverses |
| Draw Two (+2) | Next player draws 2 cards |
| Wild | Choose next color |
| Wild Draw Four (+4) | Choose color + next player draws 4 cards |

---

## 📊 Database Schema

| Table | Purpose |
|-------|---------|
| Players | Player profiles and lifetime stats |
| GameSessions | Each game played |
| SessionPlayers | Links players to sessions |
| Rounds | Individual round results |
| MoveLogs | Every card played or drawn |

---

## 🎯 Future Improvements

- [ ] LAN multiplayer using TCP sockets
- [ ] Additional sound effects (card flip, draw)
- [ ] Custom avatar image uploads
- [ ] Export statistics to CSV/PDF
- [ ] Draw Two stacking (house rule toggle)

---

## 🙏 Acknowledgments

- Official UNO rules from Mattel
- LiveChartsCore for analytics charts
- NAudio for audio playback

---

## 👨‍💻 Author

**Nicoh Barachiel A. Comendador**

- Course: CPE-262-H3
- Date: April 2026

---

## 📄 License

This project was created for educational purposes as a final project for CPE-262.

---

## 📸 Game Screenshots

- Main Menu <img width="692" height="732" alt="image" src="https://github.com/user-attachments/assets/8b616b41-8d98-4a14-ad0b-6c5b46e99607" />

- Lobby / Player Setup <img width="694" height="597" alt="image" src="https://github.com/user-attachments/assets/bcfbc78a-6591-4b5b-8b16-22229b158384" />

- Game Board <img width="1599" height="899" alt="image" src="https://github.com/user-attachments/assets/856a613d-5473-4726-9e3e-7a576ffe782f" />

- Leaderboard <img width="1599" height="899" alt="image" src="https://github.com/user-attachments/assets/d237f407-ac63-40f3-89c5-4b065a6ded05" />

- Player Profile with Charts <img width="1599" height="899" alt="image" src="https://github.com/user-attachments/assets/cd8807b4-394e-4351-bcf2-986126d0d2d8" />

- Match History <img width="1599" height="899" alt="image" src="https://github.com/user-attachments/assets/bb7ceb03-664e-40d8-99ed-2941679c0408" />

- Avatar Picker <img width="535" height="573" alt="image" src="https://github.com/user-attachments/assets/78059891-b31d-4c84-8f9f-3e4edc54b4b3" />

---



