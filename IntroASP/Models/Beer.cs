using System;
using System.Collections.Generic;

namespace IntroASP.Models;

public partial class Beer
{
    public int BeerId { get; set; }

    public string Name { get; set; } = null!;

    public int BrandId { get; set; }

    public string? CountryCode { get; set; }

    public string? FlagUrl { get; set; }

    public virtual Brand Brand { get; set; } = null!;
}
