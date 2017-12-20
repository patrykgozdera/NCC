using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomSocket;
namespace NCC
{
    class AddressesTable
    {
        public const char SEPARATOR = ' ';
        public const int USER_NAME_POSITION = 0;
        public const int ADDRESS_POSITION = 1;

        private static List<AddressesRow> translatedAddresses;

        public static void init()
        {
            translatedAddresses = new List<AddressesRow>();
            loadTranslatedAddresses();
        }

        private static void loadTranslatedAddresses()
        {
            StreamReader reader = getFileStreamReader(Config.getProperty("translatedAddressesFile"));
            String textLine = null;
            String[] parameters = null;
            String userName = null;
            String address = null;            
            while ((textLine = reader.ReadLine()) != null)
            {
                parameters = textLine.Split(SEPARATOR);
                userName = parameters[USER_NAME_POSITION];
                address = parameters[ADDRESS_POSITION];
                AddressesRow row = createAddressesRow(userName, address);
                translatedAddresses.Add(row);
            }
        }

        private static StreamReader getFileStreamReader(String fileName)
        {
            var fileStream = new System.IO.FileStream(fileName,
                                          System.IO.FileMode.Open,
                                          System.IO.FileAccess.Read,
                                          System.IO.FileShare.ReadWrite);
            var file = new System.IO.StreamReader(fileStream, System.Text.Encoding.UTF8, true, 128);
            return file;
        }

        private static AddressesRow createAddressesRow(String userName, String address)
        {
            AddressesRow row = new AddressesRow(userName, address);
            return row;
        }

        public static String getAddress(string userName)
        {
            string address = null;
            for (int i = 0; i < translatedAddresses.Count; i++)
            {
                AddressesRow checkedRow = translatedAddresses.ElementAt(i);
                string _userName = checkedRow.getUserName();                
                if (userName.Equals(_userName))
                {
                    address = checkedRow.getAddress();
                }
            }
            return address;
        }

        public static Boolean isUserAuthenticated(string userName)
        {
            bool val = false;
            for (int i = 0; i < translatedAddresses.Count; i++)
            {
                AddressesRow checkedRow = translatedAddresses.ElementAt(i);
                string _userName = checkedRow.getUserName();
                if (userName.Equals(_userName))
                {
                    val = true;
                }
            }
            return val;
        }
    }
}
