﻿namespace Common.Dto
{
    public class UsersDto : DtoBase
    {
        public Guid Guid { get; set; }

        public string Name { get; set; } = null!;

        public short Height { get; set; }

        public short Weight { get; set; }

        // RadzenRadioButtonList работает только с int!
        public int Gender { get; set; }

        public string? About { get; set; }

        public short? HairFace { get; set; }
        public short? HairHead { get; set; }
        public short? HairIntim { get; set; }

        public DateTime BirthDate { get; set; }

        public bool IsDeleted { get; set; }
    }
}
