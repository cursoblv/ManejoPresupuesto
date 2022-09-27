﻿using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Borrar(int id);
        Task Crear(TipoCuenta tipoCuenta);
        Task Ordenar(IEnumerable<TipoCuenta> tiposcuentasOrdenados);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
    }

    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;

        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>
                //(@"INSERT INTO TipoCuentas (Nombre, UsuarioId, Orden)
                //    VALUES (@Nombre, @UsuarioId, 0);
                //    SELECT SCOPE_IDENTITY();", tipoCuenta);
                ("TiposCuentas_Insertar",
                new
                {
                    usuarioId = tipoCuenta.UsuarioId,
                    nombre = tipoCuenta.Nombre
                },
                commandType: System.Data.CommandType.StoredProcedure
                );

            tipoCuenta.Id = id;
        }


        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>
                (@"SELECT 1
                    FROM TipoCuentas
                    WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;"
                    , new { nombre, usuarioId });

            return existe == 1;
        }



        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TipoCuenta>(
                    @"SELECT Id, Nombre, Orden
                        FROM TipoCuentas
                        WHERE UsuarioId = @UsuarioId 
                        ORDER BY Orden"
                        , new { usuarioId });
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                    @"UPDATE TipoCuentas
                        SET Nombre = @Nombre
                        WHERE Id = @Id"
                        , tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(
                        @"SELECT Id, Nombre, Orden
                            FROM TipoCuentas
                            WHERE Id = @Id AND UsuarioId = @UsuarioId"
                , new { id, usuarioId });
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                    @"DELETE TipoCuentas WHERE Id = @Id"
                , new { id });
        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tiposcuentasOrdenados)
        {
            var query = "UPDATE TipoCuentas SET Orden = @Orden WHERE Id = @Id";
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(query, tiposcuentasOrdenados);
        }

    }
}
