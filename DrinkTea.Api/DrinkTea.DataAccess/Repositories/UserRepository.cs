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
            // Явно указываем алиасы, чтобы Dapper точно нашел свойства
            const string sql = @"
        SELECT 
            id as Id, 
            fullname as FullName, 
            login as Login, 
            passwordhash as PasswordHash, 
            role as Role, 
            balance as Balance 
        FROM Users 
        WHERE login = @Login;";

            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Login = login });
        }


        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var connection = db.CreateConnection();
            const string sql = "SELECT * FROM Users WHERE Id = @Id;";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task CreateAsync(User user)
        {
            using var connection = db.CreateConnection();
            const string sql = @"
            INSERT INTO Users (Id, FullName, Login, PasswordHash, Role, Balance)
            VALUES (@Id, @FullName, @Login, @PasswordHash, @Role, @Balance);";

            await connection.ExecuteAsync(sql, user);
        }

        public async Task UpdateBalanceAsync(Guid userId, decimal amount)
        {
            using var connection = db.CreateConnection();
            const string sql = "UPDATE Users SET Balance = Balance + @Amount WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, new { Id = userId, Amount = amount });
        }

        public async Task UpdateAsync(User user)
        {
            using var connection = db.CreateConnection();
            const string sql = @"
        UPDATE Users 
        SET FullName = @FullName, 
            Login = @Login, 
            Role = @Role 
        WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, user);
        }
    }
}
