using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = CustomSocket.Config.getProperty("NCCname");
            AddressesTable.init();
            NccServer.init();           
            Console.ReadLine();
        }
    }
}
