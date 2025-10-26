using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerCsWorker.Dtos
{
    public class BeerDto
    {
        public int BeerId { get; set; }
        public string? Name { get; set; }
        public int BrandId { get; set; }
        public string? CountryCode { get; set; }
        public string? FlagUrl { get; set; }
    }
}
