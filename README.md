Absolutely! Here's your updated **README.md** with all sections plus code snippets. Just copy-paste and add your screenshot filenames.

---

# 🃏 UNO Card Game

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

## 📸 Screenshots

*(Replace `screenshot1.png` with your actual screenshot filenames)*

| Main Menu | Lobby Setup |
|-----------|-------------|
| ![Main Menu](screenshot1.png) | ![Lobby](screenshot2.png) |

| Game Board | Leaderboard |
|------------|-------------|
| ![Game Board](screenshot3.png) | ![Leaderboard](screenshot4.png) |

| Player Profile | Match History |
|----------------|---------------|
| ![Player Profile](screenshot5.png) | ![Match History](screenshot6.png) |

| Avatar Picker | Win Screen |
|---------------|------------|
| ![Avatar Picker](screenshot7.png) | ![Win Screen](screenshot8.png) |

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

## 💻 Key Code Snippets

### 1. Card.cs — IsPlayableOn() (UNO Rules Logic)

```csharp
// Card.cs - Validates if a card can be played on the current top card
public bool IsPlayableOn(Card topCard, CardColor activeColor)
{
    // Wild cards are always playable
    if (_type == CardType.Wild) return true;
    if (_type == CardType.WildDrawFour) return true;
    
    // Same color as active color
    if (_color == activeColor) return true;
    
    // Same number (for number cards)
    if (_type == CardType.Number && topCard.Type == CardType.Number 
        && _value == topCard.Value) return true;
    
    // Same action type (Skip on Skip, Reverse on Reverse, etc.)
    if (_type != CardType.Number && _type == topCard.Type) return true;
    
    return false;
}
```

**What it does:** Determines if a player can play a card by checking color, number, or symbol match.

---

### 2. AIPlayer.cs — ChooseCard() (AI Decision Making)

```csharp
// AIPlayer.cs - AI chooses a card based on difficulty level
public Card ChooseCard(Card topCard, CardColor activeColor)
{
    List<Card> playable = GetPlayableCards(topCard, activeColor);
    if (playable.Count == 0) return null; // Must draw
    
    switch (_difficulty)
    {
        case AIDifficulty.Easy:
            return playable[0];  // Play first valid card
            
        case AIDifficulty.Medium:
            return ChooseMedium(playable, activeColor);  // Prefer action cards
            
        case AIDifficulty.Hard:
            return ChooseHard(playable, activeColor);  // Strategic play
            
        default:
            return playable[0];
    }
}
```

**What it does:** Implements 3 AI difficulty levels using a rule-based decision tree.

---

### 3. DatabaseManager.cs — SaveSession() (Database Operation)

```csharp
// DatabaseManager.cs - Saves a game session to the database
public int SaveSession(string gameMode, string winnerName, int totalRounds, DateTime startTime)
{
    using (var conn = new SqlConnection(CONNECTION_STRING))
    {
        conn.Open();
        string sql = "INSERT INTO GameSessions (GameMode, WinnerName, TotalRounds, StartTime, EndTime) " +
                     "VALUES (@Mode, @Winner, @Rounds, @Start, @End); SELECT SCOPE_IDENTITY();";
        
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@Mode", gameMode);
            cmd.Parameters.AddWithValue("@Winner", winnerName);
            cmd.Parameters.AddWithValue("@Rounds", totalRounds);
            cmd.Parameters.AddWithValue("@Start", startTime);
            cmd.Parameters.AddWithValue("@End", DateTime.Now);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
```

**What it does:** Inserts a new game session and returns the auto-generated SessionId.

---

### 4. FormGame.cs — DrawCard() (GDI+ Rendering)

```csharp
// FormGame.cs - Draws a UNO card using GDI+ (no external images)
private void DrawCard(Graphics g, int x, int y, int w, int h, 
                      Color color, string label, bool faceUp, Card card = null)
{
    // Draw shadow behind card
    using (var sb = new SolidBrush(Color.FromArgb(48, 0, 0, 0)))
        g.FillPath(sb, RoundedPath(new Rectangle(x + 3, y + 4, w, h), 8));
    
    // Draw card background (rounded rectangle)
    var path = RoundedPath(new Rectangle(x, y, w, h), 8);
    using (var b = new SolidBrush(color)) 
        g.FillPath(b, path);
    
    // Draw inner oval highlight (glossy effect)
    using (var b = new SolidBrush(Color.FromArgb(22, 255, 255, 255)))
        g.FillPath(b, RoundedPath(new Rectangle(x + 3, y + 3, w - 6, h - 6), 6));
    
    // Draw gold border for playable cards
    if (playable)
        using (var p = new Pen(GOLD, 3f)) 
            g.DrawPath(p, path);
}
```

**What it does:** Renders a complete UNO card with shadow, background, inner highlight, and playable indicator — all with pure GDI+.

---

### 5. GameEngine.cs — ProcessAITurn() (Game Flow)

```csharp
// GameEngine.cs - Processes a complete AI turn
public void ProcessAITurn()
{
    if (CurrentPlayer.IsHuman) return;
    
    AIPlayer ai = (AIPlayer)CurrentPlayer;
    Card chosen = ai.ChooseCard(TopDiscard, _activeColor);
    
    if (chosen == null)
    {
        // No playable cards — draw one
        Card drawn = DrawCard(ai);
        if (drawn != null && drawn.IsPlayableOn(TopDiscard, _activeColor))
            PlayCard(ai, drawn);  // Play the drawn card if possible
        else
            AdvanceTurn();  // End turn
    }
    else
    {
        // Play the chosen card
        PlayCard(ai, chosen);
        
        // If Wild card was played, AI chooses the color
        if (_state == GameState.ChoosingColor)
            SetActiveColor(ai.ChooseColor());
    }
}
```

**What it does:** Orchestrates the AI's turn — choosing cards, drawing when needed, and selecting colors for Wild cards.

---

### 6. AvatarDrawer.cs — Draw() Method (Custom Graphics)

```csharp
// AvatarDrawer.cs - Draws all 16 avatars using GDI+
public static void Draw(Graphics g, int avatarId, int x, int y, int size)
{
    int idx = Math.Max(0, Math.Min(15, avatarId - 1));
    Color bg = AvatarColors[idx];
    
    // Draw circular background
    DrawCircle(g, x, y, size, bg);
    
    // Draw symbol based on avatar ID
    switch (idx)
    {
        case 0: DrawSpade(g, x, y, size); break;   // ♠
        case 1: DrawHeart(g, x, y, size); break;   // ♥
        case 2: DrawDiamond(g, x, y, size); break; // ♦
        case 3: DrawClub(g, x, y, size); break;    // ♣
        case 4: DrawCat(g, x, y, size); break;     // 🐱
        case 5: DrawDog(g, x, y, size); break;     // 🐶
        case 6: DrawFox(g, x, y, size); break;     // 🦊
        case 7: DrawFrog(g, x, y, size); break;    // 🐸
        case 8: DrawFire(g, x, y, size); break;    // 🔥
        case 9: DrawThunder(g, x, y, size); break; // ⚡
        case 10: DrawWater(g, x, y, size); break;  // 💧
        case 11: DrawLeaf(g, x, y, size); break;   // 🌿
        case 12: DrawCrown(g, x, y, size); break;  // 👑
        case 13: DrawStar(g, x, y, size); break;   // ⭐
        case 14: DrawJoystick(g, x, y, size); break; // 🎮
        case 15: DrawDice(g, x, y, size); break;   // 🎲
    }
}
```

**What it does:** Draws 16 unique avatars using pure GDI+ — no image files needed.

---

### 7. GameEngine.cs — StartGame() (Edge Case Handling)

```csharp
// GameEngine.cs - Ensures first card is never a Wild or Wild Draw Four
Card firstCard = _deck.TopDiscard;
while (firstCard != null && (firstCard.Type == CardType.Wild || firstCard.Type == CardType.WildDrawFour))
{
    // Put Wild card back, reshuffle, draw a new first card
    _deck.Discard(firstCard);
    _deck.Shuffle();
    firstCard = _deck.Draw();
    _deck.Discard(firstCard);
    firstCard = _deck.TopDiscard;
}
```

**What it does:** Prevents players from getting stuck when the first card is a Wild card.

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

## 🚀 Getting Started

### Prerequisites
- Windows 10/11
- Visual Studio 2022 (or later)
- .NET 8 SDK

### Installation

1. Clone the repository:
```bash
git clone https://github.com/topas/UNO-Card-Game.git
```

2. Open `FINALPROJ_UNO.sln` in Visual Studio

3. Restore NuGet packages (Tools → NuGet Package Manager → Restore)

4. Build the solution (Ctrl + Shift + B)

5. Run the application (F5)

### First Time Setup

The database will be created automatically when you first run the application. SQL Server LocalDB comes with Visual Studio — no additional setup required.

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
6. UNO is called automatically when down to 1 card
7. First to empty their hand wins the round

### Card Types & Effects

| Card | Effect |
|------|--------|
| Number | Play on matching color or number |
| Skip | Next player loses their turn |
| Reverse | Direction of play reverses |
| Draw Two (+2) | Next player draws 2 cards and loses turn |
| Wild | Choose the next color to play |
| Wild Draw Four (+4) | Choose color + next player draws 4 cards |

---

## 📊 Database Schema

| Table | Purpose |
|-------|---------|
| Players | Player profiles and lifetime stats (Name, AvatarId, TotalWins, TotalGames, TotalScore) |
| GameSessions | Each game played (GameMode, WinnerName, TotalRounds, StartTime, EndTime) |
| SessionPlayers | Links players to sessions (PlayerName, IsAI, AIDifficulty, FinalScore, Placement) |
| Rounds | Individual round results (RoundNumber, WinnerName, PointsScored, Duration) |
| MoveLogs | Every card played or drawn (MoveType, CardPlayed, ColorChosen) |

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

- Course: CPE-262
- Date: May 2026

---

## 📄 License

This project was created for educational purposes as a final project for CPE-262.

---

**Enjoy playing UNO! 🃏**

---

## 📝 Instructions for Adding Your Screenshots:

1. Save your screenshots as PNG files
2. Name them like:
   - `screenshot1.png` (Main Menu)
   - `screenshot2.png` (Lobby)
   - `screenshot3.png` (Game Board)
   - `screenshot4.png` (Leaderboard)
   - `screenshot5.png` (Player Profile)
   - `screenshot6.png` (Match History)
   - `screenshot7.png` (Avatar Picker)
   - `screenshot8.png` (Win Screen)

3. Put them in the same folder as your README.md
4. Or upload them to a GitHub issue/image hosting and use the direct links

---

Replace `yourusername` with your actual GitHub username, and update the screenshot filenames to match yours!
