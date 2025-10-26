using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


public class Beer
{
    public int BeerId { get; set; }      // PK según tu tabla
    public string Name { get; set; }
    public int BrandId { get; set; }
    public string CountryCode { get; set; }
    public string FlagUrl { get; set; }

    // (Opcional) navegación si tienes tabla Brand
    public Brand Brand { get; set; }
}
