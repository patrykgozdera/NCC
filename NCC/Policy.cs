using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCC
{
    public class Policy
    {
        public static void userAuthentication(string userName)
        {
            if(AddressesTable.isUserAuthenticated(userName))
            {
                CustomSocket.LogClass.Log(userName + " authentication successful.");
            }
        }
    }
}
