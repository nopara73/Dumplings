using System;
using System.Collections.Generic;
using System.Text;

namespace Dumplings.Stats
{
    public class YearMonthDay : IEquatable<YearMonthDay>
    {
        public YearMonthDay(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public int Year { get; }
        public int Month { get; }
        public int Day { get; }

        public override string ToString()
        {
            return $"{Year:0000}-{Month:00}-{Day:00}";
        }

        public YearMonth ToYearMonth() => new YearMonth(Year, Month);

        #region Equality

        public override bool Equals(object obj) => obj is YearMonthDay pubKey && this == pubKey;

        public bool Equals(YearMonthDay other) => this == other;

        public override int GetHashCode() => Year.GetHashCode() ^ Month.GetHashCode();

        public static bool operator ==(YearMonthDay x, YearMonthDay y) => x.Year == y.Year && x.Month == y.Month && x.Day == y.Day;

        public static bool operator !=(YearMonthDay x, YearMonthDay y) => !(x == y);

        #endregion Equality
    }
}
