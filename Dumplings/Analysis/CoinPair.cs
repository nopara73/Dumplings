using System;
using System.Collections.Generic;
using System.Text;

namespace Dumplings.Analysis
{
    public class CoinPair : IEquatable<CoinPair>
    {
        public Coin Coin1 { get; }
        public Coin Coin2 { get; }

        public CoinPair(Coin coin1, Coin coin2)
        {
            Coin1 = coin1;
            Coin2 = coin2;
        }

        public override bool Equals(object obj) => Equals(obj as CoinPair);

        public bool Equals(CoinPair other) => this == other;

        public override int GetHashCode() => Coin1.Id.GetHashCode() ^ Coin2.Id.GetHashCode();

        public static bool operator ==(CoinPair x, CoinPair y)
            => x?.Coin1.Id == y?.Coin1.Id && x?.Coin2.Id == y?.Coin2.Id
            || x?.Coin1.Id == y?.Coin2.Id && x?.Coin2.Id == y?.Coin1.Id;

        public static bool operator !=(CoinPair x, CoinPair y) => !(x == y);
    }
}
