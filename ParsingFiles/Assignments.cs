using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Globalization;

namespace CanvasRemindWebApp.ParsingFiles
{
    [DataContract(Name = "assignments")]
    public class Assignments
    {
        //Uses the Canvas API documentation and uses this data contract to parse the assignments JSON item

            //Gets the id from JSON of the assignment item
            [DataMember(Name = "id")]
            public Int64 ID { get; set; }

            //Gets the name from JSON of the assignment item
            [DataMember(Name = "name")]
            public string Name { get; set; }

            //Gets the submission data from JSON of the assignment item
            [DataMember(Name = "submission")]
            public string Submission { get; set; }

            //Gets the due_at data from JSON of the assignment item
            [DataMember(Name = "due_at")]
            public string DueDate { get; set; }

            //Gets the DateTime object of DueDate. If it doesnt follow the exact parse format return 1/1/0001 <-- Can't remind with a due date that doesnt exist so make it never show past current date
            [IgnoreDataMember]
            public DateTime TimeDue
            {
                get
                {
                    DateTime TimeResult = new DateTime();
                    if (DateTime.TryParseExact(DueDate, "yyyy-MM-ddTHH:mmssZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeResult))
                    {
                        return TimeResult;
                    }
                    else
                    {
                        return TimeResult;
                    }
                }
            }

        
    }
}
