using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomActivity
{
    public class SalesforceLeadDto
    {
        public string City { get; set; }
        public string Company { get; set; }
        public string Country { get; set; }
        public string CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string FirstName { get; set; }
        public string Id { get; set; }
        public string Industry { get; set; }
        public bool? IsConverted { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public string LastModifiedById { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastName { get; set; }
        public string LeadSource { get; set; }
        public string MobilePhone { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public string Phone { get; set; }
        public string PostalCode { get; set; }
        public string RecordTypeId { get; set; }
        public string State { get; set; }
        public string Status { get; set; }
        public string Street { get; set; }
        public DateTime? SystemModstamp { get; set; }


        public override string ToString()
        {
            var properties = this.GetType()
                .GetProperties()
                .OrderBy(p => p.Name)
                .ToList();

            var item = new List<string>();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(this, null) != null ? prop.GetValue(this, null).ToString() : null;

                if (value != null)
                {
                    if (prop.PropertyType == typeof(string))
                    {

                        value = value.Replace("\"", "'").Replace("\n", "").Replace("\r", "");
                        item.Add($@"""{value}""");
                        continue;
                    }

                    item.Add(value);
                }
                else
                {
                    item.Add("");
                }

            }


            return string.Join(",", item);
        }

    }
}