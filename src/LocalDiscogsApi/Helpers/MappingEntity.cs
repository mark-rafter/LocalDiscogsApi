using AutoMapper;
using LocalDiscogsApi.Models;
using MongoDB.Driver.GeoJsonObjectModel;

namespace LocalDiscogsApi.Helpers
{
    public class MappingEntity : Profile
    {
        public MappingEntity()
        {
            CreateMap<Models.Discogs.Listing, SellerListing>()
                .ForMember(dest => dest.Description, o => o.MapFrom(src => src.Release.Description))
                .ForMember(dest => dest.ReleaseId, o => o.MapFrom(src => src.Release.Id))
                .ForMember(dest => dest.Price, o => o.MapFrom(src => $"{src.Price.Currency} {src.Price.Value}"));

            CreateMap<Models.VinylHub.Shop, Store>()
                .ForMember(dest => dest.Location, o => o.MapFrom(src =>
                    new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                        new GeoJson2DGeographicCoordinates(src.Coordinates.Lng, src.Coordinates.Lat)))
                        );
        }
    }
}