// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using System;

namespace TrainSchedule.Models
{
    public class TrainLine
    {
        public string? Name { get; set; }

        public TrainSchedule? Schedule { get; set; }
    }
}
