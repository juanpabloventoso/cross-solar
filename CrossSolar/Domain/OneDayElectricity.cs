using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossSolar.Domain;
using CrossSolar.Models;

namespace CrossSolar.Domain
{
    public class OneDayElectricity
    {
        public int Id { get; set; }

        public string PanelId { get; set; }

        public double Sum { get; set; }

        public double Average { get; set; }

        public double Maximum { get; set; }

        public double Minimum { get; set; }

        public DateTime DateTime { get; set; }
    }
}
