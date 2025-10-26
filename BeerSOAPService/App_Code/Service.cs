using BeerSoapService;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;
namespace BeerSoapService   // <-- anota EXACTO este nombre
{
    public class Service : IBeerService
    {
        private readonly PubContext _db = new PubContext();

        public List<BeerDto> GetBeers()
        {
            return _db.Beer
                .Include(b => b.Brand)
                .Select(b => new BeerDto
                {
                    Id = b.BeerId,
                    Name = b.Name,
                    Brand = b.Brand != null ? b.Brand.Name : null,
                    CountryCode = b.CountryCode,
                    FlagUrl = b.FlagUrl
                })
                .ToList();
        }

        public int CreateBeer(BeerCreateDto dto)
        {
            // Validaciones simples
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new FaultException("Name es requerido.");

            var brandExists = _db.Brand.Any(x => x.BrandId == dto.BrandId);
            if (!brandExists)
                throw new FaultException(string.Format("BrandId {0} no existe.", dto.BrandId));

            // Como aún no usas la API REST, forzamos valores nulos
            var entity = new Beer
            {
                Name = dto.Name.Trim(),
                BrandId = dto.BrandId,
                CountryCode = null,  // Valor fijo
                FlagUrl = null       // Valor fijo
            };

            _db.Beer.Add(entity);
            _db.SaveChanges();

            return entity.BeerId;
        }

        public bool DeleteBeer(int beerId)
        {
            var entity = _db.Beer.Find(beerId);
            if (entity == null) return false;
            _db.Beer.Remove(entity);
            _db.SaveChanges();
            return true;
        }
        
            
        public bool UpdateBeer(BeerUpdateDto dto)
        {
            var beer = _db.Beer.FirstOrDefault(b => b.BeerId == dto.Id);
            if (beer == null)
                throw new FaultException(string.Format("BeerId {0} no existe.", dto.Id));

            var brandExists = _db.Brand.Any(b => b.BrandId == dto.BrandId);
            if (!brandExists)
                throw new FaultException(string.Format("BrandId {0} no existe.", dto.BrandId));

            // Actualizamos campos
            beer.Name = dto.Name.Trim();
            beer.BrandId = dto.BrandId;
            beer.CountryCode = dto.CountryCode ?? null;
            beer.FlagUrl = dto.FlagUrl ?? null;

            _db.SaveChanges();
            return true;
        }

    }
}
