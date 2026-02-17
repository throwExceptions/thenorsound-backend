using Application.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public class UpdateUserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<UpdateUserCommand, bool>
{
    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID '{request.Id}' not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            // Check for duplicate email if changing
            if (request.Email != user.Email)
            {
                var existingUser = await userRepository.GetByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    throw new DuplicateException($"User with email '{request.Email}' already exists.");
                }
            }

            user.Email = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName;

        if (request.Role.HasValue)
            user.Role = (Domain.Enums.Role)request.Role.Value;

        // Crew-specific fields
        if (!string.IsNullOrWhiteSpace(request.Occupation))
            user.Occupation = request.Occupation;

        if (request.CostPerHour.HasValue)
            user.CostPerHour = request.CostPerHour;

        if (!string.IsNullOrWhiteSpace(request.About))
            user.About = request.About;

        if (request.PreviousJobs != null)
            user.PreviousJobs = request.PreviousJobs;

        if (request.Skills != null)
            user.Skills = request.Skills;

        if (!string.IsNullOrWhiteSpace(request.Image))
            user.Image = request.Image;

        if (!string.IsNullOrWhiteSpace(request.Phone))
            user.Phone = request.Phone;

        if (!string.IsNullOrWhiteSpace(request.Ssn))
            user.Ssn = request.Ssn;

        if (!string.IsNullOrWhiteSpace(request.Address))
            user.Address = request.Address;

        if (!string.IsNullOrWhiteSpace(request.City))
            user.City = request.City;

        if (!string.IsNullOrWhiteSpace(request.Zip))
            user.Zip = request.Zip;

        if (!string.IsNullOrWhiteSpace(request.Co))
            user.Co = request.Co;

        if (!string.IsNullOrWhiteSpace(request.Bank))
            user.Bank = request.Bank;

        if (!string.IsNullOrWhiteSpace(request.BankAccount))
            user.BankAccount = request.BankAccount;

        if (request.IsEmployee.HasValue)
            user.IsEmployee = request.IsEmployee;

        user.UpdatedAt = DateTime.UtcNow;

        return await userRepository.UpdateAsync(request.Id, user);
    }
}
