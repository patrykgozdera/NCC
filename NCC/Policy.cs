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
                Console.WriteLine(GetTimeStamp() + " | " + userName + " authentication successful.");
            }
        }

        public static string GetTimeStamp()
        {
            DateTime dateTime = DateTime.Now;
            return dateTime.ToString("HH:mm:ss.ff");
        }
    }
}
