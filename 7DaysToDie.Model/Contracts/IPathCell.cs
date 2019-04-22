using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Contracts
{
    public interface IPathCell<T>
    {
        IPathCell<T> Next { get; set; }
        IPathCell<T> Previous { get; set; }

        T CostToDestination { get; set; }
        T CostFromOrigin { get; set; }


    }
}
