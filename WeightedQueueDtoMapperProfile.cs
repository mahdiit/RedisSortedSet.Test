using AutoMapper;
using StackExchange.Redis;

namespace RedisSortedSet.Test
{
    public class WeightedQueueDtoMapperProfile : Profile
    {
        public WeightedQueueDtoMapperProfile()
        {
            CreateMap<SortedSetEntry, WeightedQueueDto>()
                .ForMember(x => x.Value, opt => opt.MapFrom(p => p.Element.ToString()))
                .ForMember(x => x.Weight, opt => opt.MapFrom(p => p.Score));
        }
    }
}
