﻿using System;
using System.Collections.Generic;

namespace Wellness.WebAPI.Database
{
    public partial class ClanPrisustvo
    {
        public DateTime? DatumPrijave { get; set; }
        public int ClanId { get; set; }
        public int TreningId { get; set; }
        public bool? Prisustvovao { get; set; }
        public int Id { get; set; }
        public int? Ocjena { get; set; }

        public virtual Clan Clan { get; set; }
        public virtual Trening Trening { get; set; }
    }
}
