using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using static MultiVendor.Controllers.UsersController;

namespace MultiVendor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
       public static  string _connectionString = "Server=localhost;Database=MultiVendor;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;";

        public class UserDTO
        {
            public UserDTO(int UserID, string Username, string Password, string Email, String PhoneNumber, int RoleID, DateTime CreateDate)
            {
                this.UserID = UserID;
                this.Username = Username;
                this.Password = Password;
                this.Email = Email;
                this.PhoneNumber = PhoneNumber;
                this.RoleID = RoleID;
                this.CreateDate = CreateDate;
            }
            public int UserID { get; set; }
            public string Username { get; set; }
            public String Password { get; set; }
            public String Email { get; set; }
            public String PhoneNumber { get; set; }
            public int RoleID { get; set; }
            public DateTime CreateDate { get; set; }

        }
        [HttpGet("All", Name = "GetAllUsers")] // Marks this method to respond to HTTP GET requests.
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<UserDTO>> GetAllUsers() // Define a method to get all students.
        {
            List<UserDTO> UsersList = GetDataAllUsers();
            if (UsersList.Count == 0)
            {
                return NotFound("No Users Found!");
            }
            return Ok(UsersList);
        }

        public static List<UserDTO> GetDataAllUsers()
        {
            var UsersList = new List<UserDTO>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_User", conn))
                {
                    cmd.Parameters.AddWithValue("@Action", "GetAllUsers");
                   
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                UsersList.Add(new UserDTO
                                (
                                    reader.GetInt32(reader.GetOrdinal("UserID")),
                                    reader.GetString(reader.GetOrdinal("Username")),
                                    reader.GetString(reader.GetOrdinal("Password")),
                                    reader.GetString(reader.GetOrdinal("Email")),
                                    reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                    reader.GetInt32(reader.GetOrdinal("RoleID")),
                                    reader.GetDateTime(reader.GetOrdinal("CreateDate"))
                                ));
                            }

                        }

                        
                    

                }
                
                return UsersList;
            }
                
        }
        [HttpPost(Name = "AddUser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UserDTO> AddUser(UserDTO newUserDTO)
        {
            //we validate the data here
            if (newUserDTO == null || string.IsNullOrEmpty(newUserDTO.Username) || string.IsNullOrEmpty(newUserDTO.Password) || string.IsNullOrEmpty(newUserDTO.Email)||(newUserDTO.RoleID>3||newUserDTO.RoleID<1))
            {
                return BadRequest("Invalid student data.");
            }

            //newStudent.Id = StudentDataSimulation.StudentsList.Count > 0 ? StudentDataSimulation.StudentsList.Max(s => s.Id) + 1 : 1;

           UserDTO user = new UserDTO(newUserDTO.UserID, newUserDTO.Username, newUserDTO.Password, newUserDTO.Email, newUserDTO.PhoneNumber, newUserDTO.RoleID, newUserDTO.CreateDate);
            
            
             
                newUserDTO.UserID = AddDataUser(user);

            //we return the DTO only not the full student object
            //we dont return Ok here,we return createdAtRoute: this will be status code 201 created.
            if (newUserDTO.UserID == 0)
                return NotFound($"User with ID is: {0} ");
            else
            return CreatedAtRoute("GetUserById", new { id = newUserDTO.UserID }, newUserDTO);
            
        }
        public static int AddDataUser(UserDTO userDTO)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("sp_User", connection))
            {
                command.Parameters.AddWithValue("@Action", "Insert");
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Username", userDTO.Username);
                command.Parameters.AddWithValue("@Password", userDTO.Password);
                command.Parameters.AddWithValue("@Email", userDTO.Email);
                command.Parameters.AddWithValue("@PhoneNumber", userDTO.PhoneNumber);
                command.Parameters.AddWithValue("@RoleID", 1);
                command.Parameters.AddWithValue("@CreateDate", userDTO.CreateDate);
                var outputIdParam = new SqlParameter("@UserID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputIdParam);

                connection.Open();
                command.ExecuteNonQuery();
                return (int)outputIdParam.Value;

                
            }
        }
        [HttpPut("{id}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UserDTO> UpdateStudent(int id, UserDTO updatedUser)
        {
            if (id < 1 || updatedUser == null || string.IsNullOrEmpty(updatedUser.Username) || string.IsNullOrEmpty(updatedUser.Password) || string.IsNullOrEmpty(updatedUser.PhoneNumber))
            {
                return BadRequest("Invalid user data.");
            }

            //var student = StudentDataSimulation.StudentsList.FirstOrDefault(s => s.Id == id);

            UserDTO user = GetUserDataById(id);


            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }


            user.Username = updatedUser.Username;
            user.Password = updatedUser.Password;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.Email = updatedUser.Email;
            user.CreateDate=user.CreateDate;
            user.RoleID = updatedUser.RoleID;
            if (UpdateUser(user)) {
                //we return the DTO not the full student object.
                return Ok(user); }
            else
            {
                return BadRequest("Invalid Result");
            }

                
            
        
        }
    [HttpGet("{id}", Name = "GetUserById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UserDTO> GetUserById(int id)
        {

            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            //var student = StudentDataSimulation.StudentsList.FirstOrDefault(s => s.Id == id);
            //if (student == null)
            //{
            //    return NotFound($"Student with ID {id} not found.");
            //}
            UserDTO user = GetUserDataById(id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            //here we get only the DTO object to send it back.
            //StudentDTO SDTO = student.SDTO;

            //we return the DTO not the student object.
            return Ok(user);

        }
        public static UserDTO GetUserDataById(int UserID)
    {


        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand("sp_User", connection))
        {
            command.Parameters.AddWithValue("@Action", "GetAllByID");
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserID", UserID);

            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new UserDTO
                    (
                        reader.GetInt32(reader.GetOrdinal("UserID")),
                                reader.GetString(reader.GetOrdinal("Username")),
                                reader.GetString(reader.GetOrdinal("Password")),
                                reader.GetString(reader.GetOrdinal("Email")),
                                reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                reader.GetInt32(reader.GetOrdinal("RoleID")),
                                reader.GetDateTime(reader.GetOrdinal("CreateDate"))
                    );
                }
                else
                {
                    return null;
                }
            }
        }
    }
        public static bool UpdateUser(UserDTO UserDTO)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("sp_User", connection))
            {
                command.Parameters.AddWithValue("@Action", "Update");
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@UserID", UserDTO.UserID);
                command.Parameters.AddWithValue("@Username", UserDTO.Username);
                command.Parameters.AddWithValue("@Password", UserDTO.Password);
                command.Parameters.AddWithValue("@Email", UserDTO.Email);
                command.Parameters.AddWithValue("@PhoneNumber", UserDTO.PhoneNumber);
                command.Parameters.AddWithValue("@RoleID", UserDTO.RoleID);
                command.Parameters.AddWithValue("@CreateDate", UserDTO.CreateDate);

                connection.Open();
                command.ExecuteNonQuery();
                return true;

            }
        }

        //here we use HttpDelete method
        [HttpDelete("{id}", Name = "DeleteUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteUser(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            // var student = StudentDataSimulation.StudentsList.FirstOrDefault(s => s.Id == id);
            // StudentDataSimulation.StudentsList.Remove(student);

            if (DeleteDataUser(id))

                return Ok($"User with ID {id} has been deleted.");
            else
                return NotFound($"User with ID {id} not found. no rows deleted!");
        }
        public static bool DeleteDataUser(int UserID)
        {

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("sp_User", connection))
            {
                command.Parameters.AddWithValue("@Action", "Delete");
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserID", UserID);

                connection.Open();

                int rowsAffected = (int)command.ExecuteScalar();
                return (rowsAffected == 1);


            }
            
        }
    }
 }
