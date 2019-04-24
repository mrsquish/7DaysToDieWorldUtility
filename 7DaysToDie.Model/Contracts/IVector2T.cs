using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Contracts
{
    public interface IVector2<T>
    {
        T X { get; set; }
        T Z { get; set; }
    }
}
