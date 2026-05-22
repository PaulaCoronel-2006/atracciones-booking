using Mapster;
using Microservicios.Atracciones.Booking.DataAccess.Entities;
using Microservicios.Atracciones.Booking.DataManagement.Models;

namespace Microservicios.Atracciones.Booking.DataManagement.Mappers;

public class ReviewMapperConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Review, ReviewNode>()
            // ClientName y AttractionName deberían venir de snapshots o ser resueltos externamente si no están en la entidad
            .Map(dest => dest.Rating, src => (byte)src.OverallScore)
            .Map(dest => dest.Ratings, src => src.Ratings);

        config.NewConfig<ReviewRating, ReviewRatingNode>()
            .Map(dest => dest.Criteria, src => src.Criteria != null ? src.Criteria.Name : string.Empty)
            .Map(dest => dest.Rating, src => (byte)src.Score);

        config.NewConfig<ReviewNode, Review>()
            .Map(dest => dest.OverallScore, src => (decimal)src.Rating)
            .Map(dest => dest.Ratings, src => src.Ratings)
            .Ignore(dest => dest.Booking)
            .Ignore(dest => dest.Media);
            
        config.NewConfig<ReviewRatingNode, ReviewRating>()
            .Map(dest => dest.Score, src => (short)src.Rating)
            .Ignore(dest => dest.Review)
            .Ignore(dest => dest.Criteria);
    }
}
