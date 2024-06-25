/*
 * Copyright (C) RyuSOCKS
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2,
 * as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit.Sdk;

namespace RyuSocks.Test.Utils
{
    public class StringData : DataAttribute
    {
        private readonly int _min;
        private readonly int _max;
        private int _count;

        public StringData(int min, int max, int count)
        {
            _min = min;
            _max = max;
            _count = count;
        }

        public StringData(int firstLength, int secondLength)
        {
            _min = firstLength;
            _max = secondLength;
            _count = 2;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            List<object[]> data = [];
            StringBuilder builder = new();
            int currentMin = _min;
            int currentMax = _max;
            bool isMin = true;
            bool isMinReverse = false;
            bool isMaxReverse = false;

            while (_count > 0)
            {
                builder.Clear();
                _count--;

                if (isMin)
                {
                    isMin = false;

                    builder.Append('a', currentMin);
                    data.Add([builder.ToString()]);

                    if (!isMinReverse)
                    {
                        if (currentMin == _max)
                        {
                            currentMin--;
                            isMinReverse = true;
                            continue;
                        }

                        currentMin++;
                    }
                    else
                    {
                        if (currentMin == _min)
                        {
                            currentMin++;
                            isMinReverse = false;
                            continue;
                        }

                        currentMin--;
                    }
                }
                else
                {
                    isMin = true;

                    builder.Append('a', currentMax);
                    data.Add([builder.ToString()]);

                    if (!isMaxReverse)
                    {
                        if (currentMax == _min)
                        {
                            currentMax++;
                            isMaxReverse = true;
                            continue;
                        }

                        currentMax--;
                    }
                    else
                    {
                        if (currentMin == _max)
                        {
                            currentMax--;
                            isMaxReverse = false;
                            continue;
                        }

                        currentMax++;
                    }
                }
            }

            return data.ToArray();
        }
    }
}
