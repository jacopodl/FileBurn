/*
    <Shredder, Part of FileBurn>
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
using System.IO;
using System.Threading;

namespace FileBurn
{
    public delegate void PublishFinfo(object source, string file, long len);
    public delegate void PublishProgress(object source,int progress);
    public delegate void CicleN(object source, int now, int total);
    public delegate void Finish(object source, string file, long len);
    public delegate void WorkException(object source,string message);

    class Shredder
    {
        public event PublishFinfo PublishFi;
        public event PublishProgress Publishp;
        public event CicleN CicleN;
        public event Finish Finish;
        public event WorkException WorkE;
        
        private string file;
        private int cicle;
        private bool stop = false;
        private RndByte rndB;

        public Shredder()
        {

        }

        public Shredder(string file, int cicle, RndByte rndB)
        {
            this.file = file;
            this.cicle = cicle;
            this.rndB = rndB;
        }

        public Shredder(string file, RndByte rndB)
        {
            this.file = file;
            this.cicle = 1;
            this.rndB = rndB;
        }

        public void setNext(string file, int cicle, RndByte rndB)
        {
            this.file = file;
            this.cicle = cicle;
            this.rndB = rndB;
        }

        public void stopProcess()
        {
            this.stop = true;
        }

        public void Execute()
        {
            this.stop = false;
            try
            {
                FileStream fs = new FileStream(this.file, FileMode.Open, FileAccess.ReadWrite);
                BinaryWriter bw = new BinaryWriter(fs);
                long len = fs.Length;

                if (len == 0)
                {
                    bw.Close();
                    fs.Close();
                    return;
                }

                long upd = (long)Math.Round(len/60.0);
                long bupd = upd;

                if (this.PublishFi != null)
                    PublishFi(this, file, len);

                for (int i = 0; i < this.cicle && !this.stop; i++)
                {
                    if (this.CicleN != null)
                        CicleN(this, i, this.cicle);

                    if (i >= 1 && this.Publishp != null)
                    {
                        Publishp(this, 0);
                        bupd = upd;
                    }

                    long j = 0;
                    while (!this.stop)
                    {
                        if (j >= bupd)
                        {
                            if (this.Publishp != null)
                                Publishp(this, (int)Math.Round(((double)j / (double)len) * 100.0));
                            bupd += upd;
                        }

                        if (j < len)
                            bw.Write(this.rndB.getRndByte());
                        else
                            break;
                        j++;
                    }
                    
                    bw.Flush();
                    bw.Seek(0, SeekOrigin.Begin);
                    Thread.Sleep(80);
                }

                bw.Close();
                fs.Close();
                if(this.stop!=true)
                    System.IO.File.Delete(this.file);

                if (this.Finish != null)
                    Finish(this, this.file, len);
            }
            catch (Exception e)
            {
                if (this.WorkE != null)
                    WorkE(this, "\n"+e.Message);
            }
        }
    }
}
