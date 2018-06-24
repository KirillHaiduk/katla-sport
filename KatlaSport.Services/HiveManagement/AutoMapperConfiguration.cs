using AutoMapper;

namespace KatlaSport.Services.HiveManagement
{
    public class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<HiveManagementMappingProfile>();
            });

            Mapper.Configuration.AssertConfigurationIsValid();
        }
    }
}
