/*
    <IntelRnd, Part of FileBurn>
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

extern "C"
{
	_declspec(dllexport) bool RndSupported()
	{
		unsigned Recx;
		__asm
		{
			mov eax,0x1
			cpuid
			mov Recx,ecx;
		}
		if((Recx&0x40000000)==0x40000000)
			return true;
		return false;
	}

	// *********************************************************************************************************************
	// * http://software.intel.com/en-us/articles/intel-digital-random-number-generator-drng-software-implementation-guide *
	// *********************************************************************************************************************

	_declspec(dllexport) unsigned int GetRnd()
	{
		unsigned int rndret=0;
		__asm
		{
			try_again:
			xor eax,eax
			rdrand eax
			jae try_again
			mov rndret,eax
		}
		return rndret;
	}
}