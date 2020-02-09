using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Oracle_EF
{
    /*
     https://docs.oracle.com/en/database/oracle/oracle-data-access-components/19.3/odpnt/EFCoreAPI.html#GUID-770CD8EA-F963-48A5-A679-CAF471A4DB1A

     https://www.jetbrains.com/datagrip/?gclid=CjwKCAiAjrXxBRAPEiwAiM3DQkZEABG1VSjUKDQP10uaND9QSbxCPqM26a1nX-Oc_q7UocoemtgfyBoCitQQAvD_BwE

     https://github.com/oracle/dotnet-db-samples/blob/master/samples/dotnet-core/ef-core/get-started/create-model-save-query-scaffold.cs

     https://github.com/wnameless/docker-oracle-xe-11g

    */
    class Program
    {

        public class BloggingContext : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Post> Posts { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseOracle(@"User Id=system;Password=oracle;Data Source=localhost:1521/xe", b => b.UseOracleSQLCompatibility("11"));
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>().Property(p => p.BlogId).UseOracleIdentityColumn();
                modelBuilder.Entity<Blog>().Property(c => c.Url);
                modelBuilder.Entity<Blog>().ToTable("BLOG");

                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    entityType.Relational().TableName = entityType.Relational().TableName.ToUpper();

                    foreach (var property in entityType.GetProperties())
                    {
                        property.Relational().ColumnName = property.Relational().ColumnName.ToUpper();
                    }

                    foreach (var item in entityType.GetForeignKeys())
                    {
                        item.Relational().Name = item.Relational().Name.ToUpper();
                    }
                }

                base.OnModelCreating(modelBuilder);
            }
        }

        public class Blog
        {
            public int BlogId { get; set; }
            public string Url { get; set; }
            //public int Rating { get; set; }
            public List<Post> Posts { get; set; }
        }

        public class Post
        {
            public int PostId { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }

            public int BlogId { get; set; }
            public Blog Blog { get; set; }
        }

        static void Main(string[] args)
        {
            using (var db = new BloggingContext())
            {
                var blog = new Blog { Url = "https://blogs.oracle.com" };
                db.Blogs.Add(blog);
                db.SaveChanges();
            }

            using (var db = new BloggingContext())
            {
                var blogs = db.Blogs;
            }
        }
    }
}

/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Shared.ObjectValues;
using Domain.Shared.Entities;
using Domain.Shared.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using static Domain.Shared.EntityEnum;
using Domain.Shared.Interfaces.ExternalServices;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Repositories
{
    public class RepositoryShared<T> : IRepositoryShared<T> where T : Entity
    {
        protected DbContext _context;
        protected IPessoaExternalServiceShared _pessoaExternalServiceShared;
        protected IRelationshipIntegrity _relationshipIntegrity;
        public RepositoryShared(DbContext context, IRelationshipIntegrity relationshipIntegrity)
        {
            _context = context;
            _relationshipIntegrity = relationshipIntegrity;
        }

        public virtual async Task AddAsync(T t)
        {
            try
            {
                await _context.Set<T>().AddAsync(t);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual void Add(T t)
        {
            try
            {
                _context.Set<T>().Add(t);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task AddRangeAsync(List<T> lst)
        {
            try
            {
                await _context.Set<T>().AddRangeAsync(lst);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteRange(List<T> lst)
        {
            try
            {
                _context.Set<T>().RemoveRange(lst);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual T Update(T t, object key)
        {
            try
            {
                if (t == null)
                    return null;
                T exist = _context.Set<T>().Find(key);
                if (exist != null)
                {
                    _context.Entry(exist).CurrentValues.SetValues(t);
                    _context.SaveChanges();
                }
                return exist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<T> UpdateAsync(T t, object key)
        {
            try
            {
                if (t == null)
                    return null;

                T exist = await _context.Set<T>().FindAsync(key);

                if (exist == null)
                    throw new Exception("Dados informados inválidos. ");
                
                _context.Entry(exist).CurrentValues.SetValues(t);

                var objectValues = exist.GetType().GetProperties().Where(o => o.PropertyType.IsSubclassOf(typeof(ObjectValue)));

                foreach (PropertyInfo p in objectValues)
                    _context.Entry(p.GetValue(exist)).CurrentValues.SetValues(t.GetType().GetProperty(p.Name).GetValue(t, null));
            

                return exist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual void Delete(Guid id, string grupoEconomicoID)
        {
            try
            {
                T objectResult = _context.Set<T>().Where(o => o.ID == id && o.GrupoEconomicoID == grupoEconomicoID).First();

                if (objectResult == null)
                    throw new System.ArgumentException("Objeto não encontrado.");

                _context.Set<T>().Remove(objectResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task DeleteAsync(Guid id, string grupoEconomicoID)
        {
            try
            {
                T objectResult = await _context.Set<T>().Where(o => o.ID == id && o.GrupoEconomicoID == grupoEconomicoID).FirstAsync();

                if (objectResult == null)
                    throw new System.ArgumentException("Objeto não encontrado.");

                _context.Set<T>().Remove(objectResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<dynamic> GetAll(Filter filter)
        {
            try
            {
                //monta com o grupo economico
                var query = _context.Set<T>().AsQueryable();

                //Empresa
                if (!string.IsNullOrEmpty(filter.EmpresaID))
                    query = query.Where(o => o.EmpresaID == filter.EmpresaID);

                if (!string.IsNullOrEmpty(filter.GrupoEconomicoID))
                    query = query.Where(o => o.GrupoEconomicoID == filter.GrupoEconomicoID);

                //Condição
                if (!string.IsNullOrEmpty(filter.Condition))
                    query = query.Where(filter.Condition);

                //Status
                if (filter.Active)
                    query = query.Where(o => o.Status == EStatus.Ativo);

                //Decrescente?
                if (!string.IsNullOrEmpty(filter.OrderBy))
                    if (filter.SortDesc)
                        query = query.OrderBy(filter.OrderBy + " descending");
                    else
                        query = query.OrderBy(filter.OrderBy);

                //Paginação
                if (filter.Limit != -1)
                    query = query.Skip(filter.Offset).Take(filter.Limit);

                //Include
                if (!string.IsNullOrEmpty(filter.Include))
                {
                    var args = filter.Include.Split(',');

                    foreach (var item in args)
                    {
                        if (!string.IsNullOrEmpty(item.Trim()))
                        {
                            query = query.Include(item.Trim());
                        }
                    }
                }

                //Select
                if (!string.IsNullOrEmpty(filter.Select))
                {
                    if (filter.Distinct)
                        return query.Select(filter.Select).Distinct().ToDynamicList();
                    else
                        return query.Select(filter.Select).ToDynamicList();
                }
                else
                    return query.ToDynamicList();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public virtual async Task<dynamic> GetAllAsync(Filter filter)
        {
            try
            {
                //monta com o grupo economico
                var query = _context.Set<T>().AsQueryable();

                //Empresa
                if (!string.IsNullOrEmpty(filter.EmpresaID))
                    query = query.Where(o => o.EmpresaID == filter.EmpresaID);

                if (!string.IsNullOrEmpty(filter.GrupoEconomicoID))
                    query = query.Where(o => o.GrupoEconomicoID == filter.GrupoEconomicoID);

                //Condição
                if (!string.IsNullOrEmpty(filter.Condition))
                    query = query.Where(filter.Condition);

                //Status
                if (filter.Active)
                    query = query.Where(o => o.Status == EStatus.Ativo);

                // Empresas que o usuário tem acesso
                if(!filter.IsMaster)
                    if(filter.EmpresasIDs == null || !filter.EmpresasIDs.Any())
                        query = query.Where(o => o.EmpresaID == null);
                    else
                        query = query.Where(o => filter.EmpresasIDs.Contains(o.EmpresaID) || o.EmpresaID == null);

                //Decrescente?
                if (!string.IsNullOrEmpty(filter.OrderBy))
                    if (filter.SortDesc)
                        query = query.OrderBy(filter.OrderBy + " descending");
                    else
                        query = query.OrderBy(filter.OrderBy);

                //Paginação
                if (filter.Limit != -1)
                    query = query.Skip(filter.Offset).Take(filter.Limit);

                //Include
                if (!string.IsNullOrEmpty(filter.Include))
                {
                    var args = filter.Include.Split(',');

                    foreach (var item in args)
                    {
                        if (!string.IsNullOrEmpty(item.Trim()))
                        {
                            query = query.Include(item.Trim());
                        }
                    }
                }

                List<dynamic> entities;

                //Select
                if (!string.IsNullOrEmpty(filter.Select))
                {
                    if (filter.Distinct)
                        entities = await query.Select(filter.Select).Distinct().ToDynamicListAsync();
                    else
                        entities = await query.Select(filter.Select).ToDynamicListAsync();
                }
                else
                    entities = await query.ToDynamicListAsync();



                return entities;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public T Get(Guid id, string grupoEconomicoID)
        {
            try
            {
                return _context.Set<T>().Where(o => o.GrupoEconomicoID == grupoEconomicoID && o.ID == id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<T> GetAsync(Guid id, string grupoEconomicoID)
        {
            try
            {
                return await _context.Set<T>().Where(o => o.ID == id && (o.GrupoEconomicoID == grupoEconomicoID || o.GrupoEconomicoID == null)).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual T Find(Expression<Func<T, bool>> match, string include = null)
        {
            try
            {
                if (string.IsNullOrEmpty(include))
                    return _context.Set<T>().SingleOrDefault(match);
                else
                    return _context.Set<T>().Include(include).SingleOrDefault(match);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public virtual async Task<T> FindAsync(Expression<Func<T, bool>> match, string include = null)
        {
            try
            {
                if (string.IsNullOrEmpty(include))
                    return await _context.Set<T>().SingleOrDefaultAsync(match);
                else
                //Include
                {
                    var args = include.Split(',');
                    T entidade = null;
                    foreach (var item in args)
                    {
                        if (!string.IsNullOrEmpty(item.Trim()))
                        {
                            entidade = await _context.Set<T>().Include(item.Trim()).SingleOrDefaultAsync(match);
                        }
                    }
                    return entidade;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ICollection<T> FindAll(Expression<Func<T, bool>> match, string include = null)
        {
            try
            {
                var query = _context.Set<T>().AsQueryable();
                List<T> list;;

                if (string.IsNullOrEmpty(include))
                    return _context.Set<T>().Where(match).ToList();
                else
                //return _context.Set<T>().Where(match).Include(include).ToList();
                {
                    //if (!string.IsNullOrEmpty(filter.Include))
                    {
                        var args = include.Split(',');

                        foreach (var item in args)
                        {
                            if (!string.IsNullOrEmpty(item.Trim()))
                            {
                                query = query.Include(item.Trim());
                            }
                        }

                        query = query.Where(match);

                        return list = new List<T>(query);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ICollection<T>> FindAllAsync(Expression<Func<T, bool>> match, string include = null, bool isReporting = false)
        {
            try
            {
                if (string.IsNullOrEmpty(include))
                    return await _context.Set<T>().Where(match).ToListAsync();
                else
                //return await _defaultContext.Set<T>().Where(match).Include(include).ToListAsync();
                                    //return _context.Set<T>().Where(match).Include(include).ToList();
                {
                    {
                        var query = _context.Set<T>().AsQueryable();
                        List<T> list;

                        var args = include.Split(',');

                        foreach (var item in args)
                        {
                            if (!string.IsNullOrEmpty(item.Trim()))
                            {
                                query = query.Include(item.Trim());
                            }
                        }

                        query = query.Where(match);

                        return list = new List<T>(query);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Any(Expression<Func<T, bool>> match)
        {
            try
            {
                return _context.Set<T>().Any(match);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> match)
        {
            try
            {
                return await _context.Set<T>().AnyAsync(match);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int Count()
        {
            try
            {
                return _context.Set<T>().Count();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Set<T>().CountAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual void Save()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async virtual Task<int> SaveAsync()
        {
            try
            {
                if (!await _relationshipIntegrity.Commit())
                    throw new Exception("Erro ao inserir vínculos no Redis. ");

                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            try
            {
                IQueryable<T> query = _context.Set<T>().Where(predicate);
                return query;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<ICollection<T>> FindByAsyn(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _context.Set<T>().Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool disposed = false;
        public virtual void Dispose(bool disposing)
        {
            try
            {
                if (!this.disposed)
                {
                    if (disposing)
                    {
                        _context.Dispose();
                    }
                    this.disposed = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        public IQueryable<T> GetAllIncluding(string empresaID, int limit, int offset, params Expression<Func<T, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<List<T>> GetAllByIdAsync(Guid id)
        {
            try
            {
                return await _context.Set<T>().Where(o => o.ID == id).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
*/
