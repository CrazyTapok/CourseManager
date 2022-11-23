using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseManager
{
    public class Currency
    {
        public int Cur_ID { get; set; }
        public int Cur_Periodicity { get; set; }
        public string Cur_Code { get; set; }
        public string Cur_Abbreviation { get; set; }
        public string Cur_Name { get; set; }
        public string Cur_Name_Eng { get; set; }
        public DateTime Date { get; set; }
        public decimal Cur_OfficialRate { get; set; }
    }
}
