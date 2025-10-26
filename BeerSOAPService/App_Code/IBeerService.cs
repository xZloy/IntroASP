using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
namespace BeerSoapService
{
    [ServiceContract]
    public interface IBeerService
    {
        [OperationContract]
        List<BeerDto> GetBeers();

        [OperationContract]
        int CreateBeer(BeerCreateDto dto);   // devuelve el nuevo BeerId

        [OperationContract]
        bool DeleteBeer(int beerId);

        [OperationContract]
        bool UpdateBeer(BeerUpdateDto dto);

    }

    [DataContract]
    public class BeerDto
    {
        [DataMember] public int Id { get; set; }           // BeerId
        [DataMember] public string Name { get; set; }
        [DataMember] public string Brand { get; set; }     // Brand.Name
        [DataMember] public string CountryCode { get; set; }
        [DataMember] public string FlagUrl { get; set; }
    }

    [DataContract]
    public class BeerCreateDto
    {
        [DataMember(IsRequired = true)] public string Name { get; set; }
        [DataMember(IsRequired = true)] public int BrandId { get; set; }
        [DataMember] public string CountryCode { get; set; }
        [DataMember] public string FlagUrl { get; set; }
    }

    [DataContract]
    public class BeerUpdateDto
    {
        [DataMember(IsRequired = true)]
        public int Id { get; set; }               // BeerId en la BD

        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        [DataMember(IsRequired = true)]
        public int BrandId { get; set; }

        [DataMember] public string CountryCode { get; set; }  // puede ser null
        [DataMember] public string FlagUrl { get; set; }      // puede ser null
    }

}
