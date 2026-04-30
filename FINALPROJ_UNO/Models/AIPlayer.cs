using System;
using System.Collections.Generic;
using System.Linq;

namespace UNOFinal.Models
{
    public enum AIDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public class AIPlayer : Player   
    {
        
        private AIDifficulty _difficulty;
        private Random _rng;

        
        private Dictionary<CardColor, int> _colorMemory;

        
        public AIDifficulty Difficulty => _difficulty;

        
        public AIPlayer(string name, AIDifficulty difficulty)
            : base(name, isHuman: false)   
        {
            _difficulty = difficulty;
            _rng = new Random();
            _colorMemory = new Dictionary<CardColor, int>
            {
                { CardColor.Red,    0 },
                { CardColor.Blue,   0 },
                { CardColor.Green,  0 },
                { CardColor.Yellow, 0 }
            };
        }

        //Core AI decision
        public Card ChooseCard(Card topCard, CardColor activeColor)
        {
            List<Card> playable = GetPlayableCards(topCard, activeColor);

            if (playable.Count == 0) return null; //Must draw

            switch (_difficulty)
            {
                case AIDifficulty.Easy:
                    return ChooseEasy(playable);

                case AIDifficulty.Medium:
                    return ChooseMedium(playable, activeColor);

                case AIDifficulty.Hard:
                    return ChooseHard(playable, activeColor);

                default:
                    return playable[0];
            }
        }

        
        private Card ChooseEasy(List<Card> playable)
        {
            return playable[0];
        }

        
        private Card ChooseMedium(List<Card> playable, CardColor activeColor)
        {
            
            var actionCards = playable.Where(c =>
                c.IsActionCard() && !c.IsWild()).ToList();
            if (actionCards.Count > 0)
                return actionCards[0];

            
            var sameColor = playable.Where(c => c.Color == activeColor).ToList();
            if (sameColor.Count > 0)
                return sameColor[0];

           
            return playable[0];
        }

       
        private Card ChooseHard(List<Card> playable, CardColor activeColor)
        {
            
            var nonWilds = playable.Where(c => !c.IsWild()).ToList();

            if (nonWilds.Count > 0)
            {
                
                var actions = nonWilds.Where(c => c.IsActionCard()).ToList();
                if (actions.Count > 0)
                    return actions[0];

                
                CardColor leastColor = GetLeastOpponentColor();
                var leastColorCards = nonWilds
                    .Where(c => c.Color == leastColor).ToList();
                if (leastColorCards.Count > 0)
                    return leastColorCards[0];

                return nonWilds[0];
            }

            
            return playable[0];
        }

        
        public CardColor ChooseColor()
        {
            switch (_difficulty)
            {
                case AIDifficulty.Easy:
                    
                    CardColor[] colors = {
                        CardColor.Red, CardColor.Blue,
                        CardColor.Green, CardColor.Yellow
                    };
                    return colors[_rng.Next(colors.Length)];

                case AIDifficulty.Medium:
                    
                    return GetMostCommonColorInHand();

                case AIDifficulty.Hard:
                   
                    return GetLeastOpponentColor();

                default:
                    return CardColor.Red;
            }
        }

        //UNO call logic 
        public bool ShouldCallUno()
        {
            if (HandCount != 1) return false;

            switch (_difficulty)
            {
                case AIDifficulty.Easy:
                    
                    return _rng.Next(100) >= 30;

                case AIDifficulty.Medium:
                case AIDifficulty.Hard:
                    return true; 

                default:
                    return true;
            }
        }

        
        public void ObserveOpponentPlay(Card card)
        {
            if (card.Color != CardColor.Wild && _colorMemory.ContainsKey(card.Color))
                _colorMemory[card.Color]++;
        }

        
        private CardColor GetMostCommonColorInHand()
        {
            var colorCounts = new Dictionary<CardColor, int>
            {
                { CardColor.Red,    0 },
                { CardColor.Blue,   0 },
                { CardColor.Green,  0 },
                { CardColor.Yellow, 0 }
            };

            foreach (Card card in Hand)
                if (card.Color != CardColor.Wild)
                    colorCounts[card.Color]++;

            return colorCounts.OrderByDescending(kv => kv.Value).First().Key;
        }

        private CardColor GetLeastOpponentColor()
        {
            
            return _colorMemory.OrderBy(kv => kv.Value).First().Key;
        }

        
        public override string GetDisplayName()
        {
            return $"{Name} ({_difficulty} AI)";
        }
    }
}