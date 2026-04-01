using Dapper;
using DrinkTea.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DrinkTea.DataAccess.Repositories
{
    public class UserRepository(DbConnectionFactory db):IUserRepository
    {
        public async Task<User?> GetByLoginAsync(string login)
        {
            using var connection = db.CreateConnection();
            const string sql = "SELECT * FROM Users WHERE Login = @Login;";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Login = login });
        }
    }
}
