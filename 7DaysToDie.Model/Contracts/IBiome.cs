using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model.Contracts
{
    public interface IBiome : IDisposable
    {
        int Size { get; }
        string BaseDirectory { get; }

        void Generate();
    }
}
