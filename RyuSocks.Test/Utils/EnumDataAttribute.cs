// Copyright (C) RyuSOCKS
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2,
// as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace RyuSocks.Test.Utils
{
    [ExcludeFromCodeCoverage]
    public class EnumDataAttribute<T> : DataAttribute
        where T : struct, Enum
    {
        public override bool SupportsDiscoveryEnumeration() => true;

        public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
        {
            T[] values = Enum.GetValues<T>();
            TheoryDataRow<T>[] rows = new TheoryDataRow<T>[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                rows[i] = new TheoryDataRow<T>(values[i]);
            }

            return ValueTask.FromResult<IReadOnlyCollection<ITheoryDataRow>>(rows);
        }
    }
}
