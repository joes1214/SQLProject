using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SQLProj.Models;
using System.Data;
using System.Data.SqlClient;

namespace SQLProj.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration configuration;
        private string dbConnect;
        private SqlConnection connection;

        public HomeController(IConfiguration config)
        {
            this.configuration = config;
            //Grabs connection from appsettings.json
            this.dbConnect = configuration.GetConnectionString("DBConnection");
            this.connection = new SqlConnection(dbConnect);
        }

        //Creates a list and sends it to Index
        public IActionResult Index()
        {
            List<Employee> data = new List<Employee>();
            string Query = "SELECT * FROM dbo.Employee";

            connection.Open();

            SqlCommand sqlCommand = new SqlCommand(Query, connection);
            SqlDataReader reader = sqlCommand.ExecuteReader();

            while (reader.Read())
            {
                data.Add(ReadDB(reader));
            }

            connection.Close();

            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name, LastName, Email")] string Name, string LastName, string Email)
        {
            //string InsertQuery = "INSERT INTO dbo.Employee SET first_name = @FNAME, last_name = @LNAME, email = @EMAIL";
            //This is what I would've used in php, I have no idea why it doesnt work, maybe the lack of ''?
            string InsertQuery = "INSERT INTO dbo.Employee (first_name, last_name, email) VALUES (@FNAME, @LNAME, @EMAIL)";

            if (Name != null && LastName != null && Email != null)
            {
                SqlCommand sqlCommand = new SqlCommand(InsertQuery, connection);

                sqlCommand.Parameters.AddWithValue("@FName", Name);
                sqlCommand.Parameters.AddWithValue("@LNAME", LastName);
                sqlCommand.Parameters.AddWithValue("@EMAIL", Email);

                try
                {
                    connection.Open();
                    sqlCommand.ExecuteNonQuery();
                    connection.Close();
                }
                catch // I used try-catch to iron out problems I was getting
                {
                    connection.Close();
                    //Catch if there is any error, returns with ViewData
                    ViewData["ERROR"] = "ERROR";
                    return View();
                }
                //if try-catch doesn't fail, redirect to Index
                return RedirectToAction(actionName: "Index");
            }

            //If statement not met, returns to Create View with ViewData
            ViewData["ERROR"] = "Empty Values";
            return View();
        }

        public IActionResult Search()
        {
            return View();
        }

        //Access through POST. Binds the input form to their respective variables
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([Bind("Id, Name, LastName, Email")] int Id = 0, string Name = "", string LastName = "", string Email = "")
        {
            //string Query = "SELECT * FROM dbo.Employee WHERE [first_name] LIKE @FNAME";
            string Query = "SELECT * FROM dbo.Employee WHERE (id = @ID) OR " +
                            "(first_name LIKE @FNAME) OR " +
                            "(last_name LIKE @LNAME) OR " +
                            "(email LIKE @EMAIL)";
            /*
             I had the worst time trying to figure out why my query wasnt working. It would work through SQL Management studio
             however for whatever reason it wouldn't work in C#. I wasn't totally sure how C# handles parameters and I wanted to learn
             how to use them. This took the most time to figure out because my previous Query was and is correct, however C#
             did not like the way I formatted it.
             */


            //If all the fields are empty it will return you to the View with an error message
            if (Id == 0 && string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(LastName) && string.IsNullOrEmpty(Email))
            {
                ViewData["ERROR"] = "Empty Fields";
                return View();
            }

            List<Employee> data = new List<Employee>();
            SqlCommand sqlCommand = new SqlCommand(Query, connection);

            sqlCommand.Parameters.AddWithValue("@ID", Id);

            if (string.IsNullOrEmpty(Name))
            {
                Name = "''";
            }

            if (string.IsNullOrEmpty(LastName))
            {
                LastName = "''";
            }

            if (string.IsNullOrEmpty(Email))
            {
                Email = "''";
            }

            sqlCommand.Parameters.AddWithValue("@FNAME", "%" + Name + "%");
            sqlCommand.Parameters.AddWithValue("@LNAME", "%" + LastName + "%");
            sqlCommand.Parameters.AddWithValue("@EMAIL", "%" + Email + "%");

            try
            {
                connection.Open();
                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    data.Add(ReadDB(reader));
                }

                connection.Close();
                return View(data);

            }
            catch
            {
                connection.Close();

                ViewData["ERROR"] = "ERROR";
                return View();
            }
        }

        //Reads through the results of a Query, populates the Employee Model fields returns the results
        private Employee ReadDB(SqlDataReader reader)
        {
            if (reader == null)
            {
                return new Employee();
            }

            Employee temp = new Employee();

            temp.Id = (int)reader["id"];
            temp.Name = (string)reader["first_name"];
            temp.LastName = (string)reader["last_name"];
            temp.Email = (string)reader["email"];

            return temp;
        }
    }
}
