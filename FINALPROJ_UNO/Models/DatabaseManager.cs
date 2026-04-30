using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;


namespace UNOFinal.Models
{
    public class DatabaseManager
    {
        
        private const string CONNECTION_STRING =
            @"Data Source=(localdb)\MSSQLLocalDB;" +
             "Initial Catalog=UNOCardGame;" +
             "Integrated Security=True;";

        
        public int GetOrCreatePlayer(string name)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                //Cehck if player exist
                string checkSql = "SELECT PlayerId FROM Players WHERE Name = @Name";
                using (var cmd = new SqlCommand(checkSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                        return Convert.ToInt32(result);
                }

                //Create new
                string insertSql =
                    "INSERT INTO Players (Name) VALUES (@Name);" +
                    "SELECT SCOPE_IDENTITY();";
                using (var cmd = new SqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }


        public int SaveSession(string gameMode, string winnerName, int totalRounds, DateTime startTime)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "INSERT INTO GameSessions (GameMode, WinnerName, TotalRounds, StartTime, EndTime) " +
                    "VALUES (@Mode, @Winner, @Rounds, @Start, @End); " +
                    "SELECT SCOPE_IDENTITY();";

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


        public void SaveSessionPlayer(int sessionId, int? playerId,
                                      string playerName, bool isAI,
                                      string aiDifficulty, int score, int placement)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "INSERT INTO SessionPlayers " +
                    "(SessionId, PlayerId, PlayerName, IsAI, AIDifficulty, FinalScore, Placement) " +
                    "VALUES (@Sid, @Pid, @Name, @IsAI, @Diff, @Score, @Place)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Sid", sessionId);
                    cmd.Parameters.AddWithValue("@Pid",
                        playerId.HasValue ? (object)playerId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Name", playerName);
                    cmd.Parameters.AddWithValue("@IsAI", isAI ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Diff",
                        aiDifficulty != null ? (object)aiDifficulty : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Score", score);
                    cmd.Parameters.AddWithValue("@Place", placement);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //Save a round
        public void SaveRound(int sessionId, int roundNumber,
                              string winnerName, int points, int durationSeconds)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "INSERT INTO Rounds (SessionId, RoundNumber, WinnerName, PointsScored, Duration) " +
                    "VALUES (@Sid, @Round, @Winner, @Points, @Dur)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Sid", sessionId);
                    cmd.Parameters.AddWithValue("@Round", roundNumber);
                    cmd.Parameters.AddWithValue("@Winner", winnerName);
                    cmd.Parameters.AddWithValue("@Points", points);
                    cmd.Parameters.AddWithValue("@Dur", durationSeconds);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void LogMove(int sessionId, int roundNumber, string playerName,
                            string moveType, string cardPlayed, string colorChosen)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "INSERT INTO MoveLogs " +
                    "(SessionId, RoundNumber, PlayerName, MoveType, CardPlayed, ColorChosen) " +
                    "VALUES (@Sid, @Round, @Player, @Type, @Card, @Color)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Sid", sessionId);
                    cmd.Parameters.AddWithValue("@Round", roundNumber);
                    cmd.Parameters.AddWithValue("@Player", playerName);
                    cmd.Parameters.AddWithValue("@Type", moveType);
                    cmd.Parameters.AddWithValue("@Card",
                        cardPlayed != null ? (object)cardPlayed : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Color",
                        colorChosen != null ? (object)colorChosen : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //UpdatePStats
        public void UpdatePlayerStats(string playerName, bool won, int score)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "UPDATE Players SET " +
                    "TotalGames = TotalGames + 1, " +
                    "TotalWins  = TotalWins  + @Won, " +
                    "TotalScore = TotalScore + @Score " +
                    "WHERE Name = @Name";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Won", won ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Score", score);
                    cmd.Parameters.AddWithValue("@Name", playerName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        
        public DataTable GetLeaderboard()
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "SELECT Name, TotalWins, TotalGames, TotalScore, " +
                    "CASE WHEN TotalGames = 0 THEN 0 " +
                    "ELSE ROUND(TotalWins * 100.0 / TotalGames, 1) END AS WinRate " +
                    "FROM Players " +
                    "ORDER BY WinRate DESC, TotalScore DESC";

                var adapter = new SqlDataAdapter(sql, conn);
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public void UpdateSession(int sessionId, string winnerName, int totalRounds)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "UPDATE GameSessions SET WinnerName = @Winner, " +
                    "TotalRounds = @Rounds, EndTime = @End " +
                    "WHERE SessionId = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Winner", winnerName);
                    cmd.Parameters.AddWithValue("@Rounds", totalRounds);
                    cmd.Parameters.AddWithValue("@End", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Id", sessionId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateSessionPlayer(int sessionId, string playerName,
                                        int score, int placement)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "UPDATE SessionPlayers SET FinalScore = @Score, " +
                    "Placement = @Place " +
                    "WHERE SessionId = @Sid AND PlayerName = @Name";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Score", score);
                    cmd.Parameters.AddWithValue("@Place", placement);
                    cmd.Parameters.AddWithValue("@Sid", sessionId);
                    cmd.Parameters.AddWithValue("@Name", playerName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public DataTable GetMatchHistory(string playerName)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "SELECT gs.StartTime, gs.GameMode, gs.WinnerName, " +
                    "sp.FinalScore, sp.Placement, gs.TotalRounds " +
                    "FROM GameSessions gs " +
                    "JOIN SessionPlayers sp ON gs.SessionId = sp.SessionId " +
                    "WHERE sp.PlayerName = @Name " +
                    "ORDER BY gs.StartTime DESC";

                var adapter = new SqlDataAdapter(sql, conn);
                adapter.SelectCommand.Parameters.AddWithValue("@Name", playerName);
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }


        
        public void UpdatePlayerName(string oldName, string newName)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                
                string sql1 = "UPDATE Players SET Name = @New WHERE Name = @Old";
                using (var cmd = new SqlCommand(sql1, conn))
                {
                    cmd.Parameters.AddWithValue("@New", newName);
                    cmd.Parameters.AddWithValue("@Old", oldName);
                    cmd.ExecuteNonQuery();
                }

                string sql2 = "UPDATE SessionPlayers SET PlayerName = @New WHERE PlayerName = @Old";
                using (var cmd = new SqlCommand(sql2, conn))
                {
                    cmd.Parameters.AddWithValue("@New", newName);
                    cmd.Parameters.AddWithValue("@Old", oldName);
                    cmd.ExecuteNonQuery();
                }

                
                string sql3 = "UPDATE MoveLogs SET PlayerName = @New WHERE PlayerName = @Old";
                using (var cmd = new SqlCommand(sql3, conn))
                {
                    cmd.Parameters.AddWithValue("@New", newName);
                    cmd.Parameters.AddWithValue("@Old", oldName);
                    cmd.ExecuteNonQuery();
                }

                string sql4 = "UPDATE GameSessions SET WinnerName = @New WHERE WinnerName = @Old";
                using (var cmd = new SqlCommand(sql4, conn))
                {
                    cmd.Parameters.AddWithValue("@New", newName);
                    cmd.Parameters.AddWithValue("@Old", oldName);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void DeleteSession(int sessionId)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                //if delete, child records deletes 1st
                string[] sqls = {
                    "DELETE FROM MoveLogs      WHERE SessionId = @Id",
                    "DELETE FROM Rounds        WHERE SessionId = @Id",
                    "DELETE FROM SessionPlayers WHERE SessionId = @Id",
                    "DELETE FROM GameSessions   WHERE SessionId = @Id",
                };

                foreach (string sql in sqls)
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", sessionId);
                        cmd.ExecuteNonQuery();
                    }
            }
        }


        public void DeleteAllSessions()
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string[] sqls = {
                    "DELETE FROM MoveLogs",
                    "DELETE FROM Rounds",
                    "DELETE FROM SessionPlayers",
                    "DELETE FROM GameSessions",
                };
                foreach (string sql in sqls)
                    using (var cmd = new SqlCommand(sql, conn))
                        cmd.ExecuteNonQuery();
            }
        }


        public DataTable GetAllMatchHistory()
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "SELECT TOP 50 " +
                    "gs.SessionId, gs.StartTime, gs.GameMode, gs.WinnerName, " +
                    "gs.TotalRounds, " +
                    "STRING_AGG(sp.PlayerName, ', ') AS Players " +
                    "FROM GameSessions gs " +
                    "JOIN SessionPlayers sp ON gs.SessionId = sp.SessionId " +
                    "GROUP BY gs.SessionId, gs.StartTime, gs.GameMode, " +
                    "gs.WinnerName, gs.TotalRounds " +
                    "ORDER BY gs.StartTime DESC";

                var adapter = new SqlDataAdapter(sql, conn);
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }
        //DELETE HERE

        //Analytics utilizes this
        public DataTable GetSessionRounds(int sessionId)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "SELECT RoundNumber, WinnerName, PointsScored, Duration " +
                    "FROM Rounds WHERE SessionId = @Id " +
                    "ORDER BY RoundNumber ASC";
                var da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@Id", sessionId);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        //Analytics utilizes this
        public DataTable GetSessionPlayerStats(int sessionId)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "SELECT " +
                    "  PlayerName, " +
                    "  COUNT(CASE WHEN MoveType = 'Play' THEN 1 END) AS CardsPlayed, " +
                    "  COUNT(CASE WHEN MoveType = 'Draw' THEN 1 END) AS CardsDrawn, " +
                    "  COUNT(CASE WHEN MoveType = 'UNO'  THEN 1 END) AS UnoCalls, " +
                    "  COUNT(CASE WHEN MoveType = 'Play' AND CardPlayed LIKE '%Skip%'         THEN 1 END) AS Skips, " +
                    "  COUNT(CASE WHEN MoveType = 'Play' AND CardPlayed LIKE '%Reverse%'      THEN 1 END) AS Reverses, " +
                    "  COUNT(CASE WHEN MoveType = 'Play' AND CardPlayed LIKE '%Draw Two%'     THEN 1 END) AS DrawTwos, " +
                    "  COUNT(CASE WHEN MoveType = 'Play' AND CardPlayed LIKE '%Wild Draw%'    THEN 1 END) AS WildDrawFours, " +
                    "  COUNT(CASE WHEN MoveType = 'Play' AND CardPlayed LIKE '%Wild%'         " +
                    "             AND CardPlayed NOT LIKE '%Draw%' THEN 1 END) AS Wilds " +
                    "FROM MoveLogs " +
                    "WHERE SessionId = @Id " +
                    "GROUP BY PlayerName";
                var da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@Id", sessionId);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        //Analytics utilizes this
        public DataTable GetSessionPlayerColors(int sessionId)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "SELECT PlayerName, " +
                    "  COUNT(CASE WHEN CardPlayed LIKE 'Red%'    THEN 1 END) AS Reds, " +
                    "  COUNT(CASE WHEN CardPlayed LIKE 'Blue%'   THEN 1 END) AS Blues, " +
                    "  COUNT(CASE WHEN CardPlayed LIKE 'Green%'  THEN 1 END) AS Greens, " +
                    "  COUNT(CASE WHEN CardPlayed LIKE 'Yellow%' THEN 1 END) AS Yellows " +
                    "FROM MoveLogs " +
                    "WHERE SessionId = @Id AND MoveType = 'Play' " +
                    "GROUP BY PlayerName";
                var da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@Id", sessionId);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        //Analytics utilizes this
        public DataTable GetSessionTargets(int sessionId)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "SELECT sp.PlayerName, sp.FinalScore, sp.Placement, sp.IsAI, sp.AIDifficulty " +
                    "FROM SessionPlayers sp " +
                    "WHERE sp.SessionId = @Id " +
                    "ORDER BY sp.Placement ASC";
                var da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@Id", sessionId);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        //all time Player stats -- Analytics utilizes this
        public DataTable GetPlayerProfile(string playerName)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "SELECT Name, TotalWins, TotalGames, TotalScore, CreatedAt, " +
                    "CASE WHEN TotalGames = 0 THEN 0 " +
                    "ELSE ROUND(TotalWins * 100.0 / TotalGames, 1) END AS WinRate, " +
                    "CASE WHEN TotalGames = 0 THEN 0 " +
                    "ELSE TotalScore / TotalGames END AS AvgScore " +
                    "FROM Players WHERE Name = @Name";
                var da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@Name", playerName);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        //score history uses line charts!! 
        public DataTable GetPlayerScoreHistory(string playerName)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "SELECT gs.StartTime, sp.FinalScore, sp.Placement, gs.GameMode " +
                    "FROM SessionPlayers sp " +
                    "JOIN GameSessions gs ON sp.SessionId = gs.SessionId " +
                    "WHERE sp.PlayerName = @Name AND sp.IsAI = 0 " +
                    "ORDER BY gs.StartTime ASC";
                var da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@Name", playerName);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        
        public DataTable GetPlayerFavoriteColors(string playerName)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "SELECT " +
                    "  COUNT(CASE WHEN CardPlayed LIKE 'Red%'    THEN 1 END) AS Red, " +
                    "  COUNT(CASE WHEN CardPlayed LIKE 'Blue%'   THEN 1 END) AS Blue, " +
                    "  COUNT(CASE WHEN CardPlayed LIKE 'Green%'  THEN 1 END) AS Green, " +
                    "  COUNT(CASE WHEN CardPlayed LIKE 'Yellow%' THEN 1 END) AS Yellow " +
                    "FROM MoveLogs " +
                    "WHERE PlayerName = @Name AND MoveType = 'Play'";
                var da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@Name", playerName);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        
        public DataTable GetPlayerActionCards(string playerName)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "SELECT " +
                    "  COUNT(CASE WHEN CardPlayed LIKE '%Skip%'      THEN 1 END) AS Skips, " +
                    "  COUNT(CASE WHEN CardPlayed LIKE '%Reverse%'   THEN 1 END) AS Reverses, " +
                    "  COUNT(CASE WHEN CardPlayed LIKE '%Draw Two%'  THEN 1 END) AS DrawTwos, " +
                    "  COUNT(CASE WHEN CardPlayed LIKE 'Wild Draw%'  THEN 1 END) AS WildDrawFours, " +
                    "  COUNT(CASE WHEN CardPlayed = 'Wild'           THEN 1 END) AS Wilds, " +
                    "  COUNT(CASE WHEN CardPlayed LIKE '[0-9]%' OR CardPlayed LIKE 'Red [0-9]%' " +
                    "             OR CardPlayed LIKE 'Blue [0-9]%' OR CardPlayed LIKE 'Green [0-9]%' " +
                    "             OR CardPlayed LIKE 'Yellow [0-9]%' THEN 1 END) AS Numbers " +
                    "FROM MoveLogs " +
                    "WHERE PlayerName = @Name AND MoveType = 'Play'";
                var da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@Name", playerName);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        
        public DataTable GetHeadToHead(string playerName)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql =
                    "SELECT " +
                    "  opp.PlayerName AS Opponent, " +
                    "  COUNT(*) AS GamesShared, " +
                    "  SUM(CASE WHEN gs.WinnerName = @Name THEN 1 ELSE 0 END) AS Wins, " +
                    "  SUM(CASE WHEN gs.WinnerName = opp.PlayerName THEN 1 ELSE 0 END) AS Losses " +
                    "FROM SessionPlayers me " +
                    "JOIN SessionPlayers opp ON me.SessionId = opp.SessionId " +
                    "  AND opp.PlayerName != @Name AND opp.IsAI = 0 " +
                    "JOIN GameSessions gs ON me.SessionId = gs.SessionId " +
                    "WHERE me.PlayerName = @Name AND me.IsAI = 0 " +
                    "GROUP BY opp.PlayerName";
                var da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@Name", playerName);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
        public int GetPlayerAvatar(string playerName)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                string sql = "SELECT AvatarId FROM Players WHERE Name = @Name";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", playerName);
                    var result = cmd.ExecuteScalar();

                    
                    return result != null ? Convert.ToInt32(result) : 1;
                }
            }
        }

        
        public void UpdatePlayerAvatar(string playerName, int avatarId)
        {
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();

               
                string checkSql = "SELECT COUNT(*) FROM Players WHERE Name = @Name";
                using (var cmd = new SqlCommand(checkSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", playerName);
                    int count = (int)cmd.ExecuteScalar();

                    if (count == 0)
                    {
                        
                        string insertSql = "INSERT INTO Players (Name, AvatarId) VALUES (@Name, @Id)";
                        using (var insertCmd = new SqlCommand(insertSql, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@Name", playerName);
                            insertCmd.Parameters.AddWithValue("@Id", avatarId);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string updateSql = "UPDATE Players SET AvatarId = @Id WHERE Name = @Name";
                        using (var updateCmd = new SqlCommand(updateSql, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@Id", avatarId);
                            updateCmd.Parameters.AddWithValue("@Name", playerName);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}


        
   