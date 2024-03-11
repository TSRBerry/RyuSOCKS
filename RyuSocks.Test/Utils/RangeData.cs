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

using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Xunit.Sdk;

namespace RyuSocks.Test.Utils
{
    public class RangeData<T> : DataAttribute
        where T : INumber<T>
    {
        private readonly T _min;
        private readonly T _max;
        private readonly T[] _extraElements;

        public RangeData(T min, T max, params T[] extraElements)
        {
            _min = min;
            _max = max;
            _extraElements = extraElements;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            List<object> data = [];

            for (T i = _min; i < _max; i++)
            {
                data.Add(i);
            }

            foreach (T element in _extraElements)
            {
                data.Add(element);
            }

            return [[data.ToArray()]];
        }
    }
}
