namespace Clinica.Api.Controllers.Models
{
    public class People
    {

        public string Nome { set; get; }
        public string Cpf { set; get; }  
        public string Email { set; get; }
        public DateTime? Nascimento { set; get; }
        public long   Tel { set; get; }

    }
}
