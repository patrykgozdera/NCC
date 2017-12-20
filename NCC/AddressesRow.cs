using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCC
{
    class AddressesRow
    {
        private string userName;
        private string address;

        public AddressesRow(string userName, string address)
        {
            this.userName = userName;
            this.address = address;
        }

        public String getUserName()
        {
            return userName;
        }

        public String getAddress()
        {
            return address;
        }
    }
}
