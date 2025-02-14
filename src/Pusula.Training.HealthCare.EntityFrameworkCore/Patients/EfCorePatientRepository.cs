using Microsoft.EntityFrameworkCore;
using Pusula.Training.HealthCare.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Pusula.Training.HealthCare.GlobalExceptions;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Pusula.Training.HealthCare.Patients;

public class EfCorePatientRepository(IDbContextProvider<HealthCareDbContext> dbContextProvider)
    : EfCoreRepository<HealthCareDbContext, Patient, Guid>(dbContextProvider), IPatientRepository
{
    public virtual async Task DeleteAllAsync(
        string? filterText = null,
        int? patientNumber = null,
        string? firstName = null,
        string? lastName = null,
        string? identityNumber = null,
        string? passportNumber = null,
        string? nationality = null,
        DateTime? birthDateMin = null,
        DateTime? birthDateMax = null,
        string? emailAddress = null,
        string? mobilePhoneNumber = null,
        EnumPatientTypes? patientType = null,
        EnumDiscountGroup? discountGroup = null,
        EnumGender? gender = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        query = ApplyFilter(query, filterText, patientNumber, firstName, lastName, identityNumber, passportNumber, nationality, birthDateMin, birthDateMax, 
            emailAddress, mobilePhoneNumber, patientType, discountGroup, gender);

        var ids = query.Select(x => x.Id);
        await DeleteManyAsync(ids, cancellationToken: GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<Patient>> GetListAsync(
        string? filterText = null,
        int? patientNumber = null,
        string? firstName = null,
        string? lastName = null,
        string? identityNumber = null,
        string? passportNumber = null,
        string? nationality = null,
        DateTime? birthDateMin = null,
        DateTime? birthDateMax = null,
        string? emailAddress = null,
        string? mobilePhoneNumber = null,
        EnumPatientTypes? patientType = null,
        EnumDiscountGroup? discountGroup = null,
        EnumGender? gender = null,
        string? sorting = null,
        bool? isDeleted = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter((await GetQueryableAsync()), filterText, patientNumber, firstName, lastName, identityNumber, passportNumber, nationality, birthDateMin, 
            birthDateMax, emailAddress, mobilePhoneNumber, patientType, discountGroup, gender, isDeleted);
      query = query.OrderBy(e => e.IsDeleted) // IsDeleted'e göre önce sıralama
          .ThenByDescending(e => e.CreationTime) // IsDeleted içindeki her grupta en son oluşturulan en başta görünsün
          .ThenBy(string.IsNullOrWhiteSpace(sorting) ? PatientConsts.GetDefaultSorting(false) : sorting); // Ardından mevcut sıralama
        return await query.PageBy(skipCount, maxResultCount).ToListAsync(cancellationToken);
    }
    
    public virtual async Task<Patient> GetPatientByNumberAsync(
        int patientNumber ,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        var patient = await dbContext.Patients
            .Where(a => a.PatientNumber == patientNumber)
            .FirstOrDefaultAsync(cancellationToken);
        
        HealthCareGlobalException.ThrowIf(
            string.Format(HealthCareDomainErrorCodes.PatientNotFoundByNumber_MESSAGE, patientNumber),
            HealthCareDomainErrorCodes.PatientNotFoundByNumber_CODE,
            patient == null
        );
        
        return patient!;
    }
    
    public virtual async Task<Patient> GetPatientByIdentityAsync(
        string identityNumber ,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

       var patient =  await dbContext.Patients
            .Where(a => a.IdentityNumber == identityNumber)
            .FirstOrDefaultAsync(cancellationToken);
        
       HealthCareGlobalException.ThrowIf(
           $"Patient with identity number {identityNumber} not found.",
           patient == null);

       return patient!;

    }


    public virtual async Task<long> GetCountAsync(
        string? filterText = null,
        int? patientNumber = null,
        string? firstName = null,
        string? lastName = null,
        string? identityNumber = null,
        string? passportNumber = null,
        string? nationality = null,
        DateTime? birthDateMin = null,
        DateTime? birthDateMax = null,
        string? emailAddress = null,
        string? mobilePhoneNumber = null,
        EnumPatientTypes? patientType = null,
        EnumDiscountGroup? discountGroup = null,
        EnumGender? gender = null,
        bool? isDeleted = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter((await GetDbSetAsync()), filterText, patientNumber, firstName, lastName, identityNumber, passportNumber, nationality, birthDateMin, birthDateMax, 
            emailAddress, mobilePhoneNumber, patientType, discountGroup, gender, isDeleted);
        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    protected virtual IQueryable<Patient> ApplyFilter(
        IQueryable<Patient> query,
        string? filterText = null,
        int? patientNumber = null,
        string? firstName = null,
        string? lastName = null,
        string? identityNumber = null,
        string? passportNumber = null,
        string? nationality = null,
        DateTime? birthDateMin = null,
        DateTime? birthDateMax = null,
        string? emailAddress = null,
        string? mobilePhoneNumber = null,
        EnumPatientTypes? patientType = null,
        EnumDiscountGroup? discountGroup = null,
        EnumGender? gender = null, 
        bool? isDeleted = null)
    {
        // sadece isDeleted true ise silinen verileri de göster 
        query = isDeleted == true ? query.IgnoreQueryFilters() : query;
        
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(filterText),
                e => EF.Functions.ILike(e.FirstName, $"%{filterText}%") || EF.Functions.ILike(e.LastName, $"%{filterText}%") ||
                     e.IdentityNumber!.Contains(filterText!) || e.EmailAddress!.Contains(filterText!) ||
                     e.MobilePhoneNumber!.Contains(filterText!))
            .WhereIf(!string.IsNullOrWhiteSpace(firstName), e => EF.Functions.ILike(e.FirstName, $"%{firstName}%"))
            .WhereIf(!string.IsNullOrWhiteSpace(lastName), e => EF.Functions.ILike(e.LastName, $"%{lastName}%"))
            .WhereIf(!string.IsNullOrWhiteSpace(identityNumber), e => e.IdentityNumber!.Contains(identityNumber!))
            .WhereIf(!string.IsNullOrWhiteSpace(passportNumber), e => e.PassportNumber!.Contains(passportNumber!))
            .WhereIf(birthDateMin.HasValue, e => e.BirthDate >= birthDateMin!.Value)
            .WhereIf(birthDateMax.HasValue, e => e.BirthDate <= birthDateMax!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(emailAddress), e => e.EmailAddress!.Contains(emailAddress!))
            .WhereIf(!string.IsNullOrWhiteSpace(mobilePhoneNumber), e => e.MobilePhoneNumber!.Contains(mobilePhoneNumber!))
            .WhereIf(patientType.HasValue, e => e.PatientType == patientType)
            .WhereIf(discountGroup.HasValue, e => e.DiscountGroup != null && e.DiscountGroup == discountGroup)
            .WhereIf(gender.HasValue, e => e.Gender == gender)
            .WhereIf(isDeleted == true, e => e.IsDeleted) // Sadece isDeleted true ise filtre uygula
            .WhereIf(patientNumber.HasValue, e => e.PatientNumber == patientNumber);
    }
}