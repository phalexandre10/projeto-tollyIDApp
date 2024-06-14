using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using Dapper;

namespace TolyIDApp
{
    class Program
    {
        static string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=b04121996m;Database=postgres";

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Menu:");
                Console.WriteLine("1. Cadastrar Pesquisador");
                Console.WriteLine("2. Cadastrar Animal");
                Console.WriteLine("3. Listar Pesquisadores");
                Console.WriteLine("4. Listar Animais");
                Console.WriteLine("5. Sair");
                Console.Write("Escolha uma opção: ");
                string opcao = Console.ReadLine();

                switch (opcao)
                {
                    case "1":
                        CadastrarPesquisador();
                        break;
                    case "2":
                        CadastrarAnimal();
                        break;
                    case "3":
                        ListarPesquisadores();
                        break;
                    case "4":
                        ListarAnimais();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Opção inválida, tente novamente.");
                        break;
                }
            }
        }

        static void CadastrarPesquisador()
        {
            Console.Clear();
            Console.Write("Nome: ");
            string nome = Console.ReadLine();
            Console.Write("Email: ");
            string email = Console.ReadLine();
            Console.Write("Instituição: ");
            string instituicao = Console.ReadLine();
            Console.Write("Telefone: ");
            string telefone = Console.ReadLine();

            using (IDbConnection dbConnection = new NpgsqlConnection(connectionString))
            {
                dbConnection.Open();
                string insertQuery = "INSERT INTO Pesquisador (Nome, Email, Instituicao, Telefone) VALUES (@Nome, @Email, @Instituicao, @Telefone)";
                var pesquisador = new { Nome = nome, Email = email, Instituicao = instituicao, Telefone = telefone };
                dbConnection.Execute(insertQuery, pesquisador);
            }
            Console.WriteLine("Pesquisador cadastrado com sucesso.");
            Console.WriteLine("Pressione qualquer tecla para voltar ao menu.");
            Console.ReadKey();
        }

        static void CadastrarAnimal()
        {
            Console.Clear();
            Console.Write("Padrão Escudo: ");
            string padraoEscudo = Console.ReadLine();
            Console.Write("Faixa Etária: ");
            string faixaEtaria = Console.ReadLine();
            Console.Write("Sexo (M/F): ");
            char sexo = Console.ReadKey().KeyChar;
            Console.WriteLine();
            Console.Write("Peso: ");
            decimal peso = decimal.Parse(Console.ReadLine());
            Console.Write("Medições Biométricas: ");
            string medicoesBiometricas = Console.ReadLine();
            Console.Write("Data de Coleta (yyyy-mm-dd): ");
            DateTime dataColeta = DateTime.Parse(Console.ReadLine());
            Console.Write("Latitude: ");
            decimal latitude = decimal.Parse(Console.ReadLine());
            Console.Write("Longitude: ");
            decimal longitude = decimal.Parse(Console.ReadLine());
            Console.Write("ID do Responsável: ");
            int idResponsavel = int.Parse(Console.ReadLine());
            Console.Write("Capturado Anteriormente (true/false): ");
            bool capturadoAnteriormente = bool.Parse(Console.ReadLine());

            using (IDbConnection dbConnection = new NpgsqlConnection(connectionString))
            {
                dbConnection.Open();
                using (var transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        // Inserir Animal
                        string insertAnimalQuery = "INSERT INTO Animal (Padrao_Escudo, Faixa_Etaria, Sexo, Peso, Medicoes_Biometricas, Data_Coleta, Latitude, Longitude, ID_Responsavel, Capturado_Anteriormente) " +
                                                   "VALUES (@Padrao_Escudo, @Faixa_Etaria, @Sexo, @Peso, @Medicoes_Biometricas, @Data_Coleta, @Latitude, @Longitude, @ID_Responsavel, @Capturado_Anteriormente) RETURNING ID_Animal";
                        var animal = new
                        {
                            Padrao_Escudo = padraoEscudo,
                            Faixa_Etaria = faixaEtaria,
                            Sexo = sexo,
                            Peso = peso,
                            Medicoes_Biometricas = medicoesBiometricas,
                            Data_Coleta = dataColeta,
                            Latitude = latitude,
                            Longitude = longitude,
                            ID_Responsavel = idResponsavel,
                            Capturado_Anteriormente = capturadoAnteriormente
                        };
                        int idAnimal = dbConnection.ExecuteScalar<int>(insertAnimalQuery, animal, transaction);

                        // Perguntar se deseja cadastrar uma amostra
                        Console.Write("Deseja cadastrar uma amostra para este animal? (S/N): ");
                        char resposta = Console.ReadKey().KeyChar;
                        Console.WriteLine();
                        if (char.ToUpper(resposta) == 'S')
                        {
                            Console.Write("Tipo de Amostra: ");
                            string tipoAmostra = Console.ReadLine();
                            Console.Write("Descrição: ");
                            string descricao = Console.ReadLine();

                            string insertAmostraQuery = "INSERT INTO Amostra (Tipo, Data_Coleta, Descricao, ID_Animal) VALUES (@Tipo, @Data_Coleta, @Descricao, @ID_Animal)";
                            var amostra = new
                            {
                                Tipo = tipoAmostra,
                                Data_Coleta = dataColeta,
                                Descricao = descricao,
                                ID_Animal = idAnimal
                            };
                            dbConnection.Execute(insertAmostraQuery, amostra, transaction);
                        }

                        transaction.Commit();
                        Console.WriteLine("Animal e amostra (se aplicável) cadastrados com sucesso.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Erro ao cadastrar animal: {ex.Message}");
                    }
                }
            }
            Console.WriteLine("Pressione qualquer tecla para voltar ao menu.");
            Console.ReadKey();
        }

        static void ListarPesquisadores()
        {
            Console.Clear();
            using (IDbConnection dbConnection = new NpgsqlConnection(connectionString))
            {
                dbConnection.Open();
                List<Pesquisador> pesquisadores = dbConnection.Query<Pesquisador>("SELECT * FROM Pesquisador").AsList();
                foreach (var pesquisador in pesquisadores)
                {
                    Console.WriteLine($"ID: {pesquisador.ID_Responsavel}, Nome: {pesquisador.Nome}, Email: {pesquisador.Email}, Instituição: {pesquisador.Instituicao}, Telefone: {pesquisador.Telefone}");
                }
            }
            Console.WriteLine("Pressione qualquer tecla para voltar ao menu.");
            Console.ReadKey();
        }

        static void ListarAnimais()
        {
            Console.Clear();
            using (IDbConnection dbConnection = new NpgsqlConnection(connectionString))
            {
                dbConnection.Open();
                List<Animal> animais = dbConnection.Query<Animal>("SELECT * FROM Animal").AsList();
                foreach (var animal in animais)
                {
                    Console.WriteLine($"ID: {animal.ID_Animal}, Padrão Escudo: {animal.Padrao_Escudo}, Faixa Etária: {animal.Faixa_Etaria}, Sexo: {animal.Sexo}, Peso: {animal.Peso}");
                }
            }
            Console.WriteLine("Pressione qualquer tecla para voltar ao menu.");
            Console.ReadKey();
        }
    }

    public class Animal
    {
        public int ID_Animal { get; set; }
        public string Padrao_Escudo { get; set; }
        public string Faixa_Etaria { get; set; }
        public char Sexo { get; set; }
        public decimal Peso { get; set; }
        public string Medicoes_Biometricas { get; set; }
        public DateTime Data_Coleta { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int ID_Responsavel { get; set; }
        public bool Capturado_Anteriormente { get; set; }
    }

    public class Pesquisador
    {
        public int ID_Responsavel { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Instituicao { get; set; }
        public string Telefone { get; set; }
    }

    public class Amostra
    {
        public int ID_Amostra { get; set; }
        public string Tipo { get; set; }
        public DateTime Data_Coleta { get; set; }
        public string Descricao { get; set; }
        public int ID_Animal { get; set; }
    }
}
