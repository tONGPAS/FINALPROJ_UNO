using System;
using System.Collections.Generic;

namespace UNOFinal.Models
{
    public class Deck
    {
        
        private List<Card> _drawPile;
        private List<Card> _discardPile;
        private Random _rng;

        
        public int RemainingCount => _drawPile.Count;
        public Card TopDiscard => _discardPile.Count > 0
                                       ? _discardPile[_discardPile.Count - 1]
                                       : null;

        
        public Deck()
        {
            _rng = new Random();
            _drawPile = new List<Card>();
            _discardPile = new List<Card>();
            Initialize();
        }

        //108 stack
        private void Initialize()
        {
            _drawPile.Clear();

            CardColor[] colors = {
                CardColor.Red,
                CardColor.Blue,
                CardColor.Green,
                CardColor.Yellow
            };

            foreach (CardColor color in colors)
            {
                
                _drawPile.Add(new Card(color, CardType.Number, 0));

                
                for (int i = 1; i <= 9; i++)
                {
                    _drawPile.Add(new Card(color, CardType.Number, i));
                    _drawPile.Add(new Card(color, CardType.Number, i));
                }

                
                _drawPile.Add(new Card(color, CardType.Skip));
                _drawPile.Add(new Card(color, CardType.Skip));
                _drawPile.Add(new Card(color, CardType.Reverse));
                _drawPile.Add(new Card(color, CardType.Reverse));
                _drawPile.Add(new Card(color, CardType.DrawTwo));
                _drawPile.Add(new Card(color, CardType.DrawTwo));
            }

            
            for (int i = 0; i < 4; i++)
                _drawPile.Add(new Card(CardColor.Wild, CardType.Wild));

            
            for (int i = 0; i < 4; i++)
                _drawPile.Add(new Card(CardColor.Wild, CardType.WildDrawFour));

        }

        //shuffle
        public void Shuffle()
        {
            int n = _drawPile.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                Card temp = _drawPile[i];
                _drawPile[i] = _drawPile[j];
                _drawPile[j] = temp;
            }
        }

        
        public Card Draw()
        {
            if (_drawPile.Count == 0)
                ReshuffleDiscardIntoDraw();

            if (_drawPile.Count == 0)
                return null; // No cards left at all

            Card card = _drawPile[_drawPile.Count - 1];
            _drawPile.RemoveAt(_drawPile.Count - 1);
            return card;
        }

       
        public List<Card> DrawMany(int count)
        {
            var cards = new List<Card>();
            for (int i = 0; i < count; i++)
            {
                Card c = Draw();
                if (c != null) cards.Add(c);
            }
            return cards;
        }

        
        public void Discard(Card card)
        {
            _discardPile.Add(card);
        }

        
        private void ReshuffleDiscardIntoDraw()
        {
            if (_discardPile.Count <= 1) return;

            
            Card top = _discardPile[_discardPile.Count - 1];
            _discardPile.RemoveAt(_discardPile.Count - 1);

            _drawPile.AddRange(_discardPile);
            _discardPile.Clear();
            _discardPile.Add(top);

            Shuffle();
        }

        //initial no. of cards  = 7
        public void DealInitialHands(List<Player> players)
        {
            Shuffle();
            foreach (Player player in players)
                player.Hand.AddRange(DrawMany(7));

            
            Card firstCard = Draw();
            while (firstCard != null &&
                   firstCard.Type == CardType.WildDrawFour)
            {
                _drawPile.Insert(0, firstCard);
                Shuffle();
                firstCard = Draw();
            }
            if (firstCard != null)
                Discard(firstCard);
        }
    }
}