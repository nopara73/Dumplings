using System;
using System.Collections.Generic;
using System.Text;

namespace Dumplings.Analysis
{
    public class Coin : IEquatable<Coin>
    {
        public Guid Id { get; }
        public decimal Value { get; }

        public Coin(Guid id, decimal value)
        {
            Id = id;
            Value = value;
        }

        public static Coin Random(decimal value) => new Coin(Guid.NewGuid(), value);

        public override bool Equals(object obj) => Equals(obj as Coin);

        public bool Equals(Coin other) => this == other;

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(Coin x, Coin y) => x?.Id == y?.Id;

        public static bool operator !=(Coin x, Coin y) => !(x == y);

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
