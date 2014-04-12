/*
    <RndByte, Part of FileBurn>
    Copyright (C) <2014> <Jacopo De Luca>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace FileBurn
{
    class RndByte
    {
        internal static class UnsafeNativeMethods
        {
            [DllImport("IntelRnd.dll")]
            public static extern uint GetRnd();
        }

        public enum RndType
        {
            PRNG,
            TRNG
        }

        private int bytePos = 0;
        private byte[] Brnd = new byte[4];
        private RndType rdtype;
        private bool init = true;

        public RndByte(RndType rdtype)
        {
            this.rdtype = rdtype;
        }

        public byte getRndByte()
        {
            if (this.rdtype == RndType.PRNG)
            {
                if (this.bytePos == 4 || this.init)
                {
                    Random rnd = new Random();
                    rnd.NextBytes(this.Brnd);
                    this.bytePos = 0;
                    this.init = false;
                }
            }
            else
            {
                if (this.bytePos == 4 || this.init)
                {
                    uint urnd = UnsafeNativeMethods.GetRnd();
                    this.Brnd = BitConverter.GetBytes(urnd);
                    this.bytePos = 0;
                    this.init = false;
                }          
            }
            return this.Brnd[this.bytePos++];
        }
    }
}
