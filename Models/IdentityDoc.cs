using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocFormatterFace.API.Models
{
    public class IdentityDoc
    {
        public string CountryRegion { get; set; }
        public string DateOfBirth { get; set; }
        public string DateOfExpiration { get; set; }
        public string DocumentNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nationality { get; set; }
        public string Sex { get; set; }
        public string MRZ { get; set; }
        public float Confidence { get; set; }
    }
}
