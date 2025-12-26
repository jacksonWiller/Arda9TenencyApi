//using FluentValidation;

//namespace Arda9Template.Api.Application.Tenants.Commands.UpdateTenant;

//public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
//{
//    public UpdateTenantCommandValidator()
//    {
//        RuleFor(x => x.Id)
//            .NotEmpty().WithMessage("ID do tenant é obrigatório");

//        RuleFor(x => x.Name)
//            .MaximumLength(200).WithMessage("Nome não pode ter mais de 200 caracteres")
//            .When(x => !string.IsNullOrEmpty(x.Name));

//        RuleFor(x => x.Status)
//            .Must(status => new[] { "active", "inactive", "suspended" }.Contains(status!))
//            .When(x => !string.IsNullOrEmpty(x.Status))
//            .WithMessage("Status deve ser: active, inactive ou suspended");

//        RuleFor(x => x.Plan)
//            .Must(plan => new[] { "basic", "pro", "enterprise" }.Contains(plan!))
//            .When(x => !string.IsNullOrEmpty(x.Plan))
//            .WithMessage("Plano deve ser: basic, pro ou enterprise");

//        RuleFor(x => x.PrimaryColor)
//            .Matches(@"^#[0-9A-Fa-f]{6}$")
//            .When(x => !string.IsNullOrEmpty(x.PrimaryColor))
//            .WithMessage("Cor primária deve estar no formato hexadecimal (#RRGGBB)");

//        RuleFor(x => x.SecondaryColor)
//            .Matches(@"^#[0-9A-Fa-f]{6}$")
//            .When(x => !string.IsNullOrEmpty(x.SecondaryColor))
//            .WithMessage("Cor secundária deve estar no formato hexadecimal (#RRGGBB)");
//    }
//}
