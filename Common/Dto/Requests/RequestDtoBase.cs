namespace Common.Dto.Requests
{
    public abstract class RequestDtoBase
    {
        public abstract string Uri { get; }

        public string? Token { get; set; }


        int _top;
        public int? Top
        {
            get => _top;
            set => _top = value ?? 5;
        }

        int _skip;
        public int? Skip
        {
            get => _skip;
            set => _skip = value ?? 0;
        }

        public string? FilterProperty { get; set; }
        public object? FilterValue { get; set; }
    }
}
