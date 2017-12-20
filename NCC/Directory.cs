using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCC
{
    public class Directory
    {
        private static string address = null;
        public Directory()
        {

        }

        public static String getTranslatedAddress(string userName)
        {
            address = AddressesTable.getAddress(userName);
            CustomSocket.LogClass.Log("Translated " + userName + " to " + address);
            return address;
        }

    }
}
