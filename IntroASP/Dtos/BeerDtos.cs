namespace IntroASP.Dtos
{
    public class BeerDto
    {
        public int BeerId { get; set; }
        public string? Name { get; set; }
        public int BrandId { get; set; }
        public string? CountryCode { get; set; }
        public string? FlagUrl { get; set; }
    }
    public class BeerUpdateDto
    {
        public int BeerId { get; set; }   
        public string Name { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string? CountryCode { get; set; }
        public string? FlagUrl { get; set; }
    }
}
