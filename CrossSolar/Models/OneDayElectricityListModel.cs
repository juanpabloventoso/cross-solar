using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossSolar.Domain;

namespace CrossSolar.Models
{
    public class OneDayElectricityListModel
    {
        public IEnumerable<OneDayElectricityModel> OneDayElectricitys { get; set; }
    }
}
