using Common.Models;
using Microsoft.Data.SqlClient;
using PhotoSauce.MagicScaler;

namespace Program
{
    public static class AccountsPhotos
    {
        public static void Process(string connectionString)
        {
            List<AccountsData> accountsData = new List<AccountsData>();

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            SqlCommand command = new SqlCommand("TRUNCATE TABLE AccountsPhotos", conn);
            command.ExecuteNonQuery();

            command = new SqlCommand("SELECT Id, Guid, Name FROM Accounts WHERE Id <= 6", conn);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                accountsData.Add(new AccountsData { Id = reader.GetInt32(0), Guid = reader.GetGuid(1), Name = reader.GetString(2) });
            }

            ProcessUsersPhotos(accountsData, connectionString);

            conn.Close();
            conn.Dispose();
        }


        static void ProcessUsersPhotos(List<AccountsData> accountsData, string connectionString)
        {
            var dir = @"..\..\..\..\UI\wwwroot\images\AccountsPhotos\";

            foreach (var acc in accountsData)
            {
                var guid = Guid.NewGuid();

                Directory.CreateDirectory($@"{dir}\{acc.Id}\{guid}");

                foreach (var image in StaticData.Images)
                {
                    var fileName = $@"{dir}\{acc.Id}\{guid}\{image.Key}.jpg";

                    using (MemoryStream output = new MemoryStream(300000))
                    {
                        MagicImageProcessor.ProcessImage($@"{dir}\!Source\{acc.Id}.jpg", output, image.Value);
                        File.WriteAllBytes(fileName, output.ToArray());
                    }
                }

                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();

                SqlCommand command = new SqlCommand($"INSERT INTO AccountsPhotos (Guid, Comment, IsAvatar, AccountId, IsDeleted) VALUES ('{guid}', '{acc.Name}', 1, {acc.Id}, 0)", conn);

                command.ExecuteNonQuery();
            }
        }
    }


    public class AccountsData
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; } = null!;
    }
}
