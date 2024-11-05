using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pusula.Training.HealthCare.Departments;
using Pusula.Training.HealthCare.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Pusula.Training.HealthCare.MedicalServices;

public class EfCoreMedicalServiceRepository(IDbContextProvider<HealthCareDbContext> dbContextProvider)
    : EfCoreRepository<HealthCareDbContext, MedicalService, Guid>(dbContextProvider: dbContextProvider),
        IMedicalServiceRepository
{
    public async Task DeleteAllAsync(
        string? name = null,
        double? costMin = null,
        double? costMax = null,
        DateTime? serviceDateMin = null,
        DateTime? serviceDateMax = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        query = ApplyFilter(query, name, costMin, costMax, serviceDateMin, serviceDateMax);

        var ids = query.Select(x => x.Id).ToList();
        await DeleteManyAsync(ids, cancellationToken: GetCancellationToken(cancellationToken));
    }

    public async Task<List<MedicalService>> GetListAsync(
        string? name = null,
        double? costMin = null,
        double? costMax = null,
        DateTime? serviceDateMin = null,
        DateTime? serviceDateMax = null,
        string? sorting = null,
        int maxResultCount = 10,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter((await GetQueryableAsync()), name,
            costMin, costMax, serviceDateMin, serviceDateMax);

        query = query.OrderBy(string.IsNullOrWhiteSpace(sorting)
            ? MedicalServiceConsts.GetDefaultSorting(false)
            : sorting);
        return await query.PageBy(skipCount, maxResultCount).ToListAsync(cancellationToken);
    }

    public async Task<MedicalServiceWithDetails> GetWithDetailsAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        
        return (await (await GetDbSetAsync())
            .Include(medicalService => medicalService.Departments)
            .Where(medicalService => medicalService.Id == id)
            .Select(medicalService => new MedicalServiceWithDetails
            {
                MedicalService = medicalService,
                Departments = medicalService.Departments.Any() ? medicalService.Departments.ToList() : new List<Department>()
            })
            .FirstOrDefaultAsync(cancellationToken))!;
    }

    public async Task<long> GetCountAsync(
        string? name = null,
        double? costMin = null,
        double? costMax = null,
        DateTime? serviceDateMin = null,
        DateTime? serviceDateMax = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter((await GetDbSetAsync()), name,
            costMin, costMax, serviceDateMin, serviceDateMax);

        return await query.LongCountAsync(cancellationToken);
    }

    #region Filters

    protected virtual IQueryable<MedicalService> ApplyFilter(
        IQueryable<MedicalService> query,
        string? name = null,
        double? costMin = null,
        double? costMax = null,
        DateTime? serviceDateMin = null,
        DateTime? serviceDateMax = null)
    {
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(name), e => e.Name!.Contains(name!))
            .WhereIf(costMin.HasValue,
                e => e.Cost >= costMin!.Value)
            .WhereIf(costMax.HasValue,
                e => e.Cost <= costMax!.Value)
            .WhereIf(serviceDateMin.HasValue, e => e.ServiceCreatedAt >= serviceDateMin!.Value)
            .WhereIf(serviceDateMax.HasValue, e => e.ServiceCreatedAt <= serviceDateMax!.Value);
    }

    #endregion
}