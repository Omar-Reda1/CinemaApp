﻿using System.Linq.Expressions;

namespace CinemaApp.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAsync(
        Expression<Func<T, bool>>? expression = null,
        Expression<Func<T, object>>[]? includes = null,
        bool tracked = true,
        CancellationToken cancellationToken = default
    );

        Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            bool tracked = true,
            CancellationToken cancellationToken = default
        );

        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Delete(T entity);
        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
