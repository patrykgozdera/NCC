using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomSocket;

namespace NCC
{
    public class Directory
    {
        private static string address = null;

        public static String getTranslatedAddress(string userName)
        {
            address = AddressesTable.getAddress(userName);
            return address;
        }

    }
}
