using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SkFabricatorAndErector.Application.Features.Authentication;
using SkFabricatorAndErector.Application.Features.Catalog;
using SkFabricatorAndErector.Application.Features.Inquiries;
using SkFabricatorAndErector.Application.Features.Media;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Application.Validators;

namespace SkFabricatorAndErector.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateInquiryRequestValidator>();

        services.AddScoped<IInquiryService, InquiryService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IHomeSliderService, HomeSliderService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IOurServiceService, OurServiceService>();
        services.AddScoped<ITeamMemberService, TeamMemberService>();
        services.AddScoped<IClientDetailsService, ClientDetailsService>();
        return services;
    }
}
