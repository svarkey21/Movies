using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using Movie.Model;
using Newtonsoft.Json;
using System.IO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Movie.Controllers
{
    [Route("Movie")]
    [ApiController]
    public class Movie : ControllerBase
    {
        static MovieActions mop = new MovieActions();

        // GET: api/<Movie>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(JsonConvert.SerializeObject(mop.GetMovies()));
        }

        // GET api/<Movie>/5
        [HttpGet("{movieid}")]
        public IActionResult Get(int movieid)
        {
            List<MovieData> result = mop.GetMovies(movieid);
            if (result.Count == 0)
                return StatusCode(404);
            else
                return Ok(JsonConvert.SerializeObject(result));
        }

        // GET: api/<Movie>
        [HttpGet("/Movie/stats")]
        public IActionResult Stats()
        {            
            return Ok(mop.GetStats());
        }

        // POST api/<Movie>
        [HttpPost]
        public IActionResult PostAsync(MovieData newmovie)
        {
            try
            {
                mop.SaveMovie(newmovie);
            }
            catch(Exception ex) { return StatusCode(400); }
            return Ok();
        }
    }

    public class MovieActions
    {
        List<MovieData> movies;
        List<MovieStat> moviesstat;
        DataTable csvData = new DataTable();
        DataTable csvDataStats = new DataTable();


        public MovieActions()
        {

            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(@Directory.GetCurrentDirectory() + @"\Data\metadata.csv"))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields;
                    bool tableCreated = false;
                    while (tableCreated == false)
                    {
                        colFields = csvReader.ReadFields();
                        foreach (string column in colFields)
                        {
                            DataColumn datecolumn = new DataColumn(column);
                            datecolumn.AllowDBNull = true;
                            csvData.Columns.Add(datecolumn);
                        }
                        tableCreated = true;
                    }
                    while (!csvReader.EndOfData)
                    {
                        csvData.Rows.Add(csvReader.ReadFields());
                    }
                }

                using (TextFieldParser csvReader = new TextFieldParser(@Directory.GetCurrentDirectory()+@"\Data\stats.csv"))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields;
                    bool tableCreated = false;
                    while (tableCreated == false)
                    {
                        colFields = csvReader.ReadFields();
                        foreach (string column in colFields)
                        {
                            DataColumn datecolumn = new DataColumn(column);
                            datecolumn.AllowDBNull = true;
                            csvDataStats.Columns.Add(datecolumn);
                        }
                        tableCreated = true;
                    }
                    while (!csvReader.EndOfData)
                    {
                        csvDataStats.Rows.Add(csvReader.ReadFields());
                    }
                }
                

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            movies = csvData.AsEnumerable().Select(m => new MovieData()
            { Id= Int32.Parse(m.Field<string>("Id")), Duration = m.Field<string>("Duration"), Language = m.Field<string>("Language"), MovieId =Int32.Parse(m.Field<string>("MovieId")), ReleaseYear = Int32.Parse(m.Field<string>("ReleaseYear")), Title= m.Field<string>("Title") 
            }).ToList();

            moviesstat = csvDataStats.AsEnumerable().Select(m => new MovieStat()
            {
                MovieId = Int32.Parse(m.Field<string>("MovieId")),
                Watches = Int32.Parse(m.Field<string>("watchDurationMs")),
            }).ToList();
        }
        public string GetStats()
        {
            try
            {
                var ms = from csvDataStat in moviesstat
                        group csvDataStat by csvDataStat.MovieId into movgrp
                        select new
                        {
                            averageWatchDurationS = movgrp.Average(a=>a.Watches)/1000,
                            movieId = movgrp.Key,
                            watches = movgrp.Count(),
                        };

                var md = movies.Where(a => a.MovieId != 0 && a.Duration != "" && a.Language != "" && a.ReleaseYear != 0 && a.Title != "").GroupBy(x => x.MovieId).SelectMany(y => y.Where(z => z.Id == y.Max(i => i.Id))).OrderBy(a => a.Language);

                var flist = from l1 in md
                            from l2 in ms.Where(x => x.movieId == l1.MovieId)
                                            .DefaultIfEmpty()
                            select new { l1.MovieId, l1.Title, l2.averageWatchDurationS, l2.watches, l1.ReleaseYear };

                return JsonConvert.SerializeObject(flist.OrderByDescending(a=>a.watches).ThenByDescending(a=>a.ReleaseYear).Distinct());
            }
            catch
            {
                return JsonConvert.SerializeObject("");
            }
        }
        public List<MovieData> GetMovies()
        {
            try
            {
                return movies.OrderBy(a=>a.Language).ToList();
            }
            catch
            {
                return new List<MovieData>();
            }
        }
        public List<MovieData> GetMovies(int movieId)
        {
            try
            {
                List<MovieData> md = movies.Where(a => a.MovieId == movieId && a.Duration!="" && a.Language!="" && a.ReleaseYear!=0 && a.Title!="").GroupBy(x => x.Language).SelectMany(y => y.Where(z => z.Id == y.Max(i => i.Id))).OrderBy(a => a.Language).ToList();
                return md;
            }
            catch
            {
                return new List<MovieData>();
            }
        }

        public void SaveMovie(MovieData newmovie)
        {
            movies.Add(newmovie);
        }
    }
}
