using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourInARow
{
    public class Location
    {
        public int Row { get; set; }
        public int Column { get; set; }


        public Location North()
        {
            Row--;
            return this;
        }

        public Location East()
        {
            Column++;
            return this;
        }

        public Location South()
        {
            Row++;
            return this;
        }
        
        public Location West()
        {
            Column--;
            return this;
        }
    }
}
