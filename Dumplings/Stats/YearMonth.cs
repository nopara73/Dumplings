using System;
using System.Collections.Generic;
using System.Text;

namespace Dumplings.Stats
{
    public class YearMonth : IEquatable<YearMonth>
    {
        public YearMonth(int year, int month)
        {
            Year = year;
            Month = month;
        }

        public int Year { get; }
        public int Month { get; }

        public override string ToString()
        {
            return $"{Year:0000}-{Month:00}";
        }

        #region Equality

        public override bool Equals(object obj) => obj is YearMonth pubKey && this == pubKey;

        public bool Equals(YearMonth other) => this == other;

        public override int GetHashCode() => Year.GetHashCode() ^ Month.GetHashCode();

        public static bool operator ==(YearMonth x, YearMonth y) => x.Year == y.Year && x.Month == y.Month;

        public static bool operator !=(YearMonth x, YearMonth y) => !(x == y);

        #endregion Equality
    }
}
