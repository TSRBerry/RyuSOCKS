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
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace RyuSocks.Test.Utils
{
    public abstract class UsernameAndPasswordRandomUsernameAndPasswords : DataAttribute
    {

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var rand = new Random();
            var username = new string(Enumerable.Repeat(Chars, rand.Next(1, 20))
                    .Select(s => s[rand.Next(s.Length)]).ToArray());
            var password = new string(Enumerable.Repeat(Chars, rand.Next(1, 20))
                    .Select(s => s[rand.Next(s.Length)]).ToArray());
                yield return [username,password];

        }
    }
}
