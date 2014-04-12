/*
    <CpuVendor, Part of FileBurn>
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

#include <string.h>

extern "C"
{
	struct CpuReg
	{
		unsigned ebx;
		unsigned edx;
		unsigned ecx;
	};

	_declspec(dllexport) int GetCpuVendor()
	{
		CpuReg creg={0,0,0};
		__asm
		{
			xor eax,eax
			cpuid
			mov creg.ebx,ebx
			mov creg.ecx,ecx
			mov creg.edx,edx
		}
		if(memcmp((char*)&creg.ebx,"Genu",4)==0&&memcmp((char *)&creg.edx, "ineI", 4) == 0 &&memcmp((char *)&creg.ecx, "ntel", 4) == 0)
			return 0x0;
		else if(memcmp((char*)&creg.ebx,"Auth",4)==0&&memcmp((char *)&creg.edx, "enti", 4) == 0 &&memcmp((char *)&creg.ecx, "cAMD", 4) == 0)
			return 0x1;
		else
			return 0xFF;
	}
}