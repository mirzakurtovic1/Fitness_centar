﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Wellness.Model.Requests
{
    public class ClanSearchRequest
    {
        public int? ClanId { get; set; }
        public int? OsobaId { get; set; }
        public string QrCodeText { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string KorisnickoIme { get; set; }


    }
}
