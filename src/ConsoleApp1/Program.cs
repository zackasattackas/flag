using System;
using flag;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {          
            ref var ip = ref Flag.IPAddress("--ipaddress", null, "The IP Address of the host");
            ref var date = ref Flag.DateTime("--max-date", DateTime.Now, "The date to use");

            Flag.Parse();

            if (ip != null)
            {
                Console.WriteLine(ip);
            }

            if (date != null)
            {
                Console.WriteLine(date);
            }
        }
    }
}
