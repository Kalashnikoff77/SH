using Common;
using Microsoft.Data.SqlClient;
using PhotoSauce.MagicScaler;

namespace Program
{
    public static class EventsPhotos
    {
        public static void Process(string connectionString)
        {
            List<EventsData> eventsData = new List<EventsData>();

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            SqlCommand command = new SqlCommand("SELECT Id, Guid, Name FROM Events", conn);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                eventsData.Add(new EventsData { Id = reader.GetInt32(0), Guid = reader.GetGuid(1), Name = reader.GetString(2) });
            }

            ProcessEventsPhotos(eventsData, connectionString);

            conn.Close();
            conn.Dispose();

        }

        static void ProcessEventsPhotos(List<EventsData> eventsData, string connectionString)
        {
            var dir = @"..\..\..\..\UI\wwwroot\images\EventsPhotos\";

            foreach (var acc in eventsData)
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

                SqlCommand command = new SqlCommand($"INSERT INTO EventsPhotos (Guid, Comment, IsAvatar, EventId, IsDeleted) VALUES ('{guid}', '{acc.Name}', 1, {acc.Id}, 0)", conn);
                command.ExecuteNonQuery();
            }
        }



        public class EventsData
        {
            public int Id { get; set; }
            public Guid Guid { get; set; }
            public string Name { get; set; } = null!;
        }

    }
}
