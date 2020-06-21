using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getreport
{
    class Accrual
    {
        
        public string NameServ { get; set; }
        public double Amount { get; set; }
        public double Accrued { get; set; }
        public double Tariff { get; set; }

        public Accrual(string nameServ, double amount, double tariff)
        {
            NameServ = nameServ;
            Amount = amount;
            Tariff = tariff;
            Accrued = Tariff * Amount;
        }
    }
}
