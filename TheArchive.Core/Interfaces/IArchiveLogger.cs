using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Interfaces
{
    public interface IArchiveLogger
    {
        public void Success(string msg);
        public void Notice(string msg);
        public void Msg(ConsoleColor col, string msg);
        public void Info(string msg);
        public void Debug(string msg);
        public void Warning(string msg);
        public void Error(string msg);
        public void Exception(Exception ex);
    }
}
