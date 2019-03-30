using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace CanvasRemindWebApp.ParsingFiles
{
    [DataContract(Name = "courses")]
    public class Courses
    {
        //Uses the Canvas API documentation and uses this data contract to parse the courses JSON item

            //Get the id of the courses JSON object
            [DataMember(Name = "id")]
            public Int64 Id { get; set; }

            //Get the name of the courses JSON object
            [DataMember(Name = "name")]
            public string Name { get; set; }

            //Get the workflow_state of the courses JSON object
            [DataMember(Name = "workflow_state")]
            public string WorkflowState { get; set; }

        }
    }
