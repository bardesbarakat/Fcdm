using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cdm.Models
{
    public class SecondmentCountByJobVM
    {
        public string JobTitle { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }

        public int Total => MaleCount + FemaleCount;

        // نتركه الآن فارغ ونستخدمه لاحقًا لو ظهر حقل "متفرغ"
        public int SabbaticalCount { get; set; } = 0;
    }



}