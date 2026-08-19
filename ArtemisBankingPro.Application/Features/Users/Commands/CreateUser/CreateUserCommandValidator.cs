using FluentValidation;

namespace ArtemisBankingPro.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(p => p.FirstName)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(p => p.LastName)
                .NotEmpty().WithMessage("El apellido es requerido.");

            RuleFor(p => p.Cedula)
                .NotEmpty().WithMessage("La cédula es requerida.")
                .Length(11).WithMessage("La cédula debe tener exactamente 11 dígitos.");

            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("El correo es requerido.")
                .EmailAddress().WithMessage("Debe ser una dirección de correo válida.");

            RuleFor(p => p.Username)
                .NotEmpty().WithMessage("El nombre de usuario es requerido.");

            RuleFor(p => p.Password)
                .NotEmpty().WithMessage("La contraseña es requerida.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
                .Matches(@"[A-Z]+").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
                .Matches(@"[a-z]+").WithMessage("La contraseña debe contener al menos una letra minúscula.")
                .Matches(@"[0-9]+").WithMessage("La contraseña debe contener al menos un número.")
                .Matches(@"[\!\?\*\.]+").WithMessage("La contraseña debe contener al menos un carácter especial (!? *.).");

            RuleFor(p => p.RoleId)
                .InclusiveBetween(1, 4).WithMessage("Rol inválido. Debe ser entre 1 y 4.");
        }
    }
}