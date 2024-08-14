namespace Common.Dto.Requests
{
    public abstract class RequestDtoBase
    {
        public abstract string Uri { get; }

        public string? Token { get; set; }

        public int Take = 5;
        public int Skip = 0;

        public string? FilterProperty { get; set; }
        public string? FilterValue { get; set; }
    }
}
