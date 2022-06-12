using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Interfaces
{
    public interface IFeatureData<T>
    {
        public T Config { get; set; }
    }
}
