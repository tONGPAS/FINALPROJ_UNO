using System.Collections.Generic;
using System.Linq;

namespace UNOFinal.Models
{
    public class Player
    {
        
        private string _name;
        private bool _isHuman;
        private int _score;
        private bool _hasCalledUno;
       


        public string Name => _name;
        public bool IsHuman => _isHuman;
        public int Score { get => _score; set => _score = value; }
        public bool HasCalledUno { get => _hasCalledUno; set => _hasCalledUno = value; }
        public List<Card> Hand { get; private set; }
        public int HandCount => Hand.Count;

        


        public Player(string name, bool isHuman)
        {
            _name = name;
            _isHuman = isHuman;
            _score = 0;
            _hasCalledUno = false;
            Hand = new List<Card>();
        }


        
        public void AddCard(Card card)
        {
            Hand.Add(card);
            _hasCalledUno = false; 
        }

        public void AddCards(List<Card> cards)
        {
            Hand.AddRange(cards);
            _hasCalledUno = false;
        }

        public bool RemoveCard(Card card)
        {
            return Hand.Remove(card);
        }

        public void ClearHand()
        {
            Hand.Clear();
            _hasCalledUno = false;
        }

        
        public List<Card> GetPlayableCards(Card topCard, CardColor activeColor)
        {
            return Hand.Where(c => c.IsPlayableOn(topCard, activeColor)).ToList();
        }

        public bool HasPlayableCard(Card topCard, CardColor activeColor)
        {
            return Hand.Any(c => c.IsPlayableOn(topCard, activeColor));
        }

        public bool HasWon()
        {
            return Hand.Count == 0;
        }

        
        public int GetHandPoints()
        {
            return Hand.Sum(c => c.GetPoints());
        }

        public void CallUno()
        {
            _hasCalledUno = true;
        }

        
        public virtual string GetDisplayName()
        {
            return _name;
        }

        public override string ToString()
        {
            return $"{_name} ({HandCount} cards)";
        }
    }
}