namespace Program
{
    public class Program
    {
        const string connectionString = "Data Source=localhost\\SQLEXPRESS;Database=SwingHouse;Integrated Security=true;TrustServerCertificate=Yes";

        public static void Main(string[] args)
        {
            AccountsPhotos.Process(connectionString);

            //EventsPhotos.Process(connectionString);

            Console.WriteLine("Выполнено!");
            Console.ReadKey();
        }
    }
}