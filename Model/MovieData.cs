using System.ComponentModel.DataAnnotations;

namespace Movie.Model
{
    public class MovieData
    {
        public MovieData()
        {

        }
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public string Duration { get; set; }
        public int ReleaseYear { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public int Id { get; set; }
    }

    public class MovieStat
    {
        public MovieStat()
        {

        }
        public int MovieId { get; set; }
        public string Title { get; set; }
        public int AverageWatchDurationS { get; set; }
        public int Watches { get; set; } = 0;
        public int ReleaseYear { get; set; } = 0;
    }
}
