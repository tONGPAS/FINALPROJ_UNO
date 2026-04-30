using System;
using System.Collections.Generic;
using System.Linq;

namespace UNOFinal.Models
{
    public enum GameState
    {
        WaitingToStart,
        PlayerTurn,
        AITurn,
        ChoosingColor,
        RoundOver,
        GameOver
    }

    public enum TurnDirection
    {
        Clockwise = 1,
        CounterClockwise = -1
    }

    public class GameEngine
    {
        
        private Deck _deck;
        private List<Player> _players;
        private int _currentPlayerIndex;
        private TurnDirection _direction;
        private CardColor _activeColor;
        private GameState _state;
        private int _roundNumber;
        private Random _rng;

        
        public List<Player> Players => _players;
        public Player CurrentPlayer => _players[_currentPlayerIndex];
        public Card TopDiscard => _deck.TopDiscard;
        public CardColor ActiveColor => _activeColor;
        public GameState State => _state;
        public int RoundNumber => _roundNumber;
        public TurnDirection Direction => _direction;

        public event Action<Player, Card> OnCardPlayed;
        public event Action<Player, int> OnCardsDrawn;
        public event Action<Player> OnPlayerSkipped;
        public event Action<TurnDirection> OnDirectionChanged;
        public event Action<Player> OnUNOCalled;
        public event Action<Player> OnRoundWon;
        public event Action<Player> OnGameWon;
        public event Action<CardColor> OnColorChanged;

        
        public GameEngine(List<Player> players)
        {
            _players = players;
            _deck = new Deck();
            _currentPlayerIndex = 0;
            _direction = TurnDirection.Clockwise;
            _state = GameState.WaitingToStart;
            _roundNumber = 1;
            _rng = new Random();
        }


        public void StartGame()
        {
            foreach (Player p in _players)
                p.ClearHand();

            _deck = new Deck();
            _deck.DealInitialHands(_players);

            // Keep re-dealing until first card is NOT a Wild or Wild Draw Four
            Card firstCard = _deck.TopDiscard;
            while (firstCard != null && (firstCard.Type == CardType.Wild || firstCard.Type == CardType.WildDrawFour))
            {
                // Put the Wild card back into the draw pile
                _deck.Discard(firstCard);
                _deck.Shuffle();

                // Draw a new first card
                firstCard = _deck.Draw();
                _deck.Discard(firstCard);
                firstCard = _deck.TopDiscard;
            }

            // Set active color based on the valid first card
            _activeColor = firstCard?.Color ?? CardColor.Red;

            _currentPlayerIndex = 0;
            _direction = TurnDirection.Clockwise;

            // Apply first card effect if needed (Skip, Reverse, Draw Two)
            if (firstCard != null)
                ApplyFirstCardEffect(firstCard);

            _state = CurrentPlayer.IsHuman ? GameState.PlayerTurn : GameState.AITurn;
        }



        public bool PlayCard(Player player, Card card)
        {
            if (player != CurrentPlayer) return false;
            if (!player.Hand.Contains(card)) return false;
            if (!card.IsPlayableOn(TopDiscard, _activeColor)) return false;

            player.RemoveCard(card);
            _deck.Discard(card);

            
            if (!card.IsWild())
                _activeColor = card.Color;

            OnCardPlayed?.Invoke(player, card);

            
            if (player.HandCount == 1 && player.IsHuman)
            {
                
            }

            
            if (player.HasWon())
            {
                AwardRoundPoints(player);
                OnRoundWon?.Invoke(player);
                _state = GameState.RoundOver;
                return true;
            }

           
            if (card.IsWild())
            {
                _state = GameState.ChoosingColor;
                return true;
            }

            ApplyCardEffect(card);
            AdvanceTurn();
            return true;
        }

        public Card DrawCard(Player player)
        {
            Card drawn = _deck.Draw();
            if (drawn != null)
            {
                player.AddCard(drawn);
                OnCardsDrawn?.Invoke(player, 1);
            }
            return drawn;
        }

        
        public void SetActiveColor(CardColor color)
        {
            _activeColor = color;
            OnColorChanged?.Invoke(color);

            
            if (TopDiscard?.Type == CardType.WildDrawFour)
                ApplyCardEffect(TopDiscard);

            AdvanceTurn();
        }

        
        public void ProcessAITurn()
        {
            if (CurrentPlayer.IsHuman) return;

            AIPlayer ai = (AIPlayer)CurrentPlayer;

            Card chosen = ai.ChooseCard(TopDiscard, _activeColor);

            if (chosen == null)
            {
                
                Card drawn = DrawCard(ai);

                if (drawn != null && drawn.IsPlayableOn(TopDiscard, _activeColor))
                    PlayCard(ai, drawn);
                else
                    AdvanceTurn();
            }
            else
            {
                //Check UNO call before playing
                if (ai.HandCount == 2 && ai.ShouldCallUno())
                    OnUNOCalled?.Invoke(ai);

                PlayCard(ai, chosen);

                //Choose color if Wild
                if (_state == GameState.ChoosingColor)
                {
                    CardColor chosenColor = ai.ChooseColor();
                    SetActiveColor(chosenColor);
                }
            }
        }

        //Check UNO penalty 
        //Call this when a new turn starts 
       
        public void CheckUnoPenalty(Player player)
        {
            if (player.HandCount == 1 && !player.HasCalledUno)
            {
                List<Card> penalty = _deck.DrawMany(2);
                player.AddCards(penalty);
                OnCardsDrawn?.Invoke(player, 2);
            }
        }

        //challenge wild draw and penalties
        public bool ChallengeWildDrawFour(Player challenger, Player challenged)
        {
            
            bool hadLegalPlay = challenged.Hand.Any(c =>
                c.Color == _activeColor && !c.IsWild());

            if (hadLegalPlay)
            {
                
                List<Card> penalty = _deck.DrawMany(4);
                challenged.AddCards(penalty);
                OnCardsDrawn?.Invoke(challenged, 4);
                return true;
            }
            else
            {
                
                List<Card> penalty = _deck.DrawMany(6);
                challenger.AddCards(penalty);
                OnCardsDrawn?.Invoke(challenger, 6);
                return false;
            }
        }

        
        private void ApplyCardEffect(Card card)
        {
            int nextIndex = GetNextPlayerIndex(1);
            Player nextPlayer = _players[nextIndex];

            switch (card.Type)
            {
                case CardType.Skip:
                    OnPlayerSkipped?.Invoke(nextPlayer);
                    _currentPlayerIndex = nextIndex;
                    break;

                case CardType.Reverse:
                    _direction = _direction == TurnDirection.Clockwise
                                 ? TurnDirection.CounterClockwise
                                 : TurnDirection.Clockwise;
                    OnDirectionChanged?.Invoke(_direction);

                   
                    if (_players.Count == 2)
                    {
                        OnPlayerSkipped?.Invoke(nextPlayer);
                        _currentPlayerIndex = nextIndex;
                    }
                    break;

                case CardType.DrawTwo:
                    List<Card> drawTwo = _deck.DrawMany(2);
                    nextPlayer.AddCards(drawTwo);
                    OnCardsDrawn?.Invoke(nextPlayer, 2);
                    OnPlayerSkipped?.Invoke(nextPlayer);
                    _currentPlayerIndex = nextIndex;
                    break;

                case CardType.WildDrawFour:
                    List<Card> drawFour = _deck.DrawMany(4);
                    nextPlayer.AddCards(drawFour);
                    OnCardsDrawn?.Invoke(nextPlayer, 4);
                    OnPlayerSkipped?.Invoke(nextPlayer);
                    _currentPlayerIndex = nextIndex;
                    break;
            }
        }

        private void ApplyFirstCardEffect(Card card)
        {
            switch (card.Type)
            {
                case CardType.Skip:
                    AdvanceTurn();
                    break;
                case CardType.Reverse:
                    _direction = TurnDirection.CounterClockwise;
                    break;
                case CardType.DrawTwo:
                    List<Card> drawn = _deck.DrawMany(2);
                    _players[0].AddCards(drawn);
                    AdvanceTurn();
                    break;
                case CardType.Wild:
                    _state = GameState.ChoosingColor;
                    break;
            }
        }

        
        public void AdvanceTurn()
        {
            _currentPlayerIndex = GetNextPlayerIndex(1);
            _state = CurrentPlayer.IsHuman
                     ? GameState.PlayerTurn
                     : GameState.AITurn;
        }

        private int GetNextPlayerIndex(int steps)
        {
            int count = _players.Count;
            return ((_currentPlayerIndex + (int)_direction * steps) % count + count) % count;
        }

        
        private void AwardRoundPoints(Player winner)
        {
            int points = _players
                .Where(p => p != winner)
                .Sum(p => p.GetHandPoints());
            winner.Score += points;
        }

        


        public void StartNextRound()
        {
            _roundNumber++;
            _state = GameState.WaitingToStart;
            StartGame();
        }

       
        public List<Card> GetPlayableCardsForCurrentPlayer()
        {
            return CurrentPlayer.GetPlayableCards(TopDiscard, _activeColor);
        }
    }
}