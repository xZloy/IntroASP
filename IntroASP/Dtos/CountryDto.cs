namespace IntroASP.Dtos
{
    public class CountryDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? FlagPng { get; set; }
        public string? FlagSvg { get; set; }
    }
    public class CountryApiModel
    {
        public NameObj? Name { get; set; }
        public FlagsObj? Flags { get; set; }
        public string? Cca2 { get; set; }
    }

    public class NameObj
    {
        public string? Common { get; set; }
    }

    public class FlagsObj
    {
        public string? Png { get; set; }
        public string? Svg { get; set; }
    }
}
