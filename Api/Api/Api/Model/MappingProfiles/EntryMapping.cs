using AutoMapper;

namespace Api.Model.MappingProfiles
{
    public class EntryMapping : Profile
    {
        public EntryMapping()
        {
            CreateMap<Entry, EntryDto>().ForMember(c => c.POI, opt => opt.MapFrom(src => src.POI.Name))
                .ForMember(c => c.UserName, opt => opt.MapFrom(src => src.User.Username));
            CreateMap<AddEntry, Entry>().ForMember(c => c.POI, opt => opt.Ignore());
            CreateMap<UpdateEntry, Entry>().ForAllMembers(opt => opt.Condition((src, dest, srcmember) => srcmember != null));



        }
    }
}
