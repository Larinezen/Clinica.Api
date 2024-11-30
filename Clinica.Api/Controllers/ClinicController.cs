using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using DocumentValidation;
using Clinica.Api.Controllers.Models;
using System.Data;
using System.Text.RegularExpressions;


namespace Clinica.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    
    public class ClinicController : Controller
    {
        private string _connectionString = "Server=tcp:larinesql.database.windows.net,1433;Database=larinedb;User ID=larine;Password=475Z3A!!@@RikM;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        [HttpPost(Name = "SavePeople")]
        public IActionResult SavePeople([FromBody] People people)
        {
            
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            // Validação de CPF  com base na biblioteca DocumentValidation instalada
            // Negação ! -- Se o CPF não for validado retornamos BadRequest
            if (!people.Cpf.ValidateCpf())
            {
                return BadRequest("CPF inválido");
            }
            // A validação se o CPF já existe na base de dados pode ser feita antes do INSERT
            if (NumeroCpfsEncontrados(people.Cpf) > 0)
            {
                return BadRequest("Este cpf já existe em nossa base de dados");
            }
            // Validação de email  com base na biblioteca DocumentValidation instalada
            // Negação ! -- Se o email não for validado retornamos BadRequest
            if (!people.Email.ValidateEmail())
            {
                return BadRequest("E-mail inválido");
            }
            if (NumeroEmailsEncontrados(people.Email) > 0)
            {
                return BadRequest("Este endereço de email já está cadastrado em nossa base de dados. Por favor, utilize um email diferente ");
            }
            if (!people.Tel.ValidatePhone())
            {
                return BadRequest("Número de telefone inválido");
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
            var connection = new SqlConnection(_connectionString);
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
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            var command = new SqlCommand("SELECT count(*) FROM dbo.clinica WHERE Email = @NumeroEmail", connection);
            command.Parameters.AddWithValue("NumeroEmail", NumeroEmail);
            int NumeroEmailsEncontrados = Convert.ToInt32(command.ExecuteScalar());
            connection.Close();
            return NumeroEmailsEncontrados;
        }
      
       
        [HttpGet(Name = "GetPeople")]
        public IActionResult Get([FromQuery] string Cpf)
        { 
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            var command = new SqlCommand("SELECT * FROM dbo.clinica WHERE Cpf = @NumeroCpf", connection);
            command.Parameters.AddWithValue("NumeroCpf", Cpf);
            var reader = command.ExecuteReader();
            var listCpf = new List<People>();
            while (reader.Read())
            {
                var people = new People();
                people.Cpf = reader.GetString(reader.GetOrdinal("Cpf"));
                people.Nome = reader.GetString(reader.GetOrdinal("Nome"));
                people.Nascimento = reader.GetDateTime(reader.GetOrdinal("Nascimento"));
                people.Email = reader.GetString(reader.GetOrdinal("Email"));
                people.Tel = reader.GetString(reader.GetOrdinal("Tel"));
                listCpf.Add(people);

               
            }
            connection.Close();
            if(listCpf.Count > 0)
            {
                    var response = new
                {
                    Message = "CPF localizado com sucesso !",
                    

                    };
                    return Ok(response);


            }
            else {
                var response = new
                {
                    Message = "CPF não localizado"

                };
                return BadRequest(response);
               
                }     
        }
    }
}
