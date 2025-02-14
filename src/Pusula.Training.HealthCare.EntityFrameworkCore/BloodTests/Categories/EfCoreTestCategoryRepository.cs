﻿using Microsoft.EntityFrameworkCore;
using Pusula.Training.HealthCare.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Pusula.Training.HealthCare.BloodTests.Categories
{
    public class EfCoreTestCategoryRepository(IDbContextProvider<HealthCareDbContext> dbContextProvider)
        : EfCoreRepository<HealthCareDbContext, TestCategory, Guid>(dbContextProvider), ITestCategoryRepository
    {
        public virtual async Task<List<TestCategory>> GetListAsync(
            string? filterText = null,
            string? name = null,
            string? description = null,
            string? url = null,
            double? price = null,
            string? sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter(await GetQueryableAsync(), filterText, name, description, url, price);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? BloodTestConst.GetTestCategoryDefaultSorting(false) : sorting);

            return await query.PageBy(skipCount, maxResultCount).ToListAsync(cancellationToken);
        }


        public virtual async Task<long> GetCountAsync(
            string? filterText = null,
            string? name = null,
            string? description = null,
            string? url = null,
            double? price = null,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter(await GetQueryableAsync(), filterText, name, description, url);
            return await query.LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<TestCategory> ApplyFilter(
            IQueryable<TestCategory> query,
            string? filterText = null,
            string? name = null,
            string? description = null,
            string? url = null,
            double? price = null) =>
            query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => EF.Functions.ILike(e.Name, $"%{filterText}%"))
                .WhereIf(!string.IsNullOrWhiteSpace(name), e => EF.Functions.ILike(e.Name, $"%{filterText}%"))
                .WhereIf(!string.IsNullOrWhiteSpace(description), e => EF.Functions.ILike(e.Description, $"%{filterText}%"))
                .WhereIf(!string.IsNullOrWhiteSpace(url), e => EF.Functions.ILike(e.Url, $"%{filterText}%"))
                .WhereIf(price.HasValue, e => e.Price == price);
    }
}
