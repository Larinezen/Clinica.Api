using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Clinica.Api.Controllers.Models;
using System.Data;


namespace Clinica.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    
    public class ClinicController : Controller
    { 
        [HttpPost(Name = "SavePeople")]
        public IActionResult SavePeople([FromBody] People people)
        {
            var connection = new SqlConnection("Server=tcp:larinesql.database.windows.net,1433;Database=larinedb;User ID=larine;Password=475Z3A!!@@RikM;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            connection.Open();
            // A validação se o CPF já existe na base de dados pode ser feita antes do INSERT
            if (NumeroCpfsEncontrados(people.Cpf) > 0)
            {
                return BadRequest("Este cpf já existe em nossa base de dados");
            }
            if(NumeroEmailsEncontrados(people.Email) > 0)
            {
                return BadRequest("Este endereço de email já está cadastrado em nossa base de dados. Por favor, utilize um email diferente ");
            }
            var command = new SqlCommand("INSERT INTO clinica (Cpf, Nome, Nascimento, Email, Tel, Inclusao) VALUES (@Cpf, @Nome, @Nascimento, @Email, @Tel, @Inclusao)", connection);
            command.Parameters.AddWithValue("@Cpf", people.Cpf);
            command.Parameters.AddWithValue("@Nome", people.Nome);
            command.Parameters.AddWithValue("@Nascimento", people.Nascimento);
            command.Parameters.AddWithValue("@Email", people.Email);
            command.Parameters.AddWithValue("@Tel", people.Tel);
            command.Parameters.AddWithValue("@Inclusao", DateTime.Now);
            var dadosInseridos = command.ExecuteNonQuery();
            connection.Close();
            
            if (dadosInseridos > 0) 
            { 
                return Ok("Cadastro realizado com Sucesso");
            }
            else
            {
                return BadRequest("Infelizmente não foi possível salvar os dados");

            }                

        }
        // Método criado para que seja possível consultar em banco de dados quantas cpfs
        // com aquele mesmo número foram encontrados.
        private int NumeroCpfsEncontrados(string NumeroCpf) 
        { 
            var connection = new SqlConnection("Server=tcp:larinesql.database.windows.net,1433;Database=larinedb;User ID=larine;Password=475Z3A!!@@RikM;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            connection.Open ();
            var command = new SqlCommand("SELECT count(*) FROM dbo.clinica WHERE Cpf = @NumeroCpf", connection);
            command.Parameters.AddWithValue("NumeroCpf",NumeroCpf);
            int NumeroCpfsEncontrados = Convert.ToInt32(command.ExecuteScalar());
            connection.Close();
            return NumeroCpfsEncontrados;
        }
        //Método criado para consultar em banco de dados se aquele e-mail inserido ja existe na base de dados
        private int NumeroEmailsEncontrados(string NumeroEmail)
        {
            var connection = new SqlConnection("Server=tcp:larinesql.database.windows.net,1433;Database=larinedb;User ID=larine;Password=475Z3A!!@@RikM;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            connection.Open();
            var command = new SqlCommand("SELECT count(*) FROM dbo.clinica WHERE Email = @NumeroEmail", connection);
            command.Parameters.AddWithValue("NumeroEmail", NumeroEmail);
            int NumeroEmailsEncontrados = Convert.ToInt32(command.ExecuteScalar());
            connection.Close();
            return NumeroEmailsEncontrados;
        }


        public static List<People> _people = new List<People>();
        [HttpGet(Name = "GetPeople")]
        public IActionResult Get()
        {
            var connection = new SqlConnection("Server=tcp:larinesql.database.windows.net,1433;Database=larinedb;User ID=larine;Password=475Z3A!!@@RikM;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            connection.Open();
            var command = new SqlCommand("SELECT * FROM dbo.clinica", connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var people = new People();
                people.Cpf = reader.GetString(reader.GetOrdinal("Cpf"));
                people.Nome = reader.GetString(reader.GetOrdinal("Nome"));
                people.Nascimento = reader.GetDateTime(reader.GetOrdinal("Nascimento"));
                people.Email = reader.GetString(reader.GetOrdinal("Email"));
                people.Tel = reader.GetInt64(reader.GetOrdinal("Tel"));
                _people.Add(people);
            }
            connection.Close();
            return Ok(_people);
        }
    }
}
