using System;
using System.Linq;
using System.Linq.Expressions;
using NString;
using DynamicExpression = System.Linq.Dynamic.DynamicExpression;

namespace Oodrive.GetText.Core.Po
{
    public class PoHeader : PoEntryBase
    {
        public PoHeader(bool isFuzzy, int nplurals, string pluralExpression) : base(string.Empty, isFuzzy, false, Enumerable.Empty<string>(), Enumerable.Empty<string>())
        {
            NPlurals = nplurals;
            SetPluralFormSelector(pluralExpression);
        }

        private void SetPluralFormSelector(string pluralExpression)
        {
            if (pluralExpression.IsNullOrWhiteSpace())
            {
                PluralFormSelector = _ => _%2;
                return;
            }

            if (NPlurals < 2)
            {
                PluralFormSelector = _ => 0;
                return;
            }

            var p = Expression.Parameter(typeof(int), "n");
            var e = DynamicExpression.ParseLambda(new[] { p }, null, pluralExpression);
            var del = e.Compile();
            if (NPlurals == 2)
            {
                PluralFormSelector = _ =>
                {
                    var result = del.DynamicInvoke(_);
                    var booleanResult = result as bool?;
                    return booleanResult == true ? 1 : 0;
                };
                return;
            }

            PluralFormSelector = _ =>
            {
                var result = del.DynamicInvoke(_);
                var integerResult = result as int?;
                return integerResult ?? _ % 2;
            };
        }

        public override bool IsHeader => true;

        public int NPlurals { get; }

        public Func<int,int> PluralFormSelector { get; private set; } 
    }
}
