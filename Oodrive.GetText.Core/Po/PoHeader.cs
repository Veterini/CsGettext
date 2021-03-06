﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using NString;
using DynamicExpression = System.Linq.Dynamic.DynamicExpression;

namespace Oodrive.GetText.Core.Po
{
    public class PoHeader : PoEntryBase
    {
        public CultureInfo Language { get; }

        public PoHeader(bool isFuzzy, int nplurals, string pluralExpression, CultureInfo language) : base(string.Empty, isFuzzy, false, Enumerable.Empty<string>(), Enumerable.Empty<string>())
        {
            NPlurals = nplurals;
            SetPluralFormSelector(pluralExpression);
            Language = language;
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

        public override void Accept(Dictionary<string, string> dic)
        {
            throw new NotImplementedException();
        }

        public int NPlurals { get; }

        public Func<int,int> PluralFormSelector { get; private set; } 
    }
}
