using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Brand
/// </summary>
public class Brand
{
    
        public int BrandId { get; set; }
        public string Name { get; set; }
        public ICollection<Beer> Beers { get; set; }
}