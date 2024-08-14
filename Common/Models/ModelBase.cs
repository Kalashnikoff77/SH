namespace Common.Models
{
    public abstract class ModelBase
    {
        public string? Token { get; set; }

        public int Take { get; set; } = 5;
        public int Skip { get; set; } = 0;

        public string? FilterProperty { get; set; }
        public string? FilterValue { get; set; }
    }
}
