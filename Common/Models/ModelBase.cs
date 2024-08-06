namespace Common.Models
{
    public abstract class ModelBase
    {
        public string? Token { get; set; }

        public int? Top { get; set; }
        public int? Skip { get; set; }

        public string? FilterProperty { get; set; }
        public object? FilterValue { get; set; }
    }
}
