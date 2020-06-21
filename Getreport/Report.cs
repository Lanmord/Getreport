using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getreport
{
    class Report
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public double Debt { get; set; }
        public int Privilege { get; set; }
        public bool Status { get; set; }
        public double TotalAmount { get; set; }
        public double PrivilegeAmount { get; set; }
        public List<Accrual> Accruals { get; set; }
        public List<Report> ReportArr { get; set; }
        public Person Person { set; get; }
        public double AmountAccrual { get; set; }


        public Report(int year, int month, int privilege, bool status, List<Accrual> accruals, List<Report> reports)
        {
            Year = year;
            Month = month;
            Privilege = privilege;
            Status = status;
            Accruals = accruals;
            ReportArr = reports;
            findDebt();
            findAmountAccrual();
            findPrivilegeAmount();
            findTotalAmount(PrivilegeAmount);
            
        }
        private void findAmountAccrual()
        {
            AmountAccrual = 0;

            for (int i = 0; i < Accruals.Count; i++)
            {
                AmountAccrual += Accruals[i].Accrued;
            }
        }
        private void findPrivilegeAmount()
        {
            PrivilegeAmount = (AmountAccrual / 100) * Privilege;
        }
        private void findTotalAmount(double PrivilegeAmount)
        {
            TotalAmount = AmountAccrual - PrivilegeAmount + Debt;
        }
        private void findDebt()
        {
            for (int i = 0; i < ReportArr.Count; i++)
            {
                if((ReportArr[i].Status == false && ReportArr[i].Year == Year && ReportArr[i].Month == Month - 1) || (ReportArr[i].Status == false && ReportArr[i].Year == Year - 1 && ReportArr[i].Month == 12))
                {
                    Debt = ReportArr[i].TotalAmount;
                }
                else
                {
                    Debt = 0;
                }
            }
        }
    }
}
