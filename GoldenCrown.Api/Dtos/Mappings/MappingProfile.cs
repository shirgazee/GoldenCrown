using AutoMapper;
using GoldenCrown.Api.Dtos.Finance;
using GoldenCrown.Application.Dtos.Finance;

namespace GoldenCrown.Api.Dtos.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TransactionHistoryDto, TransactionHistoryResponse>()
            .ForMember(x => x.Sum, 
                opt => opt.MapFrom(dto => dto.Amount));
    }
}