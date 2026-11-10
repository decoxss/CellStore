using Microsoft.Data.SqlClient;

namespace CellStore.Data;

public class Db
{
    private readonly string _cadena;

    public Db(IConfiguration config)
    {
        _cadena = config.GetConnectionString("Default")!;
    }

    public SqlConnection Conectar() => new SqlConnection(_cadena);
}