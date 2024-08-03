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

using RyuSocks.Types;
using System;

namespace RyuSocks.Auth
{
    /// <summary>
    /// Generic Security Service Application Program Interface
    /// (RFC1961)
    /// </summary>
    [AuthMethodImpl(0x01)]
    public class GssApi : IProxyAuth
    {
        private readonly string result;
        private bool mutualReqFlag = false;
        public bool IsClient;
        public GssApiStatus Status;
        public int WrapperLength => throw new NotImplementedException();

        public bool Authenticate(ReadOnlySpan<byte> incomingPacket, out ReadOnlySpan<byte> outgoingPacket)
        {
            throw new NotImplementedException();
        }

        public int Wrap(Span<byte> packet, int packetLength, ProxyEndpoint remoteEndpoint)
        {
            throw new NotImplementedException();
        }

        public int Unwrap(Span<byte> packet, int packetLength, out ProxyEndpoint remoteEndpoint)
        {
            throw new NotImplementedException();
        }

        // CREDENTIAL MANAGEMENT
        public void GssAcquireCred()
        {
            throw new NotImplementedException();
        }

        public void GssReleaseCred()
        {
            throw new NotImplementedException();
        }

        public void GssInquireCred()
        {
            throw new NotImplementedException();
        }

        // CONTEXT-LEVEL CALLS
        public GssBufferT Gss_Init_sec_context(string targetName, bool mutualRequiredFlag)
        {
            if (mutualRequiredFlag)
            {
                mutualReqFlag = true;
                GssBufferT token = new() { TokenFlag = GssApiTokenFlags.ContextLevelToken };
                Status = GssApiStatus.GssContinueNeeded;
                return token;
            }

            throw new NotImplementedException();
        }

        public void GssAcceptSecContext()
        {
            throw new NotImplementedException();
        }
        
        public void GssDeleteSecContext()
        {
            throw new NotImplementedException();
        }
        
        public void GssProcessContextToken()
        {
            throw new NotImplementedException();
        }
        
        public void GssContextTime()
        {
            throw new NotImplementedException();
        }
        
        // PER-MESSAGE CALLS

        public void GssSign()
        {
            throw new NotImplementedException();
        }
        
        public void GssVerify()
        {
            throw new NotImplementedException();
        }
        
        public void GssSeal()
        {
            throw new NotImplementedException();
        }
        
        public void GssUnseal()
        {
            throw new NotImplementedException();
        }
        
        // SUPPORT CALLS
        public void GssDisplayStatus()
        {
            throw new NotImplementedException();
        }
        
        public void GssIndicateMechs()
        {
            throw new NotImplementedException();
        }
        
        public void GssCompareName()
        {
            throw new NotImplementedException();
        }
        
        public void GssDisplayName()
        {
            throw new NotImplementedException();
        }
        
        public void GssImportName()
        {
            throw new NotImplementedException();
        }
        
        public void GssReleaseName()
        {
            throw new NotImplementedException();
        }
        
        public void GssReleaseBuffer()
        {
            throw new NotImplementedException();
        }
        
        public void GssReleaseOidSet()
        {
            throw new NotImplementedException();
        }
    }
}
