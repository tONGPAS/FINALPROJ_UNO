
namespace UNOFinal.Models
{
    //Properties heer
    public enum CardColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Wild        
    }

    public enum CardType
    {
        Number,
        Skip,
        Reverse,
        DrawTwo,
        Wild,
        WildDrawFour
    }

    
    public class Card
    {
        
        private CardColor _color;
        private CardType _type;
        private int _value;   

        
        public CardColor Color => _color;
        public CardType Type => _type;
        public int Value => _value;

        public Card(CardColor color, CardType type, int value = -1)
        {
            _color = color;
            _type = type;
            _value = value;
        }

        //Methods 

       
        public bool IsPlayableOn(Card topCard, CardColor activeColor)
        {
            
            if (_type == CardType.Wild) return true;
            if (_type == CardType.WildDrawFour) return true;

            
            if (_color == activeColor) return true;

            
            if (_type == CardType.Number &&
                topCard.Type == CardType.Number &&
                _value == topCard.Value) return true;

            
            if (_type != CardType.Number && _type == topCard.Type) return true;

            return false;
        }

        public bool IsWild()
        {
            return _type == CardType.Wild || _type == CardType.WildDrawFour;
        }

        public bool IsActionCard()
        {
            return _type == CardType.Skip ||
                   _type == CardType.Reverse ||
                   _type == CardType.DrawTwo ||
                   _type == CardType.Wild ||
                   _type == CardType.WildDrawFour;
        }

        
        public int GetPoints()
        {
            if (_type == CardType.Number) return _value;
            if (_type == CardType.Wild) return 50;
            if (_type == CardType.WildDrawFour) return 50;
            return 20; //Skip, Reverse, DrawTwo
        }

        public override string ToString()
        {
            if (_type == CardType.Wild) return "Wild";
            if (_type == CardType.WildDrawFour) return "Wild Draw Four";
            if (_type == CardType.Number) return $"{_color} {_value}";
            return $"{_color} {_type}";
        }
    }
}