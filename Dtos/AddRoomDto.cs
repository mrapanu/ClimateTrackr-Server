using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClimateTrackr_Server.Dtos
{
    public class AddRoomDto
    {
        public string RoomName {get; set;} = string.Empty;
        public Window Window { get; set; }
    }
}